using ByDesignServices.Core.Extensions;
using ByDesignServices.Core.Models.CsvMapping;
using ByDesignServices.Core.Models.PurchaseOrders;
using ByDesignServices.Core.Models.PurchaseOrders.PO;
using ByDesignServices.Core.Models.SalesOrders;
using ByDesignServices.Core.Models.StockTransfers;
using ByDesignServices.Core.Utilities;
using CsvHelper;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ByDesignServices.Core.Services
{
    public class ApplicationServices : IApplicationServices
    {
        private readonly ByDesignHttpClient _client;
        private readonly ITenantSettingService _tenantSettingService;
        public IHostingEnvironment HostingEnvironment { get; }

        public ApplicationServices(ByDesignHttpClient client, IHostingEnvironment env,
            ITenantSettingService tenantSettingService)
        {
            _client = client;
            _tenantSettingService = tenantSettingService;
            HostingEnvironment = env;
        }




        public async Task<Tuple<int, Stream>> UploadSalesOrderAsync(MemoryStream memoryStream)
        {
            IDictionary<Stream, string> streams = new Dictionary<Stream, string>();
            using (ZipArchive archive = new ZipArchive(memoryStream))
            {
                var headerStream = archive.Entries.SingleOrDefault(x => x.FullName.Equals("sales_order_header.csv")).Open();
                var lineStream = archive.Entries.SingleOrDefault(x => x.FullName.Equals("sales_order_lines.csv")).Open();
                streams.Add(headerStream, "sales_order_header.csv");
                streams.Add(lineStream, "sales_order_lines.csv");

                var headerRecords = new List<SalesOrderHeader>();
                var lineItemRecords = new List<SalesOrderLine>();

                foreach (var stream in streams)
                {
                    using (var reader = new StreamReader(stream.Key))
                    using (var csv = new CsvReader(reader))
                    {
                        if (stream.Value.Equals("sales_order_header.csv"))
                        {
                            csv.Configuration.RegisterClassMap<SalesOrderHeaderMapping>();
                            headerRecords.AddRange(csv.GetRecords<SalesOrderHeader>());
                        }
                        else
                        {
                            csv.Configuration.RegisterClassMap<SalesOrderLineMapping>();
                            lineItemRecords.AddRange(csv.GetRecords<SalesOrderLine>());
                        }
                    }
                }

                SalesOrderList salesOrders = new SalesOrderList();

                foreach (var header in headerRecords)
                {
                    //var provider = new CultureInfo("en-GB");
                    var startDate = DateTime.ParseExact(header.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var endDate = DateTime.ParseExact(header.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    var salesOrder = new SalesOrder();
                    salesOrder.ObjectNodeSenderTechnicalID = header.ExternalReference;
                    salesOrder.Name.Text = header.Name;
                    salesOrder.DataOriginTypeCode = header.DataOriginTypeCode;
                    salesOrder.DeliveryTerms.DeliveryPriorityCode = header.DeliveryPriorityCode;
                    salesOrder.SalesUnitParty.PartyID = header.SalesUnitPartyId;
                    salesOrder.BillToParty.PartyID = header.BuyerPartyId;
                    salesOrder.PricingTerms.GrossAmountIndicator = "false";
                    salesOrder.RequestedFulfillmentPeriodPeriodTerms.StartDateTime.Text = startDate.ToString("yyyy-MM-dd");
                    salesOrder.RequestedFulfillmentPeriodPeriodTerms.EndDateTime.Text = endDate.ToString("yyyy-MM-dd");

                    var lineItems = lineItemRecords.FindAll(x => x.ExternalReference == header.ExternalReference);
                    foreach (var line in lineItems)
                    {
                        int lineItemId = 10;
                        var item = new Models.SalesOrders.Item();
                        item.ID = lineItemId.ToString();
                        item.ProcessingTypeCode = line.ProcessingTypeCode;
                        item.ItemProduct.ProductID = line.ProductId;
                        item.ItemScheduleLine.Quantity.Text = line.Quantity;
                        item.ItemScheduleLine.Quantity.UnitCode = line.QuantityUnitCode;

                        salesOrder.Items.Add(item);
                        lineItemId = lineItemId + 10;
                    }
                    salesOrders.SalesOrders.Add(salesOrder);
                }

                //var soJson = HelperExtensions.SerializeObject(salesOrders.SalesOrders);
                var salesOrderXml = HelperExtensions.SerializeToXml(salesOrders);

                //read createxml file
                string filePath = Path.Combine(HostingEnvironment.ContentRootPath, "xmlfiles/salesOrder.xml");

                //merge xml files
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);
                var nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                nsmgr.AddNamespace("glob", "http://sap.com/xi/SAPGlobal20/Global");

                var element = doc.SelectSingleNode("/soapenv:Envelope/soapenv:Body/glob:SalesOrderBundleMaintainRequest_sync", nsmgr);

                if (element != null)
                {
                    var subDoc = new XmlDocument();
                    subDoc.LoadXml(salesOrderXml);
                    var nsmgr1 = new XmlNamespaceManager(subDoc.NameTable);
                    nsmgr1.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
                    nsmgr1.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    var salesOrderListNodes = subDoc.SelectNodes("//SalesOrderList/SalesOrder", nsmgr1);
                    var sb = new StringBuilder();
                    foreach (XmlNode child in salesOrderListNodes)
                    {
                        sb.AppendLine("<SalesOrder>");
                        sb.AppendLine(child.InnerXml);
                        sb.AppendLine("</SalesOrder>");
                    }
                    var res = sb.ToString();
                    element.InnerXml = sb.ToString();

                    var basicMsgNode = doc.CreateNode(XmlNodeType.Element, "BasicMessageHeader", null);
                    XmlNode salesOrderNode = doc.SelectSingleNode("//SalesOrder");
                    element.InsertBefore(basicMsgNode, salesOrderNode);
                    doc.Save(filePath);

                    StreamReader sr = new StreamReader(filePath);
                    string soapXml = sr.ReadToEnd();
                    sr.Close();

                    var tenantSetting = await _tenantSettingService.GetSetting();
                    UriBuilder urlBuilder = new UriBuilder(tenantSetting.BaseUrl)
                    {
                        Path = "/sap/bc/srt/scs/sap/managesalesorderin5"
                    };
                    var request = new HttpRequestMessage(HttpMethod.Post, urlBuilder.ToString());
                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/xml"));
                    var byteArray = Encoding.ASCII.GetBytes($"{tenantSetting.User}:{tenantSetting.Password}");
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    request.Content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
                    var response = await _client.HttpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStreamAsync();
                        return Tuple.Create((int)response.StatusCode, responseData);
                    }
                }


                //_client.HttpClient.BaseAddress = new Uri(tenantSetting.BaseUrl);
                //var basicAuth = Encoding.ASCII.GetBytes($"{tenantSetting.User}:{tenantSetting.Password}");
                //_client.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(basicAuth));
                //_client.HttpClient.DefaultRequestHeaders.Add("x-csrf-token", "fetch");

                ////get csrf-token
                //var tokenRequest = await _client.HttpClient.GetAsync("/sap/byd/odata/cust/v1/khsalesorder/");
                //var csrfToken = tokenRequest.Headers.GetValues("x-csrf-token").FirstOrDefault();

                ////client.DefaultRequestHeaders.Accept.Clear();
                ////client.DefaultRequestHeaders.Clear();
                ////client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //_client.HttpClient.DefaultRequestHeaders.Add("x-csrf-token", csrfToken);

                //var httpContent = new StringContent(soJson, Encoding.UTF8, "application/json");

                ////dynamic dynamicObj = JsonConvert.DeserializeObject<dynamic>(soJson);

                //var salesOrderRequest = await _client.HttpClient.PostAsync("/sap/byd/odata/cust/v1/khsalesorder/SalesOrderCollection", httpContent);
                //if (salesOrderRequest.IsSuccessStatusCode)
                //{
                //    var response = await salesOrderRequest.Content.ReadAsStreamAsync();
                //    return response;
                //}
            }

            return null;
        }

        public async Task<Tuple<int, Stream>> PostSalesOrder(MemoryStream memoryStream, SalesOrderHeader model)
        {
            var lineItemRecords = new List<SalesOrderLine>();
            var startDate = DateTime.ParseExact(model.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(model.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var salesOrder = new SalesOrder();
            salesOrder.ObjectNodeSenderTechnicalID = model.ExternalReference;
            salesOrder.Name.Text = model.Name;
            salesOrder.DataOriginTypeCode = model.DataOriginTypeCode;
            salesOrder.DeliveryTerms.DeliveryPriorityCode = model.DeliveryPriorityCode;
            salesOrder.SalesUnitParty.PartyID = model.SalesUnitPartyId;
            salesOrder.BillToParty.PartyID = model.BuyerPartyId;
            salesOrder.PricingTerms.GrossAmountIndicator = "false";
            salesOrder.RequestedFulfillmentPeriodPeriodTerms.StartDateTime.Text = startDate.ToString("yyyy-MM-dd");
            salesOrder.RequestedFulfillmentPeriodPeriodTerms.EndDateTime.Text = endDate.ToString("yyyy-MM-dd");

            using (var reader = new StreamReader(memoryStream))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<SalesOrderLineMapping>();
                lineItemRecords.AddRange(csv.GetRecords<SalesOrderLine>());

                foreach (var lineItem in lineItemRecords)
                {
                    int lineItemId = 10;
                    var item = new Models.SalesOrders.Item();
                    item.ID = lineItemId.ToString();
                    item.ProcessingTypeCode = lineItem.ProcessingTypeCode;
                    item.ItemProduct.ProductID = lineItem.ProductId;
                    item.ItemScheduleLine.Quantity.Text = lineItem.Quantity;
                    item.ItemScheduleLine.Quantity.UnitCode = lineItem.QuantityUnitCode;

                    salesOrder.Items.Add(item);
                    lineItemId += 10;
                }

                //var soJson = HelperExtensions.SerializeObject(salesOrders.SalesOrders);
                var salesOrderXml = HelperExtensions.SerializeToXml(salesOrder);

                //read createxml file
                string filePath = Path.Combine(HostingEnvironment.ContentRootPath, "xmlfiles/salesOrder.xml");

                //merge xml files
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);
                var nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                nsmgr.AddNamespace("glob", "http://sap.com/xi/SAPGlobal20/Global");

                var element = doc.SelectSingleNode("/soapenv:Envelope/soapenv:Body/glob:SalesOrderBundleMaintainRequest_sync", nsmgr);

                if (element != null)
                {
                    var subDoc = new XmlDocument();
                    subDoc.LoadXml(salesOrderXml);
                    var nsmgr1 = new XmlNamespaceManager(subDoc.NameTable);
                    nsmgr1.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
                    nsmgr1.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    //var salesOrderListNodes = subDoc.SelectNodes("//SalesOrderList/SalesOrder", nsmgr1);
                    //var sb = new StringBuilder();
                    //foreach (XmlNode child in salesOrderListNodes)
                    //{
                    //    sb.AppendLine("<SalesOrder>");
                    //    sb.AppendLine(child.InnerXml);
                    //    sb.AppendLine("</SalesOrder>");
                    //}
                    //var res = sb.ToString();
                    //element.InnerXml = sb.ToString();

                    var basicMsgNode = doc.CreateNode(XmlNodeType.Element, "BasicMessageHeader", null);
                    XmlNode salesOrderNode = doc.SelectSingleNode("//SalesOrder");
                    element.InsertBefore(basicMsgNode, salesOrderNode);
                    doc.Save(filePath);

                    StreamReader sr = new StreamReader(filePath);
                    string soapXml = sr.ReadToEnd();
                    sr.Close();

                    var tenantSetting = await _tenantSettingService.GetSetting();
                    UriBuilder urlBuilder = new UriBuilder(tenantSetting.BaseUrl)
                    {
                        Path = "/sap/bc/srt/scs/sap/managesalesorderin5"
                    };
                    var request = new HttpRequestMessage(HttpMethod.Post, urlBuilder.ToString());
                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/xml"));
                    var byteArray = Encoding.ASCII.GetBytes($"{tenantSetting.User}:{tenantSetting.Password}");
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    request.Content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
                    var response = await _client.HttpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStreamAsync();
                        return Tuple.Create((int)response.StatusCode, responseData);
                    }

                    var errorResponse = await response.Content.ReadAsStreamAsync();
                    return Tuple.Create((int)response.StatusCode, errorResponse);
                }
            }
            return null;
        }

        public async Task<Tuple<int, Stream>> UploadSingleSalesOrderAsync(MemoryStream memoryStream)
        {
            using (var reader = new StreamReader(memoryStream))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.IgnoreBlankLines = false;
                csv.Configuration.RegisterClassMap<SalesOrderHeaderMapping>();
                csv.Configuration.RegisterClassMap<SalesOrderLineMapping>();

                var headerRecords = new List<SalesOrderHeader>();
                var lineItemRecords = new List<SalesOrderLine>();

                var isHeader = true;
                while (csv.Read())
                {
                    if (isHeader)
                    {
                        csv.ReadHeader();
                        isHeader = false;
                        continue;
                    }

                    if (string.IsNullOrEmpty(csv.GetField(0)))
                    {
                        isHeader = true;
                        continue;
                    }

                    switch (csv.Context.HeaderRecord[0])
                    {
                        case "ExternalReference":
                            headerRecords.Add(csv.GetRecord<SalesOrderHeader>());
                            break;
                        case "ProductId":
                            lineItemRecords.Add(csv.GetRecord<SalesOrderLine>());
                            break;
                        default:
                            throw new InvalidOperationException("Unknown record type.");
                    }
                }

                SalesOrderList salesOrders = new SalesOrderList();

                foreach (var header in headerRecords)
                {
                    //var provider = new CultureInfo("en-GB");
                    var startDate = DateTime.ParseExact(header.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var endDate = DateTime.ParseExact(header.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    var salesOrder = new SalesOrder();
                    salesOrder.ObjectNodeSenderTechnicalID = header.ExternalReference;
                    salesOrder.Name.Text = header.Name;
                    salesOrder.DataOriginTypeCode = header.DataOriginTypeCode;
                    salesOrder.DeliveryTerms.DeliveryPriorityCode = header.DeliveryPriorityCode;
                    salesOrder.SalesUnitParty.PartyID = header.SalesUnitPartyId;
                    salesOrder.BillToParty.PartyID = header.BuyerPartyId;
                    salesOrder.PricingTerms.GrossAmountIndicator = "false";
                    salesOrder.RequestedFulfillmentPeriodPeriodTerms.StartDateTime.Text = startDate.ToString("yyyy-MM-dd");
                    salesOrder.RequestedFulfillmentPeriodPeriodTerms.EndDateTime.Text = endDate.ToString("yyyy-MM-dd");

                    var lineItems = lineItemRecords.FindAll(x => x.ExternalReference == header.ExternalReference);
                    foreach (var line in lineItems)
                    {
                        int lineItemId = 10;
                        var item = new Models.SalesOrders.Item();
                        item.ID = lineItemId.ToString();
                        item.ProcessingTypeCode = line.ProcessingTypeCode;
                        item.ItemProduct.ProductID = line.ProductId;
                        item.ItemScheduleLine.Quantity.Text = line.Quantity;
                        item.ItemScheduleLine.Quantity.UnitCode = line.QuantityUnitCode;

                        salesOrder.Items.Add(item);
                        lineItemId += 10;
                    }
                    salesOrders.SalesOrders.Add(salesOrder);
                }

                //var soJson = HelperExtensions.SerializeObject(salesOrders.SalesOrders);
                var salesOrderXml = HelperExtensions.SerializeToXml(salesOrders);

                //read createxml file
                string filePath = Path.Combine(HostingEnvironment.ContentRootPath, "xmlfiles/salesOrder.xml");

                //merge xml files
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);
                var nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                nsmgr.AddNamespace("glob", "http://sap.com/xi/SAPGlobal20/Global");

                var element = doc.SelectSingleNode("/soapenv:Envelope/soapenv:Body/glob:SalesOrderBundleMaintainRequest_sync", nsmgr);

                if (element != null)
                {
                    var subDoc = new XmlDocument();
                    subDoc.LoadXml(salesOrderXml);
                    var nsmgr1 = new XmlNamespaceManager(subDoc.NameTable);
                    nsmgr1.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
                    nsmgr1.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    var salesOrderListNodes = subDoc.SelectNodes("//SalesOrderList/SalesOrder", nsmgr1);
                    var sb = new StringBuilder();
                    foreach (XmlNode child in salesOrderListNodes)
                    {
                        sb.AppendLine("<SalesOrder>");
                        sb.AppendLine(child.InnerXml);
                        sb.AppendLine("</SalesOrder>");
                    }
                    var res = sb.ToString();
                    element.InnerXml = sb.ToString();

                    var basicMsgNode = doc.CreateNode(XmlNodeType.Element, "BasicMessageHeader", null);
                    XmlNode salesOrderNode = doc.SelectSingleNode("//SalesOrder");
                    element.InsertBefore(basicMsgNode, salesOrderNode);
                    doc.Save(filePath);

                    StreamReader sr = new StreamReader(filePath);
                    string soapXml = sr.ReadToEnd();
                    sr.Close();

                    var tenantSetting = await _tenantSettingService.GetSetting();
                    UriBuilder urlBuilder = new UriBuilder(tenantSetting.BaseUrl)
                    {
                        Path = "/sap/bc/srt/scs/sap/managesalesorderin5"
                    };
                    var request = new HttpRequestMessage(HttpMethod.Post, urlBuilder.ToString());
                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/xml"));
                    var byteArray = Encoding.ASCII.GetBytes($"{tenantSetting.User}:{tenantSetting.Password}");
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    request.Content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
                    var response = await _client.HttpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStreamAsync();
                        return Tuple.Create((int)response.StatusCode, responseData);
                    }

                    var errorResponse = await response.Content.ReadAsStreamAsync();
                    return Tuple.Create((int)response.StatusCode, errorResponse);
                }
            }

            return null;
        }

        public async Task<Tuple<int, Stream>> UploadPurchaseOrderAsync(MemoryStream memoryStream)
        {
            using (var reader = new StreamReader(memoryStream))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.IgnoreBlankLines = false;
                csv.Configuration.RegisterClassMap<PurchaseOrderHeaderMapping>();
                csv.Configuration.RegisterClassMap<PurchaseOrderLineMapping>();

                var headerRecords = new List<PurchaseOrderHeader>();
                var lineItemRecords = new List<PurchaseOrderLine>();

                var isHeader = true;
                while (csv.Read())
                {
                    if (isHeader)
                    {
                        csv.ReadHeader();
                        isHeader = false;
                        continue;
                    }

                    if (string.IsNullOrEmpty(csv.GetField(0)))
                    {
                        isHeader = true;
                        continue;
                    }

                    switch (csv.Context.HeaderRecord[0])
                    {
                        case "External Reference":
                            headerRecords.Add(csv.GetRecord<PurchaseOrderHeader>());
                            break;
                        case "Product ID":
                            lineItemRecords.Add(csv.GetRecord<PurchaseOrderLine>());
                            break;
                        default:
                            throw new InvalidOperationException("Unknown record type.");
                    }
                }

                PurchaseOrderList purchaseOrders = new PurchaseOrderList();

                foreach (var header in headerRecords)
                {
                    var purchaseOrder = new PurchaseOrder();
                    purchaseOrder.ObjectNodeSenderTechnicalID = header.ExternalReference;
                    purchaseOrder.Name.Text = header.Name;
                    purchaseOrder.CurrencyCode = header.CurrencyCode;
                    purchaseOrder.DeliveryTerms.IncoTerms.ClassificationCode = header.IncotermsCode;
                    purchaseOrder.DeliveryTerms.IncoTerms.TransferLocationName = header.IncotermsLocationName;
                    purchaseOrder.SellerParty.PartyKey.PartyID = header.SupplierId;
                    purchaseOrder.ResponsiblePurchasingUnitParty.PartyKey.PartyID = header.PurchasingUnitId;

                    var lineItems = lineItemRecords.FindAll(x => x.ExternalReference == header.ExternalReference);
                    foreach (var line in lineItems)
                    {
                        int lineItemId = 10;
                        var item = new Models.PurchaseOrders.PO.Item();
                        item.ItemID = lineItemId.ToString();
                        item.Quantity.Text = line.Quantity;
                        item.Quantity.UnitCode = line.QuantityUnitCode;
                        item.ListUnitPrice.Amount.Text = line.ListUnitPriceAmount;
                        item.ListUnitPrice.Amount.CurrencyCode = header.CurrencyCode;
                        item.ShipToLocation.LocationID = line.ShipToLocationId;
                        item.DirectMaterialIndicator = "true";
                        item.ThirdPartyDealIndicator = "false";
                        item.FollowUpDelivery.EmployeeTimeConfirmationRequiredIndicator = "false";
                        item.FollowUpInvoice.EvaluatedReceiptSettlementIndicator = "false";
                        item.FollowUpInvoice.RequirementCode = "01";

                        purchaseOrder.Items.Add(item);
                        lineItemId += 10;
                    }
                    purchaseOrders.PurchaseOrders.Add(purchaseOrder);
                }

                var purchaseOrderXml = HelperExtensions.SerializeToXml(purchaseOrders);

                //read createxml file
                string filePath = Path.Combine(HostingEnvironment.ContentRootPath, "xmlfiles/purchaseOrder.xml");

                //merge xml files
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);
                var nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                nsmgr.AddNamespace("glob", "http://sap.com/xi/SAPGlobal20/Global");

                var element = doc.SelectSingleNode("/soapenv:Envelope/soapenv:Body/glob:PurchaseOrderBundleMaintainRequest_sync", nsmgr);

                if (element != null)
                {
                    var subDoc = new XmlDocument();
                    subDoc.LoadXml(purchaseOrderXml);
                    var nsmgr1 = new XmlNamespaceManager(subDoc.NameTable);
                    nsmgr1.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
                    nsmgr1.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    var purchaseOrderListNodes = subDoc.SelectNodes("//PurchaseOrderList/PurchaseOrder", nsmgr1);
                    var sb = new StringBuilder();
                    foreach (XmlNode child in purchaseOrderListNodes)
                    {
                        sb.AppendLine("<PurchaseOrder>");
                        sb.AppendLine(child.InnerXml);
                        sb.AppendLine("</PurchaseOrder>");
                    }
                    var res = sb.ToString();
                    element.InnerXml = sb.ToString();

                    var basicMsgNode = doc.CreateNode(XmlNodeType.Element, "BasicMessageHeader", null);
                    XmlNode purchaseOrderNode = doc.SelectSingleNode("//PurchaseOrder");
                    element.InsertBefore(basicMsgNode, purchaseOrderNode);
                    doc.Save(filePath);

                    StreamReader sr = new StreamReader(filePath);
                    string soapXml = sr.ReadToEnd();
                    sr.Close();

                    var tenantSetting = await _tenantSettingService.GetSetting();
                    UriBuilder urlBuilder = new UriBuilder(tenantSetting.BaseUrl)
                    {
                        Path = "/sap/bc/srt/scs/sap/managepurchaseorderin"
                    };
                    var request = new HttpRequestMessage(HttpMethod.Post, urlBuilder.ToString());
                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/xml"));
                    var byteArray = Encoding.ASCII.GetBytes($"{tenantSetting.User}:{tenantSetting.Password}");
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    request.Content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
                    var response = await _client.HttpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStreamAsync();
                        return Tuple.Create((int)response.StatusCode, responseData);
                    }

                    var errorResonseData = await response.Content.ReadAsStreamAsync();
                    return Tuple.Create((int)response.StatusCode, errorResonseData);
                }
            }

            return null;
        }

        public async Task<Tuple<int, Stream>> UploadStockTransfer(MemoryStream memoryStream, StockTransferModel model)
        {
            var lineObjectDocId = 1010;
            var stockOrderTransfers = new List<StockTransferModel>();
            using (var reader = new StreamReader(memoryStream))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<StockOrderTransferMapping>();
                stockOrderTransfers.AddRange(csv.GetRecords<StockTransferModel>());
            }

            var itemId = 10;
            CustomerRequirementList customerRequirements = new CustomerRequirementList();
            foreach (var record in stockOrderTransfers)
            {
                var customerRequirement = new CustomerRequirement();
                customerRequirement.ActionCode = "01";
                customerRequirement.ObjectNodeSenderTechnicalID = record.ObjectNodeSenderTechnicalID;
                customerRequirement.ShipFromSiteID = model.ShipFromSiteID;
                customerRequirement.ShipToSiteID = model.ShipToSiteID;
                customerRequirement.CompleteDeliveryRequestedIndicator = model.CompleteDeliveryRequestedIndicator;
                customerRequirement.DeliveryPriorityCode = model.DeliveryPriorityCode;
                customerRequirement.ExternalRquestItem.ObjectNodeSenderTechnicalID = lineObjectDocId.ToString();
                customerRequirement.ExternalRquestItem.ItemID = itemId.ToString();
                customerRequirement.ExternalRquestItem.ProductKey.ProductID = record.ProductID;
                customerRequirement.ExternalRquestItem.ProductKey.ProductIdentifierTypeCode = string.Empty;
                customerRequirement.ExternalRquestItem.ProductKey.ProductTypeCode = string.Empty;
                customerRequirement.ExternalRquestItem.Description.Text = record.Description;
                customerRequirement.ExternalRquestItem.Description.LanguageCode = "EN";
                customerRequirement.ExternalRquestItem.RequestedQuantity.Text = record.RequestedQuantity;
                customerRequirement.ExternalRquestItem.RequestedLocalDateTime.Text = Convert.ToDateTime(record.RequestedLocalDateTime).ToString("s") + "Z";
                customerRequirement.ExternalRquestItem.RequestedLocalDateTime.TimeZoneCode = "UTC";

                customerRequirements.CustomerRequirements.Add(customerRequirement);
                lineObjectDocId += 1;
                itemId += 10;
            }

            var customerRequirementXml = HelperExtensions.SerializeToXml(customerRequirements);

            string filePath = Path.Combine(HostingEnvironment.ContentRootPath, "xmlfiles/customerRequirement.xml");

            //merge xml files
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
            nsmgr.AddNamespace("glob", "http://sap.com/xi/SAPGlobal20/Global");

            var element = doc.SelectSingleNode("/soapenv:Envelope/soapenv:Body/glob:CustReqBundleMaintainRequest_sync", nsmgr);

            if (element != null)
            {
                var subDoc = new XmlDocument();
                subDoc.LoadXml(customerRequirementXml);
                var nsmgr1 = new XmlNamespaceManager(subDoc.NameTable);
                nsmgr1.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
                nsmgr1.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                var customerRequirementListNodes = subDoc.SelectNodes("//CustomerRequirementList/CustomerRequirement", nsmgr1);
                var sb = new StringBuilder();
                foreach (XmlNode child in customerRequirementListNodes)
                {
                    sb.AppendLine("<CustomerRequirement>");
                    sb.AppendLine(child.InnerXml);
                    sb.AppendLine("</CustomerRequirement>");
                }

                element.InnerXml = sb.ToString();

                //var basicMsgNode = doc.CreateNode(XmlNodeType.Element, "BasicMessageHeader", null);
                //XmlNode customerRequirementNode = doc.SelectSingleNode("//CustomerRequirement");
                //element.InsertBefore(basicMsgNode, customerRequirementNode);
                doc.Save(filePath);

                StreamReader sr = new StreamReader(filePath);
                string soapXml = sr.ReadToEnd();
                sr.Close();

                var tenantSetting = await _tenantSettingService.GetSetting();
                UriBuilder urlBuilder = new UriBuilder(tenantSetting.BaseUrl)
                {
                    Path = "/sap/bc/srt/scs/sap/managecustomerrequirementin"
                };
                var request = new HttpRequestMessage(HttpMethod.Post, urlBuilder.ToString());
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/xml"));
                var byteArray = Encoding.ASCII.GetBytes($"{tenantSetting.User}:{tenantSetting.Password}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                request.Content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
                var response = await _client.HttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStreamAsync();
                    return Tuple.Create((int)response.StatusCode, responseData);
                }

                var errorResonseData = await response.Content.ReadAsStreamAsync();
                return Tuple.Create((int)response.StatusCode, errorResonseData);
            }

            return null;
        }

        private string GetDeliveryPriorityValue(string deliveryPriorityCode)
        {
            string code;
            switch (deliveryPriorityCode)
            {
                case "Immediate":
                    code = "1";
                    break;

                case "Urgent":
                    code = "2";
                    break;

                case "Normal":
                    code = "3";
                    break;

                case "Low":
                    code = "7";
                    break;

                default:
                    code = "3";
                    break;
            }

            return code;
        }
    }
}
