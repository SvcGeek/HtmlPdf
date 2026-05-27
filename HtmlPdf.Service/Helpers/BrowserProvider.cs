using HtmlPdf.Service.Options;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.BrowserData;

namespace HtmlPdf.Service.Helpers
{
    /// <summary>
    /// Manages the lifecycle of a singleton Chromium browser instance.
    /// Downloads Chromium on first use (if not already present) and keeps the
    /// browser open for the lifetime of the application.
    /// </summary>
    public sealed class BrowserProvider : IAsyncDisposable, IHostedService
    {
        private IBrowser? _browser;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger<BrowserProvider> _logger;
        private readonly BrowserOptions _options;

        public BrowserProvider(ILogger<BrowserProvider> logger, IOptions<BrowserOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Returns the shared browser instance, initializing it on first call.
        /// Uses double-checked locking pattern to ensure thread-safe lazy initialization.
        /// </summary>
        /// <remarks>
        /// The double-check pattern:
        /// 1. First check (no lock): Fast path when browser is already initialized
        /// 2. Acquire lock: Ensures only one thread initializes the browser
        /// 3. Second check (inside lock): Handles race condition where multiple threads
        ///    pass the first check before initialization completes
        /// </remarks>
        public async Task<IBrowser> GetBrowserAsync()
        {
            // Fast path: return immediately if already initialized
            if (_browser is not null)
                return _browser;

            // Slow path: initialize the browser with thread safety
            await _lock.WaitAsync();
            try
            {
                // Double-check: another thread might have initialized while we waited for the lock
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

        /// <summary>
        /// Downloads (if needed) and launches a headless Chromium browser instance.
        /// </summary>
        /// <remarks>
        /// Launch options explained:
        /// - Headless=true: Runs browser without UI (for server environments)
        /// - --no-sandbox: Required in containerized environments (Docker, Kubernetes)
        /// - --disable-setuid-sandbox: Alternative sandboxing method for environments without SUID
        /// - --disable-dev-shm-usage: Writes shared memory to /tmp instead of /dev/shm
        ///   (fixes crashes in Docker containers with limited /dev/shm space)
        /// </remarks>
        private async Task<IBrowser> LaunchAsync()
        {
            // Use a dedicated volume path so Chromium survives container restarts.
            // If the binary is already present, skip the download entirely.
            var chromiumPath = _options.ChromiumPath;
            Directory.CreateDirectory(chromiumPath);

            var fetcher = new BrowserFetcher(new BrowserFetcherOptions { Path = chromiumPath });

            var installed = fetcher.GetInstalledBrowsers().ToList();
            InstalledBrowser installedBrowser;

            if (installed.Count > 0)
            {
                installedBrowser = installed[0];
                _logger.LogInformation("Chromium already present at '{Path}', skipping download.", chromiumPath);
            }
            else
            {
                _logger.LogInformation("Chromium not found at '{Path}', downloading…", chromiumPath);
                installedBrowser = await fetcher.DownloadAsync();
                _logger.LogInformation("Chromium download complete.");
            }

            _logger.LogInformation("Launching Chromium browser…");
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = installedBrowser.GetExecutablePath(),
                Args = ["--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage"]
            });

            _logger.LogInformation("Chromium launched (PID {Pid})", browser.Process?.Id);
            return browser;
        }

        /// <summary>
        /// IHostedService implementation: pre-warms the browser at application startup.
        /// This prevents the first PDF request from experiencing a cold start delay.
        /// </summary>
        /// <remarks>
        /// By implementing IHostedService, the browser is launched before the application
        /// starts accepting HTTP requests. This can add 2-5 seconds to startup time
        /// but eliminates the delay on the first PDF generation request.
        /// </remarks>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await GetBrowserAsync();
        }

        /// <summary>
        /// IHostedService implementation: called during graceful shutdown.
        /// Cleanup is handled by DisposeAsync instead.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async ValueTask DisposeAsync()
        {
            if (_browser is not null)
            {
                _logger.LogInformation("Disposing Chromium browser…");
                await _browser.DisposeAsync();
                _browser = null;
            }

            _lock.Dispose();
        }
    }
}
