using Fcg.Core.Abstractions.Interfaces;
using Fcg.Orders.Application.Common.Interfaces;
using Fcg.Orders.Domain.Repositories;
using Fcg.Orders.Infrastructure.Caching;
using Fcg.Orders.Infrastructure.Persistence;
using Fcg.Orders.Infrastructure.Queries;
using Fcg.Orders.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Fcg.Orders.Infrastructure.Extensions
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddMongoExtension(configuration);
            services.AddEntityExtension(configuration);
            services.AddMassTransitExtension(configuration);    
            services.AddCacheExtension(configuration);
            services.AddHealthCheckExtension(configuration);            
            services.AddScoped<IDbConnection>(sp => sp.GetRequiredService<OrderDbContext>().Database.GetDbConnection());
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IGameSnapshotRepository, GameSnapshotRepository>();
            services.AddScoped<IOrderQueryRepository, OrderQueryRepository>();
            services.AddScoped<ICustomerSnapshotRepository, CustomerRepository>();
            services.AddScoped<ICacheService, RedisCacheService>();            
            return services;
        }
    }
}
