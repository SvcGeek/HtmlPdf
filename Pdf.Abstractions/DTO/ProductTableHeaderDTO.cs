namespace Pdf.Abstractions.DTO
{
    /// <summary>
    /// Defines the column headers for the product items table in order documents.
    /// Each property represents a table column header with translatable labels.
    /// </summary>
    /// <remarks>
    /// Using HtmlDataDTO allows each header to have both a translated label
    /// and potentially a value (though typically only Label is used for headers).
    /// This structure supports multi-language PDFs where headers are translated.
    /// </remarks>
    public class ProductTableHeaderDTO
    {
        /// <summary>Header label for the product image column.</summary>
        public HtmlDataDTO? ImageLabel { get; set; }

        /// <summary>Header label for the product name/description column.</summary>
        public HtmlDataDTO? ProductLabel { get; set; }

        /// <summary>Header label for the material column.</summary>
        public HtmlDataDTO? MaterialLabel { get; set; }

        /// <summary>Header label for the color column.</summary>
        public HtmlDataDTO? ColorLabel { get; set; }

        /// <summary>Header label for the size column.</summary>
        public HtmlDataDTO? SizeLabel { get; set; }

        /// <summary>Header label for the quantity column.</summary>
        public HtmlDataDTO? QtyLabel { get; set; }

        /// <summary>Header label for the unit price column.</summary>
        public HtmlDataDTO? UnitLabel { get; set; }

        /// <summary>Header label for the total price column.</summary>
        public HtmlDataDTO? TotalLabel { get; set; }
    }
}
