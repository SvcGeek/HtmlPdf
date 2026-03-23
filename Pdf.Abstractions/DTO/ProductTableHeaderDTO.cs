namespace Pdf.Abstractions.DTO
{
    public class ProductTableHeaderDTO
    {
        public HtmlDataDTO? ImageLabel { get; set; }
        public HtmlDataDTO? ProductLabel { get; set; }
        public HtmlDataDTO? MaterialLabel { get; set; }
        public HtmlDataDTO? ColorLabel { get; set; }
        public HtmlDataDTO? SizeLabel { get; set; }
        public HtmlDataDTO? QtyLabel { get; set; }
        public HtmlDataDTO? UnitLabel { get; set; }
        public HtmlDataDTO? TotalLabel { get; set; }
    }
}
