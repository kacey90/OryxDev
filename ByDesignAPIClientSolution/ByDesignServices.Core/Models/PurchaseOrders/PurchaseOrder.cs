using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ByDesignServices.Core.Models.PurchaseOrders.PO
{
    [XmlRoot(ElementName = "Name")]
    public class Name
    {
        [XmlAttribute(AttributeName = "languageCode")]
        public string LanguageCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "PartyKey")]
    public class PartyKey
    {
        [XmlElement(ElementName = "PartyID")]
        public string PartyID { get; set; }
    }

    [XmlRoot(ElementName = "SellerParty")]
    public class SellerParty
    {
        public SellerParty()
        {
            PartyKey = new PartyKey();
        }
        [XmlElement(ElementName = "PartyKey")]
        public PartyKey PartyKey { get; set; }
    }

    [XmlRoot(ElementName = "ResponsiblePurchasingUnitParty")]
    public class ResponsiblePurchasingUnitParty
    {
        public ResponsiblePurchasingUnitParty()
        {
            PartyKey = new PartyKey();
        }
        [XmlElement(ElementName = "PartyKey")]
        public PartyKey PartyKey { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "IncoTerms")]
    public class IncoTerms
    {
        [XmlElement(ElementName = "ClassificationCode")]
        public string ClassificationCode { get; set; }
        [XmlElement(ElementName = "TransferLocationName")]
        public string TransferLocationName { get; set; }
    }

    [XmlRoot(ElementName = "DeliveryTerms")]
    public class DeliveryTerms
    {
        public DeliveryTerms()
        {
            IncoTerms = new IncoTerms();
        }
        [XmlElement(ElementName = "IncoTerms")]
        public IncoTerms IncoTerms { get; set; }
        [XmlAttribute(AttributeName = "ActionCode")]
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

    [XmlRoot(ElementName = "Description")]
    public class Description
    {
        [XmlAttribute(AttributeName = "languageCode")]
        public string LanguageCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Amount")]
    public class Amount
    {
        [XmlAttribute(AttributeName = "currencyCode")]
        public string CurrencyCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "BaseQuantity")]
    public class BaseQuantity
    {
        [XmlAttribute(AttributeName = "unitCode")]
        public string UnitCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "NetUnitPrice")]
    public class NetUnitPrice
    {
        public NetUnitPrice()
        {
            Amount = new Amount();
            BaseQuantity = new BaseQuantity();
        }
        [XmlElement(ElementName = "Amount")]
        public Amount Amount { get; set; }
        [XmlElement(ElementName = "BaseQuantity")]
        public BaseQuantity BaseQuantity { get; set; }
        [XmlElement(ElementName = "BaseQuantityTypeCode")]
        public string BaseQuantityTypeCode { get; set; }
    }

    [XmlRoot(ElementName = "GrossAmount")]
    public class GrossAmount
    {
        [XmlAttribute(AttributeName = "currencyCode")]
        public string CurrencyCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "GrossUnitPrice")]
    public class GrossUnitPrice
    {
        public GrossUnitPrice()
        {
            Amount = new Amount();
            BaseQuantity = new BaseQuantity();
        }
        [XmlElement(ElementName = "Amount")]
        public Amount Amount { get; set; }
        [XmlElement(ElementName = "BaseQuantity")]
        public BaseQuantity BaseQuantity { get; set; }
        [XmlElement(ElementName = "BaseQuantityTypeCode")]
        public string BaseQuantityTypeCode { get; set; }
    }

    [XmlRoot(ElementName = "ListUnitPrice")]
    public class ListUnitPrice
    {
        public ListUnitPrice()
        {
            Amount = new Amount();
            BaseQuantity = new BaseQuantity();
        }
        [XmlElement(ElementName = "Amount")]
        public Amount Amount { get; set; }
        [XmlElement(ElementName = "BaseQuantity")]
        public BaseQuantity BaseQuantity { get; set; }
        [XmlElement(ElementName = "BaseQuantityTypeCode")]
        public string BaseQuantityTypeCode { get; set; }
    }

    [XmlRoot(ElementName = "FollowUpPurchaseOrderConfirmation")]
    public class FollowUpPurchaseOrderConfirmation
    {
        [XmlElement(ElementName = "RequirementCode")]
        public string RequirementCode { get; set; }
    }

    [XmlRoot(ElementName = "FollowUpDelivery")]
    public class FollowUpDelivery
    {
        [XmlElement(ElementName = "RequirementCode")]
        public string RequirementCode { get; set; }
        [XmlElement(ElementName = "EmployeeTimeConfirmationRequiredIndicator")]
        public string EmployeeTimeConfirmationRequiredIndicator { get; set; }
    }

    [XmlRoot(ElementName = "FollowUpInvoice")]
    public class FollowUpInvoice
    {
        [XmlElement(ElementName = "BusinessTransactionDocumentSettlementRelevanceIndicator")]
        public string BusinessTransactionDocumentSettlementRelevanceIndicator { get; set; }
        [XmlElement(ElementName = "RequirementCode")]
        public string RequirementCode { get; set; }
        [XmlElement(ElementName = "EvaluatedReceiptSettlementIndicator")]
        public string EvaluatedReceiptSettlementIndicator { get; set; }
        [XmlElement(ElementName = "DeliveryBasedInvoiceVerificationIndicator")]
        public string DeliveryBasedInvoiceVerificationIndicator { get; set; }
    }

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

    [XmlRoot(ElementName = "ItemProduct")]
    public class ItemProduct
    {
        public ItemProduct()
        {
            ProductKey = new ProductKey();
        }
        [XmlElement(ElementName = "CashDiscountDeductibleIndicator")]
        public string CashDiscountDeductibleIndicator { get; set; }
        [XmlElement(ElementName = "ProductKey")]
        public ProductKey ProductKey { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "ShipToLocation")]
    public class ShipToLocation
    {
        [XmlElement(ElementName = "LocationID")]
        public string LocationID { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "Item")]
    public class Item
    {
        public Item()
        {
            Quantity = new Quantity();
            Description = new Description();
            NetUnitPrice = new NetUnitPrice();
            GrossAmount = new GrossAmount();
            GrossUnitPrice = new GrossUnitPrice();
            ListUnitPrice = new ListUnitPrice();
            FollowUpDelivery = new FollowUpDelivery();
            FollowUpInvoice = new FollowUpInvoice();
            ItemProduct = new ItemProduct();
            ShipToLocation = new ShipToLocation();
        }
        [XmlElement(ElementName = "ObjectNodeSenderTechnicalID")]
        public string ObjectNodeSenderTechnicalID { get; set; }
        [XmlElement(ElementName = "ItemID")]
        public string ItemID { get; set; }
        [XmlElement(ElementName = "Quantity")]
        public Quantity Quantity { get; set; }
        [XmlElement(ElementName = "Description")]
        public Description Description { get; set; }
        [XmlElement(ElementName = "NetUnitPrice")]
        public NetUnitPrice NetUnitPrice { get; set; }
        [XmlElement(ElementName = "GrossAmount")]
        public GrossAmount GrossAmount { get; set; }
        [XmlElement(ElementName = "GrossUnitPrice")]
        public GrossUnitPrice GrossUnitPrice { get; set; }
        [XmlElement(ElementName = "ListUnitPrice")]
        public ListUnitPrice ListUnitPrice { get; set; }
        [XmlElement(ElementName = "DirectMaterialIndicator")]
        public string DirectMaterialIndicator { get; set; }
        [XmlElement(ElementName = "ThirdPartyDealIndicator")]
        public string ThirdPartyDealIndicator { get; set; }
        [XmlElement(ElementName = "FollowUpPurchaseOrderConfirmation")]
        public FollowUpPurchaseOrderConfirmation FollowUpPurchaseOrderConfirmation { get; set; }
        [XmlElement(ElementName = "FollowUpDelivery")]
        public FollowUpDelivery FollowUpDelivery { get; set; }
        [XmlElement(ElementName = "FollowUpInvoice")]
        public FollowUpInvoice FollowUpInvoice { get; set; }
        [XmlElement(ElementName = "ItemProduct")]
        public ItemProduct ItemProduct { get; set; }
        [XmlElement(ElementName = "ShipToLocation")]
        public ShipToLocation ShipToLocation { get; set; }
        [XmlElement(ElementName = "DeliveryStartDate")]
        public string DeliveryStartDate { get; set; }
        [XmlElement(ElementName = "DeliveryStartTimeZone")]
        public string DeliveryStartTimeZone { get; set; }
        [XmlElement(ElementName = "DeliveryEndDate")]
        public string DeliveryEndDate { get; set; }
        [XmlElement(ElementName = "DeliveryEndTimeZone")]
        public string DeliveryEndTimeZone { get; set; }
        [XmlAttribute(AttributeName = "ItemImatListCompleteTransmissionIndicator")]
        public string ItemImatListCompleteTransmissionIndicator { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
    }

    [XmlRoot(ElementName = "PurchaseOrder")]
    public class PurchaseOrder
    {
        public PurchaseOrder()
        {
            Name = new Name();
            SellerParty = new SellerParty();
            ResponsiblePurchasingUnitParty = new ResponsiblePurchasingUnitParty();
            DeliveryTerms = new DeliveryTerms();
            Items = new List<Item>();
        }
        [XmlElement(ElementName = "ObjectNodeSenderTechnicalID")]
        public string ObjectNodeSenderTechnicalID { get; set; }
        [XmlElement(ElementName = "Name")]
        public Name Name { get; set; }
        [XmlElement(ElementName = "PurchaseOrderID")]
        public string PurchaseOrderID { get; set; }
        [XmlElement(ElementName = "Date")]
        public string Date { get; set; }
        [XmlElement(ElementName = "OrderPurchaseOrderActionIndicator")]
        public string OrderPurchaseOrderActionIndicator { get; set; }
        [XmlElement(ElementName = "CancelPurchaseOrderActionIndicator")]
        public string CancelPurchaseOrderActionIndicator { get; set; }
        [XmlElement(ElementName = "PreventDocumentOutputIndicator")]
        public string PreventDocumentOutputIndicator { get; set; }
        [XmlElement(ElementName = "CurrencyCode")]
        public string CurrencyCode { get; set; }
        [XmlElement(ElementName = "SellerParty")]
        public SellerParty SellerParty { get; set; }
        [XmlElement(ElementName = "ResponsiblePurchasingUnitParty")]
        public ResponsiblePurchasingUnitParty ResponsiblePurchasingUnitParty { get; set; }
        [XmlElement(ElementName = "DeliveryTerms")]
        public DeliveryTerms DeliveryTerms { get; set; }
        [XmlElement(ElementName = "Item")]
        public List<Item> Items { get; set; }
        [XmlAttribute(AttributeName = "actionCode")]
        public string ActionCode { get; set; }
        [XmlAttribute(AttributeName = "ItemListCompleteTransmissionIndicator")]
        public string ItemListCompleteTransmissionIndicator { get; set; }
    }

    public class PurchaseOrderList
    {
        public PurchaseOrderList()
        {
            PurchaseOrders = new List<PurchaseOrder>();
        }
        [XmlElement("PurchaseOrder")]
        public List<PurchaseOrder> PurchaseOrders { get; set; }
    }
}
