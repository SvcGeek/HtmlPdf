# HtmlPdf.Service

A high-performance, minimal web API for converting HTML to PDF using Razor templates and headless Chromium.

## 🔥 Hot-Reload Configuration Support
This service supports **runtime configuration updates** without restart! See [HOT-RELOAD-GUIDE.md](HOT-RELOAD-GUIDE.md) for details.

## 🏗️ Architecture Overview

```
HTTP Request → Endpoint Handler → Template Renderer → PDF Renderer → PDF Response
                     ↓                    ↓                  ↓
               Validation         RazorLight          PuppeteerSharp
                                 (.cshtml → HTML)    (HTML → PDF)
```

### Key Components

1. **BrowserProvider** (`Infrastructure/BrowserProvider.cs`)
   - Manages a singleton headless Chromium instance
   - Downloads and launches browser on startup (pre-warming)
   - Uses double-checked locking for thread-safe lazy initialization
   - Implements IHostedService to start browser before accepting requests

2. **TemplateRenderer** (`Infrastructure/TemplateRenderer.cs`)
   - Compiles and renders Razor (.cshtml) templates using RazorLight
   - Caches compiled templates in memory for performance
   - Requires `PreserveCompilationContext=true` in .csproj

3. **PuppeteerPdfRenderer** (`Renderer/PuppeteerPdfRenderer.cs`)
   - Orchestrates the full rendering pipeline: Template → HTML → PDF
   - Manages concurrent page creation with SemaphoreSlim (max 10 concurrent)
   - Waits for network idle to ensure all resources are loaded

4. **IEndpoint** (`Infrastructure/IEndpoint.cs`)
   - Interface for PDF handler endpoints
   - Provides `BuildModel<T>()` helper for transforming flexible request data into strongly-typed DTOs

5. **Endpoint Handlers** (`PdfHandlerEndpoints/`)
   - Implement specific PDF generation endpoints
   - Automatically discovered and registered via reflection
   - Each defines its own route pattern and DTO

## 🔄 Request Flow

### 1. Client sends POST request:
```json
POST /pdf/sample-endpoint-render
{
  "template": "sample-endpoint-render",
  "language": "en",
  "direction": "ltr",
  "data": {
    "orderId": "ORD-12345",
    "orderDate": "2024-01-15",
    "client": {
      "name": "John Doe",
      "email": "john@example.com"
    },
    "items": [...]
  }
}
```

### 2. Validation & Security:
- Template name is validated (required, not empty)
- Template is checked against security whitelist
- Prevents path traversal attacks

### 3. Model Transformation:
```
RenderPdfRequestBase → JSON serialization → SampleEndpointRenderDTO
```
- The flexible `Data` dictionary is serialized to JSON
- JSON is deserialized to a strongly-typed DTO
- Top-level fields (Language, Direction) are injected via reflection

### 4. Template Rendering:
- RazorLight locates `Templates/sample-endpoint-render.cshtml`
- Compiles the template to C# code (cached for reuse)
- Executes with the DTO as `@Model`
- Produces HTML string

### 5. PDF Generation:
- Creates a new Chromium page (browser tab)
- Loads HTML content
- Waits for network idle (all resources loaded)
- Generates PDF with A4 format and margins
- Returns PDF bytes to client

## 🔐 Security Features

### Template Whitelist
```csharp
// RenderPdfEndpointExtensions.cs
private static HashSet<string> allowedTemplates = new HashSet<string>
{
    "sample-endpoint-render",
};
```
- Prevents arbitrary file access
- Must explicitly add new templates to whitelist

### Path Traversal Prevention
- Template names are validated against whitelist
- Prevents requests like `../../etc/passwd`

## 🚀 Performance Optimizations

### 1. Singleton Browser
- Browser is launched once at startup
- Reused for all requests (avoids 2-5 second launch delay per request)
- Gracefully disposed on application shutdown

