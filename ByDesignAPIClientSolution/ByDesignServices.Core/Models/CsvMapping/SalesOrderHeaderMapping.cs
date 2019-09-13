using ByDesignServices.Core.Models.SalesOrders;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.Models.CsvMapping
{
    public class SalesOrderHeaderMapping : ClassMap<SalesOrderHeader>
    {
        public SalesOrderHeaderMapping()
        {
            Map(s => s.ExternalReference).Name("External Reference");
            Map(s => s.Name).Name("Description");
            Map(s => s.DeliveryPriorityCode).Name("Delivery Priority");
            Map(s => s.DistributionChannelCode).Name("Distribution Channel");
            Map(s => s.SalesUnitPartyId).Name("Sales Unit");
            Map(s => s.BuyerPartyId).Name("Account");
            Map(s => s.EmployeeResponsible).Name("Employee Responsible");
            Map(s => s.StartDate).Name("Date Requested");
            Map(s => s.EndDate).Name("Posting Date");
        }
    }
}
