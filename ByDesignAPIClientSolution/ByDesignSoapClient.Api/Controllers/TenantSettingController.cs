using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByDesignServices.Core.Models;
using ByDesignServices.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ByDesignSoapClient.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantSettingController : ControllerBase
    {
        private readonly ITenantSettingService _tenantSettingService;
        public TenantSettingController(ITenantSettingService tenantSettingService)
        {
            _tenantSettingService = tenantSettingService;
        }

        [HttpGet, Route("ViewSetting")]
        public async Task<JsonResult> ViewSetting()
        {
            var setting = await _tenantSettingService.GetSetting();
            return new JsonResult(setting);
        }

        [HttpPost, Route("SaveSetting")]
        public JsonResult ViewTenantSetting([FromBody] TenantSetting tenantSetting)
        {
            _tenantSettingService.UpdateSetting(tenantSetting);
            return new JsonResult("Saved");
        }
    }
}