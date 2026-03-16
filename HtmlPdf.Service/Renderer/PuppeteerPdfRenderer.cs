using System;
using System.Threading;
using System.Threading.Tasks;
using HtmlPdf.Service.Infrastructure;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace HtmlPdf.Service.Renderer
{
    public sealed class PuppeteerPdfRenderer : IPdfRenderer
    {
        /// <summary>
        /// Limits the number of Chromium pages that can be open simultaneously
        /// to prevent resource exhaustion under high load.
        /// </summary>
        private static readonly SemaphoreSlim _concurrencyLimiter = new SemaphoreSlim(10, 10);

        private readonly BrowserProvider _browserProvider;
        private readonly TemplateRenderer _templateRenderer;
        private readonly ILogger<PuppeteerPdfRenderer> _logger;

        public PuppeteerPdfRenderer(
            BrowserProvider browserProvider,
            TemplateRenderer templateRenderer,
            ILogger<PuppeteerPdfRenderer> logger)
        {
            _browserProvider = browserProvider;
            _templateRenderer = templateRenderer;
            _logger = logger;
        }

        public async Task<byte[]> RenderAsync(string templateName, object model)
        {
            _logger.LogInformation("Rendering PDF for template '{Template}'", templateName);

            var html = await _templateRenderer.RenderTemplateAsync(templateName, model);

            var browser = await _browserProvider.GetBrowserAsync();

            await _concurrencyLimiter.WaitAsync();
            try
            {
                await using var page = await browser.NewPageAsync();

                await page.SetContentAsync(html, new NavigationOptions
                {
                    WaitUntil = [WaitUntilNavigation.Networkidle0]
                });

                var pdf = await page.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "15mm",
                        Bottom = "15mm",
                        Left = "15mm",
                        Right = "15mm"
                    }
                });

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
