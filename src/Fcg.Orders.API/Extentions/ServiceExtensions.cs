

using Dapper;
using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.WebApi.Security;
using Fcg.Orders.API.Consumer;
using Fcg.Orders.Application.Features.Commands.FinalizeFailedOrder;
using Fcg.Orders.Application.Interfaces;
using Fcg.Orders.Domain.Interfaces;
using Fcg.Orders.Domain.Repositories;
using Fcg.Orders.Infrastructure.Caching;
using Fcg.Orders.Infrastructure.MessageBroker;
using Fcg.Orders.Infrastructure.Persistence;
using Fcg.Orders.Infrastructure.Queries;
using Fcg.Orders.Infrastructure.Queries.DapperHandler;
using Fcg.Orders.Infrastructure.Repositories;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using System.Data;
using System.Text;

namespace Fcg.Orders.API.Extentions
{
    public static class ServiceExtensions
    {
        public static WebApplicationBuilder AddServicesExtensions(this WebApplicationBuilder builder)
        {
            builder
                   .HealthCheckExtension()
                   .AddSerilogExtension()
                   .AddMasstransitExtension()
                   .JsonExtensions()
                   .AddRedisExtension()
                   .AddDbContextExtension()
                   .AddCQRSExtension()
                   .AddJwtBearerExtension()
                   .AddDependecyInjection();

            builder.Services.AddAuthorization(options =>
            {   
                options.AddPolicy("PlayersOnly", policy => policy.RequireRole("PlayerRole"));
            });

            SqlMapper.AddTypeHandler(new PriceTypeHandler());

            return builder;
        }
        #region Health Check
        private static WebApplicationBuilder HealthCheckExtension(this WebApplicationBuilder builder)
        {
            var redisConfig = builder.Configuration.GetSection(RedisSettings.RedisSectionName).Get<RedisSettings>();
            var connectionString = redisConfig != null && !string.IsNullOrEmpty(redisConfig.Host)
                ? $"{redisConfig.Host}:{redisConfig.Port},password={redisConfig.Password}"
                : "localhost:6379,password=secret_password";

            builder.Services.AddHealthChecks()
                .AddDbContextCheck<OrderDbContext>(
                name: "database-healthcheck",
                tags: new[] { "ready" })
                .AddRedis(
                    connectionString,
                    name: "redis-healthcheck",
                    tags: new[] { "ready" });
            return builder;
        }
        #endregion

        #region Serilog
        private static WebApplicationBuilder AddSerilogExtension(this WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });

            return builder;
        }
        #endregion

        #region Json Extension
        private static WebApplicationBuilder JsonExtensions(this WebApplicationBuilder builder)
        {
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

            builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
            return builder;
        }
        #endregion

        #region MassTransit
        private static WebApplicationBuilder AddMasstransitExtension(this WebApplicationBuilder builder)
        {
            builder.Services.AddOptions<RabbitMqSettings>().BindConfiguration(RabbitMqSettings.SectionName)
            .ValidateDataAnnotations().ValidateOnStart();

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(Program).Assembly);
                x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });
                x.AddConfigureEndpointsCallback((context, name, cfg) =>
                {
                    cfg.UseEntityFrameworkOutbox<OrderDbContext>(context);
                });
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqConfig = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
                    
                    cfg.Host(rabbitMqConfig.Host, rabbitMqConfig.Port, "/", h =>
                    {
                        h.Username(rabbitMqConfig.Username);
                        h.Password(rabbitMqConfig.Password);
                    });

                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                    cfg.ReceiveEndpoint(rabbitMqConfig.GameCreatedIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<GameCreatedConsumer>(context);  
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.OrderPaymentFailedQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<PaymentFailedEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.OrderPaymentSucessedQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<PaymentProcessedEventConsumer>(context);
                    });                   

                });
            });

            return builder;
        }
        #endregion

        #region DbContext Extension
        private static WebApplicationBuilder AddDbContextExtension(this WebApplicationBuilder builder)
        {
            var dbConfig = builder.Configuration.GetSection(DatabaseSettings.SectionaName).Get<DatabaseSettings>();
            ArgumentNullException.ThrowIfNull(dbConfig, nameof(DatabaseSettings));
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = $"{dbConfig.Host},{dbConfig.Port}",
                InitialCatalog = dbConfig.Databasename,
                Password = dbConfig.Password,
                UserID = dbConfig.Username,
                TrustServerCertificate = true,
                 Encrypt = false
            };

            builder.Services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseSqlServer(connectionStringBuilder.ConnectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                      maxRetryCount: 3,
                      maxRetryDelay: TimeSpan.FromSeconds(10),
                      errorNumbersToAdd: null);
                });
            });

            return builder;
        }
        #endregion

        #region Redis Extension
        private static WebApplicationBuilder AddRedisExtension(this WebApplicationBuilder builder)
        {
            var redisConfig = builder.Configuration.GetSection(RedisSettings.RedisSectionName).Get<RedisSettings>();
            ArgumentNullException.ThrowIfNull(redisConfig, nameof(RedisSettings));
            builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection(RedisSettings.RedisSectionName));

            var host = string.IsNullOrEmpty(redisConfig.Host) ? "localhost" : redisConfig.Host;
            var port = redisConfig.Port == 0 ? 6379 : redisConfig.Port;

            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { { host, port } },
                Password = redisConfig.Password,
                AbortOnConnectFail = false,
                ConnectRetry = 5,
                ReconnectRetryPolicy = new ExponentialRetry(5000, 30000)
            };

            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return ConnectionMultiplexer.Connect(configurationOptions);
            });

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = configurationOptions;
                options.InstanceName = redisConfig.InstanceName;
            });

            return builder;
        }
        #endregion

        #region JWT Bearer Extension
        private static WebApplicationBuilder AddJwtBearerExtension(this WebApplicationBuilder builder)
        {
            var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
            builder.Services.Configure<JwtSettings>(jwtSettingsSection);
            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
            var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
            builder.Services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience
                };
            });
            return builder;
        }
        #endregion

        #region CQRS Extension
        private static WebApplicationBuilder AddCQRSExtension(this WebApplicationBuilder builder)
        {
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(FinalizeFailedCommand).Assembly);
            });
            builder.Services.AddValidatorsFromAssembly(typeof(FinalizeFailedCommand).Assembly);
            return builder;
        }
        #endregion

        #region Dependecy Injection
        private static WebApplicationBuilder AddDependecyInjection(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IDbConnection>(sp => sp.GetRequiredService<OrderDbContext>().Database.GetDbConnection());
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IGameSnapshotRepository, GameSnapshotRepository>();
            builder.Services.AddScoped<IOrderQueryRepository, OrderQueryRepository>();
            return builder;
        }
        #endregion
    }
}
