using ByDesignServices.Core.Models.SalesQuotes;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.CsvMapping
{
    public class SalesQuoteItemMapping : ClassMap<SalesQuoteLine>
    {
        public SalesQuoteItemMapping()
        {
            Map(s => s.ProductId).Name("Product ID");
        }
    }
}
