using RazorLight;

namespace HtmlPdf.Service.Renderer
{
    /// <summary>
    /// Renders Razor (.cshtml) templates from the DynamicTemplates volume folder.
    /// Separate from <see cref="TemplateRenderer"/> so static templates are never affected.
    /// </summary>
    public sealed class DynamicTemplateRenderer
    {
        private readonly RazorLightEngine _engine;
        private readonly ILogger<DynamicTemplateRenderer> _logger;

        public static string TemplatesRoot =>
            Path.Combine(AppContext.BaseDirectory, "Templates", "DynamicTemplates");

        public DynamicTemplateRenderer(ILogger<DynamicTemplateRenderer> logger)
        {
            _logger = logger;

            // Ensure the folder exists on first startup (cold container start with empty volume).
            Directory.CreateDirectory(TemplatesRoot);

            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(TemplatesRoot)
                .UseMemoryCachingProvider()
                .Build();
        }

        /// <summary>
        /// Renders the given dynamic template with a JSON model and returns the HTML string.
        /// </summary>
        /// <param name="templateName">File name without extension (e.g. "fattura").</param>
        /// <param name="model">Arbitrary model passed to @Model inside the template.</param>
        public async Task<string> RenderTemplateAsync(string templateName, object model)
        {
            _logger.LogDebug("Rendering dynamic Razor template '{TemplateName}'", templateName);

            var key = templateName + ".cshtml";
            var html = await _engine.CompileRenderAsync(key, model);

            return html;
        }
    }
}
