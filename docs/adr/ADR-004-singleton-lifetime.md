# ADR-004 — Singleton Lifetime for Core Services

**Date:** 2025  
**Status:** Accepted

---

## Context

ASP.NET Core's DI container offers three service lifetimes: Transient, Scoped, and Singleton. Choosing the wrong lifetime for a service that manages expensive resources (browser processes, compiled template caches, semaphores) leads to either resource exhaustion (too many instances) or incorrect behavior (shared mutable state without synchronization).

The core services in question:

| Service | Resource managed |
|---|---|
| `BrowserProvider` | Chromium browser process |
| `TemplateRenderer` | RazorLight engine + compiled template cache |
| `DynamicTemplateRenderer` | RazorLight engine + compiled template cache |
| `PuppeteerPdfRenderer` | `SemaphoreSlim` concurrency limiter, config change subscription |

---

## Decision

Register all core PDF pipeline services as **Singletons**.

---

## Rationale

### BrowserProvider — must be Singleton

Launching a Chromium instance takes 2-5 seconds and consumes ~200MB RAM. Creating one per request (Transient) or per HTTP scope (Scoped) would make the service unusable under any load. A single shared browser instance handles all requests via isolated pages (tabs). Each page is opened, used, and closed within a single render call — so concurrent requests don't share mutable page state.

Thread safety is guaranteed by double-checked locking (`_initLock`) during initialization and by PuppeteerSharp's own page isolation model.

### TemplateRenderer / DynamicTemplateRenderer — must be Singleton

RazorLight compiles templates to in-memory assemblies and caches them keyed by template name. If a new engine instance is created per request, every request pays the 100-500ms compilation cost. With a singleton engine, compilation happens once; all subsequent requests use the cached compiled template and complete in 5-20ms.

### PuppeteerPdfRenderer — must be Singleton

`PuppeteerPdfRenderer` owns a `SemaphoreSlim` that enforces the `MaxConcurrentRenderings` limit across all requests. If this service were Transient or Scoped, each instance would have its own semaphore, and the limit would be meaningless — all requests would proceed in parallel without throttling.

The renderer also subscribes to `IOptionsMonitor<PdfRenderingOptions>.OnChange()` to react to hot-reload configuration changes. This subscription must live for the application lifetime; a Transient service would create subscriptions that are never unsubscribed, leaking memory.

---

## Thread Safety Measures

Since singletons are shared across concurrent requests, each service implements explicit thread safety:

**BrowserProvider**
```csharp
private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

// Double-checked locking pattern:
if (_browser is not null) return _browser;
await _initLock.WaitAsync();
try {
	if (_browser is not null) return _browser;
	_browser = await LaunchAsync();
	return _browser;
} finally {
	_initLock.Release();
}
```

**PuppeteerPdfRenderer — semaphore recreation on config change**
```csharp
private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1, 1);

private void OnConfigurationChanged(PdfRenderingOptions newOptions) {
	_updateLock.Wait();
	try {
		var old = _semaphore;
		_semaphore = new SemaphoreSlim(newOptions.MaxConcurrentRenderings, ...);
		old.Dispose();
	} finally {
		_updateLock.Release();
	}
}
```

In-flight requests hold a reference to the old semaphore and complete normally. New requests acquire the new semaphore. No requests are cancelled.

---

## Consequences

### Positive
- Browser starts once — no per-request cold start
- Templates compile once — fast subsequent renders
- Concurrency limit is global and effective
- Config change subscriptions do not leak

### Negative / Mitigations
- **Singleton captured by Scoped/Transient services**: Must not inject singletons into Scoped services that expect per-request state. In this service all consumers are also Singleton or the DI graph is carefully constructed to avoid this. ASP.NET Core will throw a scope validation error at startup if this constraint is violated.
- **State must be thread-safe**: All mutable state (browser reference, semaphore) is protected by locks. This was explicitly implemented in each service.
- **Disposal**: `BrowserProvider` implements `IAsyncDisposable` to gracefully close the Chromium process on application shutdown.

---

## Alternatives Considered and Rejected

**Scoped (per-request) `PuppeteerPdfRenderer`**: Would make the concurrency semaphore per-request, completely defeating its purpose. Rejected.

**Transient `TemplateRenderer`**: Correct for isolation, but unacceptable performance cost (re-compile every template on every request). Rejected.

**Object pooling for Chromium browsers**: Multiple browser instances in a pool. Could improve isolation, but dramatically increases memory usage (200MB × pool size). The single-browser + page-per-request model is sufficient at the current scale. Can be reconsidered if concurrency requirements exceed what a single browser can handle.
