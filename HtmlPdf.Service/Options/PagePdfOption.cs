using PuppeteerSharp.Media;

namespace HtmlPdf.Service.Options
{
    /// <summary>
    /// Represents a named PDF generation configuration profile.
    /// Allows defining multiple PDF layouts (portrait/landscape, different margins) 
    /// that can be selected per-request or per-template.
    /// </summary>
    /// <remarks>
    /// Multiple profiles enable scenarios like:
    /// - Portrait invoices vs. landscape reports
    /// - Standard margins vs. minimal margins for forms
    /// - Print-optimized vs. screen-optimized layouts
    /// </remarks>
    public class PagePdfOption
    {
        /// <summary>
        /// Unique name for this PDF configuration profile.
        /// Referenced when requesting a specific layout (e.g., "portrait-standard", "landscape-wide").
        /// </summary>
        public string? NameOption { get; set; } = "defaultOption";

        /// <summary>
        /// Indicates if this is the default profile used when no specific profile is requested.
        /// Only one profile should have IsDefault=true.
        /// </summary>
        public bool IsDefault { get; set; } = true;

        /// <summary>
        /// Include CSS backgrounds and colors in the PDF output.
        /// Without this, background colors, images, and gradients are omitted.
        /// </summary>
        public bool PrintBackground { get; set; } = true;

        /// <summary>
        /// Generate PDF in landscape orientation (horizontal).
        /// False = portrait (vertical), True = landscape (horizontal).
        /// </summary>
        public bool Landscape { get; set; } = false;

        /// <summary>
        /// Page margin settings for the PDF output.
        /// Margins are specified in CSS units (mm, cm, in, px).
        /// </summary>
        public MarginOptions MarginOptions { get; set; } = new MarginOptions
        {
            Top = "20mm",
            Bottom = "20mm",
            Left = "15mm",
            Right = "15mm"
        };
    }
}
