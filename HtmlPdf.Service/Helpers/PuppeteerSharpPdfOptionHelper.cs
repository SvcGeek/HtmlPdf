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
        public static PdfOptions GetDefaultPdfOptions()
        {
            return new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "20mm",
                    Bottom = "20mm",
                    Left = "15mm",
                    Right = "15mm"
                }
            };
        }
    }
}
