using ByDesignServices.Core.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ByDesignServices.Core.Services
{
    public class TenantSettingService : ITenantSettingService
    {
        public IHostingEnvironment HostingEnvironment { get; }
        public TenantSettingService(IHostingEnvironment env)
        {
            HostingEnvironment = env;
        }

        public void UpdateSetting(TenantSetting setting)
        {
            string settingsFilePath = Path.Combine(HostingEnvironment.ContentRootPath, "xmlfiles/TenantSettings.xml");

            XmlDocument doc = new XmlDocument();
            doc.Load(settingsFilePath);

            var baseUrlNode = doc.SelectSingleNode("/TenantSetting/BaseUrl");
            baseUrlNode.InnerText = setting.BaseUrl;

            var userNode = doc.SelectSingleNode("/TenantSetting/User");
            userNode.InnerText = setting.User;

            var passwordNode = doc.SelectSingleNode("/TenantSetting/Password");
            passwordNode.InnerText = setting.Password;

            doc.Save(settingsFilePath);
        }

        public async Task<TenantSetting> GetSetting()
        {
            string settingsFilePath = Path.Combine(HostingEnvironment.ContentRootPath, "xmlfiles/TenantSettings.xml");

            string xmlString = await File.ReadAllTextAsync(settingsFilePath);

            XmlSerializer serializer = new XmlSerializer(typeof(TenantSetting), new XmlRootAttribute("TenantSetting"));
            var stringReader = new StringReader(xmlString);
            var tenantSetting = (TenantSetting)serializer.Deserialize(stringReader);
            return tenantSetting;
        }
    }
}
