using System.Threading.Tasks;

namespace HtmlPdf.Service.Renderer
{
    public interface IPdfRenderer
    {
        /// <summary>
        /// Renders a Razor template with the given model to a PDF byte array.
        /// </summary>
        /// <param name="templateName">The logical template name (e.g. "delivery").</param>
        /// <param name="model">The view model passed to the Razor template.</param>
        /// <returns>Raw PDF bytes.</returns>
        Task<byte[]> RenderAsync(string templateName, object model);

        /// <summary>
        /// Converts a pre-rendered HTML string to a PDF byte array using the headless browser.
        /// Use this when the HTML has already been produced (e.g. by a custom template renderer).
        /// </summary>
        /// <param name="html">The full HTML content to render as PDF.</param>
        /// <returns>Raw PDF bytes.</returns>
        Task<byte[]> RenderHtmlAsync(string html);
    }
}
