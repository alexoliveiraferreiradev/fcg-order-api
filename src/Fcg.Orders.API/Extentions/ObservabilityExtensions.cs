using Fcg.Orders.Infrastructure.Caching;
using Fcg.Orders.Infrastructure.Persistence;
using Serilog;

namespace Fcg.Orders.API.Extentions
{
    public static class ObservabilityExtensions
    {
        public static WebApplicationBuilder AddObservabilityExtension(this WebApplicationBuilder builder)
        {
            builder.AddSerilog();
            return builder;
        }

        #region Serilog
        private static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });

            return builder;
        }
        #endregion
    }
}
