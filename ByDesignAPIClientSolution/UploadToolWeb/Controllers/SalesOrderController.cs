using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ByDesignServices.Core.Models;
using ByDesignSoapClient.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            return View(salesOrder);
        }

        [HttpPost]
        public async Task<IActionResult> PostSalesOrder(SalesOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                Tuple<int, Stream> data = null;
                var response = await _apiClient.HttpClient.PostAsJsonAsync("SalesOrderUpload/PostSalesOrder", model);
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
                Tuple<int, Stream> data = null;
                var response = await _apiClient.HttpClient.PostAsJsonAsync("SalesOrderUpload/UploadSalesOrder", file);
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