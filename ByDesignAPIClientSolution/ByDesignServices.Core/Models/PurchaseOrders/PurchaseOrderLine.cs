using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.PurchaseOrders
{
    public class PurchaseOrderLine
    {
        public string ProductId { get; set; }
        public string ExternalReference { get; set; }
        public string Quantity { get; set; }
        public string QuantityUnitCode { get; set; }
        public string ListUnitPriceAmount { get; set; }
        public string ShipToLocationId { get; set; }
    }
}
