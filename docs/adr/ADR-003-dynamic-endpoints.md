# ADR-003 — Dynamic Endpoint Registration via File System Volume

**Date:** 2025  
**Status:** Accepted

---

## Context

The service needed a way for users to add new PDF templates **without modifying code, rebuilding the image, or redeploying**. The workflow should be:

1. Drop a `.cshtml` file into a folder
2. Restart the container
3. A new `POST /pdf/dynamic/{name}` endpoint appears automatically

The solution had to integrate cleanly with the existing static endpoint architecture (`IPdfEndpoint` + reflection-based auto-discovery) without breaking it.

---

## Decision

Use a **file-system volume scan at startup** to discover `.cshtml` files in `Templates/DynamicTemplates/` and programmatically register one `DynamicEndpointPdfHandler` instance per file into the DI container and route table.

---

## Design

### Static endpoints (existing pattern)
```
IPdfEndpoint implementation (class in code)
	→ reflection scan at startup
	→ registered in DI as scoped/transient
	→ mapped to route via Map(IEndpointRouteBuilder)
```

### Dynamic endpoints (new pattern)
```
.cshtml file in Templates/DynamicTemplates/
	→ file scan at startup in RegisterDynamicPdfHandlers()
	→ one DynamicEndpointPdfHandler instantiated per file
	→ registered in DI under a keyed or indexed service entry
	→ mapped to route POST /pdf/dynamic/{templateName}
```

### Key design choices

**`DynamicEndpointPdfHandler` is route-specific** — each instance is constructed with a `templateName` string that corresponds to the `.cshtml` filename (without extension). The route pattern `dynamic/{templateName}` is baked into the instance.

**Separate DI registration path** — `DynamicEndpointPdfHandler` is explicitly excluded from the reflection-based static scan (which would fail because `string templateName` cannot be resolved from the DI container). Dynamic handlers are registered manually in `RegisterDynamicPdfHandlers()`.

**Overridden `Map()` method** — static `IPdfEndpoint.Map()` validates that the request body contains a matching `template` field. Dynamic endpoints receive the template name from the URL route, not the body, so `DynamicEndpointPdfHandler` overrides `Map()` to skip body validation and bind from the route instead.

**Separate `DynamicTemplateRenderer`** — a dedicated `RazorLightEngine` instance points to `Templates/DynamicTemplates/`. This keeps static and dynamic template caches completely separate and allows independent configuration.

**Dynamic model** — templates use `@model dynamic` and receive data via `@Model["key"]`. No DTO is required. The request body's `data` dictionary is passed as-is to the renderer.

---

## Request Flow (Dynamic Template)

```
POST /pdf/dynamic/invoice
	│
	▼
DynamicEndpointPdfHandler.ProcessHandle()
	│  templateName = "invoice" (from route)
	▼
DynamicTemplateRenderer.RenderTemplateAsync("invoice", model)
	│  model = request.Data (Dictionary<string,object>)
	▼
RazorLight renders Templates/DynamicTemplates/invoice.cshtml
	│
	▼
IPdfRenderer.RenderHtmlAsync(html)
	│
	▼
PuppeteerSharp → Chromium → PDF bytes
	│
	▼
HTTP 200 application/pdf
```

---

## Consequences

### Positive
- Zero code changes to add a new template — only a file drop and restart
- Fully compatible with existing static endpoint pattern
- Template whitelist (`AllowedTemplates`) still applies, preventing unauthorized templates from being served
- Clear separation of concerns between static and dynamic rendering paths

### Negative / Mitigations
- **Restart required**: New templates are not hot-loaded while the service runs. This is a deliberate simplicity tradeoff — file system watching introduces complexity and edge cases (partial writes, invalid Razor syntax mid-compile). A restart is safe, fast, and predictable.
- **No compile-time validation**: Dynamic templates are compiled at first request, not at startup. A broken `.cshtml` will return a 500 on first call. Mitigated by testing templates locally before deploying.
- **`@model dynamic`**: No IntelliSense or compile-time type safety. This is inherent to the "drop a file" workflow. Developers who need type safety should use static endpoints instead.

---

## Alternatives Considered and Rejected

**Runtime file system watching (`FileSystemWatcher`)**: Would allow hot-load without restart. Rejected due to complexity — partial writes, in-flight requests referencing the old compiled template, and the need to handle compiler errors gracefully at runtime. The restart model is simpler and safer.

**Single catch-all route with dynamic template name from body**: Would allow any template name to be called without registration. Rejected — it bypasses the whitelist security model and makes the API surface implicit and undiscoverable.

**Code generation at startup**: Generating and compiling C# handler classes on the fly. Rejected — vastly over-engineered for this use case.
