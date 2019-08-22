using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ByDesignServices.Core.Models;
using ByDesignSoapClient.Api.Models;
using Microsoft.AspNetCore.Mvc;
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
            return View(model);
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

                Tuple<int, Stream> data = null;

                var fileContent = new StreamContent(model.FileTemplate.OpenReadStream())
                {
                    Headers =
                    {
                        ContentLength = model.FileTemplate.Length,
                        ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.FileTemplate.ContentType)
                    }
                };

                var formDataContent = new MultipartFormDataContent();
                formDataContent.Add(fileContent, "FileTemplate", model.FileTemplate.FileName);
                formDataContent.Add(new StringContent(model.ShipFromSiteID), "ShipFromSiteID");
                formDataContent.Add(new StringContent(model.ShipToSiteID), "ShipToSiteID");
                formDataContent.Add(new StringContent(model.CompleteDeliveryRequestedIndicator.ToString()), "CompleteDeliveryRequestedIndicator");
                formDataContent.Add(new StringContent(model.DeliveryPriorityCode), "DeliveryPriorityCode");

                var response = await _apiClient.HttpClient.PostAsync("StockTransfer/UploadStockTransfer", formDataContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    data = JsonConvert.DeserializeObject<Tuple<int, Stream>>(responseData);
                    _toastNotification.AddSuccessToastMessage("Successful!");
                    return new FileStreamResult(data.Item2, "text/xml") { FileDownloadName = "response.xml" };
                }
                _toastNotification.AddErrorToastMessage("Failed");
                return new FileStreamResult(data.Item2, "text/xml") { FileDownloadName = "errorResponse.xml" };
            }
            return View();
        }
    }
}