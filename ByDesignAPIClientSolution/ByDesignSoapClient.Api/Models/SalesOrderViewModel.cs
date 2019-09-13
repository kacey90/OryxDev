using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ByDesignSoapClient.Api.Models
{
    public class SalesOrderViewModel
    {
        public SalesOrderViewModel()
        {
            DataOriginTypeCode = "4";
            RequestedStartDate = DateTime.Now.Date;
            PostingDate = DateTime.Now.Date;
        }
        [Display(Name = "External Reference"), Required]
        public string ExternalReference { get; set; }
        public string Description { get; set; }
        public string DataOriginTypeCode { get; set; }

        [Display(Name = "Delivery Priority"), Required]
        public string DeliveryPriorityCode { get; set; }

        [Display(Name = "Distribution Channel"), Required]
        public string DistributionChannelCode { get; set; }

        [Display(Name = "Sales Unit"), Required]
        public string SalesUnit { get; set; }
        public IEnumerable<SelectListItem> Employees { get; set; }

        [Display(Name = "Account"), Required]
        public string AccountId { get; set; }
        public IEnumerable<SelectListItem> Customers { get; set; }

        [Display(Name = "Employee Responsible"), Required]
        public string EmployeeResponsible { get; set; }

        [Display(Name = "Date Requested"), Required]
        public DateTime? RequestedStartDate { get; set; }
        public DateTime? RequestedEndDate { get; set; }

        [Display(Name = "Posting Date"), Required]
        public DateTime? PostingDate { get; set; }

        [Display(Name = "File Template")]
        public IFormFile FileTemplate { get; set; }
    }
}
