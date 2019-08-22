using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ByDesignServices.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UploadToolWeb.Extensions;
using UploadToolWeb.Models;

namespace UploadToolWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiHttpClient _apiClient;
        public HomeController(ApiHttpClient apiHttpClient)
        {
            _apiClient = apiHttpClient;
        }
        public async Task<IActionResult> Index()
        {
            var response = await _apiClient.HttpClient.GetAsync("TenantSetting/ViewSetting");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var setting = JsonConvert.DeserializeObject<TenantSetting>(data);
                ViewData["BaseUrl"] = setting.BaseUrl;
                ViewData["User"] = setting.User;
            }
            return View();
        }

        public async Task<IActionResult> Privacy()
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
