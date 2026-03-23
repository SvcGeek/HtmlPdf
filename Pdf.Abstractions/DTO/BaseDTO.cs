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

        // ── Branding ─────────────────────────────────────────────────────────────────────
        /// <summary>Brand or company name to display on the document.</summary>
        public string? Brand { get; set; }

        /// <summary>
        /// Base64-encoded brand logo image.
        /// Embedded directly in the PDF to avoid external resource dependencies.
        /// </summary>
        public string? BrandLogo { get; set; }

        /// <summary>
        /// MIME type of the brand logo (e.g., "image/png", "image/jpeg").
        /// Defaults to PNG format.
        /// </summary>
        public string? BrandLogoType { get; set; } = "image/png";

        /// <summary>
        /// Complete data URL for the brand logo.
        /// Automatically constructed from BrandLogo and BrandLogoType.
        /// Can be used directly in &lt;img src="@Model.BrandLogoUrl" /&gt;
        /// </summary>
        public string? BrandLogoUrl => $"data:{BrandLogoType};base64,{BrandLogo}";

        // ── Header Section ───────────────────────────────────────────────────────────────
        /// <summary>Document title (e.g., "Order Confirmation", "Invoice").</summary>
        public HtmlDataDTO? TitleHeader { get; set; }

        /// <summary>Date information for the document header.</summary>
        public HtmlDataDTO? DateHeader { get; set; }

        /// <summary>Call-to-action text or button in the header (optional).</summary>
        public HtmlDataDTO? CtaHeader { get; set; }

        // ── Client Details Section ───────────────────────────────────────────────────────
        /// <summary>Client/customer name with optional label for translation.</summary>
        public HtmlDataDTO? ClientLabelDetails { get; set; }

        /// <summary>Client ID or account number with optional label.</summary>
        public HtmlDataDTO? ClientIDLabelDetails { get; set; }

        /// <summary>Client mobile phone number with optional label.</summary>
        public HtmlDataDTO? MobileLabelDetails { get; set; }

        /// <summary>Client email address with optional label.</summary>
        public HtmlDataDTO? EmailLabelDetails { get; set; }

        /// <summary>Sales advisor name with optional label.</summary>
        public HtmlDataDTO? ClientAdvisorLabelDetails { get; set; }

        /// <summary>Sales advisor ID with optional label.</summary>
        public HtmlDataDTO? ClientAdvisorIDLabelDetails { get; set; }

        /// <summary>Store name with optional label.</summary>
        public HtmlDataDTO? StoreLabelDetails { get; set; }

        // ── Products Section ─────────────────────────────────────────────────────────────
        /// <summary>Section title for the product items list (e.g., "Ordered Items").</summary>
        public HtmlDataDTO? ProductItemsTitle { get; set; }

        /// <summary>Column headers for the product table (Image, Product, Size, Qty, etc.).</summary>
        public ProductTableHeaderDTO? ProductTableHeader { get; set; }

        /// <summary>
        /// List of product items in the document.
        /// Generic type T allows different product representations (basic vs. with pricing).
        /// </summary>
        public List<T> ProductItems { get; set; } = new List<T>();

        // ── Footer Section ───────────────────────────────────────────────────────────────
        /// <summary>Footer data including copyright, VAT, and terms & conditions.</summary>
        public FooterDataDTO? FooterData { get; set; }
    }
}
