using Fcg.Core.WebApi.Middleware;
using Fcg.Orders.API.Endpoints.Player;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

namespace Fcg.Orders.API.Extentions
{
    public static class ApplicationExtensions
    {
        public static WebApplication AddAppConfiguration(this WebApplication app)
        {
            app.ConfigureEndpoints();
            app.UseSerilogRequestLogging();
            app.UseMiddleware<ExceptionMiddleware>();
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
            app.UseRouting();
            app.UseSwaggerExtension();  
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            return app;
        }

        private static WebApplication ConfigureEndpoints(this WebApplication app)
        {
            #region Order Endpoint
            app.MapOrderEndpoint();
            #endregion

            #region Health Check
            app.MapHealthChecks("/health/liveness", new HealthCheckOptions { Predicate = check => check.Tags.Contains("live") });
            app.MapHealthChecks("/health/readiness", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
            #endregion

            return app;
        }
    }
}