### 2. Template Caching
- Compiled Razor templates are cached in memory
- First request compiles, subsequent requests use cached assembly
- Eliminates compilation overhead on every request

### 3. Concurrency Management
- SemaphoreSlim limits concurrent pages to 10
- Prevents memory exhaustion under high load
- Each page is properly disposed after PDF generation

### 4. Network Idle Strategy
- Waits for `Networkidle0` before PDF generation
- Ensures CSS, images, and fonts are fully loaded
- Prevents incomplete or broken PDF output

## 🛠️ Creating a New PDF Endpoint

### Step 1: Create a DTO
```csharp
// Pdf.Abstractions/DTO/MyCustomDTO.cs
public class MyCustomDTO
{
    public string? Language { get; set; }  // Injected from request
    public string? Direction { get; set; }  // Injected from request
    
    // Your custom properties
    public string? Title { get; set; }
    public List<string> Items { get; set; } = new();
}
```

### Step 2: Create a Template
```html
<!-- HtmlPdf.Service/Templates/my-custom-template.cshtml -->
@model Pdf.Abstractions.DTO.MyCustomDTO
<!DOCTYPE html>
<html lang="@Model.Language" dir="@Model.Direction">
<head>
    <title>@Model.Title</title>
</head>
<body>
    <!-- Your HTML content -->
</body>
</html>
```

### Step 3: Create an Endpoint Handler
```csharp
// HtmlPdf.Service/PdfHandlerEndpoints/MyCustomPdfHandler.cs
public class MyCustomPdfHandler : IEndpoint
{
    private readonly IPdfRenderer _renderer;

    public string Pattern => "/pdf/my-custom-template";

    public MyCustomPdfHandler(IPdfRenderer renderer)
    {
        _renderer = renderer;
    }

    public void Map(IEndpointRouteBuilder app, HashSet<string> allowedTemplates)
    {
        app.MapPost(Pattern, (RenderPdfRequestBase request) => Handle(request, allowedTemplates));
    }

    public async Task<IResult> Handle(RenderPdfRequestBase request, HashSet<string> allowedTemplates)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Template))
            return Results.BadRequest("'template' field is required.");

        if (allowedTemplates.Count > 0 && !allowedTemplates.Contains(request.Template))
            return Results.BadRequest($"Unknown template '{request.Template}'.");

        // Transform request to DTO
        var model = IEndpoint.BuildModel<MyCustomDTO>(request);

        // Generate PDF
        var pdf = await _renderer.RenderAsync(request.Template, model);

        return Results.File(pdf, "application/pdf", $"{request.Template}.pdf");
    }
}
```

### Step 4: Add Template to Whitelist
```json
// HtmlPdf.Service/appsettings.json
{
  "PdfRendering": {
    "AllowedTemplates": [
      "sample-endpoint-render",
      "my-custom-template"  // Add your template here
    ]
  }
}
```

**Note**: Changes to appsettings.json are detected automatically - no restart needed!

### Step 5: Test the Endpoint
```bash
curl -X POST http://localhost:5000/pdf/my-custom-template \
  -H "Content-Type: application/json" \
  -d '{
    "template": "my-custom-template",
    "language": "en",
    "direction": "ltr",
    "data": {
      "title": "My Document",
      "items": ["Item 1", "Item 2"]
    }
  }' \
  --output result.pdf
```

## ⚙️ Configuration

### appsettings.json
```json
{
  "Browser": {
    "DownloadPath": null  // Optional: custom path for Chromium binaries
  },
  "PdfRendering": {
    "MaxConcurrentRenderings": 10,  // Max concurrent PDF generations (hot-reload supported)
    "AllowedTemplates": [            // Security whitelist (hot-reload supported)
      "sample-endpoint-render",
      "invoice",
      "receipt"
    ]
  }
}
```

### Hot-Reload Support ♻️

Both `MaxConcurrentRenderings` and `AllowedTemplates` support **hot-reload**:
- Modify `appsettings.json` while the application is running
- Changes take effect immediately for new requests
- No application restart required!

