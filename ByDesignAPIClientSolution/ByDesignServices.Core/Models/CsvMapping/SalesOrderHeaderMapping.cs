using ByDesignServices.Core.Models.SalesOrders;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.CsvMapping
{
    public class SalesOrderHeaderMapping : ClassMap<SalesOrderHeader>
    {
        public SalesOrderHeaderMapping()
        {
            Map(s => s.ExternalReference).Name("ExternalReference");
            Map(s => s.Name).Name("Name");
            Map(s => s.DataOriginTypeCode).Name("DataOriginTypeCode");
            Map(s => s.DeliveryPriorityCode).Name("DeliveryPriorityCode");
            Map(s => s.SalesUnitPartyId).Name("SalesUnit");
            Map(s => s.BuyerPartyId).Name("BuyerPartyId");
            Map(s => s.StartDate).Name("StartDate");
            Map(s => s.EndDate).Name("EndDate");
        }
    }
}
