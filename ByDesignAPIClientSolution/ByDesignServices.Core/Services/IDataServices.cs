using ByDesignServices.Core.DBModels;
using ByDesignServices.Core.DBModels.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ByDesignServices.Core.Services
{
    public interface IDataServices
    {
        Task<List<ByDCustomerDto>> GetCustomers();
        Task<List<ByDEmployeeDto>> GetEmployees();
        Task<List<SiteDto>> GetSites();
        Task<List<SiteDto>> GetLocations();
        Task AddEmployees();
        Task AddCustomers();
    }
}
