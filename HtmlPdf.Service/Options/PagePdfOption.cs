using PuppeteerSharp.Media;

namespace HtmlPdf.Service.Options
{
    public class PagePdfOption
    {
        public string? NameOption { get; set; } = "defaultOption";
        public bool IsDefault { get; set; } = true;
        public bool PrintBackground { get; set; } = true;
        public bool Landscape { get; set; } = false;
        public MarginOptions MarginOptions { get; set; } = new MarginOptions
        {
            Top = "20mm",
            Bottom = "20mm",
            Left = "15mm",
            Right = "15mm"
        };
    }
}
