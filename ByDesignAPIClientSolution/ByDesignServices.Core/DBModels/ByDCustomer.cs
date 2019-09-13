using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.DBModels
{
    public class ByDCustomer
    {
        public int ID { get; set; }
        public string SAPId { get; set; }
        public string CustomerName { get; set; }
        public string FullName => $"{SAPId} - {CustomerName}";
    }
}
