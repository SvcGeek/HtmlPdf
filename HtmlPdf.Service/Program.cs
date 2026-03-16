using HtmlPdf.Service.DependencyInjection;
using HtmlPdf.Service.Extensions;

// ══════════════════════════════════════════════════════════════════════════════
// HtmlPdf.Service - Minimal Web API for HTML-to-PDF conversion
// ══════════════════════════════════════════════════════════════════════════════
// Architecture:
// 1. Receives HTTP POST requests with template name + data
// 2. Renders Razor template to HTML (via RazorLight)
// 3. Converts HTML to PDF using headless Chromium (via PuppeteerSharp)
// 4. Returns PDF bytes to the client
// ══════════════════════════════════════════════════════════════════════════════

var builder = WebApplication.CreateBuilder(args);

// ── Logging Configuration ────────────────────────────────────────────────────
// Clear default providers and use console logging for simplicity
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ── Dependency Injection ─────────────────────────────────────────────────────
// Register all core services: browser provider, template renderer, PDF renderer,
// and endpoint handlers (see ServiceCollectionExtensions for details)
builder.Services.AddCoreDependencies(builder.Configuration);

// ── Application Pipeline ─────────────────────────────────────────────────────
var app = builder.Build();

// Map all PDF rendering endpoints (automatically discovers IEndpoint implementations)
// Each endpoint defines its own route pattern (e.g., /pdf/sample-endpoint-render)
app.MapRenderPdfEndpoints();

// Start the web server and begin accepting requests
app.Run();
