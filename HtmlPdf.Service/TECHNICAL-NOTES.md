# Technical Notes - Advanced Topics

This document explains the more complex and potentially confusing aspects of the HtmlPdf.Service implementation.

## 🧩 Complex Pattern #1: Hot-Reload Configuration with IOptionsMonitor

### Location: `PuppeteerPdfRenderer` and `SampleEndpointPdfHandler`

### The Challenge
We want to change configuration (concurrency limits, template whitelist) **without restarting the application**.

### The Solution: IOptionsMonitor

```csharp
public class PuppeteerPdfRenderer : IPdfRenderer
{
    private SemaphoreSlim _concurrencyLimiter;
    private readonly IOptionsMonitor<PdfRenderingOptions> _options;

    public PuppeteerPdfRenderer(IOptionsMonitor<PdfRenderingOptions> options)
    {
        _options = options;
        _concurrencyLimiter = new SemaphoreSlim(
            _options.CurrentValue.MaxConcurrentRenderings,
            _options.CurrentValue.MaxConcurrentRenderings);

        // Register callback for config changes
        _options.OnChange(OnConfigurationChanged);
    }

    private void OnConfigurationChanged(PdfRenderingOptions newOptions)
    {
        // Recreate semaphore with new limit
        var oldSemaphore = _concurrencyLimiter;
        _concurrencyLimiter = new SemaphoreSlim(newOptions.MaxConcurrentRenderings, 
                                                  newOptions.MaxConcurrentRenderings);
        oldSemaphore.Dispose();
    }
}
```

### IOptions vs. IOptionsSnapshot vs. IOptionsMonitor

| Type | Lifetime | Reload | Use Case |
|------|----------|--------|----------|
| `IOptions<T>` | Singleton | ❌ Never | Read once at startup |
| `IOptionsSnapshot<T>` | Scoped | ✅ Per request | Different per HTTP request |
| `IOptionsMonitor<T>` | Singleton | ✅ Immediate | Real-time config changes |

### How Hot-Reload Works

1. ASP.NET Core watches `appsettings.json` for changes
2. When file is saved, configuration is reloaded
3. `IOptionsMonitor.OnChange()` callbacks are invoked
4. Our code updates runtime behavior

### Real-World Example

**Edit appsettings.json**:
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 20,  // Changed from 10 to 20
    "AllowedTemplates": [
      "sample-endpoint-render",
      "new-template"  // Added new template
    ]
  }
}
```

**Save the file** → Application logs:
```
Concurrency limit changed from 10 to 20. Recreating semaphore...
Concurrency limiter updated successfully.
```

**Next request** → Uses new settings immediately!

### Thread Safety Considerations

```csharp
private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1, 1);

