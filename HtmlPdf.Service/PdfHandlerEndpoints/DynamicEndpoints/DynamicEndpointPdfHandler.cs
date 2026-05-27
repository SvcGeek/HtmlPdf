using HtmlPdf.Service.Options;
using HtmlPdf.Service.PdfEndpoints;
using HtmlPdf.Service.Renderer;
using Microsoft.Extensions.Options;
using Pdf.Abstractions.Models;
using System.Text.Json.Nodes;

namespace HtmlPdf.Service.PdfHandlerEndpoints.DynamicEndpoints
{
    /// <summary>
    /// One instance of this handler is created per .cshtml file found in DynamicTemplates/.
    /// The route is automatically /pdf/dynamic/{templateName}.
    /// </summary>
    public class DynamicEndpointPdfHandler : IPdfEndpoint
    {
        private readonly IPdfRenderer _renderer;
        private readonly DynamicTemplateRenderer _dynamicTemplateRenderer;
        private readonly string _templateName;

        /// <summary>Route segment: "dynamic/{templateName}" → full path /pdf/dynamic/{templateName}.</summary>
        public string Pattern => $"dynamic/{_templateName}";

        public void Map(IEndpointRouteBuilder app, IOptionsMonitor<PdfRenderingOptions> optionsMonitor)
        {
            var endpointPath = $"{IPdfEndpoint.BASE_PATH}/dynamic/{_templateName}";
#if DEBUG
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"📄 Dynamic PDF Endpoint registered → POST /{endpointPath}");
            Console.ResetColor();
#endif
            // Skip ValidateTemplate: the template name comes from the route, not the body.
            app.MapPost(endpointPath, (RenderPdfRequestBase request) =>
                ProcessHandle(request, optionsMonitor));
        }

        public DynamicEndpointPdfHandler(
            IPdfRenderer renderer,
            DynamicTemplateRenderer dynamicTemplateRenderer,
            string templateName)
        {
            _renderer = renderer;
            _dynamicTemplateRenderer = dynamicTemplateRenderer;
            _templateName = templateName;
        }

        public async Task<IResult> ProcessHandle(RenderPdfRequestBase request, IOptionsMonitor<PdfRenderingOptions> optionsMonitor)
        {
            // Parse the incoming Data as a JsonNode so it can be used directly in the Razor template via @Model
            JsonNode model = JsonNode.Parse(
                System.Text.Json.JsonSerializer.Serialize(request.Data)) ?? JsonNode.Parse("{}")!;

            var html = await _dynamicTemplateRenderer.RenderTemplateAsync(_templateName, model);
            var pdf = await _renderer.RenderHtmlAsync(html);

            return Results.File(pdf, "application/pdf");
        }
    }
}

