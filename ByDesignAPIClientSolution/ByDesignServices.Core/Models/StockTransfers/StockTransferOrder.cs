using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ByDesignServices.Core.Models.StockTransfers
{
    [XmlRoot(ElementName = "ProductKey")]
    public class ProductKey
    {
        [XmlElement(ElementName = "ProductTypeCode")]
        public string ProductTypeCode { get; set; }
        [XmlElement(ElementName = "ProductIdentifierTypeCode")]
        public string ProductIdentifierTypeCode { get; set; }
        [XmlElement(ElementName = "ProductID")]
        public string ProductID { get; set; }
    }

    [XmlRoot(ElementName = "RequestedQuantity")]
    public class RequestedQuantity
    {
        [XmlAttribute(AttributeName = "unitCode")]
        public string UnitCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "RequestedLocalDateTime")]
    public class RequestedLocalDateTime
    {
        [XmlAttribute(AttributeName = "timeZoneCode")]
        public string TimeZoneCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Description")]
    public class Description
    {
        [XmlAttribute(AttributeName = "languageCode")]
        public string LanguageCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "ExternalRquestItem")]
    public class ExternalRquestItem
    {
        public ExternalRquestItem()
        {
            ProductKey = new ProductKey();
            RequestedQuantity = new RequestedQuantity();
            RequestedLocalDateTime = new RequestedLocalDateTime();
            Description = new Description();
        }
        [XmlElement(ElementName = "ObjectNodeSenderTechnicalID")]
        public string ObjectNodeSenderTechnicalID { get; set; }
        [XmlElement(ElementName = "ItemID")]
        public string ItemID { get; set; }
        [XmlElement(ElementName = "ProductKey")]
        public ProductKey ProductKey { get; set; }
        [XmlElement(ElementName = "RequestedQuantity")]
        public RequestedQuantity RequestedQuantity { get; set; }
        [XmlElement(ElementName = "RequestedLocalDateTime")]
        public RequestedLocalDateTime RequestedLocalDateTime { get; set; }
        [XmlElement(ElementName = "Description")]
        public Description Description { get; set; }
        [XmlAttribute(AttributeName = "ActionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "CustomerRequirement")]
    public class CustomerRequirement
    {
        public CustomerRequirement()
        {
            ExternalRquestItem = new ExternalRquestItem();
        }
        [XmlElement(ElementName = "ObjectNodeSenderTechnicalID")]
        public string ObjectNodeSenderTechnicalID { get; set; }
        [XmlElement(ElementName = "ShipFromSiteID")]
        public string ShipFromSiteID { get; set; }
        [XmlElement(ElementName = "ShipToSiteID")]
        public string ShipToSiteID { get; set; }
        [XmlElement(ElementName = "ShipToLocationID")]
        public string ShipToLocationID { get; set; }
        [XmlElement(ElementName = "CompleteDeliveryRequestedIndicator")]
        public string CompleteDeliveryRequestedIndicator { get; set; }
        [XmlElement(ElementName = "DeliveryPriorityCode")]
        public string DeliveryPriorityCode { get; set; }
        [XmlElement(ElementName = "ExternalRquestItem")]
        public ExternalRquestItem ExternalRquestItem { get; set; }
        [XmlAttribute(AttributeName = "ActionCode")]
        public string ActionCode { get; set; }
    }

    public class CustomerRequirementList
    {
        public CustomerRequirementList()
        {
            CustomerRequirements = new List<CustomerRequirement>();
        }

        [XmlElement("CustomerRequirement")]
        public List<CustomerRequirement> CustomerRequirements { get; set; }
    }
}
