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

    [XmlRoot(ElementName = "AccountParty")]
    public class AccountParty
    {
        [XmlElement(ElementName = "PartyID")]
        public string PartyID { get; set; }
    }

    [XmlRoot(ElementName = "BillToParty")]
    public class BillToParty
    {
        [XmlElement(ElementName = "PartyID")]
        public string PartyID { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "EmployeeResponsibleParty")]
    public class EmployeeResponsibleParty
    {
        [XmlElement(ElementName = "PartyID")]
        public string PartyID { get; set; }
        [XmlAttribute(AttributeName = "partyContactPartyListCompleteTransmissionIndicator")]
        public string PartyContactPartyListCompleteTransmissionIndicator { get; set; }
    }

    [XmlRoot(ElementName = "SalesUnitParty")]
    public class SalesUnitParty
    {
        [XmlElement(ElementName = "PartyID")]
        public string PartyID { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "SalesAndServiceBusinessArea")]
    public class SalesAndServiceBusinessArea
    {
        [XmlElement(ElementName = "DistributionChannelCode")]
        public string DistributionChannelCode { get; set; }
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

    [XmlRoot(ElementName = "LocationID")]
    public class LocationID
    {
        [XmlAttribute(AttributeName = "schemeID")]
        public string SchemeID { get; set; }
        [XmlAttribute(AttributeName = "schemeAgencyID")]
        public string SchemeAgencyID { get; set; }
        [XmlAttribute(AttributeName = "schemeAgencySchemeID")]
        public string SchemeAgencySchemeID { get; set; }
        [XmlAttribute(AttributeName = "schemeAgencySchemeAgencyID")]
        public string SchemeAgencySchemeAgencyID { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "ShipFromItemLocation")]
    public class ShipFromItemLocation
    {
        public ShipFromItemLocation()
        {
            LocationID = new LocationID();
        }
        [XmlElement(ElementName = "LocationID")]
        public LocationID LocationID { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "TaxationCharacteristicsCode")]
    public class TaxationCharacteristicsCode
    {
        [XmlAttribute(AttributeName = "listID")]
        public string ListID { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "PriceAndTaxCalculationItem")]
    public class PriceAndTaxCalculationItem
    {
        public PriceAndTaxCalculationItem()
        {
            TaxationCharacteristicsCode = new TaxationCharacteristicsCode();
        }
        [XmlElement(ElementName = "TaxationCharacteristicsCode")]
        public TaxationCharacteristicsCode TaxationCharacteristicsCode { get; set; }
    }

    [XmlRoot(ElementName = "Item")]
    public class Item
    {
        public Item()
        {
            ItemProduct = new ItemProduct();
            ItemScheduleLine = new ItemScheduleLine();
            ShipFromItemLocation = new ShipFromItemLocation();
            PriceAndTaxCalculationItem = new PriceAndTaxCalculationItem();
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
        [XmlElement(ElementName = "ShipFromItemLocation")]
        public ShipFromItemLocation ShipFromItemLocation { get; set; }
        [XmlElement(ElementName = "PriceAndTaxCalculationItem")]
        public PriceAndTaxCalculationItem PriceAndTaxCalculationItem { get; set; }
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
            AccountParty = new AccountParty();
            SalesUnitParty = new SalesUnitParty();
            EmployeeResponsibleParty = new EmployeeResponsibleParty();
            RequestedFulfillmentPeriodPeriodTerms = new RequestedFulfillmentPeriodPeriodTerms();
            SalesAndServiceBusinessArea = new SalesAndServiceBusinessArea();
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
        [XmlElement(ElementName = "AccountParty")]
        public AccountParty AccountParty { get; set; }
        [XmlElement(ElementName = "BillToParty")]
        public BillToParty BillToParty { get; set; }
        [XmlElement(ElementName = "SalesUnitParty")]
        public SalesUnitParty SalesUnitParty { get; set; }
        [XmlElement(ElementName = "SalesAndServiceBusinessArea")]
        public SalesAndServiceBusinessArea SalesAndServiceBusinessArea { get; set; }
        [XmlElement(ElementName = "EmployeeResponsibleParty")]
        public EmployeeResponsibleParty EmployeeResponsibleParty { get; set; }
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
