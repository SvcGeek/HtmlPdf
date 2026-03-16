using System.ComponentModel.DataAnnotations;

namespace HtmlPdf.Service.Infrastructure
{
    /// <summary>
    /// Configuration options for PDF rendering behavior.
    /// These settings can be updated at runtime by modifying appsettings.json.
    /// </summary>
    public class PdfRenderingOptions
    {
        /// <summary>
        /// Configuration section name in appsettings.json
        /// </summary>
        public const string SectionName = "PdfRendering";

        /// <summary>
        /// Maximum number of concurrent PDF generations allowed.
        /// Controls how many browser pages can be open simultaneously.
        /// </summary>
        /// <remarks>
        /// Default: 10
        /// Recommended range: 3-25 depending on server resources
        /// Higher values = more throughput but higher memory usage (~100-200 MB per page)
        /// Lower values = less memory but slower under high load
        /// </remarks>
        [Range(1, 50, ErrorMessage = "MaxConcurrentRenderings must be between 1 and 50")]
        public int MaxConcurrentRenderings { get; set; } = 10;

        /// <summary>
        /// Whitelist of allowed template names for security.
        /// Prevents path traversal attacks by restricting which templates can be rendered.
        /// </summary>
        /// <remarks>
        /// Add template names without the .cshtml extension.
        /// Example: "sample-endpoint-render" for Templates/sample-endpoint-render.cshtml
        /// Case-insensitive matching is used.
        /// 
        /// IMPORTANT: Changes to this list take effect immediately when appsettings.json is saved.
        /// </remarks>
        public List<string> AllowedTemplates { get; set; } = new List<string>
        {
            "sample-endpoint-render"
        };

        /// <summary>
        /// Validates the configuration to ensure all settings are within acceptable ranges.
        /// </summary>
        public void Validate()
        {
            if (MaxConcurrentRenderings < 1 || MaxConcurrentRenderings > 50)
                throw new InvalidOperationException(
                    $"MaxConcurrentRenderings must be between 1 and 50. Current value: {MaxConcurrentRenderings}");

            if (AllowedTemplates == null || AllowedTemplates.Count == 0)
                throw new InvalidOperationException(
                    "AllowedTemplates cannot be empty. At least one template must be configured.");

            // Check for invalid template names (basic validation)
            foreach (var template in AllowedTemplates)
            {
                if (string.IsNullOrWhiteSpace(template))
                    throw new InvalidOperationException("AllowedTemplates cannot contain null or empty values.");

                if (template.Contains("..") || template.Contains("/") || template.Contains("\\"))
                    throw new InvalidOperationException(
                        $"Invalid template name '{template}'. Template names cannot contain path separators or '..'");
            }
        }
    }
}
