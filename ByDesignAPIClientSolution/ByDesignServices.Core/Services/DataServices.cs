using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using ByDesignServices.Core.DBModels;
using ByDesignServices.Core.DBModels.DTOs;
using ByDesignServices.Core.Services.DataAccess;
using ByDesignServices.Core.Utilities;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ByDesignServices.Core.Services
{
    public class DataServices : IDataServices
    {
        private readonly DataContext _dbContext;
        private readonly ByDesignHttpClient _client;
        private readonly ITenantSettingService _tenantSettingService;
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly ILogger _logger;
        public IHostingEnvironment HostingEnvironment { get; }

        public DataServices(DataContext dbContext, ByDesignHttpClient client, IHostingEnvironment env, 
            ITenantSettingService tenantSettingService, ISqlConnectionFactory connectionFactory, ILogger<DataServices> logger)
        {
            _dbContext = dbContext;
            _client = client;
            _tenantSettingService = tenantSettingService;
            HostingEnvironment = env;
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public async Task AddCustomers()
        {
            string filePath = Path.Combine(HostingEnvironment.ContentRootPath, "xmlfiles/queryAccounts.xml");
            StreamReader sr = new StreamReader(filePath);
            string soapXml = sr.ReadToEnd();
            sr.Close();

            var tenantSetting = await _tenantSettingService.GetSetting();
            UriBuilder urlBuilder = new UriBuilder(tenantSetting.BaseUrl)
            {
                Path = "/sap/bc/srt/scs/sap/querycustomerin1"
            };
            var request = new HttpRequestMessage(HttpMethod.Get, urlBuilder.ToString());
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/xml"));
            var byteArray = Encoding.ASCII.GetBytes($"{tenantSetting.User}:{tenantSetting.Password}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            request.Content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
            var response = await _client.HttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(responseData);
                var nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("soap-env", "http://schemas.xmlsoap.org/soap/envelope/");
                nsmgr.AddNamespace("n0", "http://sap.com/xi/SAPGlobal20/Global");

                var customerNodes = doc.SelectNodes("/soap-env:Envelope/soap-env:Body/n0:CustomerByElementsResponse_sync/Customer", nsmgr);

                if (customerNodes != null && customerNodes.Count > 0)
                {
                    var customers = new List<ByDCustomer>();
                    foreach (XmlNode customerNode in customerNodes)
                    {
                        var customerIdNode = customerNode.SelectSingleNode("InternalID");
                        var nameNode = customerNode.SelectSingleNode("Organisation/FirstLineName");
                        if (nameNode == null)
                            nameNode = customerNode.SelectSingleNode("AddressInformation/Address/FormattedAddress/FormattedAddressDescription");
                        
                        customers.Add(new ByDCustomer
                        {
                            SAPId = customerIdNode.InnerText,
                            CustomerName = nameNode.InnerText
                        });
                        _logger.LogInformation("done object - " + customerIdNode.InnerText);
                    }

                    await _dbContext.ByDCustomers.AddRangeAsync(customers);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task AddEmployees()
        {
            string filePath = Path.Combine(HostingEnvironment.ContentRootPath, "xmlfiles/queryCustomers.xml");
            StreamReader sr = new StreamReader(filePath);
            string soapXml = sr.ReadToEnd();
            sr.Close();

            var tenantSetting = await _tenantSettingService.GetSetting();
            UriBuilder urlBuilder = new UriBuilder(tenantSetting.BaseUrl)
            {
                Path = "/sap/bc/srt/scs/sap/queryemployeein"
            };
            var request = new HttpRequestMessage(HttpMethod.Get, urlBuilder.ToString());
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/xml"));
            var byteArray = Encoding.ASCII.GetBytes($"{tenantSetting.User}:{tenantSetting.Password}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            request.Content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
            var response = await _client.HttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(responseData);
                var nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("soap-env", "http://schemas.xmlsoap.org/soap/envelope/");
                nsmgr.AddNamespace("n0", "http://sap.com/xi/SAPGlobal20/Global");

                var employeeNodes = doc.SelectNodes("/soap-env:Envelope/soap-env:Body/n0:EmployeeDataByIdentificationResponse_sync/EmployeeData", nsmgr);

                if (employeeNodes != null && employeeNodes.Count > 0)
                {
                    var employees = new List<ByDEmployee>();
                    foreach (XmlNode empNode in employeeNodes)
                    {
                        var empIdNode = empNode.SelectSingleNode("EmployeeID");
                        var givenNameNode = empNode.SelectSingleNode("BiographicalData/GivenName");
                        var familyNameNode = empNode.SelectSingleNode("BiographicalData/FamilyName");
                        employees.Add(new ByDEmployee
                        {
                            SAPId = empIdNode.InnerText,
                            FirstName = givenNameNode.InnerText,
                            LastName = familyNameNode.InnerText
                        });
                    }

                    await _dbContext.ByDEmployees.AddRangeAsync(employees);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task<List<ByDCustomerDto>> GetCustomers()
        {
            //var connection = _connectionFactory.GetOpenConnection();
            //const string getCustomersCmd = "SELECT [SAPId], [CustomerName] FROM [ByDCustomers]";
            //var command = await connection.QueryAsync<ByDCustomerDto>(getCustomersCmd);
            //var list = command.AsList();
            //return list;

            var tenantSetting = await _tenantSettingService.GetSetting();

            var url = tenantSetting.BaseUrl + "/sap/byd/odata/ana_businessanalytics_analytics.svc/RPBPCSCONTB_Q0001QueryResults?$select=TBP_UUID,CBP_UUID&$format=json";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var byteArray = Encoding.ASCII.GetBytes("ADMIN2ORYX7000001:Oryx5678");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            //request.Content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
            var response = await _client.HttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                List<ByDCustomerDto> accounts = new List<ByDCustomerDto>();
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(json);

                foreach (var item in data.d.results)
                {
                    accounts.Add(new ByDCustomerDto { SAPId = item.CBP_UUID, CustomerName = item.TBP_UUID });
                }

                return accounts;
            }

            return new List<ByDCustomerDto>();
        }

        public async Task<List<ByDEmployeeDto>> GetEmployees()
        {
            //var connection = _connectionFactory.GetOpenConnection();
            //const string sql = "SELECT [SAPId], [FirstName], [LastName] FROM [ByDEmployees]";
            //var command = await connection.QueryAsync<ByDEmployeeDto>(sql);
            //var list = command.AsList();
            //return list;

            var tenantSetting = await _tenantSettingService.GetSetting();
           
            var url = tenantSetting.BaseUrl + "/sap/byd/odata/cust/v1/employeeslisting/EmployeeCollection?$expand=EmployeeCommon&$format=json";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var byteArray = Encoding.ASCII.GetBytes("ADMIN2ORYX7000001:Oryx5678");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            var response = await _client.HttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                List<ByDEmployeeDto> employees = new List<ByDEmployeeDto>();
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(json);
                foreach (var item in data.d.results)
                {
                    employees.Add(new ByDEmployeeDto
                    {
                        SAPId = item.EmployeeID,
                        FirstName = item.EmployeeCommon[0].GivenName,
                        LastName = item.EmployeeCommon[0].FamilyName
                    });
                }

                return employees;
            }

            return new List<ByDEmployeeDto>();
        }

        public async Task<List<SiteDto>> GetSites()
        {
            var tenantSetting = await _tenantSettingService.GetSetting();

            var url = tenantSetting.BaseUrl + "/sap/byd/odata/ana_businessanalytics_analytics.svc/RPZF0535B8EB1B7E7E5C830ACQueryResults?$filter=CSITE_IND eq 1&$format=json";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var byteArray = Encoding.ASCII.GetBytes("ADMIN2ORYX7000001:Oryx5678");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            //request.Content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
            var response = await _client.HttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                List<SiteDto> sites = new List<SiteDto>();
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(json);

                foreach (var item in data.d.results)
                {
                    sites.Add(new SiteDto { ID = item.CLOC_ID, Name = item.TLOC_ID });
                }

                return sites;
            }

            return new List<SiteDto>();
        }

        public async Task<List<SiteDto>> GetLocations()
        {
            var tenantSetting = await _tenantSettingService.GetSetting();

            var url = tenantSetting.BaseUrl + "/sap/byd/odata/ana_businessanalytics_analytics.svc/RPZF0535B8EB1B7E7E5C830ACQueryResults?$filter=CSTLOC_IND eq 1&$format=json";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var byteArray = Encoding.ASCII.GetBytes("ADMIN2ORYX7000001:Oryx5678");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            //request.Content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
            var response = await _client.HttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                List<SiteDto> sites = new List<SiteDto>();
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(json);

                foreach (var item in data.d.results)
                {
                    sites.Add(new SiteDto { ID = item.CLOC_ID, Name = item.TLOC_ID });
                }

                return sites;
            }

            return new List<SiteDto>();
        }
    }
}
