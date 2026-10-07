using Fcg.Core.WebApi.Database;
using Fcg.Orders.Infrastructure.Persistence;
using Fcg.Orders.Infrastructure.Persistence.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Fcg.Orders.Infrastructure.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddMongoExtension(this IServiceCollection services, IConfiguration configuration)
        {
            MongoConventions.EnsureRegistered();

            services.AddOptions<MongoSettings>()
                .Bind(configuration.GetSection(MongoSettings.SectionName))
                .ValidateDataAnnotations().ValidateOnStart();

            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });

            services.AddScoped(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
                return client.GetDatabase(settings.DatabaseName);
            });

            return services;
        }

        public static async Task EnsureMongoIndexesAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
            await MongoIndexInitializer.EnsureIndexesAsync(database);
        }

        public static IServiceCollection AddEntityExtension(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<DatabaseConnectionSettings>()
                .Bind(configuration.GetSection(DatabaseConnectionSettings.DatabaseSettingsSection))
                .ValidateDataAnnotations().ValidateOnStart();

            var dbConfig = configuration.GetSection(DatabaseConnectionSettings.DatabaseSettingsSection).Get<DatabaseConnectionSettings>();
            ArgumentNullException.ThrowIfNull(dbConfig, nameof(DatabaseConnectionSettings));
            
            services.AddDbContext<OrderDbContext>((sp, options) =>
            {
                options.UseSqlServer(dbConfig.ToConnectionString(), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                      maxRetryCount: 3,
                      maxRetryDelay: TimeSpan.FromSeconds(10),
                      errorNumbersToAdd: null);
                });
            });

            return services;
        }
    }
}