private void OnConfigurationChanged(PdfRenderingOptions newOptions)
{
    _updateLock.Wait();  // Prevent concurrent updates
    try
    {
        // Update semaphore
    }
    finally
    {
        _updateLock.Release();
    }
}
```

**Why needed**: Multiple file saves could trigger overlapping callbacks. The lock ensures only one update happens at a time.

### Allowed Templates Hot-Reload

```csharp
public async Task<IResult> Handle(RenderPdfRequestBase request, 
                                   IOptionsMonitor<PdfRenderingOptions> optionsMonitor)
{
    // Get current whitelist - automatically updates when config changes
    var allowedTemplates = new HashSet<string>(
        optionsMonitor.CurrentValue.AllowedTemplates,
        StringComparer.OrdinalIgnoreCase);

    // Validation uses current whitelist
    if (!allowedTemplates.Contains(request.Template))
        return Results.BadRequest("Template not allowed");
}
```

**Key Point**: We access `CurrentValue` on each request, so template whitelist changes take effect immediately.

### Limitations and Gotchas

1. **In-flight requests**: Continue with old configuration (can't change mid-execution)
2. **Semaphore recreation**: Existing waiters on old semaphore complete normally
3. **No rollback**: Invalid configuration could break the app (validate before deploying)

### Best Practices

1. **Validate configuration**: Add validation in `PdfRenderingOptions`
2. **Log changes**: Always log when configuration changes (for debugging)
3. **Test hot-reload**: Ensure application behaves correctly with dynamic changes
4. **Document limits**: Make it clear what values are safe for `MaxConcurrentRenderings`

---

## 🧩 Complex Pattern #2: Dynamic Model Building via JSON Serialization

### Location: `IEndpoint.BuildModel<T>()`

### The Challenge
We need to accept flexible, dynamic data in HTTP requests while maintaining type safety in Razor templates.

### The Solution
```csharp
public static T BuildModel<T>(RenderPdfRequestBase request)
{
    // 1. Serialize the dynamic Data dictionary to JSON
    var json = JsonSerializer.Serialize(request.Data);
    
    // 2. Deserialize JSON into strongly-typed DTO
    var model = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });
    
    // 3. Use reflection to inject top-level fields
    var props = typeof(T).GetProperties();
    var langProp = props.FirstOrDefault(p => p.Name == "Language");
    langProp?.SetValue(model, request.Language);
    
    return model;
}
```

### Why This Approach?

**Problem**: The request has a `Dictionary<string, object> Data` property, but templates need strongly-typed models.

**Alternative Approaches**:
1. ❌ Manual mapping: Tedious, error-prone, requires updates for every new property
2. ❌ AutoMapper: Additional dependency, configuration overhead
3. ✅ JSON round-trip: Leverages existing serialization logic, handles nested objects automatically

**Trade-offs**:
- **Pros**: Flexible, automatic type conversion, handles complex nested structures
- **Cons**: Small performance overhead from serialization/deserialization, uses reflection
- **Verdict**: Performance cost is negligible compared to PDF generation (which takes 100-500ms)

### Why Reflection for Language/Direction?

These fields exist at the **request root level** (not in `Data`), but templates need them in the model:

```
RenderPdfRequestBase {
    Template: "..."
    Language: "en"      ← Root level
    Direction: "ltr"    ← Root level
    Data: { ... }
}
```

We inject them into the DTO after deserialization so templates can use `@Model.Language` naturally.

---

## 🧩 Complex Pattern #3: Double-Checked Locking for Browser Initialization

### Location: `BrowserProvider.GetBrowserAsync()`

### The Pattern
```csharp
public async Task<IBrowser> GetBrowserAsync()
{
    // First check (unlocked) - fast path
    if (_browser is not null)
        return _browser;

    await _lock.WaitAsync();
    try
    {
        // Second check (locked) - handles race condition
        if (_browser is not null)
            return _browser;

        _browser = await LaunchAsync();
    }
    finally
    {
        _lock.Release();
    }

    return _browser;
}
```

### Why Double-Checked Locking?

**The Race Condition**:
Imagine two threads call `GetBrowserAsync()` simultaneously on startup:

Without double-check:
```
Thread A: Check _browser → null
Thread B: Check _browser → null
Thread A: Acquire lock
Thread A: Launch browser
Thread A: Release lock
Thread B: Acquire lock
Thread B: Launch browser AGAIN! ← Problem!
```

With double-check:
```
Thread A: First check → null
Thread B: First check → null
Thread A: Acquire lock
Thread A: Second check → null → Launch browser
Thread A: Release lock
Thread B: Acquire lock
Thread B: Second check → not null! → Return existing
Thread B: Release lock
```

### Why Two Checks?

1. **First check (no lock)**: 99.99% of calls hit this fast path after initialization
2. **Second check (inside lock)**: Prevents the race condition during initialization

**Performance Impact**:
- After initialization: Zero overhead (fast path)
- During initialization: Prevents launching multiple browsers

---

## 🧩 Complex Pattern #4: Singleton Services Resolved at Startup

### Location: `ServiceCollectionExtensions.RegisterPdfHandlers()`

### The Challenge

```csharp
// In Program.cs
app.MapRenderPdfEndpoints();  // Called at startup, before requests

// In RenderPdfEndpointExtensions.cs
var endpoints = app.Services.GetServices<IEndpoint>();  // Resolves from root container
```

This code runs **at application startup**, not during an HTTP request.

### The Problem with Scoped Lifetimes

If handlers were registered as **scoped**:
```csharp
services.AddScoped<IEndpoint, SampleEndpointPdfHandler>();  // ❌ Would fail
```

**Scoped services** require an active scope (typically one per HTTP request). At startup, no scope exists yet!

```
Application Startup
    ↓
  No HTTP Request Context
    ↓
  No Scope Available
    ↓
  ❌ Cannot resolve scoped services
