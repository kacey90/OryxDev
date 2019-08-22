using ByDesignServices.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ByDesignServices.Core.Services
{
    public interface ITenantSettingService
    {
        void UpdateSetting(TenantSetting setting);
        Task<TenantSetting> GetSetting();
    }
}
