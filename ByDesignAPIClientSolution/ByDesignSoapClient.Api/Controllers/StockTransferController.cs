using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ByDesignServices.Core.Models.StockTransfers;
using ByDesignServices.Core.Services;
using ByDesignSoapClient.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ByDesignSoapClient.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockTransferController : ControllerBase
    {
        private readonly IApplicationServices _applicationServices;
        private readonly ITenantSettingService _tenantSettingService;

        public StockTransferController(IApplicationServices applicationServices,
                              ITenantSettingService tenantSettingService,
                              IHostingEnvironment environment)
        {
            _applicationServices = applicationServices;
            _tenantSettingService = tenantSettingService;
            Environment = environment;
        }

        public IHostingEnvironment Environment;

        [HttpPost, Route("UploadStockTransfer")]
        public async Task<IActionResult> UploadStockTransfer([FromForm] CustomerRequirementModel model)
        {
            if (model.FileTemplate != null || model.FileTemplate.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    model.FileTemplate.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    var response = await _applicationServices.UploadStockTransfer(ms, new StockTransferModel
                    {
                        ShipFromSiteID = model.ShipFromSiteID,
                        ShipToSiteID = model.ShipToSiteID,
                        CompleteDeliveryRequestedIndicator = model.CompleteDeliveryRequestedIndicator ? "true" : "false",
                        DeliveryPriorityCode = model.DeliveryPriorityCode
                    });

                    if (response != null)
                    {
                        return Ok(response);
                    }
                }
            }

            return null;
        }
    }
}