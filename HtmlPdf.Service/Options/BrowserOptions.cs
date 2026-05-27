namespace HtmlPdf.Service.Options
{
    /// <summary>
    /// Configuration options for the Chromium browser managed by BrowserProvider.
    /// </summary>
    public class BrowserOptions
    {
        /// <summary>
        /// Configuration section name in appsettings.json
        /// </summary>
        public const string SectionName = "Browser";

        /// <summary>
        /// Absolute path where Chromium binaries are stored.
        /// Mount this path as a Docker volume to persist Chromium across restarts
        /// and avoid re-downloading on every container start.
        /// Default: /app/chromium (matches the declared Docker VOLUME).
        /// Override in appsettings.Development.json for local development.
        /// </summary>
        public string ChromiumPath { get; set; } = "/app/chromium";
    }
}
