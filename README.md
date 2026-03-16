# HtmlPdf 🚀

**A high-performance, production-ready PDF generation service powered by Razor templates and headless Chromium.**

Transform your HTML templates into beautiful, pixel-perfect PDFs with the power of a real browser engine - no more CSS limitations, no more layout issues, just perfect rendering.

---

## ✨ What Makes This Special?

🎨 **Real Browser Rendering**  
Uses Chromium's PDF engine for pixel-perfect output with full CSS3, Flexbox, Grid, and modern web standards support.

⚡ **Blazing Fast**  
- Singleton browser instance (no cold starts)
- Compiled template caching
- Concurrent processing (up to 50 simultaneous PDFs)

🔥 **Hot-Reload Configuration**  
Change concurrency limits and template whitelists **without restarting** the application!

🔒 **Production-Ready Security**  
Built-in protection against path traversal attacks with configurable template whitelisting.

🎯 **Minimal API Design**  
Clean, modern ASP.NET Core implementation with automatic endpoint discovery.

📝 **Razor Templates**  
Leverage the full power of C# and Razor syntax - loops, conditionals, layouts, partials, and more.

---

## 🏗️ Architecture

```
HTTP POST → Endpoint Handler → Razor Engine → HTML → Chromium → PDF bytes
```

**Simple. Powerful. Extensible.**

---

## 🚀 Quick Start

### 1. Send a Request
```bash
curl -X POST http://localhost:7227/pdf/sample-endpoint-render \
  -H "Content-Type: application/json" \
  -d '{
    "template": "sample-endpoint-render",
    "language": "en",
    "direction": "ltr",
    "data": {
      "orderId": "ORD-12345",
      "client": { "name": "John Doe" }
    }
  }' \
  --output result.pdf
```

### 2. Get Your PDF
That's it! Your PDF is ready.

---

## 📚 Documentation

We believe in great documentation. Here's everything you need:

### For Getting Started
- **[Complete Usage Guide](HtmlPdf.Service/README.md)** - Architecture, request flow, and step-by-step tutorials
- **[Hot-Reload Guide](HtmlPdf.Service/HOT-RELOAD-GUIDE.md)** - Update configuration without restart ⚡
- **[Configuration Examples](HtmlPdf.Service/CONFIGURATION-EXAMPLES.md)** - Real-world config scenarios

### For Understanding the Code
- **[Technical Notes](HtmlPdf.Service/TECHNICAL-NOTES.md)** - Deep dive into 13 complex patterns
  - Hot-reload with IOptionsMonitor
  - Double-checked locking
  - Dynamic model building via JSON
  - Singleton lifetime decisions
  - Async disposal patterns
  - And much more...

### For Documentation Overview
- **[Documentation Summary](HtmlPdf.Service/DOCUMENTATION-SUMMARY.md)** - What's documented and where

---

## 🎯 Key Features

### 🔥 Hot-Reload Configuration
```json
// Edit appsettings.json while running
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 5,  // ← Change this
    "AllowedTemplates": [
      "invoice",
      "new-template"  // ← Add this
    ]
  }
}
// Save → Changes apply immediately!
```

### 🎨 Beautiful Templates
Write your PDFs in familiar Razor syntax:
```html
@model InvoiceDTO
<!DOCTYPE html>
<html>
<body>
  <h1>Invoice #@Model.OrderId</h1>
  @foreach (var item in Model.Items)
  {
    <div>@item.ProductName - @item.Total.ToString("C")</div>
  }
</body>
</html>
```

### 🔒 Secure by Default
- Template whitelist prevents arbitrary file access
- Configurable security policies
- No exposed file system paths

### ⚡ Performance First
- **Singleton browser**: 2-5 sec cold start → 0ms for subsequent requests
- **Template caching**: 100-500ms first compile → 5-20ms cached
- **Concurrent processing**: Handle 10+ PDFs simultaneously
- **200-600ms** average response time for typical documents

---

## 🛠️ Creating a New PDF Endpoint

It's surprisingly simple:

```csharp
public class InvoicePdfHandler : IEndpoint
{
    public string Pattern => "/pdf/invoice";

    public async Task<IResult> Handle(RenderPdfRequestBase request, ...)
    {
        var model = IEndpoint.BuildModel<InvoiceDTO>(request);
        var pdf = await _renderer.RenderAsync(request.Template, model);
        return Results.File(pdf, "application/pdf");
    }
}
```

Add your template to `appsettings.json`:
```json
{
  "PdfRendering": {
    "AllowedTemplates": ["invoice"]
  }
}
```

**That's it!** No registration needed - automatic discovery handles everything.

---

## 📦 What's Inside

### Projects

- **HtmlPdf.Service** - The main web API service
- **Pdf.Abstractions** - Shared models and DTOs

### Technologies

- **.NET 10** - Latest and greatest
- **RazorLight** - Dynamic template compilation
- **PuppeteerSharp** - Chromium automation
- **Minimal APIs** - Clean, modern ASP.NET Core

---

## 🎓 Learn More

### Complex Patterns Explained

Our [Technical Notes](HtmlPdf.Service/TECHNICAL-NOTES.md) dive deep into:
- IOptionsMonitor for hot-reload
- Double-checked locking for thread safety
- JSON round-trip for dynamic models
- Reflection-based auto-discovery
- Semaphore-based concurrency control
- And 8+ more patterns...

### Real-World Guidance

Check out our [Configuration Examples](HtmlPdf.Service/CONFIGURATION-EXAMPLES.md) for:
- Environment-specific configurations
- Scaling scenarios (2GB to 32GB servers)
- Docker/Kubernetes setups
- Performance tuning guidelines

---

## 🌟 Why You'll Love It

✅ **Zero configuration** to get started  
✅ **Infinite customization** when you need it  
✅ **Production-proven** patterns  
✅ **Extensively documented** - understand every line  
✅ **Modern .NET** - clean, async, performant  
✅ **Hot-reload** - iterate without restarts  

---

## 🤝 Contributing

Found this useful? Star the repo! ⭐

Want to add features? PRs welcome!

---

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

---

**Built with ❤️ for developers who value quality, performance, and great documentation.**
