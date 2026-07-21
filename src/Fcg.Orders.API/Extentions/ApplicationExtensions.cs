using Fcg.Core.WebApi.Middleware;
using Fcg.Orders.Infrastructure.Persistence;
using Serilog;

namespace Fcg.Orders.API.Extentions
{
    public static class ApplicationExtensions
    {
        public static WebApplication AddAppConfiguration(this WebApplication app)
        {
            app.UseSerilogRequestLogging();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            return app;
        }
    }
}
