using RazorLight;

namespace HtmlPdf.Service.Renderer
{
    /// <summary>
    /// Renders Razor (.cshtml) templates to HTML strings using RazorLight.
    /// Compiled templates are cached automatically by RazorLight's memory cache provider.
    /// </summary>
    /// <remarks>
    /// RazorLight performs dynamic runtime compilation:
    /// 1. Parses the .cshtml file
    /// 2. Generates C# code from Razor syntax
    /// 3. Compiles the C# code to an in-memory assembly
    /// 4. Executes the compiled code with the provided model
    /// 5. Caches the compiled assembly for subsequent requests
    /// 
    /// IMPORTANT: Requires PreserveCompilationContext=true in .csproj to provide
    /// metadata references needed for runtime compilation.
    /// </remarks>
    public sealed class TemplateRenderer
    {
        private readonly RazorLightEngine _engine;
        private readonly ILogger<TemplateRenderer> _logger;

        public TemplateRenderer(ILogger<TemplateRenderer> logger)
        {
            _logger = logger;

            // Template files are located in the Templates folder relative to the application's base directory
            // At runtime, templates are copied to bin/Debug/net10.0/Templates/ (see .csproj)
            var templatesRoot = Path.Combine(AppContext.BaseDirectory, "Templates");

            // Configure RazorLight engine:
            // - UseFileSystemProject: load templates from the file system (not embedded resources)
            // - UseMemoryCachingProvider: cache compiled templates in memory to avoid recompiling on every request
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

