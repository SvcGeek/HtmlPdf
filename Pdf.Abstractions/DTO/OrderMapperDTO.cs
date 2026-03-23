using Pdf.Abstractions.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pdf.Abstractions.Models
{
    public class OrderMapperDTO
    {
        public string? Logo { get; set; }
        public string? LogoType { get; set; }
        public string? Brand { get; set; }

        //header
        public string? TitleHeader { get; set; }
        public string? DateHeader { get; set; }

        // client, advisor, store
        public string? Client { get; set; }
        public string? ClientId { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? ClientAdvisor { get; set; }
        public string? ClientAdvisorID { get; set; }
        public string? Store { get; set; }

        // products section title ,product table

        // invoice breakdown
        public string? OrderedItemsInvoiceBreakdown { get; set; }
        public string? DiscountInvoiceBreakdown { get; set; }
        public string? DiscountLocalInvoiceBreakdown { get; set; }
        public string? TotalAmountInvoiceBreakdown { get; set; }
        public string? TotalAmountLocalInvoiceBreakdown { get; set; }
        public string? AdvancedPaymentInvoiceBreakdown { get; set; }
        public string? AdvancedPaymentLocalInvoiceBreakdown { get; set; }
        public string? OrderBalanceInvoiceBreakdown { get; set; }
        public string? OrderBalanceLocalInvoiceBreakdown { get; set; }

        //pickup store
        public string? AddressPickUpStore { get; set; }
        public string? CapPickUpStore { get; set; }
        public string? PhonePickUpStore { get; set; }

        //consignment    
        public string? ClientSignatureImage { get; set; }
        public string? ClientSignatureImageType { get; set; }

        //footer
        public string? Copyright { get; set; }
        public string? Vat { get; set; }
        public string? TermsAndConditionLink { get; set; }
    }
}
