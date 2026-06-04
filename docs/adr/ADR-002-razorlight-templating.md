# ADR-002 — Use RazorLight for Runtime Template Compilation

**Date:** 2025  
**Status:** Accepted

---

## Context

Templates need to be compiled and rendered at runtime — not at build time. The service must support both:
- **Static templates** embedded in the project (`.cshtml` files alongside the code)
- **Dynamic templates** dropped into a volume folder at runtime, unknown at compile time

The renderer must handle:
- Razor syntax (`@Model.Property`, `@foreach`, `@if`, `@Html.Raw(...)`)
- Strongly-typed models (compile-time checked)
- Weakly-typed / dynamic models (`dynamic`, `JsonNode`) for volume-backed templates
- Caching of compiled templates to avoid re-compilation on every request
- No dependency on ASP.NET Core's view engine (the service is a Minimal API, not MVC)

---

## Decision

Use **RazorLight** — a standalone Razor template engine for .NET that works without ASP.NET Core MVC.

---

## Rationale

### 1. Works outside MVC
ASP.NET Core's built-in Razor engine is tightly coupled to the MVC controller/view pipeline. Using it for programmatic template rendering in a Minimal API would require either embedding a full MVC context or workarounds that create significant complexity. RazorLight is designed explicitly for programmatic use.

### 2. File-system project support
RazorLight supports pointing to a directory as the template root (`RazorLightEngineBuilder.UseFileSystemProject(path)`). This maps directly to both the static `Templates/` folder and the dynamic `Templates/DynamicTemplates/` volume folder.

### 3. Compiled template caching
Templates are compiled once on first use and cached in memory. Subsequent calls with the same template key return the cached compiled result in milliseconds (~5-20ms vs 100-500ms for first compile). This is critical for throughput.

### 4. Dynamic model support
Dynamic templates use `@model dynamic`, allowing any key-value structure from the request `data` dictionary to be accessed via `@Model["key"]` without defining a DTO.

### 5. Familiar syntax
Razor syntax is the standard for .NET developers. Any developer can write a template without learning a new DSL.

---

## Architecture

Two separate `RazorLightEngine` instances are maintained:

| Instance | Root Path | Used By |
|---|---|---|
| `TemplateRenderer` | `Templates/` | Static typed endpoints |
| `DynamicTemplateRenderer` | `Templates/DynamicTemplates/` | Dynamic volume-backed endpoints |

Separation avoids cross-contamination of compiled template caches and keeps each renderer independently configurable.

---

## Consequences

### Positive
- No MVC dependency in a Minimal API service
- Templates compile once, serve fast
- Volume-backed templates are first-class citizens
- Razor is familiar to the entire .NET ecosystem

### Negative / Mitigations
- **First-render latency**: Template compilation adds 100-500ms on the first call. Acceptable — subsequent calls are fast. Could be pre-warmed at startup if needed.
- **RazorLight is a third-party library**: Not part of the .NET BCL. The library is actively maintained and widely used, but the team should monitor for updates. Current version: 2.3.x.
- **No partial views / layouts by default**: Advanced Razor features like `@RenderBody()` layouts require additional RazorLight configuration. Currently not implemented; can be added if needed.

---

## Alternatives Considered and Rejected

**Scriban** — fast, lightweight, but uses its own non-Razor DSL. Developers would need to learn a new syntax. Rejected to reduce friction.

**Fluid** — Liquid template syntax, similar concern as Scriban. Not idiomatic for .NET developers.

**ASP.NET Core Razor (built-in)** — requires MVC pipeline wiring in a Minimal API context. Significant complexity for no benefit over RazorLight.

**Handlebars.Net** — limited .NET integration, no type-safe model support. Rejected.
