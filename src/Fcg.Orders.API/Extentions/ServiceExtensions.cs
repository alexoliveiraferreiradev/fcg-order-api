using Fcg.Core.Abstractions.Interfaces;
using Fcg.Orders.Application.Interfaces;
using Fcg.Orders.Domain.Interfaces;
using Fcg.Orders.Infrastructure.MessageBroker;
using Fcg.Orders.Infrastructure.Persistence;
using Fcg.Orders.Infrastructure.Queries;
using Fcg.Orders.Infrastructure.Repositories;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;

namespace Fcg.Orders.API.Extentions
{
    public static class ServiceExtensions
    {
        public static WebApplicationBuilder AddServicesExtensions(this WebApplicationBuilder builder)
        {
            builder
                .AddMasstransitExtension()
                .AddDbContextExtension();
            return builder;
        }

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
                });
            });

            return builder;
        }

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


        private static WebApplicationBuilder AddDependecyInjection(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IDbConnection>(sp => sp.GetRequiredService<OrderDbContext>().Database.GetDbConnection());
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IGameSnapshotRepository, GameSnapshotRepository>();
            builder.Services.AddScoped<IOrderQueryRepository, OrderQueryRepository>();
            return builder;
        }
    }
}
