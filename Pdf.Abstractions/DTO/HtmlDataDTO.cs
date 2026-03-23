using System;
using System.Collections.Generic;
using System.Text;

namespace Pdf.Abstractions.DTO
{
    /// <summary>
    /// This DTO represents a simple key-value pair structure for HTML data to be used in Razor templates.
    /// </summary>
    public class HtmlDataDTO
    {
        /// <summary>
        /// Value of the label to be displayed in the HTML document, translated in the specified language. This could represent a field name, title, or any descriptive text.
        /// </summary>
        /// <remarks>
        /// <example>
        /// Example EN: <code>Label = "Customer Name"</code>
        /// Example IT: <code>Label = "Nome Cliente"</code>
        /// </example>
        /// </remarks>
        public string? Label { get; set; }

        /// <summary>
        /// Value associated with the label. This could represent the actual data or content to be displayed in the HTML document.
        /// </summary>
        /// <remarks>
        /// <example>
        /// Example: <code>Value = "John Doe"</code>
        /// </example>
        /// </remarks>
        public string? Value { get; set; }
    }
}
