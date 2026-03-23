using HtmlPdf.Service.Options;
using HtmlPdf.Service.PdfEndpoints;
using HtmlPdf.Service.Renderer;
using Microsoft.Extensions.Options;
using Pdf.Abstractions.DTO;
using Pdf.Abstractions.Models;

namespace HtmlPdf.Service.PdfHandlers
{
    /// <summary>
    /// Sample endpoint handler that demonstrates PDF generation from a Razor template.
    /// Maps POST requests to /pdf/sample-endpoint-render and generates PDFs using the "sample-endpoint-render" template.
    /// </summary>
    /// <remarks>
    /// To create a new PDF endpoint:
    /// 1. Create a new class implementing IEndpoint
    /// 2. Define the Pattern (URL route)
    /// 3. Create a corresponding DTO class with the model structure
    /// 4. Create a .cshtml template in the Templates folder
    /// 5. Add the template name to appsettings.json under PdfRendering:AllowedTemplates
    /// </remarks>
    public class SampleEndpointPdfHandler : IPdfEndpoint
    {
        private readonly IPdfRenderer _renderer;

        /// <summary>
        /// The URL pattern where this endpoint will be accessible.
        /// </summary>
        public string Pattern => "/pdf/sample-endpoint-render";

        public SampleEndpointPdfHandler(IPdfRenderer renderer)
        {
            _renderer = renderer;
        }

        /// <summary>
        /// Handles the PDF generation request with validation and model transformation.
        /// Uses IOptionsMonitor to access the current configuration (supports hot-reload).
        /// </summary>
        /// <param name="request">The incoming PDF render request with template name and data.</param>
        /// <param name="optionsMonitor">Monitor for accessing current configuration with hot-reload support.</param>
        /// <returns>PDF file result or BadRequest if validation fails.</returns>
        public async Task<IResult> ProcessHandle(RenderPdfRequestBase request, IOptionsMonitor<PdfRenderingOptions> optionsMonitor)
        {
            // Validation: Ensure template name is provided
            if (string.IsNullOrWhiteSpace(request.Template))
                return Results.BadRequest("'template' field is required.");

            // Get current allowed templates (supports hot-reload when appsettings.json changes)
            var allowedTemplates = new HashSet<string>(
                optionsMonitor.CurrentValue.AllowedTemplates,
                StringComparer.OrdinalIgnoreCase);

            // Security: Prevent path traversal attacks by checking against whitelist
            // This ensures users cannot request arbitrary file paths like "../../etc/passwd"
            if (allowedTemplates.Count > 0 && !allowedTemplates.Contains(request.Template))
                return Results.BadRequest($"Unknown template '{request.Template}'. Allowed values: {string.Join(", ", allowedTemplates)}.");

            // Transform the generic request into a strongly-typed DTO
            // This converts the flexible Dictionary<string, object> Data property
            // into a SampleEndpointRenderDTO with proper types for template usage
            var modelDict = IPdfEndpoint.BuildModel<SampleEndpointRenderDTO>(request);

            // Render the PDF: Template → HTML → PDF bytes
            var pdf = await _renderer.RenderAsync(request.Template, modelDict);

            // Return the PDF as a downloadable file with appropriate content type
            return Results.File(pdf, "application/pdf", $"{request.Template}.pdf");
        }
    }
}
