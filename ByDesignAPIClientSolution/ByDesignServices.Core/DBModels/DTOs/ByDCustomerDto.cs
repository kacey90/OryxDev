using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.DBModels.DTOs
{
    public class ByDCustomerDto
    {
        public string SAPId { get; set; }
        public string CustomerName { get; set; }
        public string FullName => $"{SAPId} - {CustomerName}";
    }
}
