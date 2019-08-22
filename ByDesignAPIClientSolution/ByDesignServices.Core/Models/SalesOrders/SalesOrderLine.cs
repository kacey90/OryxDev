using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.SalesOrders
{
    public class SalesOrderLine
    {
        public string ExternalReference { get; set; }
        public string ProcessingTypeCode { get; set; }
        public string ProductId { get; set; }
        public string Quantity { get; set; }
        public string QuantityUnitCode { get; set; }
    }
}