**Example**: Change `MaxConcurrentRenderings` from 10 to 20:
```json
"PdfRendering": {
  "MaxConcurrentRenderings": 20  // ← Edit and save
}
```
The application will log:
```
Concurrency limit changed from 10 to 20. Recreating semaphore...
Concurrency limiter updated successfully.
```

### Required .csproj Settings
```xml
<PropertyGroup>
  <!-- CRITICAL: Required for RazorLight runtime compilation -->
  <PreserveCompilationContext>true</PreserveCompilationContext>
</PropertyGroup>

<!-- Copy templates to output directory -->
<ItemGroup>
  <Content Update="Templates/**/*.cshtml">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

## 🐛 Common Issues

### Issue: "Can't load metadata reference from the entry assembly"
**Cause**: Missing `PreserveCompilationContext=true` in .csproj  
**Solution**: Add the property to your .csproj file (already configured in this project)

### Issue: PDFs missing images or CSS
**Cause**: PDF generated before resources loaded  
**Solution**: Using `Networkidle0` wait strategy (already configured)

### Issue: Memory issues under high load
**Cause**: Too many concurrent browser pages  
**Solution**: Concurrency limiter (SemaphoreSlim with max 10) already in place

### Issue: Chromium fails to launch in Docker
**Cause**: Missing dependencies or sandbox restrictions  
**Solution**: Use the provided launch args: `--no-sandbox`, `--disable-setuid-sandbox`, `--disable-dev-shm-usage`

## 📦 Dependencies

- **PuppeteerSharp** (24.39.0): Chromium automation for PDF generation
- **RazorLight** (2.3.1): Dynamic Razor template compilation
- **Microsoft.NET.Sdk.Web**: ASP.NET Core minimal API

## 🔍 Dependency Injection Lifetime Notes

### Why Singletons?
All core services (BrowserProvider, TemplateRenderer, PuppeteerPdfRenderer, and endpoint handlers) are registered as **singletons** because:

1. **Performance**: Browser and template compilation are expensive; reuse maximizes throughput
2. **Startup Registration**: Endpoint handlers are resolved at application startup in `MapRenderPdfEndpoints()`, which happens before any HTTP request scope exists
3. **Thread Safety**: All services are designed to be thread-safe for concurrent use

### Working with Scoped Services
If you need to inject per-request services (e.g., database contexts):
```csharp
// DON'T: Inject scoped services directly into singleton handlers
public MyHandler(DbContext db) // ❌ Will fail

// DO: Use IServiceScopeFactory to create scopes manually
public MyHandler(IServiceScopeFactory scopeFactory)
{
    using var scope = scopeFactory.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DbContext>();
}
```

## 📁 Project Structure

```
HtmlPdf.Service/
├── DependencyInjection/
│   └── ServiceCollectionExtensions.cs    # DI registration
├── Extensions/
│   └── RenderPdfEndpointExtensions.cs    # Endpoint mapping + whitelist
├── Helpers/
│   └── PuppeteerSharpPdfOptionHelper.cs  # PDF generation settings
├── Infrastructure/
│   ├── BrowserOptions.cs                 # Configuration model
│   ├── BrowserProvider.cs                # Chromium instance manager
│   ├── IEndpoint.cs                      # Endpoint interface
│   └── TemplateRenderer.cs               # Razor template engine
├── PdfHandlerEndpoints/
│   └── SampleEndpointPdfHandler.cs       # Example endpoint
├── Renderer/
│   ├── IPdfRenderer.cs                   # PDF renderer interface
│   └── PuppeteerPdfRenderer.cs           # PDF renderer implementation
├── Templates/
│   └── sample-endpoint-render.cshtml     # Razor templates
└── Program.cs                            # Application entry point
```

## 🔗 Related Projects

- **Pdf.Abstractions**: Shared models and DTOs for request/response structures
