using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.SalesQuotes
{
    public class SalesQuoteHeader
    {
        public string ExternalReference { get; set; }
        public string Description { get; set; }
        public string AccountId { get; set; }
        public string EmployeeResponsible { get; set; }
        public string DistributionChannelCode { get; set; }
        public string SalesUnitId { get; set; }
        public string RequestedDate { get; set; }
        public string PostingDate { get; set; }
    }

    public class SalesQuoteLine
    {
        public string ExternalReference { get; set; }
        public string ProductId { get; set; }
        public string Quantity { get; set; }
        public string TaxCode { get; set; }
    }
}
