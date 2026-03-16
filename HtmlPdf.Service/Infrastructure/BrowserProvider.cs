using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace HtmlPdf.Service.Infrastructure
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

        public BrowserProvider(ILogger<BrowserProvider> logger)
        {
            _logger = logger;
        }

        /// <summary>Returns the shared browser, initialising it on first call.</summary>
        public async Task<IBrowser> GetBrowserAsync()
        {
            if (_browser is not null)
                return _browser;

            await _lock.WaitAsync();
            try
            {
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

        private async Task<IBrowser> LaunchAsync()
        {
            _logger.LogInformation("Downloading / verifying Chromium…");
            var fetcher = new BrowserFetcher();
            await fetcher.DownloadAsync();

            _logger.LogInformation("Launching Chromium browser…");
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = ["--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage"]
            });

            _logger.LogInformation("Chromium launched (PID {Pid})", browser.Process?.Id);
            return browser;
        }

        // IHostedService: pre-warm the browser at application startup.
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await GetBrowserAsync();
        }

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
