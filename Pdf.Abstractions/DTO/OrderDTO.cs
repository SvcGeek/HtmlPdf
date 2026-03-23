namespace Pdf.Abstractions.DTO
{
    public class OrderDTO : BaseDTO<ProductBaseDTO>
    {
        public HtmlDataDTO? TermsAndConditionsClientSignature { get; set; }

        /// <summary>
        /// base64-encoded image data for the client signature to display on the order document. This should be a PNG or JPEG image that captures the client's signature, encoded as a base64 string to be embedded directly in the PDF.
        /// </summary>
        public string? ClientSignatureImage { get; set; }
        public string? ClientSignatureImageType { get; set; } = "image/png";
        public string? ClientSignatureImageUrl => $"data:{ClientSignatureImageType};base64,{ClientSignatureImage}";
    }
}
