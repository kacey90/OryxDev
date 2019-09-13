using ByDesignServices.Core.Models.SalesOrders;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.CsvMapping
{
    public class SalesOrderLineMapping : ClassMap<SalesOrderLine>
    {
        public SalesOrderLineMapping()
        {
            Map(s => s.ExternalReference).Name("ExternalReference").Ignore();
            Map(s => s.ProductId).Name("ProductId");
            Map(s => s.Quantity).Name("Quantity");
            Map(s => s.QuantityUnitCode).Name("QuantityUnitCode");
            Map(s => s.ShipFromLocationId).Name("Ship From");
            Map(s => s.TaxCode).Name("Tax Code");
        }
    }
}
