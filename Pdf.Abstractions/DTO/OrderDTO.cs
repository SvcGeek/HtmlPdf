namespace Pdf.Abstractions.DTO
{
    /// <summary>
    /// Complete data model for order documents (invoices, receipts, order confirmations).
    /// Extends BaseDTO with order-specific fields like client signature.
    /// </summary>
    /// <remarks>
    /// This DTO is used directly by Razor templates as @Model.
    /// Inherits common fields (brand, client details, products) from BaseDTO.
    /// </remarks>
    public class OrderDTO : BaseDTO<ProductBaseDTO>
    {
        /// <summary>Terms and conditions text to display above the signature area.</summary>
        public HtmlDataDTO? TermsAndConditionsClientSignature { get; set; }

        /// <summary>
        /// base64-encoded image data for the client signature to display on the order document. This should be a PNG or JPEG image that captures the client's signature, encoded as a base64 string to be embedded directly in the PDF.
        /// </summary>
        public string? ClientSignatureImage { get; set; }
        public string? ClientSignatureImageType { get; set; } = "image/png";
        public string? ClientSignatureImageUrl => $"data:{ClientSignatureImageType};base64,{ClientSignatureImage}";
    }
}
