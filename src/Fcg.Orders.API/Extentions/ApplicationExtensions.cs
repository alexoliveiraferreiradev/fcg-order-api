using Fcg.Core.Abstractions.Common;
using Fcg.Core.WebApi.Middleware;
using Fcg.Orders.API.Endpoints.Player;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using Serilog;
using System.Security.Claims;

namespace Fcg.Orders.API.Extentions
{
    public static class ApplicationExtensions
    {
        public static WebApplication AddAppConfiguration(this WebApplication app)
        {
            app.UseHttpMetrics();
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate =
                    "[OrderAPI] {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000}ms";

                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        diagnosticContext.Set("UserId", userId);
                    }
                };
            });
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwaggerExtension();

            app.MapMetrics();
            app.ConfigureEndpoints();
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
            return app;
        }

        private static WebApplication ConfigureEndpoints(this WebApplication app)
        {
            #region Order Endpoint
            app.MapOrderEndpoint();
            #endregion

            #region Health Check
            app.MapHealthChecks("/health/liveness", new HealthCheckOptions { Predicate = check => check.Tags.Contains(HealthCheckTags.Live) });
            app.MapHealthChecks("/health/readiness", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(HealthCheckTags.Ready),
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                }
            });
            #endregion

            return app;
        }
    }
}
