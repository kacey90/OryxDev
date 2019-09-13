using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ByDesignServices.Core.DBModels.DTOs;
using ByDesignServices.Core.Models;
using ByDesignSoapClient.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NToastNotify;
using UploadToolWeb.Extensions;

namespace UploadToolWeb.Controllers
{
    public class StockOrderController : Controller
    {
        private readonly ApiHttpClient _apiClient;
        private readonly IToastNotification _toastNotification;

        public StockOrderController(ApiHttpClient apiClient, IToastNotification toastNotification)
        {
            _apiClient = apiClient;
            _toastNotification = toastNotification;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> PostStockOrder()
        {
            var response = await _apiClient.HttpClient.GetAsync("TenantSetting/ViewSetting");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var setting = JsonConvert.DeserializeObject<TenantSetting>(data);
                ViewData["BaseUrl"] = setting.BaseUrl;
            }
            var model = new CustomerRequirementModel();
            model.Sites = await GetSitesList();
            model.Locations = await GetLocationsList();
            model.Employees = await GetEmployeesList();
            return View(model);
        }

        private async Task<IEnumerable<SelectListItem>> GetEmployeesList()
        {
            var response = await _apiClient.HttpClient.GetAsync("Backend/loademployees");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsAsync<List<ByDEmployeeDto>>();
                var list = data.Select(x => new SelectListItem
                {
                    Value = x.SAPId,
                    Text = x.FullName
                });

                return new List<SelectListItem>(list);
            }
            return new List<SelectListItem>();
        }

        private async Task<IEnumerable<SelectListItem>> GetSitesList()
        {
            var response = await _apiClient.HttpClient.GetAsync("Backend/loadsites");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsAsync<List<SiteDto>>();
                var list = data.Select(x => new SelectListItem
                {
                    Value = x.ID,
                    Text = x.DisplayItem
                });

                return new List<SelectListItem>(list);
            }
            return new List<SelectListItem>();
        }

        private async Task<IEnumerable<SelectListItem>> GetLocationsList()
        {
            var response = await _apiClient.HttpClient.GetAsync("Backend/loadlocations");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsAsync<List<SiteDto>>();
                var list = data.Select(x => new SelectListItem
                {
                    Value = x.ID,
                    Text = x.DisplayItem
                });

                return new List<SelectListItem>(list);
            }
            return new List<SelectListItem>();
        }

        [HttpPost]
        public async Task<IActionResult> PostStockOrder(CustomerRequirementModel model)
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

                //Stream data = null;

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
                formDataContent.Add(new StringContent(model.ShipFromSiteID), "ShipFromSiteID");
                formDataContent.Add(new StringContent(model.ShipToSiteID), "ShipToSiteID");
                formDataContent.Add(new StringContent(model.ShipToLocationID), "ShipToLocationID");
                formDataContent.Add(new StringContent(model.CompleteDeliveryRequestedIndicator.ToString()), "CompleteDeliveryRequestedIndicator");
                formDataContent.Add(new StringContent(model.DeliveryPriorityCode), "DeliveryPriorityCode");
                if (model.RaiseSalesQuote)
                {
                    formDataContent.Add(new StringContent(model.RaiseSalesQuote.ToString()), "RaiseSalesQuote");
                    formDataContent.Add(new StringContent(model.AccountId), "AccountId");
                    formDataContent.Add(new StringContent(model.ExternalReference), "ExternalReference");
                    formDataContent.Add(new StringContent(description), "Description");
                    formDataContent.Add(new StringContent(model.DistributionChannelCode), "DistributionChannelCode");
                    formDataContent.Add(new StringContent(model.PostingDate.Value.ToString()), "PostingDate");
                    formDataContent.Add(new StringContent(model.RequestedDate.Value.ToString()), "RequestedDate");
                    formDataContent.Add(new StringContent(model.SalesUnitId), "SalesUnitId");
                    formDataContent.Add(new StringContent(model.EmployeeResponsible), "EmployeeResponsible");
                }
                

                var response = await _apiClient.HttpClient.PostAsync("StockTransfer/UploadStockTransfer", formDataContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStreamAsync();
                    _toastNotification.AddSuccessToastMessage("Successful!");
                    return new FileStreamResult(responseData, "text/xml") { FileDownloadName = "response.xml" };
                }
                _toastNotification.AddErrorToastMessage("Failed");
                return View();
            }
            return View();
        }
    }
}