# ADR-001 — Use PuppeteerSharp (Chromium) for PDF Rendering

**Date:** 2025  
**Status:** Accepted

---

## Context

Generating PDFs from HTML is a common requirement, but the quality of the output varies dramatically depending on the rendering engine used. The core problem: most PDF libraries either implement their own HTML/CSS parser (incomplete, outdated, diverges from browser standards) or wrap an old browser engine.

The options evaluated:

| Library | Engine | CSS Support | Actively Maintained |
|---|---|---|---|
| **iTextSharp / iText7** | Custom HTML parser | Partial CSS2 | ✅ (commercial) |
| **WkHtmlToPdf** | WebKit (old) | Good but frozen | ❌ (archived 2020) |
| **Syncfusion / Telerik PDF** | Custom | Variable | ✅ (commercial) |
| **DinkToPdf** | libwkhtmltopdf | Good but frozen | ❌ (wrapper, unmaintained) |
| **PuppeteerSharp** | Chromium (latest) | Full CSS3 | ✅ (open source) |

---

## Decision

Use **PuppeteerSharp** — the .NET port of Puppeteer — which drives a headless Chromium instance to render HTML and export it as PDF.

---

## Rationale

### 1. Real browser, real rendering
Chromium is the same engine powering Google Chrome, Microsoft Edge, and most modern browsers. Any HTML/CSS that renders correctly in Chrome will render identically in the generated PDF. No surprises, no layout regressions, no "works in browser but broken in PDF" issues.

### 2. Full modern CSS support
Flexbox, CSS Grid, custom fonts via `@font-face`, CSS variables, `@page` rules for headers/footers, media queries — all supported out of the box. No workarounds needed.

### 3. Active maintenance
PuppeteerSharp tracks the upstream Puppeteer project closely. Chromium receives regular security and feature updates. The library is widely used in production.

### 4. No commercial license required
Unlike iText7 (AGPL / commercial), Syncfusion, or Telerik, PuppeteerSharp is MIT-licensed and free for any use.

### 5. Network idle wait strategy
PuppeteerSharp exposes `Networkidle0` — wait until all network requests are complete before capturing the PDF. This ensures fonts, images, and stylesheets are fully loaded, eliminating blank assets in output.

---

## Consequences

### Positive
- Pixel-perfect PDFs matching browser rendering
- Zero CSS limitations
- Free and open source

### Negative / Mitigations
- **Binary size**: Chromium is ~170MB. Mitigated by persisting it in a Docker volume (`/app/chromium`) — downloaded once, reused across restarts.
- **Memory usage**: Each page (tab) consumes ~50-100MB RAM. Mitigated by a `SemaphoreSlim` concurrency limiter (configurable via `MaxConcurrentRenderings`).
- **Startup time**: First launch downloads and starts Chromium (~2-5s). Mitigated by singleton browser instance and `IHostedService` pre-warming — cost is paid once at startup, not per request.
- **Linux dependencies**: Chromium requires system libraries on minimal Docker images. Documented and handled in the Dockerfile with `apt-get install`.

---

## Alternatives Considered and Rejected

**WkHtmlToPdf / DinkToPdf** — archived, no longer maintained, will not receive security patches. Ruled out for production use.

**iText7** — excellent for programmatic PDF construction, but HTML rendering is limited and requires a commercial license for closed-source use.

**Syncfusion / Telerik** — viable but introduces a paid vendor dependency. The goal was a fully open-source, self-hostable service.
