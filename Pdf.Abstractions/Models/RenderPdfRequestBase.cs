using System.Collections.Generic;

namespace Pdf.Abstractions.Models
{
    /// <summary>
    /// Base request model for PDF rendering operations.
    /// Contains the template name, localization settings, and dynamic data payload.
    /// </summary>
    /// <remarks>
    /// This flexible structure allows different endpoints to accept varying data structures
    /// while maintaining a common base for template selection and localization.
    /// </remarks>
    public class RenderPdfRequestBase
    {
        /// <summary>
        /// Name of the Razor template to render (without .cshtml extension).
        /// Must match a file in the Templates folder and be in the allowedTemplates whitelist.
        /// </summary>
        public string? Template { get; set; }

        /// <summary>
        /// BCP-47 language tag (e.g., "en", "it", "ar", "he").
        /// Used for localized content and can be accessed in templates via @Model.Language.
        /// </summary>
        public string? Language { get; set; } = "en";

        /// <summary>
        /// Text direction for the document: "ltr" (left-to-right) or "rtl" (right-to-left).
        /// Critical for proper rendering of Arabic, Hebrew, and other RTL languages.
        /// </summary>
        public string? Direction { get; set; } = "ltr";

        /// <summary>
        /// Arbitrary data passed to the Razor template as the model.
        /// This flexible dictionary is transformed into a strongly-typed DTO
        /// specific to each endpoint handler (see IEndpoint.BuildModel).
        /// </summary>
        public object? Data { get; set; }
    }
}
