# HtmlPdf — HTML to PDF as a Microservice

> **A production-ready, self-contained PDF generation service built on real browser rendering.**  
> Drop in a Razor template. Call an API. Get a pixel-perfect PDF. That's it.

[![Docker Hub](https://img.shields.io/badge/Docker%20Hub-svctech%2Fsvc--html--pdf-blue?logo=docker)](https://hub.docker.com/r/svctech/svc-html-pdf)
[![.NET](https://img.shields.io/badge/.NET-10-purple?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green)](LICENSE)

---

## Why This Exists

PDF generation is a solved problem — until you try to do it right.

Most libraries render HTML in a stripped-down engine with partial CSS support, missing fonts, broken layouts, and no Flexbox. The result: PDFs that look nothing like the design.

This project was born from that frustration. Instead of fighting with rendering quirks, **HtmlPdf delegates the entire rendering pipeline to a real Chromium browser** — the same engine your users see every day. Full CSS3, Flexbox, Grid, custom fonts, `@page` rules. If it renders in Chrome, it renders in your PDF.

Beyond that, the service is built to be **plug-and-play for any system**. You don't need to migrate your stack. Call the REST API from any language, any platform, any legacy system. Add custom templates at runtime without touching the code or restarting the container.

This project is part of my professional portfolio — a demonstration of how I approach real engineering problems: with clean architecture, thoughtful design decisions, and production-grade quality.

---

## What You Get

| Feature | Details |
|---|---|
| 🖨️ **Real Browser Rendering** | Full Chromium engine — pixel-perfect PDFs with complete CSS3, Flexbox, Grid support |
| 📂 **Dynamic Templates** | Drop a `.cshtml` file into a volume, restart, get a new endpoint. No code changes |
| ⚡ **Hot-Reload Config** | Change concurrency limits and template whitelists while the service runs |
| 🔒 **Secure by Default** | Template whitelist prevents path traversal — only explicitly allowed templates are served |
| 🐳 **Docker-First** | One `docker compose up` and you're live. Chromium cached in a persistent volume |
| 🌐 **Language Agnostic** | Pure REST API — call it from Python, Java, PHP, Ruby, legacy .NET, anything |
| 🔁 **Concurrent Processing** | Configurable semaphore-based concurrency — handles simultaneous requests safely |

---

## Architecture at a Glance

```
HTTP POST
    │
    ▼
Endpoint Handler          ← auto-discovered via reflection
    │
    ▼
Template Renderer         ← RazorLight compiles .cshtml → HTML (cached)
    │
    ▼
PDF Renderer              ← PuppeteerSharp sends HTML to headless Chromium
    │
    ▼
PDF bytes → Response
```

Two flavors of endpoints:

- **Static** — strongly-typed DTO, defined in code, registered at build time
- **Dynamic** — `.cshtml` file dropped in a volume folder, registered at startup automatically

---

## Quick Start

### Option A — Docker (recommended, zero setup)

```bash
docker pull svctech/svc-html-pdf
```

Or with `docker compose`:

```yaml
# docker-compose.yml
name: htmlpdf-stack

services:
  htmlpdf:
    image: svctech/svc-html-pdf:latest
    ports:
      - "6000:8080"
    volumes:
      - chromium_data:/app/chromium
      - ./my-templates:/app/Templates/DynamicTemplates
    restart: unless-stopped

volumes:
  chromium_data:
```

```bash
docker compose up -d
```

The first start downloads Chromium into the `chromium_data` volume. Every subsequent start is instant — no re-download, no cold start.

---

### Option B — Clone & Run (compatible with any .NET environment)

This option is ideal if you want to **integrate the service directly into an existing solution**, call it locally, or run it on a server without Docker.

```bash
git clone https://github.com/SvcGeek/HtmlPdf.git
cd HtmlPdf/HtmlPdf.Service
dotnet run
```

The service will be available at `http://localhost:5008` (configured in `appsettings.Development.json` and `launchSettings.json`).

> On first run, Chromium is downloaded automatically if not already present at the configured `ChromiumPath`.

You can then call it from any system or language via HTTP — no .NET dependency on the client side.

---

## Try It — Your First PDF in 60 Seconds

The service ships with a working sample endpoint.

> **Port reference**  
> Docker (`docker compose up`): `http://localhost:6000`  
> Clone & run (`dotnet run`): `http://localhost:5008`

```bash
# Docker
curl -X POST http://localhost:6000/pdf/sample-endpoint-render \
  -H "Content-Type: application/json" \
  -d '{
    "template": "sample-endpoint-render",
    "language": "en",
    "direction": "ltr",
    "data": {
      "orderId": "ORD-2025-001",
      "orderDate": "2025-01-15",
      "client": {
        "name": "Jane Smith",
        "email": "jane@example.com"
      },
      "items": []
    }
  }' \
  --output my-first.pdf
```

Open `my-first.pdf`. That's a real browser-rendered PDF — generated in under a second.

---

## Dynamic Templates — No Code Required

The most powerful feature: **add a PDF template to a running service without writing code or rebuilding the image**.

### 1. Write your template

```html
<!-- invoices/invoice.cshtml -->
@model dynamic
<!DOCTYPE html>
<html>
<body>
  <h1>Invoice</h1>
  <p>Client: @Model["client"]</p>
  <p>Amount: @Model["amount"]</p>
</body>
</html>
```

### 2. Drop it in the volume

```bash
cp invoice.cshtml ./my-templates/
```

### 3. Restart the container

```bash
docker compose restart
```

The service logs:
```
📄 Dynamic template discovered → POST /pdf/dynamic/invoice
```

### 4. Call the new endpoint

```http
POST http://localhost:6000/pdf/dynamic/invoice
Content-Type: application/json

{
  "data": {
    "client": "Acme Corp",
    "amount": "€ 4,200.00"
  }
}
```

**No code. No build. No deployment pipeline.**

---

## Configuration

```json
// appsettings.json
{
  "Browser": {
    "ChromiumPath": "/app/chromium"
  },
  "PdfRendering": {
    "MaxConcurrentRenderings": 10,
    "AllowedTemplates": [
      "sample-endpoint-render"
    ],
    "PagePdfOptions": [
      {
        "NameOption": "default",
        "PrintBackground": true,
        "Landscape": false,
        "MarginOptions": {
          "Top": "1cm",
          "Bottom": "1cm",
          "Left": "1cm",
          "Right": "1cm"
        }
      }
    ]
  }
}
```

**Hot-reload supported:** `MaxConcurrentRenderings` and `AllowedTemplates` update instantly when you save the file — no restart needed.

---

## Adding a Static Endpoint (code-defined)

When you need a strongly-typed, validated endpoint with a specific DTO:

**1. Create the DTO** (`Pdf.Abstractions/DTO/InvoiceDTO.cs`)
```csharp
public class InvoiceDTO
{
    public string? Language { get; set; }
    public string? Direction { get; set; }
    public string? ClientName { get; set; }
    public decimal Total { get; set; }
}
```

**2. Create the handler** (`PdfHandlerEndpoints/InvoicePdfHandler.cs`)
```csharp
public class InvoicePdfHandler : IPdfEndpoint
{
    public string Pattern => "invoice";
    private readonly IPdfRenderer _renderer;

    public InvoicePdfHandler(IPdfRenderer renderer) => _renderer = renderer;

    public async Task<IResult> ProcessHandle(RenderPdfRequestBase request,
        IOptionsMonitor<PdfRenderingOptions> options)
    {
        var model = IPdfEndpoint.BuildModel<InvoiceDTO>(request);
        var pdf = await _renderer.RenderAsync(request.Template, model);
        return Results.File(pdf, "application/pdf");
    }
}
```

**3. Create the template** (`Templates/invoice.cshtml`)
```html
@model Pdf.Abstractions.DTO.InvoiceDTO
<h1>Invoice for @Model.ClientName</h1>
<p>Total: @Model.Total.ToString("C")</p>
```

**4. Add to whitelist** in `appsettings.json` — no restart needed.

That's it. Auto-discovery registers the endpoint at startup.

---

## Project Structure

```
HtmlPdf/
├── HtmlPdf.Service/               # Main web API
│   ├── DependencyInjection/       # DI wiring + auto-discovery
│   ├── Helpers/                   # BrowserProvider, PDF option helpers
│   ├── Options/                   # Configuration models
│   ├── PdfEndpoints/              # IPdfEndpoint interface
│   ├── PdfHandlerEndpoints/
│   │   ├── DynamicEndpoints/      # Dynamic template handler
│   │   └── SampleEndpointPdfHandler.cs
│   ├── Renderer/                  # TemplateRenderer, PuppeteerPdfRenderer
│   ├── Templates/
│   │   ├── DynamicTemplates/      # ← mount your volume here
│   │   └── sample-endpoint-render.cshtml
│   └── Program.cs
│
├── Pdf.Abstractions/              # Shared DTOs and request models
│
└── docs/
    └── adr/                       # Architecture Decision Records
        ├── ADR-001-chromium-rendering.md
        ├── ADR-002-razorlight-templating.md
        ├── ADR-003-dynamic-endpoints.md
        └── ADR-004-singleton-lifetime.md
```

---

## Architecture Decisions

Key technical choices are documented as [Architecture Decision Records](docs/adr/):

| ADR | Decision |
|---|---|
| [ADR-001](docs/adr/ADR-001-chromium-rendering.md) | Why Chromium (PuppeteerSharp) over iTextSharp, WkHtmlToPdf, and others |
| [ADR-002](docs/adr/ADR-002-razorlight-templating.md) | Why RazorLight for runtime template compilation |
| [ADR-003](docs/adr/ADR-003-dynamic-endpoints.md) | How and why Dynamic Endpoints work via file system volume |
| [ADR-004](docs/adr/ADR-004-singleton-lifetime.md) | Why all core services are singletons |

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10, ASP.NET Core Minimal APIs |
| PDF Engine | PuppeteerSharp 24 (Chromium) |
| Templating | RazorLight 2.3 |
| Containerization | Docker, Docker Compose |

---

## About

Built by [Silviu Cătălin Valcu](https://www.linkedin.com/in/silviu-catalin-valcu-svctech/) — a .NET developer who got tired of bad PDF libraries and decided to build the right tool.

If you find this useful, a ⭐ on the repo goes a long way.  
Questions, issues, or ideas? Open an issue or reach out on LinkedIn.

---

## License

MIT — see [LICENSE](LICENSE) for details.
