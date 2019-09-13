using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ByDesignServices.Core.Models.SalesQuotes
{
    public class SalesQuote
    {
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
            [XmlAttribute(AttributeName = "partyContactPartyListCompleteTransmissionIndicator")]
            public string PartyContactPartyListCompleteTransmissionIndicator { get; set; }
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

        [XmlRoot(ElementName = "RequestedFulfillmentPeriodPeriodTerms")]
        public class RequestedFulfillmentPeriodPeriodTerms
        {
            public RequestedFulfillmentPeriodPeriodTerms()
            {
                StartDateTime = new StartDateTime();
            }
            [XmlElement(ElementName = "StartDateTime")]
            public StartDateTime StartDateTime { get; set; }
        }

        [XmlRoot(ElementName = "ItemProduct")]
        public class ItemProduct
        {
            [XmlElement(ElementName = "ProductInternalID")]
            public string ProductInternalID { get; set; }
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

        [XmlRoot(ElementName = "Items")]
        public class Items
        {
            public Items()
            {
                ItemProduct = new ItemProduct();
                ItemScheduleLine = new ItemScheduleLine();
                PriceAndTaxCalculationItem = new PriceAndTaxCalculationItem();
            }
            [XmlElement(ElementName = "ID")]
            public string ID { get; set; }
            [XmlElement(ElementName = "ItemProduct")]
            public ItemProduct ItemProduct { get; set; }
            [XmlElement(ElementName = "ItemScheduleLine")]
            public ItemScheduleLine ItemScheduleLine { get; set; }
            [XmlElement(ElementName = "PriceAndTaxCalculationItem")]
            public PriceAndTaxCalculationItem PriceAndTaxCalculationItem { get; set; }
        }

        [XmlRoot(ElementName = "CustomerQuote")]
        public class CustomerQuote
        {
            public CustomerQuote()
            {
                AccountParty = new AccountParty();
                BillToParty = new BillToParty();
                EmployeeResponsibleParty = new EmployeeResponsibleParty();
                SalesUnitParty = new SalesUnitParty();
                SalesAndServiceBusinessArea = new SalesAndServiceBusinessArea();
                RequestedFulfillmentPeriodPeriodTerms = new RequestedFulfillmentPeriodPeriodTerms();
                Items = new List<Items>();
            }
            [XmlElement(ElementName = "ObjectNodeSenderTechnicalID")]
            public string ObjectNodeSenderTechnicalID { get; set; }
            [XmlElement(ElementName = "BuyerID")]
            public string BuyerID { get; set; }
            [XmlElement(ElementName = "Name")]
            public string Name { get; set; }
            [XmlElement(ElementName = "PostingDate")]
            public string PostingDate { get; set; }
            [XmlElement(ElementName = "DataOriginTypeCode")]
            public string DataOriginTypeCode { get; set; }
            [XmlElement(ElementName = "AccountParty")]
            public AccountParty AccountParty { get; set; }
            [XmlElement(ElementName = "BillToParty")]
            public BillToParty BillToParty { get; set; }
            [XmlElement(ElementName = "EmployeeResponsibleParty")]
            public EmployeeResponsibleParty EmployeeResponsibleParty { get; set; }
            [XmlElement(ElementName = "SalesUnitParty")]
            public SalesUnitParty SalesUnitParty { get; set; }
            [XmlElement(ElementName = "SalesAndServiceBusinessArea")]
            public SalesAndServiceBusinessArea SalesAndServiceBusinessArea { get; set; }
            [XmlElement(ElementName = "RequestedFulfillmentPeriodPeriodTerms")]
            public RequestedFulfillmentPeriodPeriodTerms RequestedFulfillmentPeriodPeriodTerms { get; set; }
            [XmlElement(ElementName = "Items")]
            public List<Items> Items { get; set; }
            [XmlAttribute(AttributeName = "itemListCompleteTransmissionIndicator")]
            public string ItemListCompleteTransmissionIndicator { get; set; }
            [XmlAttribute(AttributeName = "actionCode")]
            public string ActionCode { get; set; }
        }

    }
}
