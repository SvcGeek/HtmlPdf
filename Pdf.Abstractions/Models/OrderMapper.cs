using System;
using System.Collections.Generic;
using System.Text;

namespace Pdf.Abstractions.Models
{
    /// <summary>
    /// Simplified mapping structure for order data transformation.
    /// Contains essential order fields without the complexity of OrderMapperDTO.
    /// </summary>
    /// <remarks>
    /// Use this when you need basic order mapping without invoice breakdown details.
    /// For full invoice details including financial breakdowns, use OrderMapperDTO.
    /// </remarks>
    public class OrderMapper
    {
        /// <summary>Base64-encoded brand logo image.</summary>
        public string? Logo { get; set; }

        /// <summary>Brand or company name.</summary>
        public string? Brand { get; set; }

        // ── Header Information ────────────────────────────────────────────────────────
        /// <summary>Document title for the header section.</summary>
        public string? TitleHeader { get; set; }

        /// <summary>Date string for the header section.</summary>
        public string? DateHeader { get; set; }

        // ── Client, Advisor, and Store Details ───────────────────────────────────────
        /// <summary>Customer name.</summary>
        public string? Client { get; set; }

        /// <summary>Customer ID or account number.</summary>
        public string? ClientId { get; set; }

        /// <summary>Customer mobile phone.</summary>
        public string? Mobile { get; set; }

        /// <summary>Customer email address.</summary>
        public string? Email { get; set; }

        /// <summary>Sales advisor name.</summary>
        public string? ClientAdvisor { get; set; }

        /// <summary>Sales advisor ID.</summary>
        public string? ClientAdvisorID { get; set; }

        /// <summary>Store name.</summary>
        public string? Store { get; set; }

        // ── Client Signature ──────────────────────────────────────────────────────────
        /// <summary>Base64-encoded client signature image for order confirmation.</summary>
        public string? ClientSignatureImage { get; set; }

        // ── Footer Information ────────────────────────────────────────────────────────
        /// <summary>Copyright notice text.</summary>
        public string? Copyright { get; set; }

        /// <summary>VAT or tax identification number.</summary>
        public string? Vat { get; set; }

        /// <summary>URL to terms and conditions.</summary>
        public string? TermsAndConditionLink { get; set; }
    }
}
