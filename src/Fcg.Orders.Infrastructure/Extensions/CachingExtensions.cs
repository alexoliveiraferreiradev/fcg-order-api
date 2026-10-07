using Fcg.Core.WebApi.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Fcg.Orders.Infrastructure.Extensions
{
    internal static class CachingExtensions
    {
        public static IServiceCollection AddCacheExtension(this IServiceCollection services, IConfiguration configuration )
        {
            services.AddOptions<RedisConnectionSettings>()
                .Bind(configuration.GetSection(RedisConnectionSettings.RedisSectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var redisConfig = configuration.GetSection(RedisConnectionSettings.RedisSectionName).Get<RedisConnectionSettings>();
            ArgumentNullException.ThrowIfNull(redisConfig, nameof(RedisConnectionSettings));
            services.Configure<RedisConnectionSettings>(configuration.GetSection(RedisConnectionSettings.RedisSectionName));

            var host = redisConfig.Host;
            var port = redisConfig.Port;

            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { { host, port } },
                Password = redisConfig.Password,
                AbortOnConnectFail = false,
                ConnectRetry = 5,
                ReconnectRetryPolicy = new ExponentialRetry(5000, 30000)
            };

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return ConnectionMultiplexer.Connect(configurationOptions);
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = configurationOptions;
                options.InstanceName = redisConfig.InstanceName;
            });

            return services;
        }

    }
}
