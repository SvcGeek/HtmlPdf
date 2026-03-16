using Microsoft.Extensions.Options;
using Pdf.Abstractions.Models;
using System.Text.Json;

namespace HtmlPdf.Service.Infrastructure
{
    /// <summary>
    /// Base interface for PDF endpoint handlers.
    /// Each implementation defines a route pattern and handles PDF generation requests.
    /// </summary>
    public interface IEndpoint
    {
        /// <summary>
        /// Maps this endpoint's HTTP route to the application.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        /// <param name="optionsMonitor">Configuration monitor for accessing allowed templates and other settings.</param>
        void Map(IEndpointRouteBuilder app, IOptionsMonitor<PdfRenderingOptions> optionsMonitor);

        /// <summary>
        /// The URL pattern for this endpoint (e.g., "/pdf/sample-endpoint-render").
        /// </summary>
        public abstract string Pattern { get; }

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
                });

            if (model == null)
                throw new InvalidOperationException("Model deserialization failed.");

            // Step 3: Inject shared top-level fields using reflection
            // These fields (Language, Direction) are at the request root, not in Data
            var props = typeof(T).GetProperties();

            var langProp = props.FirstOrDefault(p => p.Name == "Language");
            langProp?.SetValue(model, request.Language);

            var dirProp = props.FirstOrDefault(p => p.Name == "Direction");
            dirProp?.SetValue(model, request.Direction);

            return model;
        }
    }
}
