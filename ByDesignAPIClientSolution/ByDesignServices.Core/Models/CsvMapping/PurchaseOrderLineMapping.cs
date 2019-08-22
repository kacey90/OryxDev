using ByDesignServices.Core.Models.PurchaseOrders;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.CsvMapping
{
    public class PurchaseOrderLineMapping : ClassMap<PurchaseOrderLine>
    {
        public PurchaseOrderLineMapping()
        {
            Map(p => p.ExternalReference).Name("External Reference");
            Map(p => p.ProductId).Name("Product ID");
            Map(p => p.Quantity).Name("Quantity");
            Map(p => p.QuantityUnitCode).Name("Quantity Unit Code");
            Map(p => p.ListUnitPriceAmount).Name("List Unit Price Amount");
            Map(p => p.ShipToLocationId).Name("Ship to Location Id");
        }
    }
}
