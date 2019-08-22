using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ByDesignServices.Core.Models.SalesOrders;
using ByDesignServices.Core.Services;
using ByDesignSoapClient.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ByDesignSoapClient.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesOrderUploadController : ControllerBase
    {
        private readonly IApplicationServices _applicationServices;
        private readonly ITenantSettingService _tenantSettingService;

        public SalesOrderUploadController(IApplicationServices applicationServices,
                              ITenantSettingService tenantSettingService,
                              IHostingEnvironment environment)
        {
            _applicationServices = applicationServices;
            _tenantSettingService = tenantSettingService;
            Environment = environment;
        }

        public IHostingEnvironment Environment;

        [HttpPost, Route("UploadSalesOrder")]
        public async Task<JsonResult> UploadSalesOrder(IFormFile file)
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
                        //if (response.Item1 == 200 || response.Item1 == 201)
                        //    return new FileStreamResult(response.Item2, "text/xml") { FileDownloadName = "response.xml" };
                        //else
                        //    return new FileStreamResult(response.Item2, "text/plain") { FileDownloadName = "errorResponse.txt" };
                        return new JsonResult(response);
                    }

                }
            }

            return null;
            //var noResponseFile = GetNoResponseFile();
            //return new FileStreamResult(noResponseFile, "text/plain") { FileDownloadName = "noresponse.txt" };
        }

        [HttpPost, Route("PostSalesOrder")]
        public async Task<JsonResult> UploadSalesOrder([FromBody] SalesOrderViewModel model)
        {
            if (model.FileTemplate != null || model.FileTemplate.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    model.FileTemplate.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    var response = await _applicationServices.PostSalesOrder(ms, new SalesOrderHeader
                    {
                        ExternalReference = model.ExternalReference,
                        Name = model.Description,
                        DataOriginTypeCode = model.DataOriginTypeCode,
                        DeliveryPriorityCode = model.DeliveryPriorityCode,
                        SalesUnitPartyId = model.SalesUnit,
                        BuyerPartyId = model.BuyerParty,
                        StartDate = model.RequestedStartDate.Value.ToString("dd/MM/yyyy"),
                        EndDate = model.RequestedEndDate.Value.ToString("dd/MM/yyyy")
                    });

                    if (response != null)
                    {
                        return new JsonResult(response);
                    }
                }
            }

            return null;
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
    }
}