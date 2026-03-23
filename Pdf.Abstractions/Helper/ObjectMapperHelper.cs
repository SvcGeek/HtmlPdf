using Pdf.Abstractions.DTO;
using Pdf.Abstractions.Models;
using System;

namespace Pdf.Abstractions.Helpers
{
    public static class ObjectMapperHelper
    {
        public static OrderDTO MapOrderTo(Action<OrderMapperDTO> objModelValues)
        {
            var orderMapperDTO = new OrderMapperDTO();

            objModelValues.Invoke(orderMapperDTO);

            var target = new OrderDTO()
            {
                // no need to be mapped as they are set in EndpointHandler when i build the model for the template, from request data.
                // Template 
                // Language
                // Direction

                Brand = orderMapperDTO.Brand,
                BrandLogo = orderMapperDTO.Logo,

                TitleHeader = new HtmlDataDTO { Value = orderMapperDTO.TitleHeader },
                DateHeader = new HtmlDataDTO { Value = orderMapperDTO.DateHeader },

                ClientLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.Client },
                ClientIDLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.ClientId },
                MobileLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.Mobile },
                EmailLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.Email },
                ClientAdvisorLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.ClientAdvisor },
                ClientAdvisorIDLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.ClientAdvisorID },
                StoreLabelDetails = new HtmlDataDTO { Value = orderMapperDTO.Store },

                // ProductItemsTitle as only transalation 
                // poducts header table has only transalation  outside of this method, so we don't have to set it here
                // poducts mapped outside of this method, so we don't have to set it here

                // ClientSignatureTitle as only transalation
                // TermsAndConditionsClientSignature as only transalation
                ClientSignatureImage = orderMapperDTO.ClientSignatureImage,

                FooterData = new FooterDataDTO
                {
                    Copyright = new HtmlDataDTO { Value = orderMapperDTO.Copyright },
                    Vat = new HtmlDataDTO { Value = orderMapperDTO.Vat },
                    TermsAndConditions = new HtmlDataDTO { Value = orderMapperDTO.TermsAndConditionLink },
                }
            };

            if (!string.IsNullOrWhiteSpace(orderMapperDTO.LogoType)) target.BrandLogoType = orderMapperDTO.LogoType;
            if (!string.IsNullOrWhiteSpace(orderMapperDTO.ClientSignatureImageType)) target.ClientSignatureImageType = orderMapperDTO.ClientSignatureImageType;

            return target;
        }
    }
}
