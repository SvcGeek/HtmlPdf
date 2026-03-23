using HtmlPdf.Service.Options;
using HtmlPdf.Service.PdfEndpoints;
using Microsoft.Extensions.Options;
using System.Net;

namespace HtmlPdf.Service.Extensions
{
    /// <summary>
    /// Extension methods for registering PDF rendering endpoints in the application pipeline.
    /// Endpoints are now configured via appsettings.json for easier management.
    /// </summary>
    public static class RenderPdfEndpointExtensions
    {
        /// <summary>
        /// Discovers and maps all registered IEndpoint implementations to their routes.
        /// Template whitelist is loaded from configuration and supports hot-reload.
        /// Called from Program.cs after services are built but before app.Run().
        /// </summary>
        /// <param name="app">The configured web application.</param>
        public static void MapRenderPdfEndpoints(this WebApplication app)
        {
            // Get the options monitor for accessing current configuration
            // Using IOptionsMonitor instead of IOptions enables hot-reload support
            var optionsMonitor = app.Services.GetRequiredService<IOptionsMonitor<PdfRenderingOptions>>();

            // Resolve all registered IEndpoint implementations from DI
            // These were registered automatically in RegisterPdfHandlers()
            var endpoints = app.Services.GetServices<IPdfEndpoint>();


            //easy way to check if the service is up and running without hitting the actual PDF rendering endpoints
            app.Map("api/HealthCheck", async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync("OK");
            });

            // Each endpoint registers its own route pattern and handler
            // Pass the options monitor so handlers can access the current whitelist
            foreach (var endpoint in endpoints)
            {
                endpoint.Map(app, optionsMonitor);
            }
        }
    }
}
