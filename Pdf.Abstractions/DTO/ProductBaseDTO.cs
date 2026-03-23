using System;
using System.Collections.Generic;
using System.Text;

namespace Pdf.Abstractions.DTO
{
    public class ProductBaseDTO
    {
        /// <summary>
        /// base64-encoded image data for the brand logo to display on the wishlist document.
        /// </summary>
        public string? Image { get; set; }
        public string? ImageType { get; set; } = "image/png";
        /// <summary>
        /// Assuming the image is in PNG format. Adjust if necessary.
        /// </summary>
        public string? ImageUrl => $"data:{ImageType};base64,{Image}";
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? SkuCode { get; set; }
        public string? Material { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public int Qty { get; set; }
    }
}
