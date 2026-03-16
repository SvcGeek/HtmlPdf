using HtmlPdf.Service.Infrastructure;
using HtmlPdf.Service.PdfHandlers;
using HtmlPdf.Service.Renderer;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HtmlPdf.Service.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering HtmlPdf.Service dependencies in the DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all core dependencies required for PDF generation:
        /// - Browser configuration and provider (singleton Chromium instance)
        /// - Template renderer (RazorLight engine)
        /// - PDF renderer (orchestrates template + browser)
        /// - All IEndpoint implementations (PDF handler endpoints)
        /// </summary>
        public static IServiceCollection AddCoreDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Browser configuration from appsettings (Browser section)
            services.Configure<BrowserOptions>(configuration.GetSection(BrowserOptions.SectionName));

            // PDF rendering configuration from appsettings (PdfRendering section)
            // Uses IOptionsMonitor for hot-reload support when appsettings.json changes
            services.Configure<PdfRenderingOptions>(configuration.GetSection(PdfRenderingOptions.SectionName));

            // Singleton browser – launched once at startup, reused for every request.
            // Registered as both singleton and hosted service to pre-warm the browser on app startup.
            services.AddSingleton<BrowserProvider>();
            services.AddHostedService(sp => sp.GetRequiredService<BrowserProvider>());

            // Template renderer (caches compiled Razor templates in memory).
            // RazorLight compiles .cshtml files to C# code, then to assemblies at runtime.
            services.AddSingleton<TemplateRenderer>();

            // PDF renderer – uses the shared browser and the template renderer.
            // This is the main orchestrator: Template → HTML → PDF.
            services.AddSingleton<IPdfRenderer, PuppeteerPdfRenderer>();

            // Register all PDF handlers (IEndpoint implementations)
            // This automatically discovers and registers all endpoint handlers in the assembly.
            services.RegisterPdfHandlers();

            return services;
        }

        /// <summary>
        /// Dynamically discovers and registers all IEndpoint implementations as singletons.
        /// This enables automatic registration of new PDF handlers without manual configuration.
        /// </summary>
        /// <remarks>
        /// IMPORTANT: Handlers are registered as singletons (not scoped) because:
        /// 1. They are resolved at application startup in MapRenderPdfEndpoints()
        /// 2. Startup happens before any HTTP request scope exists
        /// 3. The DI container prohibits resolving scoped services from the root provider
        /// 
        /// This means handlers cannot inject scoped dependencies directly.
        /// If you need per-request services, inject IServiceScopeFactory instead.
        /// </remarks>
        private static void RegisterPdfHandlers(this IServiceCollection services)
        {
            // Use reflection to find all concrete types that implement IEndpoint
            var endpointType = typeof(IEndpoint);
            var handlers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => endpointType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var handler in handlers)
            {
                // Register each handler as a singleton under the IEndpoint interface
                services.AddSingleton(endpointType, handler);
            }
        }
    }
}
