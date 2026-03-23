namespace Pdf.Abstractions.DTO
{
    /// <summary>
    /// Contains footer information for PDF documents.
    /// Typically includes legal information, company details, and links.
    /// </summary>
    public class FooterDataDTO
    {
        /// <summary>Copyright notice with label and value (e.g., "© 2024 Company Name").</summary>
        public HtmlDataDTO? Copyright { get; set; }

        /// <summary>VAT/Tax ID with label and value (e.g., "VAT: IT12345678901").</summary>
        public HtmlDataDTO? Vat { get; set; }

        /// <summary>Terms and conditions with label and link/value.</summary>
        public HtmlDataDTO? TermsAndConditions { get; set; }
    }
}
