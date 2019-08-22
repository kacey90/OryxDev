using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByDesignSoapClient.Api.Models
{
    public class CustomerRequirementModel
    {
        public string ShipFromSiteID { get; set; }
        public string ShipToSiteID { get; set; }
        public bool CompleteDeliveryRequestedIndicator { get; set; }
        public string DeliveryPriorityCode { get; set; }
        public IFormFile FileTemplate { get; set; }
    }
}
