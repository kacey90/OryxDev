using ByDesignServices.Core.Models.PurchaseOrders;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.CsvMapping
{
    public class PurchaseOrderHeaderMapping : ClassMap<PurchaseOrderHeader>
    {
        public PurchaseOrderHeaderMapping()
        {
            Map(p => p.ExternalReference).Name("External Reference");
            Map(p => p.Name).Name("Name");
            Map(p => p.CurrencyCode).Name("Currency Code");
            Map(p => p.IncotermsCode).Name("Incoterms Code");
            Map(p => p.IncotermsLocationName).Name("Incoterms Location Name");
            Map(p => p.SupplierId).Name("Supplier ID");
            Map(p => p.PurchasingUnitId).Name("Purchasing Unit ID");
        }
    }
}
