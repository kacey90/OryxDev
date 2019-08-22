using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ByDesignServices.Core.Models.SalesOrders
{
    [XmlRoot(ElementName = "Name")]
    public class Name
    {
        [XmlAttribute(AttributeName = "languageCode")]
        public string LanguageCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "BillToParty")]
    public class BillToParty
    {
        [XmlElement(ElementName = "PartyID")]
        public string PartyID { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "SalesUnitParty")]
    public class SalesUnitParty
    {
        [XmlElement(ElementName = "PartyID")]
        public string PartyID { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "StartDateTime")]
    public class StartDateTime
    {
        [XmlAttribute(AttributeName = "timeZoneCode")]
        public string TimeZoneCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "EndDateTime")]
    public class EndDateTime
    {
        [XmlAttribute(AttributeName = "timeZoneCode")]
        public string TimeZoneCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "RequestedFulfillmentPeriodPeriodTerms")]
    public class RequestedFulfillmentPeriodPeriodTerms
    {
        public RequestedFulfillmentPeriodPeriodTerms()
        {
            StartDateTime = new StartDateTime();
            EndDateTime = new EndDateTime();
        }
        [XmlElement(ElementName = "StartDateTime")]
        public StartDateTime StartDateTime { get; set; }
        [XmlElement(ElementName = "EndDateTime")]
        public EndDateTime EndDateTime { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "DeliveryTerms")]
    public class DeliveryTerms
    {
        [XmlElement(ElementName = "DeliveryPriorityCode")]
        public string DeliveryPriorityCode { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "PricingTerms")]
    public class PricingTerms
    {
        [XmlElement(ElementName = "GrossAmountIndicator")]
        public string GrossAmountIndicator { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "ItemProduct")]
    public class ItemProduct
    {
        [XmlElement(ElementName = "ProductID")]
        public string ProductID { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "Quantity")]
    public class Quantity
    {
        [XmlAttribute(AttributeName = "unitCode")]
        public string UnitCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "ItemScheduleLine")]
    public class ItemScheduleLine
    {
        public ItemScheduleLine()
        {
            Quantity = new Quantity();
        }
        [XmlElement(ElementName = "Quantity")]
        public Quantity Quantity { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "Item")]
    public class Item
    {
        public Item()
        {
            ItemProduct = new ItemProduct();
            ItemScheduleLine = new ItemScheduleLine();
        }
        [XmlElement(ElementName = "ObjectNodeSenderTechnicalID")]
        public string ObjectNodeSenderTechnicalID { get; set; }
        [XmlElement(ElementName = "ID")]
        public string ID { get; set; }
        [XmlElement(ElementName = "ProcessingTypeCode")]
        public string ProcessingTypeCode { get; set; }
        [XmlElement(ElementName = "ItemProduct")]
        public ItemProduct ItemProduct { get; set; }
        [XmlElement(ElementName = "ItemScheduleLine")]
        public ItemScheduleLine ItemScheduleLine { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "SalesOrder")]
    public class SalesOrder
    {
        public SalesOrder()
        {
            Name = new Name();
            BillToParty = new BillToParty();
            SalesUnitParty = new SalesUnitParty();
            RequestedFulfillmentPeriodPeriodTerms = new RequestedFulfillmentPeriodPeriodTerms();
            DeliveryTerms = new DeliveryTerms();
            PricingTerms = new PricingTerms();
            Items = new List<Item>();
        }
        [XmlElement(ElementName = "ObjectNodeSenderTechnicalID")]
        public string ObjectNodeSenderTechnicalID { get; set; }
        [XmlElement(ElementName = "ID")]
        public string ID { get; set; }
        [XmlElement(ElementName = "BuyerID")]
        public string BuyerID { get; set; }
        [XmlElement(ElementName = "PostingDate")]
        public string PostingDate { get; set; }
        [XmlElement(ElementName = "Name")]
        public Name Name { get; set; }
        [XmlElement(ElementName = "DataOriginTypeCode")]
        public string DataOriginTypeCode { get; set; }
        [XmlElement(ElementName = "BillToParty")]
        public BillToParty BillToParty { get; set; }
        [XmlElement(ElementName = "SalesUnitParty")]
        public SalesUnitParty SalesUnitParty { get; set; }
        [XmlElement(ElementName = "RequestedFulfillmentPeriodPeriodTerms")]
        public RequestedFulfillmentPeriodPeriodTerms RequestedFulfillmentPeriodPeriodTerms { get; set; }
        [XmlElement(ElementName = "DeliveryTerms")]
        public DeliveryTerms DeliveryTerms { get; set; }
        [XmlElement(ElementName = "PricingTerms")]
        public PricingTerms PricingTerms { get; set; }
        [XmlElement(ElementName = "Item")]
        public List<Item> Items { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    public class SalesOrderList
    {
        public SalesOrderList()
        {
            SalesOrders = new List<SalesOrder>();
        }

        [XmlElement("SalesOrder")]
        public List<SalesOrder> SalesOrders { get; set; }
    }
}
