using System.Collections.Generic;

namespace HtmlPdf.Service.Models
{
    public class RenderPdfRequest
    {
        /// <summary>The template name (e.g. "delivery", "invoice").</summary>
        public string Template { get; set; } = string.Empty;

        /// <summary>BCP-47 language tag (e.g. "en", "ar", "he").</summary>
        public string Language { get; set; } = "en";

        /// <summary>Text direction: "ltr" or "rtl".</summary>
        public string Direction { get; set; } = "ltr";

        /// <summary>Arbitrary data passed to the Razor template as the model.</summary>
        public Dictionary<string, object?> Data { get; set; } = new Dictionary<string, object?>();
    }
}
