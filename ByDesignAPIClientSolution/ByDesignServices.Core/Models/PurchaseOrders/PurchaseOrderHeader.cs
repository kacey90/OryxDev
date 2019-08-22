using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.PurchaseOrders
{
    public class PurchaseOrderHeader
    {
        public string ExternalReference { get; set; }
        public string Name { get; set; }
        public string CurrencyCode { get; set; }
        public string IncotermsCode { get; set; }
        public string IncotermsLocationName { get; set; }
        public string SupplierId { get; set; }
        public string PurchasingUnitId { get; set; }
    }
}
