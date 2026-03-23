using HtmlPdf.Service.Options;
using Microsoft.Extensions.Options;
using Pdf.Abstractions.Models;
using System.Text.Json;

namespace HtmlPdf.Service.PdfEndpoints
{
    /// <summary>
    /// Base interface for PDF endpoint handlers.
    /// Each implementation defines a route pattern and handles PDF generation requests.
    /// </summary>
    public interface IPdfEndpoint
    {
        public const string BASE_PATH = "pdf";
        /// <summary>
        /// The URL pattern for this endpoint (e.g., "/pdf/sample-endpoint-render").
        /// </summary>
        public abstract string Pattern { get; }

        /// <summary>
        /// Maps this endpoint's HTTP route to the application. Using this by default will validate the template data or make a custom one overring this method. 
        /// <param name="app">The endpoint route builder.</param>
        /// <param name="optionsMonitor">Configuration monitor for accessing allowed templates and other settings.</param>
        public virtual void Map(IEndpointRouteBuilder app, IOptionsMonitor<PdfRenderingOptions> optionsMonitor)
        {
            var endpointPath = string.Join("/", BASE_PATH, Pattern);
            app.MapPost(endpointPath, (RenderPdfRequestBase request) =>
            {
                ValidateTemplate(request, optionsMonitor);
                return ProcessHandle(request, optionsMonitor);
            });
        }

        /// <summary>
        /// Handles the PDF generation request with validation and model transformation.
        /// Uses IOptionsMonitor to access the current configuration (supports hot-reload).
        /// </summary>
        /// <param name="request">The incoming PDF render request with template name and data.</param>
        /// <param name="optionsMonitor">Monitor for accessing current configuration with hot-reload support.</param>
        /// <returns>PDF file result or BadRequest if validation fails.</returns>
        public abstract Task<IResult> ProcessHandle(RenderPdfRequestBase request, IOptionsMonitor<PdfRenderingOptions> optionsMonitor);

        /// <summary>
        /// Builds a strongly-typed model from the base request by deserializing the Data dictionary.
        /// This is a two-step transformation:
        /// 1. Serialize the Data dictionary to JSON
        /// 2. Deserialize to the target DTO type T
        /// 3. Inject shared top-level fields (Language, Direction) into the model
        /// </summary>
        /// <remarks>
        /// This approach allows flexible, dynamic data in the request while maintaining
        /// type safety in the template. The JSON round-trip handles property mapping and
        /// type conversions automatically.
        /// </remarks>
        /// <typeparam name="T">The target DTO type with properties matching the Data structure.</typeparam>
        /// <param name="request">The incoming PDF render request.</param>
        /// <returns>Strongly-typed model instance populated from request data.</returns>
        public static T BuildModel<T>(RenderPdfRequestBase request)
        {
            // Step 1: Serialize the Data dictionary to JSON string
            var json = System.Text.Json.JsonSerializer.Serialize(request.Data);

            // Step 2: Deserialize JSON to the strongly-typed DTO
            // PropertyNameCaseInsensitive allows flexible property name matching
            var model = System.Text.Json.JsonSerializer.Deserialize<T>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new InvalidOperationException("Model deserialization failed.");

            // Step 3: Inject shared top-level fields using reflection
            // These fields (Language, Direction) are at the request root, not in Data
            var props = typeof(T).GetProperties();

            var langProp = props.FirstOrDefault(p => p.Name == "Language");
            langProp?.SetValue(model, request.Language);

            var dirProp = props.FirstOrDefault(p => p.Name == "Direction");
            dirProp?.SetValue(model, request.Direction);

            return model;
        }

        /// <summary>
        /// Validates that the specified template in the request is present and allowed according to the current PDF
        /// rendering options.
        /// </summary>
        /// <remarks>This method enforces template validation to prevent unauthorized access to templates
        /// and to mitigate path traversal attacks. The list of allowed templates is dynamically retrieved from the
        /// current options, supporting configuration changes at runtime.</remarks>
        /// <param name="request">The request containing the template name to validate. The 'Template' property must specify the name of the
        /// template to be rendered.</param>
        /// <param name="optionsMonitor">The options monitor providing access to the current PDF rendering options, including the list of allowed
        /// templates.</param>
        /// <exception cref="ArgumentException">Thrown if the 'Template' property of <paramref name="request"/> is null, empty, or consists only of
        /// white-space characters.</exception>
        /// <exception cref="Exception">Thrown if the specified template is not included in the list of allowed templates.</exception>
        private static void ValidateTemplate(RenderPdfRequestBase request, IOptionsMonitor<PdfRenderingOptions> optionsMonitor)
        {
            // Validation: Ensure template name is provided
            if (string.IsNullOrWhiteSpace(request.Template))
                throw new ArgumentException("'template' field is required.");

            // Get current allowed templates (supports hot-reload when appsettings.json changes)
            var allowedTemplates = new HashSet<string>(
                optionsMonitor.CurrentValue.AllowedTemplates,
                StringComparer.OrdinalIgnoreCase);

            // Security: Prevent path traversal attacks by checking against whitelist
            // This ensures users cannot request arbitrary file paths like "../../etc/passwd"
            if (allowedTemplates.Count > 0 && !allowedTemplates.Contains(request.Template))
                throw new Exception($"Unknown template '{request.Template}'. Allowed values: {string.Join(", ", allowedTemplates)}.");
        }
    }
}