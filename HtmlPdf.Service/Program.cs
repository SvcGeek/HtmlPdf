using HtmlPdf.Service.Infrastructure;
using HtmlPdf.Service.Models;
using HtmlPdf.Service.Renderer;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ── Logging ──────────────────────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ── Services ─────────────────────────────────────────────────────────────────

// Singleton browser – launched once at startup, reused for every request.
builder.Services.AddSingleton<BrowserProvider>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BrowserProvider>());

// Template renderer (caches compiled Razor templates in memory).
builder.Services.AddSingleton<TemplateRenderer>();

// PDF renderer – uses the shared browser and the template renderer.
builder.Services.AddScoped<IPdfRenderer, PuppeteerPdfRenderer>();

var app = builder.Build();

// ── Endpoints ────────────────────────────────────────────────────────────────

/// <summary>
/// POST /pdf/render
/// Body: { "template": "delivery", "language": "en", "direction": "ltr", "data": { … } }
/// Response: application/pdf
/// </summary>
app.MapPost("/pdf/render", async (
    [FromBody] RenderPdfRequest request,
    IPdfRenderer renderer) =>
{
    if (string.IsNullOrWhiteSpace(request.Template))
        return Results.BadRequest("'template' field is required.");

    // Build a dynamic model from the flat Data dictionary plus the top-level
    // language / direction fields so templates can use @Model.Language, etc.
    var modelDict = new System.Dynamic.ExpandoObject() as IDictionary<string, object?>;
    modelDict["Language"] = request.Language;
    modelDict["Direction"] = request.Direction;

    foreach (var kv in request.Data)
        modelDict[kv.Key] = kv.Value;

    var pdf = await renderer.RenderAsync(request.Template, modelDict);

    return Results.File(pdf, "application/pdf", $"{request.Template}.pdf");
});

app.Run();
