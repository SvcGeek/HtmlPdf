using System;
using System.Collections.Generic;
using System.Text;

namespace Pdf.Abstractions.DTO
{
    public class BaseDTO<T> where T : ProductBaseDTO
    {
        /// <summary>
        /// Name of the Razor template to render (without .cshtml extension).
        /// Must match a file in the Templates folder and be in the allowedTemplates whitelist.
        /// </summary>
        public string? Template { get; set; }

        /// <summary>
        /// BCP-47 language tag (e.g., "en", "it", "ar", "he").
        /// Used for localized content and can be accessed in templates via @Model.Language.
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// Text direction for the document: "ltr" (left-to-right) or "rtl" (right-to-left).
        /// Critical for proper rendering of Arabic, Hebrew, and other RTL languages.
        /// </summary>
        public string Direction { get; set; } = "ltr";

        public string? Brand { get; set; }
        /// <summary>
        /// string BrandLogo: base64-encoded image data for the brand logo to display on the wishlist document.
        /// </summary>
        public string? BrandLogo { get; set; }
        public string? BrandLogoType { get; set; } = "image/png";
        /// <summary>
        /// Assuming the image is in PNG format. Adjust if necessary.
        /// </summary>
        public string? BrandLogoUrl => $"data:{BrandLogoType};base64,{BrandLogo}";
        public HtmlDataDTO? TitleHeader { get; set; }
        public HtmlDataDTO? DateHeader { get; set; }
        public HtmlDataDTO? CtaHeader { get; set; }
        public HtmlDataDTO? ClientLabelDetails { get; set; }
        public HtmlDataDTO? ClientIDLabelDetails { get; set; }
        public HtmlDataDTO? MobileLabelDetails { get; set; }
        public HtmlDataDTO? EmailLabelDetails { get; set; }
        public HtmlDataDTO? ClientAdvisorLabelDetails { get; set; }
        public HtmlDataDTO? ClientAdvisorIDLabelDetails { get; set; }
        public HtmlDataDTO? StoreLabelDetails { get; set; }
        public HtmlDataDTO? ProductItemsTitle { get; set; }
        public ProductTableHeaderDTO? ProductTableHeader { get; set; }
        public List<T> ProductItems { get; set; } = new List<T>();
        public FooterDataDTO? FooterData { get; set; }
    }
}
