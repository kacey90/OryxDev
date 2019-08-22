using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ByDesignWebClient.Models;
using ByDesignAPIClient.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;

namespace ByDesignWebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApplicationServices _applicationServices;
        private readonly ITenantSettingService _tenantSettingService;

        public HomeController(IApplicationServices applicationServices,
                              ITenantSettingService tenantSettingService,
                              IHostingEnvironment environment)
        {
            _applicationServices = applicationServices;
            _tenantSettingService = tenantSettingService;
            Environment = environment;
        }

        public IHostingEnvironment Environment;
        public async Task<IActionResult> Index()
        {
            var setting = await _tenantSettingService.GetSetting();
            ViewData["BaseUrl"] = setting.BaseUrl;
            return View();
        }

        public async Task<IActionResult> UploadSalesOrder()
        {
            var setting = await _tenantSettingService.GetSetting();
            ViewData["BaseUrl"] = setting.BaseUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<FileStreamResult> UploadSalesOrder(IFormFile file)
        {
            if (file != null || file.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    var response = await _applicationServices.UploadSingleSalesOrderAsync(ms);

                    if (response != null)
                    {
                        if (response.Item1 == 200 || response.Item1 == 201)
                            return new FileStreamResult(response.Item2, "text/xml") { FileDownloadName = "response.xml" };
                        else
                            return new FileStreamResult(response.Item2, "text/plain") { FileDownloadName = "errorResponse.txt" };
                    }
                        
                }
            }

            var noResponseFile = GetNoResponseFile();
            return new FileStreamResult(noResponseFile, "text/plain") { FileDownloadName = "noresponse.txt" };
        }

        public Stream GetNoResponseFile()
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                var text = "Http Response failed: The request was not a success.";
                writer.Write(text);

                return ms;
            }
        }

        public IActionResult DownloadSalesOrderTemplate()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes($"{Environment.ContentRootPath}/fileUploads/salesOrderTemplate.zip");
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Zip, "salesOrderTemplate.zip");
        }

        public async Task<IActionResult> Privacy()
        {
            var setting = await _tenantSettingService.GetSetting();
            ViewData["BaseUrl"] = setting.BaseUrl;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
