# HtmlPdf.Service

A high-performance, minimal web API for converting HTML to PDF using Razor templates and headless Chromium.

## ⚡ Adding a New PDF Endpoint

1. **DTO** — Create a class in `Pdf.Abstractions/DTO/` extending `BaseDTO<T>`.
2. **Handler** — Add a new class in `PdfHandlerEndpoints/` implementing `IPdfEndpoint` and set its `Pattern` property to the route segment.
3. **Template** — Add a `.cshtml` Razor file in `HtmlPdf.Service/Templates/` whose name matches the `template` field in the request.
4. **Deploy** — Copy the new files to the server and restart the service. Auto-discovery registers the endpoint on startup.

For the full step-by-step guide see [🛠️ Creating a New PDF Endpoint](#️-creating-a-new-pdf-endpoint).

---

## 📂 Dynamic PDF Endpoints

> **In breve:** metti un file `.cshtml` nella cartella `DynamicTemplates`, riavvia il container, e il servizio espone automaticamente un endpoint PDF per quel template. Niente codice, niente build.

### Come funziona

Il servizio, ad ogni avvio, scansiona la cartella:
```
/app/Templates/DynamicTemplates/
```
Per ogni file `.cshtml` trovato crea automaticamente un endpoint:
```
POST /pdf/dynamic/{nome-del-file}
```

La cartella è un **volume Docker persistente**: i file che ci metti sopravvivono ai riavvii del container.

---

### Aggiungere un nuovo template in 3 passi

**1. Crea il file `.cshtml`**

Esempio: `fattura.cshtml`
```html
@model dynamic

<!DOCTYPE html>
<html>
<body>
  <h1>Fattura</h1>
  <p>Cliente: @Model["cliente"]</p>
  <p>Importo: @Model["importo"]</p>
</body>
</html>
```

Usa `@Model["chiave"]` per accedere ai dati che arrivano dal body della richiesta.

**2. Copia il file nella cartella del volume**

```sh
# Esempio con bind mount locale
cp fattura.cshtml ./volumes/dynamic-templates/
```

**3. Riavvia il container**

```sh
docker compose restart
```

Al prossimo avvio vedrai nel log:
```
📄 Dynamic template discovered → POST /pdf/dynamic/fattura
```

---

### Fare una richiesta

```http
POST http://localhost:6000/pdf/dynamic/fattura
Content-Type: application/json

{
  "data": {
    "cliente": "Mario Rossi",
    "importo": "1.200,00 €"
  }
}
```

La risposta è direttamente il file PDF (`application/pdf`).

> **Nota:** il campo `template` nel body **non è necessario** per gli endpoint dinamici — il nome del template è già nell'URL.

---

### Differenze rispetto agli endpoint statici

| | Endpoint statico | Endpoint dinamico |
|---|---|---|
| Dove si crea | Nel codice (`PdfHandlerEndpoints/`) | Nella cartella `DynamicTemplates/` |
| Richiede build | ✅ Sì | ❌ No |
| Modello dati | DTO tipizzato | JSON generico (`dynamic`) |
| Whitelist | `appsettings.json` | File system (solo i `.cshtml` presenti) |
| Attivazione | Al deploy | Al prossimo `docker restart` |

---

## 🔥 Hot-Reload Configuration Support

This service supports

## 🏗️ Architecture Overview

```
HTTP Request → Endpoint Handler → Template Renderer → PDF Renderer → PDF Response
                     ↓                    ↓                  ↓
               Validation         RazorLight          PuppeteerSharp
                                 (.cshtml → HTML)    (HTML → PDF)
```

### Key Components

1. **BrowserProvider** (`Helpers/BrowserProvider.cs`)
   - Manages a singleton headless Chromium instance
   - Downloads and launches browser on startup (pre-warming)
   - Uses double-checked locking for thread-safe lazy initialization
   - Implements IHostedService to start browser before accepting requests

2. **TemplateRenderer** (`Renderer/TemplateRenderer.cs`)
   - Compiles and renders Razor (.cshtml) templates using RazorLight
   - Caches compiled templates in memory for performance
   - Requires `PreserveCompilationContext=true` in .csproj

3. **PuppeteerPdfRenderer** (`Renderer/PuppeteerPdfRenderer.cs`)
   - Orchestrates the full rendering pipeline: Template → HTML → PDF
   - Manages concurrent page creation with SemaphoreSlim (max 10 concurrent)
   - Waits for network idle to ensure all resources are loaded

4. **IPdfEndpoint** (`PdfEndpoints/IPdfEndpoint.cs`)
   - Interface for PDF endpoint handlers
   - Defines `Pattern` for route registration and a default `Map()` implementation

5. **Endpoint Handlers** (`PdfHandlerEndpoints/`)
   - Implement `IPdfEndpoint` for specific PDF generation endpoints
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
public class MyCustomPdfHandler : IPdfEndpoint
{
    public string Pattern => "my-custom-template";

    private readonly IPdfRenderer _renderer;
    private readonly ILogger<MyCustomPdfHandler> _logger;

    public MyCustomPdfHandler(IPdfRenderer renderer, ILogger<MyCustomPdfHandler> logger)
    {
        _renderer = renderer;
        _logger = logger;
    }

    public async Task<IResult> ProcessHandle(RenderPdfRequestBase request, IOptionsMonitor<PdfRenderingOptions> optionsMonitor)
    {
        try
        {
            var model = ObjectMapperHelper.BuildModel<MyCustomDTO>(request);
            var pdf = await _renderer.RenderAsync(request.Template, model);
            return Results.File(pdf, "application/pdf", $"{request.Template}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering PDF for request {Request}", System.Text.Json.JsonSerializer.Serialize(request));
            return Results.Problem($"An error occurred while processing the PDF rendering request: {ex.Message}");
        }
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
│   └── ServiceCollectionExtensions.cs          # DI registration
├── Extensions/
│   └── RenderPdfEndpointExtensions.cs          # Endpoint mapping
├── Helpers/
│   ├── BrowserProvider.cs                      # Chromium instance manager
│   └── PuppeteerSharpPdfOptionHelper.cs        # PDF generation settings
├── Options/
│   ├── BrowserOptions.cs                       # Browser configuration model
│   ├── PagePdfOption.cs                        # PDF page options model
│   └── PdfRenderingOptions.cs                  # PDF rendering configuration model
├── PdfEndpoints/
│   └── IPdfEndpoint.cs                         # Endpoint interface
├── PdfHandlerEndpoints/
│   └── SampleEndpointPdfHandler.cs             # Sample PDF endpoint handler
├── Renderer/
│   ├── IPdfRenderer.cs                         # PDF renderer interface
│   ├── PuppeteerPdfRenderer.cs                 # PDF renderer implementation
│   └── TemplateRenderer.cs                     # Razor template engine
├── Templates/
│   └── sample-endpoint-render.cshtml           # Sample Razor template
└── Program.cs                                  # Application entry point

Pdf.Abstractions/
├── DTO/
│   ├── BaseDTO.cs                              # Generic base DTO (Language, Direction, …)
│   ├── FooterDataDTO.cs
│   ├── HtmlDataDTO.cs
│   ├── OrderDTO.cs
│   ├── ProductBaseDTO.cs                       # Base product row (Name, SkuCode, Image, …)
│   ├── ProductTableHeaderDTO.cs
│   └── SampleEndpointRenderDTO.cs              # Sample endpoint DTO
├── Helper/
│   └── ObjectMapperHelper.cs
└── Models/
    ├── OrderMapper.cs
    └── RenderPdfRequestBase.cs                 # Base HTTP request model
```

## 🔗 Related Projects

- **Pdf.Abstractions**: Shared models and DTOs for request/response structures
