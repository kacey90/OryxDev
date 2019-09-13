using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.SalesOrders
{
    public class SalesOrderHeader
    {
        public string ExternalReference { get; set; }
        public string Name { get; set; }
        public string DataOriginTypeCode { get; set; }
        public string DeliveryPriorityCode { get; set; }
        public string DistributionChannelCode { get; set; }
        public string AccountId { get; set; }
        public string SalesUnitPartyId { get; set; }
        public string EmployeeResponsible { get; set; }
        public string PostingDate { get; set; }
        public string BuyerPartyId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

    }
}
