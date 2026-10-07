using Fcg.Orders.Application.Extension;
using Fcg.Orders.Infrastructure.Extensions;

namespace Fcg.Orders.API.Extentions
{
    public static class ServiceExtensions
    {
        public static WebApplicationBuilder AddServicesExtensions(this WebApplicationBuilder builder)
        {
            builder.AddObservabilityExtension()
                   .AddAuthorizationExtension()
                   .AddSwaggerExtension();

            builder.Services.AddAplicationServices();
            builder.Services.AddInfrastructure(builder.Configuration);
            return builder;
        }      
    }
}
