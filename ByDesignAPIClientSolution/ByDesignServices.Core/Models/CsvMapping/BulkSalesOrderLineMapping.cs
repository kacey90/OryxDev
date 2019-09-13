using ByDesignServices.Core.Models.SalesOrders;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.CsvMapping
{
    public class BulkSalesOrderLineMapping : ClassMap<SalesOrderLine>
    {
        public BulkSalesOrderLineMapping()
        {
            Map(s => s.ExternalReference).Name("External Reference");
            Map(s => s.ProductId).Name("Product Id");
            Map(s => s.Quantity).Name("Quantity");
            Map(s => s.QuantityUnitCode).Name("Unit Code");
            Map(s => s.ShipFromLocationId).Name("Ship From");
            Map(s => s.TaxCode).Name("Tax Code");
        }
    }
}
