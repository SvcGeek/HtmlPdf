using Pdf.Abstractions.DTO;
using Pdf.Abstractions.Models;
using System;

namespace Pdf.Abstractions.Helpers
{
    /// <summary>
    /// Helper class providing mapping utilities for transforming flat data structures
    /// into hierarchical DTOs suitable for Razor templates.
    /// </summary>
    /// <remarks>
    /// This mapper uses a fluent configuration approach with Action delegates,
    /// allowing callers to populate the source DTO in a readable, expressive way.
    /// </remarks>
    public static class ObjectMapperHelper
    {
        /// <summary>
        /// Maps a flat OrderMapperDTO structure to a hierarchical OrderDTO structure.
        /// Uses an Action delegate for fluent configuration of the source data.
        /// </summary>
        /// <param name="objModelValues">Action that populates the OrderMapperDTO with source data.</param>
        /// <returns>Fully populated OrderDTO with nested HtmlDataDTO objects ready for template rendering.</returns>
        /// <remarks>
        /// This mapping transforms flat properties into structured HtmlDataDTO objects
        /// which separate labels (for translation) from values (for data display).
        /// </remarks>
        /// <example>
        /// <code>
        /// var order = ObjectMapperHelper.MapOrderTo(mapper => 
        /// {
        ///     mapper.Client = "John Doe";
        ///     mapper.TotalAmount = "$1,500.00";
        /// });
        /// </code>
        /// </example>
        public static OrderDTO MapOrderTo(Action<OrderMapperDTO> objModelValues)
        {
            // Create an empty mapper DTO
            var orderMapperDTO = new OrderMapperDTO();

            // Invoke the caller's configuration action to populate the mapper
            // This allows fluent syntax: MapOrderTo(m => { m.Client = "John"; m.Brand = "Acme"; })
            objModelValues.Invoke(orderMapperDTO);

            // Transform flat mapper structure to hierarchical OrderDTO
            // Each string property becomes an HtmlDataDTO for label/value separation
            var target = new OrderDTO()
            {
                // Note: Template, Language, and Direction are injected separately
                // by the endpoint handler from the request root (not from Data)

                // Branding
                Brand = orderMapperDTO.Brand,
                BrandLogo = orderMapperDTO.Logo,

                // Header fields wrapped in HtmlDataDTO
                TitleHeader = new HtmlDataDTO { Value = orderMapperDTO.TitleHeader },
                DateHeader = new HtmlDataDTO { Value = orderMapperDTO.DateHeader },

                // Client details wrapped in HtmlDataDTO for label/value pattern
                ClientLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.Client },
                ClientIDLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.ClientId },
                MobileLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.Mobile },
                EmailLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.Email },
                ClientAdvisorLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.ClientAdvisor },
                ClientAdvisorIDLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.ClientAdvisorID },
                StoreLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.Store },

                // Note: ProductItemsTitle, ProductTableHeader, and ProductItems
                // are typically set outside this method as they require translation
                // or more complex mapping logic

                // Client signature
                ClientSignatureImage = orderMapperDTO.ClientSignatureImage,

                // Footer data grouped in nested DTO
                FooterData = new FooterDataDTO
                {
                    Copyright = new HtmlDataDTO { Value = orderMapperDTO.Copyright },
                    Vat = new HtmlDataDTO { Value = orderMapperDTO.Vat },
                    TermsAndConditions = new HtmlDataDTO { Value = orderMapperDTO.TermsAndConditionLink },
                }
            };

            // Conditionally set image types only if provided (avoid overwriting defaults)
            if (!string.IsNullOrWhiteSpace(orderMapperDTO.LogoType)) 
                target.BrandLogoType = orderMapperDTO.LogoType;

            if (!string.IsNullOrWhiteSpace(orderMapperDTO.ClientSignatureImageType)) 
                target.ClientSignatureImageType = orderMapperDTO.ClientSignatureImageType;

            return target;
        }
    }
}
