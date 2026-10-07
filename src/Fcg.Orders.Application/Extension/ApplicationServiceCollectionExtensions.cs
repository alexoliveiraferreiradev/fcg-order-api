using Fcg.Core.Abstractions.Application;
using Fcg.Orders.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Orders.Application.Extension
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddAplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
            });
            services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
            // A ordem importa: o LoggingBehavior é o mais externo, então também
            // registra as falhas de validação lançadas pelo ValidationBehavior.
            services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(LoggingBehavior<,>));
            services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(ValidationBehavior<,>));
            return services;
        }
    }
}
