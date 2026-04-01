namespace Pdf.Abstractions.Models
{
    /// <summary>
    /// Data transfer object used as an intermediate mapping structure for order documents.
    /// Facilitates transformation from external data sources to the OrderDTO format.
    /// </summary>
    /// <remarks>
    /// This DTO acts as a flat structure that can be easily populated from various sources
    /// (API responses, database queries) before being mapped to the hierarchical OrderDTO
    /// used in Razor templates.
    /// </remarks>
    public class OrderMapper
    {
        /// <summary>Base64-encoded brand logo image.</summary>
        public string? BrandLogo { get; set; }

        /// <summary>MIME type of the logo image (e.g., "image/png", "image/jpeg").</summary>
        public string? LogoType { get; set; }

        // ── Header Section ───────────────────────────────────────────────────────────
        /// <summary>Document title displayed in the header.</summary>
        public string? TitleHeader { get; set; }

        /// <summary>Date displayed in the header (formatted string).</summary>
        public string? DateHeader { get; set; }

        // ── Client, Advisor, Store Information ───────────────────────────────────────
        /// <summary>Customer/client name.</summary>
        public string? ClientName { get; set; }

        /// <summary>Customer identifier or account number.</summary>
        public string? ClientId { get; set; }

        /// <summary>Customer mobile phone number.</summary>
        public string? Mobile { get; set; }

        /// <summary>Customer email address.</summary>
        public string? Email { get; set; }

        /// <summary>Store name where the order was placed.</summary>
        public string? DeliveryDetails { get; set; }

        // ── Invoice Breakdown (Financial Details) ────────────────────────────────────
        /// <summary>Total number of items ordered (sum of quantities).</summary>
        public string? DeliveryAddress { get; set; }

        /// <summary>Discount amount in primary currency.</summary>
        public string? DeliveryMobileContact { get; set; }

        // ── Client Signature ──────────────────────────────────────────────────────────
        /// <summary>Base64-encoded client signature image.</summary>
        public string? ClientSignatureImage { get; set; }

        /// <summary>MIME type of the signature image (e.g., "image/png").</summary>
        public string? ClientSignatureImageType { get; set; }

        // ── Footer Information ────────────────────────────────────────────────────────
        /// <summary>Copyright notice text for the document footer.</summary>
        public string? Copyright { get; set; }

        /// <summary>VAT/tax identification number.</summary>
        public string? Vat { get; set; }

        /// <summary>URL link to terms and conditions document.</summary>
        public string? TermsAndConditionLink { get; set; }
    }
}
