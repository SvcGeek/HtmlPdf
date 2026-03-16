using HtmlPdf.Service.Helpers;
using HtmlPdf.Service.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HtmlPdf.Service.Renderer
{
    /// <summary>
    /// Renders PDF documents by combining Razor template rendering with Chromium/Puppeteer.
    /// The process: Template (.cshtml) → HTML → Browser → PDF bytes.
    /// </summary>
    /// <remarks>
    /// This implementation uses a singleton browser instance with concurrent page management.
    /// Each PDF generation creates a new browser page (tab) and disposes it after rendering.
    /// Concurrency limits are configurable via appsettings.json and support hot-reload.
    /// </remarks>
    public sealed class PuppeteerPdfRenderer : IPdfRenderer
    {
        /// <summary>
        /// Dynamic concurrency limiter that adjusts when configuration changes.
        /// Limits the number of Chromium pages that can be open simultaneously.
        /// </summary>
        /// <remarks>
        /// The semaphore is recreated when MaxConcurrentRenderings changes in appsettings.json.
        /// Note: Changes take effect for new requests; in-flight requests continue with the old limit.
        /// </remarks>
        private SemaphoreSlim _concurrencyLimiter;

        private readonly BrowserProvider _browserProvider;
        private readonly TemplateRenderer _templateRenderer;
        private readonly ILogger<PuppeteerPdfRenderer> _logger;
        private readonly IOptionsMonitor<PdfRenderingOptions> _options;
        private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1, 1);

        public PuppeteerPdfRenderer(
            BrowserProvider browserProvider,
            TemplateRenderer templateRenderer,
            ILogger<PuppeteerPdfRenderer> logger,
            IOptionsMonitor<PdfRenderingOptions> options)
        {
            _browserProvider = browserProvider;
            _templateRenderer = templateRenderer;
            _logger = logger;
            _options = options;

            // Initialize with current configuration
            _concurrencyLimiter = new SemaphoreSlim(
                _options.CurrentValue.MaxConcurrentRenderings,
                _options.CurrentValue.MaxConcurrentRenderings);

            // Register callback for configuration changes
            _options.OnChange(OnConfigurationChanged);
        }

        /// <summary>
        /// Handles configuration changes by recreating the concurrency limiter.
        /// Called automatically when appsettings.json is modified.
        /// Includes validation to ensure configuration is safe before applying.
        /// </summary>
        private void OnConfigurationChanged(PdfRenderingOptions newOptions)
        {
            _updateLock.Wait();
            try
            {
                // Validate configuration before applying
                try
                {
                    newOptions.Validate();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Invalid PdfRenderingOptions configuration. Changes will not be applied. Error: {Message}", 
                        ex.Message);
                    return;
                }

                var oldLimit = _concurrencyLimiter.CurrentCount;
                var newLimit = newOptions.MaxConcurrentRenderings;

                if (oldLimit != newLimit)
                {
                    _logger.LogInformation(
                        "Concurrency limit changed from {OldLimit} to {NewLimit}. Recreating semaphore...",
                        oldLimit, newLimit);

                    // Dispose old semaphore and create new one
                    var oldSemaphore = _concurrencyLimiter;
                    _concurrencyLimiter = new SemaphoreSlim(newLimit, newLimit);
                    oldSemaphore.Dispose();

                    _logger.LogInformation("Concurrency limiter updated successfully.");
                }
            }
            finally
            {
                _updateLock.Release();
            }
        }

        /// <summary>
        /// Renders a PDF by first compiling the Razor template to HTML,
        /// then using Chromium's PDF engine to convert the HTML to PDF bytes.
        /// </summary>
        /// <param name="templateName">Template name (without .cshtml extension).</param>
        /// <param name="model">Strongly-typed model passed to the Razor template as @Model.</param>
        /// <returns>Raw PDF bytes ready to be served to the client.</returns>
        public async Task<byte[]> RenderAsync(string templateName, object model)
        {
            _logger.LogInformation("Rendering PDF for template '{Template}'", templateName);
            var html = string.Empty;
            try
            {
                // Step 1: Compile and render the Razor template to HTML string
                // RazorLight compiles .cshtml → C# code → Assembly → Executes with model
                html = await _templateRenderer.RenderTemplateAsync(templateName, model);
            }
            catch (Exception e)
            {
                // Re-throw template compilation/rendering errors
                // Common issues: missing PreserveCompilationContext, template syntax errors
                throw;
            }

            // Step 2: Get the shared Chromium browser instance (initialized at startup)
            var browser = await _browserProvider.GetBrowserAsync();

            // Acquire a concurrency slot (max N concurrent pages)
            // This prevents memory issues when handling many simultaneous PDF requests
            await _concurrencyLimiter.WaitAsync();
            try
            {
                // Create a new browser page (tab) for this PDF generation
                await using var page = await browser.NewPageAsync();

                // Load the HTML content into the page and wait for network to be idle
                // Networkidle0: waits until there are no network connections for at least 500ms
                // This ensures all resources (images, CSS, fonts) are fully loaded
                await page.SetContentAsync(html, new NavigationOptions
                {
                    WaitUntil = [WaitUntilNavigation.Networkidle0]
                });

                // Generate PDF from the rendered page using default A4 settings
                var pdf = await page.PdfDataAsync(PuppeteerSharpPdfOptionHelper.GetDefaultPdfOptions());

                _logger.LogInformation("PDF rendered successfully for template '{Template}' ({Bytes} bytes)", templateName, pdf.Length);

                return pdf;
            }
            finally
            {
                _concurrencyLimiter.Release();
            }
        }
    }
}
