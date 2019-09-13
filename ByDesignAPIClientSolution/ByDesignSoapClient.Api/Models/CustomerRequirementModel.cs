using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ByDesignSoapClient.Api.Models
{
    public class CustomerRequirementModel : IValidatableObject
    {
        public CustomerRequirementModel()
        {
            PostingDate = DateTime.Today;
            RequestedDate = DateTime.Today;
        }
        [Display(Name = "From Site ID"), Required]
        public string ShipFromSiteID { get; set; }

        [Display(Name = "To Site ID"), Required]
        public string ShipToSiteID { get; set; }
        public IEnumerable<SelectListItem> Sites { get; set; }
        public IEnumerable<SelectListItem> Locations { get; set; }

        [Display(Name = "To Location ID"), Required]
        public string ShipToLocationID { get; set; }

        [Display(Name = "Complete Delivery Requested?")]
        public bool CompleteDeliveryRequestedIndicator { get; set; }

        [Display(Name = "Delivery Priority"), Required]
        public string DeliveryPriorityCode { get; set; }


        [Display(Name = "Raise Sales Quote?")]
        public bool RaiseSalesQuote { get; set; }

        [Display(Name = "Customer ID")]
        public string AccountId { get; set; }
        public IEnumerable<SelectListItem> Accounts { get; set; }

        [Display(Name = "External Reference")]
        public string ExternalReference { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Distribution Channel")]
        public string DistributionChannelCode { get; set; }

        [Display(Name = "Posting Date"), DataType(DataType.Date)]
        public DateTime? PostingDate { get; set; }

        [Display(Name = "Requested Date"), DataType(DataType.Date)]
        public DateTime? RequestedDate { get; set; }

        [Display(Name = "Sales Unit ID")]
        public string SalesUnitId { get; set; }

        [Display(Name = "Employee Responsible")]
        public string EmployeeResponsible { get; set; }
        public IEnumerable<SelectListItem> Employees { get; set; }

        public IFormFile FileTemplate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            IDictionary<string, IEnumerable<string>> messages = new Dictionary<string, IEnumerable<string>>();

            if (RaiseSalesQuote)
            {
                if (string.IsNullOrWhiteSpace(AccountId))
                    messages.Add("Customer ID is required.", new[] { "AccountId" });
                if (string.IsNullOrWhiteSpace(ExternalReference))
                    messages.Add("External Reference is required.", new[] { "ExternalReference" });
                if (string.IsNullOrWhiteSpace(DistributionChannelCode))
                    messages.Add("Distribution Channel is required.", new[] { "DistributionChannelCode" });
                if (!PostingDate.HasValue)
                    messages.Add("Posting Date is required.", new[] { "PostingDate" });
                if (!RequestedDate.HasValue)
                    messages.Add("Requested Date is required.", new[] { "RequestedDate" });
                if (string.IsNullOrWhiteSpace(SalesUnitId))
                    messages.Add("Sales Unit ID is required.", new[] { "SalesUnitId" });
                if (string.IsNullOrWhiteSpace(EmployeeResponsible))
                    messages.Add("Employee Responsible is required.", new[] { "EmployeeResponsible" });
            }

            if (messages.Count > 0)
            {
                foreach (var msg in messages)
                {
                    yield return new ValidationResult(msg.Key, msg.Value);
                }
            }
        }
    }
}
