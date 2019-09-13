using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ByDesignServices.Core.DBModels;
using ByDesignServices.Core.Models;
using ByDesignSoapClient.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NToastNotify;
using UploadToolWeb.Extensions;

namespace UploadToolWeb.Controllers
{
    public class SalesOrderController : Controller
    {
        private readonly ApiHttpClient _apiClient;
        private readonly IToastNotification _toastNotification;

        public SalesOrderController(ApiHttpClient apiClient, IToastNotification toastNotification)
        {
            _apiClient = apiClient;
            _toastNotification = toastNotification;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PostSalesOrder()
        {
            var response = await _apiClient.HttpClient.GetAsync("TenantSetting/ViewSetting");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var setting = JsonConvert.DeserializeObject<TenantSetting>(data);
                ViewData["BaseUrl"] = setting.BaseUrl;
            }
            var salesOrder = new SalesOrderViewModel();
            salesOrder.Customers = await GetCustomers();
            salesOrder.Employees = await GetEmployees();
            return View(salesOrder);
        }

        private async Task<IEnumerable<SelectListItem>> GetEmployees()
        {
            var response = await _apiClient.HttpClient.GetAsync("Backend/loademployees");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var customers = JsonConvert.DeserializeObject<IEnumerable<ByDEmployee>>(data);
                var list = customers.Select(x => new SelectListItem
                {
                    Value = x.SAPId,
                    Text = x.FullName
                });

                return new List<SelectListItem>(list);
            }
            return new List<SelectListItem>();
        }

        private async Task<IEnumerable<SelectListItem>> GetCustomers()
        {
            var response = await _apiClient.HttpClient.GetAsync("Backend/loadcustomers");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var customers = JsonConvert.DeserializeObject<IEnumerable<ByDCustomer>>(data);
                var list = customers.Select(x => new SelectListItem
                {
                    Value = x.SAPId,
                    Text = x.FullName
                });

                return new List<SelectListItem>(list);
            }
            return new List<SelectListItem>();
        }

        [HttpPost]
        public async Task<IActionResult> PostSalesOrder(SalesOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.FileTemplate == null || model.FileTemplate.Length == 0)
                {
                    var toastrOptions = new ToastrOptions()
                    {
                        ProgressBar = true,
                        PositionClass = ToastPositions.BottomCenter
                    };
                    _toastNotification.AddWarningToastMessage("Please attach a csv file", toastrOptions);
                    return View();
                }

                var fileContent = new StreamContent(model.FileTemplate.OpenReadStream())
                {
                    Headers =
                    {
                        ContentLength = model.FileTemplate.Length,
                        ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.FileTemplate.ContentType)
                    }
                };

                var formDataContent = new MultipartFormDataContent();
                var description = string.IsNullOrEmpty(model.Description) ? "" : model.Description;
                formDataContent.Add(fileContent, "FileTemplate", model.FileTemplate.FileName);
                formDataContent.Add(new StringContent(model.ExternalReference), "ExternalReference");
                formDataContent.Add(new StringContent(model.AccountId), "AccountId");
                formDataContent.Add(new StringContent(description), "Description");
                formDataContent.Add(new StringContent(model.DataOriginTypeCode), "DataOriginTypeCode");
                formDataContent.Add(new StringContent(model.DeliveryPriorityCode), "DeliveryPriorityCode");
                formDataContent.Add(new StringContent(model.DistributionChannelCode), "DistributionChannelCode");
                formDataContent.Add(new StringContent(model.SalesUnit), "SalesUnit");
                formDataContent.Add(new StringContent(model.EmployeeResponsible), "EmployeeResponsible");
                formDataContent.Add(new StringContent(model.RequestedStartDate.Value.ToString()), "RequestedStartDate");
                formDataContent.Add(new StringContent(model.PostingDate.Value.ToString()), "PostingDate");

                
                var response = await _apiClient.HttpClient.PostAsync("SalesOrderUpload/PostSalesOrder", formDataContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStreamAsync();
                    _toastNotification.AddSuccessToastMessage("Successful!");
                    return new FileStreamResult(responseData, "text/xml") { FileDownloadName = "salesOrderResponse.xml" };
                }
                _toastNotification.AddErrorToastMessage("Failed");
                return View();
            }
            return View();
        }

        public async Task<IActionResult> UploadSalesOrder()
        {
            var response = await _apiClient.HttpClient.GetAsync("TenantSetting/ViewSetting");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var setting = JsonConvert.DeserializeObject<TenantSetting>(data);
                ViewData["BaseUrl"] = setting.BaseUrl;
            }
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadSalesOrder(IFormFile file)
        {
            if (file != null || file.Length > 0)
            {
                var response = await _apiClient.HttpClient.PostAsJsonAsync("SalesOrderUpload/UploadSalesOrder", file);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStreamAsync();
                    _toastNotification.AddSuccessToastMessage("Successful!");
                    return new FileStreamResult(responseData, "text/xml") { FileDownloadName = "bulksalesuploadresponse.xml" };
                }
                _toastNotification.AddErrorToastMessage("Failed");
            }
            return View();
        }

    }
}