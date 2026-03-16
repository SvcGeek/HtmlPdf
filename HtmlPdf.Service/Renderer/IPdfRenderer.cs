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
    }
}