```

### The Solution: Singletons

```csharp
services.AddSingleton(endpointType, handler);  // ✅ Works
```

Singletons can be resolved from the root container without a scope.

### Implications

**If you need per-request services in a handler**:

❌ **DON'T** inject scoped services directly:
```csharp
public MyHandler(DbContext db)  // ❌ Fails: DbContext is scoped
```

✅ **DO** use `IServiceScopeFactory`:
```csharp
public class MyHandler : IEndpoint
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MyHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<IResult> Handle(RenderPdfRequestBase request)
    {
        // Create a scope manually for this request
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DbContext>();
        
        // Use db here...
    }
}
```

---

## 🧩 Complex Pattern #5: PreserveCompilationContext Requirement

### The Issue

RazorLight needs to compile .cshtml files to C# code at runtime. Compilation requires **metadata references** (type information from your assemblies).

### What Happens at Compile Time

```
Your Code → C# Compiler → Assembly + Metadata References
                                         ↓
                                   (embedded in .deps.json)
```

### The Problem

By default, .NET omits metadata from published apps to reduce size:

```
.NET Publish (default) → Assembly WITHOUT metadata
                              ↓
                    RazorLight can't compile templates
                              ↓
                         ❌ Exception
```

### The Solution

```xml
<PreserveCompilationContext>true</PreserveCompilationContext>
```

This tells the compiler:
1. Keep metadata references in the output
2. Include them in the `.deps.json` file
3. Allow runtime access via `DependencyContext`

### Trade-offs

- **Cost**: Slightly larger output (~few MB for assemblies metadata)
- **Benefit**: Enables dynamic Razor compilation
- **Alternatives**: Pre-compile templates (eliminates RazorLight, loses flexibility)

---

## 🧩 Complex Pattern #6: Concurrency Management with SemaphoreSlim

### Location: `PuppeteerPdfRenderer.RenderAsync()`

### The Resource Problem

Each PDF generation creates a Chromium page (browser tab):
```
Request 1 → Page 1 (50-200 MB memory)
Request 2 → Page 2 (50-200 MB memory)
Request 3 → Page 3 (50-200 MB memory)
...
Request 100 → Page 100 (5-20 GB memory) ← Server crashes!
```

### The Solution: Semaphore

```csharp
private static readonly SemaphoreSlim _concurrencyLimiter = new SemaphoreSlim(10, 10);

