using Pdf.Abstractions.DTO;
using System;
using System.Collections.Generic;
using System.Text;

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
    public class OrderMapperDTO
    {
        /// <summary>Base64-encoded brand logo image.</summary>
        public string? Logo { get; set; }

        /// <summary>MIME type of the logo image (e.g., "image/png", "image/jpeg").</summary>
        public string? LogoType { get; set; }

        /// <summary>Brand or company name.</summary>
        public string? Brand { get; set; }

        // ── Header Section ───────────────────────────────────────────────────────────
        /// <summary>Document title displayed in the header.</summary>
        public string? TitleHeader { get; set; }

        /// <summary>Date displayed in the header (formatted string).</summary>
        public string? DateHeader { get; set; }

        // ── Client, Advisor, Store Information ───────────────────────────────────────
        /// <summary>Customer/client name.</summary>
        public string? Client { get; set; }

        /// <summary>Customer identifier or account number.</summary>
        public string? ClientId { get; set; }

        /// <summary>Customer mobile phone number.</summary>
        public string? Mobile { get; set; }

        /// <summary>Customer email address.</summary>
        public string? Email { get; set; }

        /// <summary>Name of the sales advisor handling this order.</summary>
        public string? ClientAdvisor { get; set; }

        /// <summary>Unique identifier for the sales advisor.</summary>
        public string? ClientAdvisorID { get; set; }

        /// <summary>Store name where the order was placed.</summary>
        public string? Store { get; set; }

        // ── Invoice Breakdown (Financial Details) ────────────────────────────────────
        /// <summary>Total number of items ordered (sum of quantities).</summary>
        public string? OrderedItemsInvoiceBreakdown { get; set; }

        /// <summary>Discount amount in primary currency.</summary>
        public string? DiscountInvoiceBreakdown { get; set; }

        /// <summary>Discount amount in local currency (for multi-currency support).</summary>
        public string? DiscountLocalInvoiceBreakdown { get; set; }

        /// <summary>Total order amount in primary currency.</summary>
        public string? TotalAmountInvoiceBreakdown { get; set; }

        /// <summary>Total order amount in local currency.</summary>
        public string? TotalAmountLocalInvoiceBreakdown { get; set; }

        /// <summary>Advance payment received in primary currency.</summary>
        public string? AdvancedPaymentInvoiceBreakdown { get; set; }

        /// <summary>Advance payment received in local currency.</summary>
        public string? AdvancedPaymentLocalInvoiceBreakdown { get; set; }

        /// <summary>Remaining balance due in primary currency.</summary>
        public string? OrderBalanceInvoiceBreakdown { get; set; }

        /// <summary>Remaining balance due in local currency.</summary>
        public string? OrderBalanceLocalInvoiceBreakdown { get; set; }

        // ── Pickup Store Information ──────────────────────────────────────────────────
        /// <summary>Physical street address of the pickup store.</summary>
        public string? AddressPickUpStore { get; set; }

        /// <summary>Postal code (CAP in Italy) of the pickup store.</summary>
        public string? CapPickUpStore { get; set; }

        /// <summary>Contact phone number for the pickup store.</summary>
        public string? PhonePickUpStore { get; set; }

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
