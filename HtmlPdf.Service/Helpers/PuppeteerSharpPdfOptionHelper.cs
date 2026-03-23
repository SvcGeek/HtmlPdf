using HtmlPdf.Service.Options;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace HtmlPdf.Service.Helpers
{
    /// <summary>
    /// Helper class providing default configuration for Puppeteer PDF generation.
    /// Centralize PDF options here to maintain consistency across all endpoints.
    /// </summary>
    public static class PuppeteerSharpPdfOptionHelper
    {
        /// <summary>
        /// Returns default PDF generation options optimized for document printing.
        /// Configured for A4 paper with reasonable margins for professional documents.
        /// </summary>
        /// <returns>PdfOptions instance configured for A4 documents with margins.</returns>
        /// <remarks>
        /// PrintBackground=true ensures CSS backgrounds and colors are included in the PDF.
        /// Without this, background colors and images would be omitted.
        /// </remarks>
        public static PdfOptions GetPdfOptionsOrDefault(IOptionsMonitor<PdfRenderingOptions> options, string? pagePdfConfigurationName = null)
        {

            var pdfOptions = new PagePdfOption();

            if (options.CurrentValue.PagePdfOptions.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(pagePdfConfigurationName)) throw new Exception("PagePdfOptions has multiple values, you need to specify the one you need!");
                pdfOptions = options.CurrentValue.PagePdfOptions.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.NameOption) && x.NameOption.Equals(pagePdfConfigurationName, StringComparison.CurrentCultureIgnoreCase)) ?? throw new Exception($"PagePdfOptions with name {pagePdfConfigurationName} not found!");
            }

            pdfOptions = options.CurrentValue.PagePdfOptions.FirstOrDefault() ?? throw new Exception("Ops... PagePdfOptions need to be inited!");

            return new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = pdfOptions.PrintBackground,
                Landscape = pdfOptions.Landscape,
                MarginOptions = new MarginOptions
                {
                    Top = pdfOptions.MarginOptions.Top,
                    Bottom = pdfOptions.MarginOptions.Bottom,
                    Left = pdfOptions.MarginOptions.Left,
                    Right = pdfOptions.MarginOptions.Right
                },

            };
        }
    }
}