public async Task<byte[]> RenderAsync(...)
{
    await _concurrencyLimiter.WaitAsync();  // Acquire slot (or wait)
    try
    {
        await using var page = await browser.NewPageAsync();
        // ... render PDF ...
    }
    finally
    {
        _concurrencyLimiter.Release();  // Release slot
    }
}
```

### How It Works

1. **SemaphoreSlim(10, 10)**: 10 available slots initially
2. **WaitAsync()**: Takes a slot (blocks if all 10 are taken)
3. **Release()**: Returns the slot (unblocks a waiting thread)

### Real-World Behavior

```
Requests 1-10:  Start immediately (slots available)
Request 11:     Waits until one of 1-10 completes
Request 12:     Waits in queue
...
```

### Why Static?

```csharp
private static readonly SemaphoreSlim _concurrencyLimiter = ...
```

The limiter is **static** because:
- `PuppeteerPdfRenderer` is a singleton (one instance for the entire app)
- Static ensures the same limiter is used even if the class were instantiated multiple times
- Thread-safe across all requests to the application

### Tuning the Limit

Current limit: **10 concurrent pages**

**Increase if**:
- Server has lots of RAM (16+ GB)
- You need higher throughput
- PDF generation is fast (simple templates)

**Decrease if**:
- Running in containers with limited memory
- Experiencing out-of-memory errors
- PDFs are complex (many images, heavy CSS)

**Rule of thumb**: ~100-200 MB per concurrent page

---

## 🧩 Complex Pattern #7: IHostedService for Pre-Warming

### Location: `BrowserProvider` and `ServiceCollectionExtensions`

### The Problem

Launching Chromium is slow (2-5 seconds):
```
First Request → Launch Browser (5 sec) → Render PDF (500 ms) → Total: 5.5 sec ❌
Second Request → Render PDF (500 ms) → Total: 500 ms ✓
```

### The Solution: Pre-Warm at Startup

```csharp
// Register as both singleton AND hosted service
services.AddSingleton<BrowserProvider>();
services.AddHostedService(sp => sp.GetRequiredService<BrowserProvider>());
```

### How IHostedService Works

```csharp
public async Task StartAsync(CancellationToken cancellationToken)
{
    await GetBrowserAsync();  // Launch browser during startup
}
```

**Execution Order**:
```
1. Application starts
2. IHostedService.StartAsync() called
3. Browser launches (5 sec)
4. Application begins accepting HTTP requests
5. First request → Browser already running → Fast! ✓
```

### The Trick: Shared Instance

```csharp
services.AddHostedService(sp => sp.GetRequiredService<BrowserProvider>());
```

- `GetRequiredService<BrowserProvider>()`: Returns the **same singleton instance**
- Not creating a new instance for IHostedService
- Both `BrowserProvider` and `IHostedService` interfaces point to the same object

**Why this works**:
- Singleton ensures only one BrowserProvider exists
- The same instance handles both browser management and startup logic
- No duplicate browser launches

---

## 🧩 Complex Pattern #8: Reflection-Based Endpoint Discovery

### Location: `ServiceCollectionExtensions.RegisterPdfHandlers()`

### The Pattern

```csharp
var endpointType = typeof(IEndpoint);
var handlers = AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => a.GetTypes())
    .Where(t => endpointType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

foreach (var handler in handlers)
{
    services.AddSingleton(endpointType, handler);
}
```

### What This Does

1. **Scan all loaded assemblies** in the current AppDomain
2. **Find all types** that implement `IEndpoint`
3. **Filter out** interfaces and abstract classes
4. **Register each** as a singleton under the `IEndpoint` interface

### The Benefit: Auto-Discovery

When you create a new endpoint handler:
```csharp
public class InvoicePdfHandler : IEndpoint { ... }
```

It's **automatically registered** - no manual configuration needed!

### The Trade-off

**Pros**:
- Convention over configuration
- Impossible to forget registration
- Less boilerplate code

**Cons**:
- Reflection has a small performance cost (only at startup)
- All IEndpoint implementations are registered (no selective registration)
- Slightly less explicit than manual registration

### Performance Note

The reflection scan happens **once at startup**, not per request. The overhead is ~10-50ms, which is negligible compared to Chromium launch time.

---

## 🧩 Complex Pattern #9: Networkidle0 Wait Strategy

### Location: `PuppeteerPdfRenderer.RenderAsync()`

### The Code

```csharp
await page.SetContentAsync(html, new NavigationOptions
{
    WaitUntil = [WaitUntilNavigation.Networkidle0]
});
```

### What Are the Options?

| Strategy | Waits For | Use Case |
|----------|-----------|----------|
| `Load` | DOM loaded | Simple HTML, no external resources |
| `DOMContentLoaded` | DOM parsed | Basic interactivity, minimal resources |
| `Networkidle0` | No connections for 500ms | All resources loaded (CSS, images, fonts) |
| `Networkidle2` | ≤2 connections for 500ms | Acceptable with lazy-loading |

### Why Networkidle0?

PDF generation requires **all visual resources** to be loaded:

**Without proper waiting**:
```
Load HTML → Generate PDF immediately → Missing images/fonts ❌
```

**With Networkidle0**:
```
Load HTML → Wait for all images → Wait for all CSS → Wait for fonts → Generate PDF ✓
```

### How It Works

1. Browser loads the HTML
2. Browser starts fetching resources (images, CSS, fonts)
3. Monitor network activity
4. When activity stops for 500ms → consider "idle"
5. Continue to PDF generation

### Real-World Example

Template with external image:
```html
<img src="https://example.com/logo.png">
```

Timeline:
```
0ms:    SetContentAsync called
10ms:   HTML parsed
20ms:   Image fetch started
150ms:  Image downloaded
150ms:  Network idle timer starts
650ms:  500ms elapsed with no activity → IDLE
650ms:  PDF generation starts ✓
```

### The Edge Case: Infinite Waiting

If a resource **never loads** (404, timeout), Networkidle0 could wait forever.

**Mitigation** (not currently implemented, but could be added):
```csharp
await page.SetContentAsync(html, new NavigationOptions
{
    WaitUntil = [WaitUntilNavigation.Networkidle0],
    Timeout = 30000  // 30 second timeout
});
```

---

## 🧩 Complex Pattern #10: Template File Copying

### Location: `HtmlPdf.Service.csproj`

### The Configuration

```xml
<ItemGroup>
  <Content Update="Templates/**/*.cshtml">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

### What This Does

At build time:
```
HtmlPdf.Service/Templates/sample-endpoint-render.cshtml
    ↓ (copied to)
bin/Debug/net10.0/Templates/sample-endpoint-render.cshtml
```

### Why Is This Necessary?

RazorLight uses the **file system** to load templates:

```csharp
var templatesRoot = Path.Combine(AppContext.BaseDirectory, "Templates");
_engine = new RazorLightEngineBuilder()
    .UseFileSystemProject(templatesRoot)
    .Build();
```

- `AppContext.BaseDirectory`: `bin/Debug/net10.0/` (or publish folder)
- Templates must exist at runtime in the **output directory**

### PreserveNewest vs. Always

```xml
<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
```

- **PreserveNewest**: Copy only if source is newer (or file doesn't exist in output)
- **Always**: Copy every time (slower builds)

**PreserveNewest** is optimal: templates update when you edit them, but don't cause unnecessary copies on every build.

### The Glob Pattern

```xml
Templates/**/*.cshtml
```

- `**`: Matches any subdirectory (recursive)
- `*.cshtml`: Matches all Razor files

Supports nested structures:
```
Templates/
  invoices/
    invoice.cshtml      ✓ Copied
  receipts/
    receipt.cshtml      ✓ Copied
  sample-endpoint-render.cshtml  ✓ Copied
```

---

## 🧩 Complex Pattern #11: Chromium Launch Arguments for Containers

### Location: `BrowserProvider.LaunchAsync()`

### The Arguments

```csharp
Args = ["--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage"]
```

### Why Each Argument Is Needed

#### 1. `--no-sandbox`
**Problem**: Chromium's sandbox requires kernel features not available in containers.

Container error:
```
Failed to move to new namespace: PID namespaces supported, 
Network namespace supported, but failed: errno = Operation not permitted
```

**Solution**: Disable sandboxing. ⚠️ **Security note**: Only use in trusted environments.

#### 2. `--disable-setuid-sandbox`
**Problem**: Setuid sandbox requires special file permissions.

```bash
# Requires this:
-rwsr-xr-x chrome-sandbox  # Notice the 's' (setuid bit)
```

Containers often don't support setuid binaries.

**Solution**: Disable this sandbox method.

#### 3. `--disable-dev-shm-usage`
**Problem**: Chromium uses `/dev/shm` (shared memory) for performance.

Docker default `/dev/shm` size: **64 MB**  
Chromium typical usage: **100-500 MB**

Result:
```
Shared memory allocation failed: Out of memory
```

**Solution**: Force Chromium to use `/tmp` instead (disk-backed, unlimited).

### Docker Best Practices

If you want to keep sandboxing:

```yaml
# docker-compose.yml
services:
  htmlpdf:
    shm_size: '2gb'  # Increase shared memory
    security_opt:
      - seccomp:unconfined  # Allow sandbox syscalls
```

Then you can remove `--no-sandbox` from the launch args.

---

## 🧩 Complex Pattern #11: Automatic Handler Registration via Reflection

### Why Automatic Registration?

**Manual approach** (verbose):
```csharp
services.AddSingleton<IEndpoint, SampleEndpointPdfHandler>();
services.AddSingleton<IEndpoint, InvoicePdfHandler>();
services.AddSingleton<IEndpoint, ReceiptPdfHandler>();
// ... add more for every new handler
```

**Automatic approach** (scalable):
```csharp
services.RegisterPdfHandlers();  // Finds all IEndpoint implementations
```

### The Discovery Process

```csharp
AppDomain.CurrentDomain.GetAssemblies()  // All loaded assemblies
    .SelectMany(a => a.GetTypes())        // All types in each assembly
    .Where(t => endpointType.IsAssignableFrom(t)  // Implements IEndpoint
            && !t.IsInterface                      // Not an interface
            && !t.IsAbstract)                      // Not abstract
```

### What Gets Registered

✅ **Concrete classes**:
```csharp
public class SampleEndpointPdfHandler : IEndpoint  // ✓ Registered
```

❌ **Interfaces**:
```csharp
public interface IEndpoint  // ✗ Skipped
```

❌ **Abstract classes**:
```csharp
public abstract class BasePdfHandler : IEndpoint  // ✗ Skipped
```

### Multiple Registrations

```csharp
services.AddSingleton(endpointType, handler);
```

This registers multiple implementations of the **same interface**:
```
IEndpoint → [SampleEndpointPdfHandler, InvoicePdfHandler, ReceiptPdfHandler]
```

Later retrieved with:
```csharp
var endpoints = app.Services.GetServices<IEndpoint>();  // Returns all implementations
```

---

## 🧩 Complex Pattern #13: Async Disposal of Browser Pages

### Location: `PuppeteerPdfRenderer.RenderAsync()`

### The Pattern

```csharp
await using var page = await browser.NewPageAsync();
```

### Breaking It Down

**`await using`**: C# 8.0 syntax for async disposal

Equivalent to:
```csharp
var page = await browser.NewPageAsync();
try
{
    // Use page
}
finally
{
    await page.DisposeAsync();  // Async cleanup
}
```

### Why Async Disposal?

Closing a browser page involves network communication with Chromium:
```
Your Code → IPC → Chromium Process → Close Tab → Cleanup Memory
```

This is I/O-bound, so async disposal prevents thread blocking.

### Why It Matters

**Synchronous disposal** (would block):
```csharp
using var page = ...  // Blocks thread during Dispose()
```

**Async disposal** (non-blocking):
```csharp
await using var page = ...  // Yields thread during DisposeAsync()
```

With 10 concurrent requests, async disposal prevents thread pool starvation.

---

## 📊 Performance Characteristics

### Typical Timings

| Operation | First Time | Subsequent |
|-----------|------------|------------|
| Browser launch | 2-5 sec | N/A (cached) |
| Template compilation | 100-500 ms | N/A (cached) |
| Template rendering | 5-20 ms | 5-20 ms |
| PDF generation | 100-500 ms | 100-500 ms |
| **Total (first)** | **2-6 sec** | - |
| **Total (subsequent)** | - | **200-600 ms** |

### Memory Usage

| Component | Memory |
|-----------|--------|
| Chromium browser | ~100-150 MB |
| Per browser page | ~50-200 MB |
| Compiled template cache | ~1-5 MB per template |
| **Total (idle)** | **~150 MB** |
| **Total (10 concurrent PDFs)** | **~700 MB - 2 GB** |

### Scaling Considerations

**Vertical scaling** (single instance):
- Limited by concurrency limiter (10 concurrent)
- ~200-600 ms per PDF
- ~60-100 PDFs per minute per instance

**Horizontal scaling** (multiple instances):
- Each instance has its own browser
- Load balancer distributes requests
- Linear scalability

---

## 🔍 Debugging Tips

### Enable RazorLight Diagnostics

```csharp
_engine = new RazorLightEngineBuilder()
    .UseFileSystemProject(templatesRoot)
    .UseMemoryCachingProvider()
    .EnableDebugMode()  // Add this for detailed error messages
    .Build();
```

### Check Template Path

```csharp
var templatesRoot = Path.Combine(AppContext.BaseDirectory, "Templates");
Console.WriteLine($"Templates path: {templatesRoot}");
Console.WriteLine($"Exists: {Directory.Exists(templatesRoot)}");
```

### Monitor Browser Process

```bash
# Linux/Mac
ps aux | grep chromium

# Windows
tasklist | findstr chrome
```

### Check Compiled Template Cache

RazorLight caches templates in memory. To clear cache, restart the application.

---

## 🎯 Best Practices

1. **Always use PreserveCompilationContext=true** for RazorLight projects
2. **Always await DisposeAsync()** for Puppeteer pages to prevent resource leaks
3. **Use Networkidle0** for production PDFs with external resources
4. **Whitelist templates** to prevent security vulnerabilities
5. **Pre-warm the browser** via IHostedService to avoid cold start delays
6. **Limit concurrency** to prevent memory exhaustion
7. **Use singleton lifetimes** for expensive services (browser, template engine)
8. **Test in containers** with the appropriate launch arguments before deploying
