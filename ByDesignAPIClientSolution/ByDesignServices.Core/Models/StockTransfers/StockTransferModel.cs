using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.StockTransfers
{
    public class StockTransferModel
    {
        public string ObjectNodeSenderTechnicalID { get; set; }
        public string ShipFromSiteID { get; set; }
        public string ShipToSiteID { get; set; }
        public string ShipToLocationID { get; set; }
        public string CompleteDeliveryRequestedIndicator { get; set; }
        public string DeliveryPriorityCode { get; set; }
        public string ItemID { get; set; }
        public string ProductID { get; set; }
        public string RequestedQuantity { get; set; }
        public string QuantityCode { get; set; }
        public string RequestedLocalDateTime { get; set; }
        public string Description { get; set; }
        public string TaxCode { get; set; }
    }
}
