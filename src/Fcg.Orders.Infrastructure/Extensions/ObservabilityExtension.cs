using Fcg.Core.Abstractions.Common;
using Fcg.Orders.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Fcg.Orders.Infrastructure.Extensions
{
    public static class ObservabilityExtension
    {
        #region Health Check
        public static IServiceCollection AddHealthCheckExtension(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHealthChecks()
                 .AddCheck("live", () => HealthCheckResult.Healthy(), tags: new[] { HealthCheckTags.Live })
                .AddDbContextCheck<OrderDbContext>(
                name: "database-healthcheck",
                tags: new[] { HealthCheckTags.Ready })
                .AddRedis(
                    sp => sp.GetRequiredService<IConnectionMultiplexer>(),
                    name: "redis-healthcheck",
                    tags: new[] { HealthCheckTags.Ready });
            return services;
        }
        #endregion
    }
}
