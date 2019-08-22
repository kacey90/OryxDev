using ByDesignServices.Core.Models.StockTransfers;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.CsvMapping
{
    public class StockOrderTransferMapping : ClassMap<StockTransferModel>
    {
        public StockOrderTransferMapping()
        {
            Map(s => s.ObjectNodeSenderTechnicalID).Name("Document ID").Ignore();
            Map(s => s.ShipFromSiteID).Name("From Site ID").Ignore();
            Map(s => s.ShipToSiteID).Name("To Site ID").Ignore();
            Map(s => s.DeliveryPriorityCode).Name("Delivery Priority Code").Ignore();
            Map(s => s.ProductID).Name("Product ID");
            Map(s => s.RequestedQuantity).Name("Requested Quantity");
            Map(s => s.RequestedLocalDateTime).Name("Requested Delivery Date");
            Map(s => s.Description).Name("Product Description");
            Map(s => s.TaxCode).Name("Tax Code").Ignore();
        }
    }
}
