using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazorLight;

namespace HtmlPdf.Service.Infrastructure
{
    /// <summary>
    /// Renders Razor (.cshtml) templates to HTML strings using RazorLight.
    /// Compiled templates are cached automatically by RazorLight.
    /// </summary>
    public sealed class TemplateRenderer
    {
        private readonly RazorLightEngine _engine;
        private readonly ILogger<TemplateRenderer> _logger;

        public TemplateRenderer(ILogger<TemplateRenderer> logger)
        {
            _logger = logger;

            var templatesRoot = Path.Combine(AppContext.BaseDirectory, "Templates");

            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(templatesRoot)
                .UseMemoryCachingProvider()
                .Build();
        }

        /// <summary>
        /// Renders the named template with the supplied model and returns the HTML string.
        /// </summary>
        /// <param name="templateName">Relative path inside Templates/, without extension (e.g. "delivery").</param>
        /// <param name="model">View model passed to @Model inside the template.</param>
        public async Task<string> RenderTemplateAsync(string templateName, object model)
        {
            _logger.LogDebug("Rendering Razor template '{TemplateName}'", templateName);

            var key = templateName + ".cshtml";
            var html = await _engine.CompileRenderAsync(key, model);

            return html;
        }
    }
}
