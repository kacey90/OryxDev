using ByDesignServices.Core.Extensions;
using ByDesignServices.Core.Models.CsvMapping;
using ByDesignServices.Core.Models.PurchaseOrders;
using ByDesignServices.Core.Models.PurchaseOrders.PO;
using ByDesignServices.Core.Models.SalesOrders;
using ByDesignServices.Core.Models.SalesQuotes;
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
using static ByDesignServices.Core.Models.SalesQuotes.SalesQuote;

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




        public async Task<Stream> UploadSalesOrderAsync(MemoryStream memoryStream)
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
                            csv.Configuration.RegisterClassMap<BulkSalesOrderLineMapping>();
                            lineItemRecords.AddRange(csv.GetRecords<SalesOrderLine>());
                        }
                    }
                }

                SalesOrderList salesOrders = new SalesOrderList();

                foreach (var header in headerRecords)
                {
                    //var provider = new CultureInfo("en-GB");
                    var startDate = DateTime.ParseExact(header.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var postingDate = DateTime.ParseExact(header.PostingDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    var salesOrder = new SalesOrder();
                    salesOrder.ObjectNodeSenderTechnicalID = header.ExternalReference;
                    salesOrder.Name.Text = header.Name;
                    salesOrder.DataOriginTypeCode = "4";
                    salesOrder.DeliveryTerms.DeliveryPriorityCode = GetDeliveryPriorityValue(header.DeliveryPriorityCode);
                    salesOrder.SalesAndServiceBusinessArea.DistributionChannelCode = GetDistributionChannelValue(header.DistributionChannelCode);
                    salesOrder.SalesUnitParty.PartyID = header.SalesUnitPartyId;
                    salesOrder.BillToParty.PartyID = header.BuyerPartyId;
                    salesOrder.AccountParty.PartyID = header.BuyerPartyId;
                    salesOrder.PricingTerms.GrossAmountIndicator = "false";
                    salesOrder.RequestedFulfillmentPeriodPeriodTerms.StartDateTime.Text = Convert.ToDateTime(startDate).ToString("s") + "Z";
                    salesOrder.RequestedFulfillmentPeriodPeriodTerms.StartDateTime.TimeZoneCode = "UTC+1";
                    salesOrder.PostingDate = Convert.ToDateTime(postingDate).ToString("s") + "Z";
                    salesOrder.EmployeeResponsibleParty.PartyID = header.EmployeeResponsible;
                    //salesOrder.RequestedFulfillmentPeriodPeriodTerms.EndDateTime.Text = endDate.ToString("yyyy-MM-dd");

                    var lineItems = lineItemRecords.FindAll(x => x.ExternalReference == header.ExternalReference);
                    foreach (var line in lineItems)
                    {
                        int lineItemId = 10;
                        var item = new Models.SalesOrders.Item();
                        item.ID = lineItemId.ToString();
                        item.ProcessingTypeCode = "TAN";
                        item.ItemProduct.ProductID = line.ProductId;
                        item.ItemScheduleLine.Quantity.Text = line.Quantity;
                        item.ItemScheduleLine.Quantity.UnitCode = line.QuantityUnitCode;
                        item.PriceAndTaxCalculationItem.TaxationCharacteristicsCode.Text = line.TaxCode;
                        item.PriceAndTaxCalculationItem.TaxationCharacteristicsCode.ListID = "1";
                        item.ShipFromItemLocation.LocationID.Text = line.ShipFromLocationId;

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
                        return responseData;
                    }
                }
            }

            return null;
        }

        public async Task<Stream> PostSalesOrder(MemoryStream memoryStream, SalesOrderHeader model)
        {
            var lineItemRecords = new List<SalesOrderLine>();
            var startDate = DateTime.ParseExact(model.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            //var endDate = DateTime.ParseExact(model.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var salesOrder = new SalesOrder();
            salesOrder.ObjectNodeSenderTechnicalID = model.ExternalReference;
            salesOrder.Name.Text = model.Name;
            salesOrder.BuyerID = model.ExternalReference;
            salesOrder.DataOriginTypeCode = model.DataOriginTypeCode;
            salesOrder.DeliveryTerms.DeliveryPriorityCode = model.DeliveryPriorityCode;
            salesOrder.SalesAndServiceBusinessArea.DistributionChannelCode = model.DistributionChannelCode;
            salesOrder.SalesUnitParty.PartyID = model.SalesUnitPartyId;
            salesOrder.AccountParty.PartyID = model.AccountId;
            salesOrder.BillToParty.PartyID = model.AccountId;
            salesOrder.EmployeeResponsibleParty.PartyID = model.EmployeeResponsible;
            salesOrder.PostingDate = Convert.ToDateTime(model.PostingDate).ToString("s") + "Z";
            salesOrder.PricingTerms.GrossAmountIndicator = "false";
            salesOrder.RequestedFulfillmentPeriodPeriodTerms.StartDateTime.Text = Convert.ToDateTime(model.StartDate).ToString("s") + "Z";
            salesOrder.RequestedFulfillmentPeriodPeriodTerms.StartDateTime.TimeZoneCode = "UTC+1";
            //salesOrder.RequestedFulfillmentPeriodPeriodTerms.EndDateTime.Text = endDate.ToString("yyyy-MM-dd");

            using (var reader = new StreamReader(memoryStream))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<SalesOrderLineMapping>();
                lineItemRecords.AddRange(csv.GetRecords<SalesOrderLine>());
                int lineItemId = 10;
                foreach (var lineItem in lineItemRecords)
                {
                    var item = new Models.SalesOrders.Item();
                    item.ID = lineItemId.ToString();
                    item.ProcessingTypeCode = "TAN";
                    item.ShipFromItemLocation.LocationID.Text = lineItem.ShipFromLocationId;
                    //item.ShipFromItemLocation.ActionCode = "01";
                    item.ItemProduct.ProductID = lineItem.ProductId;
                    item.ItemScheduleLine.Quantity.Text = lineItem.Quantity;
                    item.ItemScheduleLine.Quantity.UnitCode = lineItem.QuantityUnitCode;
                    item.PriceAndTaxCalculationItem.TaxationCharacteristicsCode.Text = lineItem.TaxCode;
                    item.PriceAndTaxCalculationItem.TaxationCharacteristicsCode.ListID = "1";

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
                    var salesOrderNode = subDoc.SelectSingleNode("//SalesOrder", nsmgr1);
                    var sb = new StringBuilder();
                    sb.AppendLine("<SalesOrder>");
                    sb.AppendLine(salesOrderNode.InnerXml);
                    sb.AppendLine("</SalesOrder>");
                    element.InnerXml = sb.ToString();

                    

                    var basicMsgNode = doc.CreateNode(XmlNodeType.Element, "BasicMessageHeader", null);
                    XmlNode salesOrderNode2 = doc.SelectSingleNode("//SalesOrder");
                    element.InsertBefore(basicMsgNode, salesOrderNode2);
                    //XmlAttribute attribute = salesOrderNode2.OwnerDocument.CreateAttribute("actionCode");
                    //attribute.Value = "01";
                    //salesOrderNode2.Attributes.Append(attribute);
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
                        return responseData;
                    }
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

        public async Task<Stream> UploadStockTransfer(MemoryStream memoryStream, StockTransferModel model)
        {
            var lineObjectDocId = 1010;
            var stockOrderTransfers = new List<StockTransferModel>();
            using (var reader = new StreamReader(memoryStream))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<StockOrderTransferMapping>();
                stockOrderTransfers.AddRange(csv.GetRecords<StockTransferModel>());
            }

            List<SalesQuoteLine> quotesLineItems = new List<SalesQuoteLine>();
            var itemId = 10;
            CustomerRequirementList customerRequirements = new CustomerRequirementList();
            foreach (var record in stockOrderTransfers)
            {
                var customerRequirement = new CustomerRequirement();
                customerRequirement.ActionCode = "01";
                customerRequirement.ObjectNodeSenderTechnicalID = record.ObjectNodeSenderTechnicalID;
                customerRequirement.ShipFromSiteID = model.ShipFromSiteID;
                customerRequirement.ShipToSiteID = model.ShipToSiteID;
                customerRequirement.ShipToLocationID = model.ShipToLocationID;
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
                customerRequirement.ExternalRquestItem.RequestedQuantity.UnitCode = "XCT";
                customerRequirement.ExternalRquestItem.RequestedLocalDateTime.Text = Convert.ToDateTime(record.RequestedLocalDateTime).ToString("s") + "Z";
                customerRequirement.ExternalRquestItem.RequestedLocalDateTime.TimeZoneCode = "UTC+1";

                customerRequirements.CustomerRequirements.Add(customerRequirement);
                lineObjectDocId += 1;
                itemId += 10;

                if (model.RaiseSalesQuote)
                {
                    quotesLineItems.Add(new SalesQuoteLine
                    {
                        ExternalReference = model.ExternalReference,
                        ProductId = record.ProductID,
                        Quantity = record.RequestedQuantity,
                        TaxCode = record.TaxCode
                    });
                }
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

                var customerReqNodes = doc.SelectNodes("//CustomerRequirement");
                foreach(XmlNode node in customerReqNodes)
                {
                    XmlAttribute attribute = node.OwnerDocument.CreateAttribute("ActionCode");
                    attribute.Value = "01";
                    node.Attributes.Append(attribute);
                }

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
                    if (quotesLineItems.Count > 0)
                    {
                        var salesQuoteRequest = await PostSalesQuote(new SalesQuoteHeader
                        {
                            AccountId = model.AccountId,
                            Description = model.Description,
                            DistributionChannelCode = model.DistributionChannelCode,
                            EmployeeResponsible = model.EmployeeResponsible,
                            ExternalReference = model.ExternalReference,
                            PostingDate = model.PostingDate.ToString(),
                            RequestedDate = model.RequestedDate.Value.ToString(),
                            SalesUnitId = model.SalesUnitId
                        }, quotesLineItems);

                        return salesQuoteRequest;
                    }
                    return responseData;
                }
            }

            return null;
        }

        public async Task<Stream> PostSalesQuote(SalesQuoteHeader model, List<SalesQuoteLine> lineItems)
        {
            var customerQuote = new CustomerQuote();

            customerQuote.ObjectNodeSenderTechnicalID = model.ExternalReference;
            customerQuote.BuyerID = model.ExternalReference;
            customerQuote.Name = model.Description;
            customerQuote.PostingDate = Convert.ToDateTime(model.PostingDate).ToString("s") + "Z";
            customerQuote.DataOriginTypeCode = "4";
            customerQuote.AccountParty.PartyID = model.AccountId;
            customerQuote.BillToParty.PartyID = model.AccountId;
            customerQuote.EmployeeResponsibleParty.PartyID = model.EmployeeResponsible;
            customerQuote.SalesUnitParty.PartyID = model.SalesUnitId;
            customerQuote.SalesAndServiceBusinessArea.DistributionChannelCode = model.DistributionChannelCode;
            customerQuote.RequestedFulfillmentPeriodPeriodTerms.StartDateTime.Text = Convert.ToDateTime(model.RequestedDate).ToString("s") + "Z";
            customerQuote.RequestedFulfillmentPeriodPeriodTerms.StartDateTime.TimeZoneCode = "UTC+1";

            var lineId = 10;
            foreach (var lineItem in lineItems)
            {
                var item = new Items();
                item.ID = lineId.ToString();
                item.ItemProduct.ProductInternalID = lineItem.ProductId;
                item.ItemScheduleLine.Quantity.Text = lineItem.Quantity;
                item.ItemScheduleLine.Quantity.UnitCode = "XCT";
                item.PriceAndTaxCalculationItem.TaxationCharacteristicsCode.Text = lineItem.TaxCode;
                item.PriceAndTaxCalculationItem.TaxationCharacteristicsCode.ListID = "1";

                customerQuote.Items.Add(item);
                lineId += 10;
            }

            var customerQuoteXml = HelperExtensions.SerializeToXml(customerQuote);

            string filePath = Path.Combine(HostingEnvironment.ContentRootPath, "xmlfiles/salesQuote.xml");

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
            nsmgr.AddNamespace("glob", "http://sap.com/xi/SAPGlobal20/Global");

            var element = doc.SelectSingleNode("/soapenv:Envelope/soapenv:Body/glob:CustomerQuoteBundleMaintainRequest_sync", nsmgr);

            if (element != null)
            {
                var subDoc = new XmlDocument();
                subDoc.LoadXml(customerQuoteXml);
                var nsmgr1 = new XmlNamespaceManager(subDoc.NameTable);
                nsmgr1.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
                nsmgr1.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

                var cqNode = subDoc.SelectSingleNode("//CustomerQuote", nsmgr1);
                var sb = new StringBuilder();
                sb.AppendLine("<CustomerQuote>");
                sb.AppendLine(cqNode.InnerXml);
                sb.AppendLine("</CustomerQuote>");
                element.InnerXml = sb.ToString();

                var sqItemNodes = doc.SelectNodes("//CustomerQuote/Items");
                foreach (XmlNode node in sqItemNodes)
                {
                    XmlAttribute attribute = node.OwnerDocument.CreateAttribute("itemScheduleLineListCompleteTransmissionIndicator");
                    attribute.Value = "true";
                    node.Attributes.Append(attribute);
                }

                var basicMsgNode = doc.CreateNode(XmlNodeType.Element, "BasicMessageHeader", null);
                XmlNode customerQuoteNode = doc.SelectSingleNode("//CustomerQuote");
                element.InsertBefore(basicMsgNode, customerQuoteNode);
                doc.Save(filePath);

                StreamReader sr = new StreamReader(filePath);
                string soapXml = sr.ReadToEnd();
                sr.Close();

                var tenantSetting = await _tenantSettingService.GetSetting();
                UriBuilder urlBuilder = new UriBuilder(tenantSetting.BaseUrl)
                {
                    Path = "/sap/bc/srt/scs/sap/managecustomerquotein"
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
                    return responseData;
                }
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

        private string GetDistributionChannelValue(string name)
        {
            string code;
            switch (name)
            {
                case "Direct Sales":
                    code = "01";
                    break;

                case "Indirect Sales":
                    code = "02";
                    break;

                default:
                    code = "01";
                    break;
            }

            return code;
        }
    }
}
