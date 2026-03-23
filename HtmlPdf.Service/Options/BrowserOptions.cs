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
        /// The download path for Chromium binaries.
        /// If not specified, defaults to the system temp directory.
        /// </summary>
        public string? DownloadPath { get; set; }
    }
}
