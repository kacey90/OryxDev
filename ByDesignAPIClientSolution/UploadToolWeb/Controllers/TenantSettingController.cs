using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ByDesignServices.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NToastNotify;
using UploadToolWeb.Extensions;

namespace UploadToolWeb.Controllers
{
    public class TenantSettingController : Controller
    {
        private readonly ApiHttpClient _apiClient;
        private readonly IToastNotification _toastNotification;

        public TenantSettingController(ApiHttpClient apiClient, IToastNotification toastNotification)
        {
            _apiClient = apiClient;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _apiClient.HttpClient.GetAsync("TenantSetting/ViewSetting");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<TenantSetting>(json);
                ViewData["BaseUrl"] = data.BaseUrl;
                return View(data);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(TenantSetting tenantSetting)
        {
            if (ModelState.IsValid)
            {
                var obj = new TenantSettingDto { BaseUrl = tenantSetting.BaseUrl, Password = tenantSetting.Password, User = tenantSetting.User };
                var response = await _apiClient.HttpClient.PostAsJsonAsync("TenantSetting/SaveSetting", obj);
                if (response.IsSuccessStatusCode)
                {
                    _toastNotification.AddSuccessToastMessage("Success!");
                    return RedirectToAction(nameof(Index));
                }
                
            }
            return View();
        }
    }

    public class TenantSettingDto
    {
        public string BaseUrl { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
