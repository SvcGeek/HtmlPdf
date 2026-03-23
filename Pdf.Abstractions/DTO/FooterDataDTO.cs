namespace Pdf.Abstractions.DTO
{
    public class FooterDataDTO
    {
        public HtmlDataDTO? Copyright { get; set; }
        public HtmlDataDTO? Vat { get; set; }
        public HtmlDataDTO? TermsAndConditions { get; set; }
    }
}
