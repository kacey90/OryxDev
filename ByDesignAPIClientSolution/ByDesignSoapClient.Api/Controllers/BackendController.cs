using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByDesignServices.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ByDesignSoapClient.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackendController : ControllerBase
    {
        private readonly IDataServices _dataServices;

        public BackendController(IDataServices dataServices)
        {
            _dataServices = dataServices;
        }

        [Route("AddEmployees")]
        public async Task<IActionResult> AddEmployees()
        {
            await _dataServices.AddEmployees();
            var emps = await _dataServices.GetEmployees();
            return Ok(emps);
        }

        [Route("AddCustomers")]
        public async Task<IActionResult> AddCustomers()
        {
            await _dataServices.AddCustomers();
            var customers = await _dataServices.GetCustomers();
            return Ok(customers);
        }

        [Route("loadcustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            var list = await _dataServices.GetCustomers();
            return Ok(list);
        }

        [Route("loademployees")]
        public async Task<IActionResult> GetEmployees()
        {
            var list = await _dataServices.GetEmployees();
            return Ok(list);
        }

        [Route("loadsites")]
        public async Task<IActionResult> GetSites()
        {
            var list = await _dataServices.GetSites();
            return Ok(list);
        }

        [Route("loadlocations")]
        public async Task<IActionResult> GetLocations()
        {
            var list = await _dataServices.GetLocations();
            return Ok(list);
        }

    }
}