using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByDesignSoapClient.Api.Models
{
    public class SalesOrderViewModel
    {
        public SalesOrderViewModel()
        {
            DataOriginTypeCode = "1";
            DeliveryPriorityCode = "1";
            RequestedStartDate = DateTime.Now.Date;
            PostingDate = DateTime.Now.Date;
        }
        public string ExternalReference { get; set; }
        public string Description { get; set; }
        public string DataOriginTypeCode { get; set; }
        public string DeliveryPriorityCode { get; set; }
        public string SalesUnit { get; set; }
        public string BuyerParty { get; set; }
        public string EmployeeResponsible { get; set; }
        public DateTime? RequestedStartDate { get; set; }
        public DateTime? RequestedEndDate { get; set; }
        public DateTime? PostingDate { get; set; }
        public IFormFile FileTemplate { get; set; }
    }
}
