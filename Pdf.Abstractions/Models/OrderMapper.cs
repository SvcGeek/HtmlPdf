using System;
using System.Collections.Generic;
using System.Text;

namespace Pdf.Abstractions.Models
{
    public class OrderMapper
    {
        public string? Logo { get; set; }
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

        //consignment    
        public string? ClientSignatureImage { get; set; }

        //footer
        public string? Copyright { get; set; }
        public string? Vat { get; set; }
        public string? TermsAndConditionLink { get; set; }
    }
}
