using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    [Serializable]
    
    public partial class PurchaseItemType
    {
        #region Felder

        private string productIdField;

        private string productNameField;

        private string productNameExtensionField;

        private string yourProductIdField;

        private string yourProductNameField;

        private string productEanField;

        private string productIsbnField;

        private string productReportingGroupField;

        private string internalCategoryField;

        private string supportContactIdField;

        private string supportContactField;

        private string clientIdField;

        private string quantityField;

        private string couponcodeField;

        private string promotionIdField;

        private string promotionNameField;

        private string licenseeStringField;

        private string previousLicenseField;

        private string shippingOptionField;

        private string refBundleItemRunningNoField;

        private string refBundleItemProductIdField;

        private string refCrossSellingItemRunningNoField;

        private string refCrossSellingItemProductIdField;

        private string refSubSellingItemRunningNoField;

        private string refSubSellingItemProductIdField;

        private string refUpSellingProductIdField;

        private string crossSubUpSellingIdField;

        private string crossSubUpSellingNameField;

        private string crossSubUpRefPurchaseIdField;

        private string affiliateIdField;

        private string affiliateField;

        private string yourCurrencyField;

        private CurrencyIdType yourCurrencyIdField;

        private ProfitCalculationType profitCalculationField;

        private PurchaseItemPriceBlock yourPriceField;

        private PurchaseItemPriceBlock customerPriceField;

        private PurchaseItemDeliveryListType deliveriesField;

        private PurchaseItemAdditionalListType additionalsField;

        private PurchaseExtraParameterListType extraParametersField;

        private PurchaseItemRecurringBillingType recurringBillingField;

        private string runningNoField;

        #endregion

        #region Eigenschaften

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string ProductId
        {
            get
            {
                return productIdField;
            }
            set
            {
                productIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string ProductName
        {
            get
            {
                return productNameField;
            }
            set
            {
                productNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string ProductNameExtension
        {
            get
            {
                return productNameExtensionField;
            }
            set
            {
                productNameExtensionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string YourProductId
        {
            get
            {
                return yourProductIdField;
            }
            set
            {
                yourProductIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string YourProductName
        {
            get
            {
                return yourProductNameField;
            }
            set
            {
                yourProductNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string ProductEan
        {
            get
            {
                return productEanField;
            }
            set
            {
                productEanField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string ProductIsbn
        {
            get
            {
                return productIsbnField;
            }
            set
            {
                productIsbnField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string ProductReportingGroup
        {
            get
            {
                return productReportingGroupField;
            }
            set
            {
                productReportingGroupField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string InternalCategory
        {
            get
            {
                return internalCategoryField;
            }
            set
            {
                internalCategoryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string SupportContactId
        {
            get
            {
                return supportContactIdField;
            }
            set
            {
                supportContactIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string SupportContact
        {
            get
            {
                return supportContactField;
            }
            set
            {
                supportContactField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string ClientId
        {
            get
            {
                return clientIdField;
            }
            set
            {
                clientIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string Quantity
        {
            get
            {
                return quantityField;
            }
            set
            {
                quantityField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Couponcode
        {
            get
            {
                return couponcodeField;
            }
            set
            {
                couponcodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string PromotionId
        {
            get
            {
                return promotionIdField;
            }
            set
            {
                promotionIdField = value;
            }
        }



        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string PromotionName
        {
            get
            {
                return promotionNameField;
            }
            set
            {
                promotionNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string LicenseeString
        {
            get
            {
                return licenseeStringField;
            }
            set
            {
                licenseeStringField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string PreviousLicense
        {
            get
            {
                return previousLicenseField;
            }
            set
            {
                previousLicenseField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string ShippingOption
        {
            get
            {
                return shippingOptionField;
            }
            set
            {
                shippingOptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string RefBundleItemRunningNo
        {
            get
            {
                return refBundleItemRunningNoField;
            }
            set
            {
                refBundleItemRunningNoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string RefBundleItemProductId
        {
            get
            {
                return refBundleItemProductIdField;
            }
            set
            {
                refBundleItemProductIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string RefCrossSellingItemRunningNo
        {
            get
            {
                return refCrossSellingItemRunningNoField;
            }
            set
            {
                refCrossSellingItemRunningNoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string RefCrossSellingItemProductId
        {
            get
            {
                return refCrossSellingItemProductIdField;
            }
            set
            {
                refCrossSellingItemProductIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string RefSubSellingItemRunningNo
        {
            get
            {
                return refSubSellingItemRunningNoField;
            }
            set
            {
                refSubSellingItemRunningNoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string RefSubSellingItemProductId
        {
            get
            {
                return refSubSellingItemProductIdField;
            }
            set
            {
                refSubSellingItemProductIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string RefUpSellingProductId
        {
            get
            {
                return refUpSellingProductIdField;
            }
            set
            {
                refUpSellingProductIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string CrossSubUpSellingId
        {
            get
            {
                return crossSubUpSellingIdField;
            }
            set
            {
                crossSubUpSellingIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string CrossSubUpSellingName
        {
            get
            {
                return crossSubUpSellingNameField;
            }
            set
            {
                crossSubUpSellingNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string CrossSubUpRefPurchaseId
        {
            get
            {
                return crossSubUpRefPurchaseIdField;
            }
            set
            {
                crossSubUpRefPurchaseIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string AffiliateId
        {
            get
            {
                return affiliateIdField;
            }
            set
            {
                affiliateIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Affiliate
        {
            get
            {
                return affiliateField;
            }
            set
            {
                affiliateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string YourCurrency
        {
            get
            {
                return yourCurrencyField;
            }
            set
            {
                yourCurrencyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public CurrencyIdType YourCurrencyId
        {
            get
            {
                return yourCurrencyIdField;
            }
            set
            {
                yourCurrencyIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public ProfitCalculationType ProfitCalculation
        {
            get
            {
                return profitCalculationField;
            }
            set
            {
                profitCalculationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PurchaseItemPriceBlock YourPrice
        {
            get
            {
                return yourPriceField;
            }
            set
            {
                yourPriceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PurchaseItemPriceBlock CustomerPrice
        {
            get
            {
                return customerPriceField;
            }
            set
            {
                customerPriceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PurchaseItemDeliveryListType Deliveries
        {
            get
            {
                return deliveriesField;
            }
            set
            {
                deliveriesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PurchaseItemAdditionalListType Additionals
        {
            get
            {
                return additionalsField;
            }
            set
            {
                additionalsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PurchaseExtraParameterListType ExtraParameters
        {
            get
            {
                return extraParametersField;
            }
            set
            {
                extraParametersField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PurchaseItemRecurringBillingType RecurringBilling
        {
            get
            {
                return recurringBillingField;
            }
            set
            {
                recurringBillingField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute()]
        public int LicensePeriodInMonths
        {
            get;
            set;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "integer")]
        public string RunningNo
        {
            get
            {
                return runningNoField;
            }
            set
            {
                runningNoField = value;
            }
        }

        #endregion
    }
}