using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.OrderProcess)]
    [DataContract]
    public class OrderProcess
        :
        CitaviCrmEntity
    {
        #region Felder

        OrderProcessProductGroup? _orderProcessProductGroup;

        #endregion

        #region Konstruktor

        public OrderProcess()
            :
            base(CrmEntityNames.OrderProcess)
        {

        }

        #endregion

        #region Eigenschaften

        #region BillingAccountText

        [CrmProperty]
        public string BillingAccountText
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region BillingContact

        ManyToOneRelationship<OrderProcess, Contact> _billingContact;
        /// <summary>
        /// Rechnungskontakt
        /// </summary>
        public ManyToOneRelationship<OrderProcess, Contact> BillingContact
        {
            get
            {
                if (_billingContact == null)
                {
                    _billingContact = new ManyToOneRelationship<OrderProcess, Contact>(this, CrmRelationshipNames.ContactOrderProcess, "new_billingcontactid");
                    Observe(_billingContact, true);
                }
                return _billingContact;
            }
        }

        #endregion

        #region BillomatCustomerOrderNumber

        /// <summary>
        /// Billomat Bestellnummer
        /// </summary>
        [CrmProperty]
        public string BillomatCustomerOrderNumber
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region BillomatInvoiceNumber

        /// <summary>
        /// Billomat Rechnungsnummer
        /// </summary>
        [CrmProperty]
        public string BillomatInvoiceNumber
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region CbOrderPdf

        /// <summary>
        /// Cleverbridge Bestellung PDF
        /// </summary>
        [CrmProperty]
        public string CbOrderPdf
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region CleverBridgeOrderNr

        /// <summary>
        /// Cleverbridge Bestellnummer
        /// </summary>
        [CrmProperty]
        public string CleverBridgeOrderNr
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region CitaviLicenses

        OneToManyRelationship<OrderProcess, CitaviLicense> _citaviLicenses;
        public OneToManyRelationship<OrderProcess, CitaviLicense> CitaviLicenses
        {
            get
            {
                if (_citaviLicenses == null)
                {
                    _citaviLicenses = new OneToManyRelationship<OrderProcess, CitaviLicense>(this, CrmRelationshipNames.LicenseOrderProcess, "new_orderprocessid");
                    Observe(_citaviLicenses, true);
                }
                return _citaviLicenses;
            }
        }

        #endregion

        #region DeliveryAccountText

        [CrmProperty]
        public string DeliveryAccountText
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region DeliveryContact

        ManyToOneRelationship<OrderProcess, Contact> _deliveryContact;
        /// <summary>
        /// Zustell Kontakt
        /// </summary>
        public ManyToOneRelationship<OrderProcess, Contact> DeliveryContact
        {
            get
            {
                if (_deliveryContact == null)
                {
                    _deliveryContact = new ManyToOneRelationship<OrderProcess, Contact>(this, CrmRelationshipNames.ContactOrderProcessDC, "new_deliverycontactid");
                    Observe(_deliveryContact, true);
                }
                return _deliveryContact;
            }
        }

        #endregion

        #region IsReseller

        /// <summary>
        /// Rechnungskontakt ist Partner
        /// </summary>
        [CrmProperty]
        public bool? IsReseller
        {
            get
            {
                return GetValue<bool?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region LicenseAmmount

        /// <summary>
        /// Anzahl Lizenzen
        /// </summary>
        [CrmProperty]
        public int LicenseAmmount
        {
            get
            {
                return GetValue<int>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region LicenseContact

        ManyToOneRelationship<OrderProcess, Contact> _licenseContact;
        /// <summary>
        /// Zustell Kontakt
        /// </summary>
        public ManyToOneRelationship<OrderProcess, Contact> LicenseContact
        {
            get
            {
                if (_licenseContact == null)
                {
                    _licenseContact = new ManyToOneRelationship<OrderProcess, Contact>(this, CrmRelationshipNames.ContactOrderProcessLC, "new_licensecontactid");
                    Observe(_licenseContact, true);
                }
                return _licenseContact;
            }
        }

        #endregion

        #region OrderDate

        [CrmProperty]
        public DateTime? OrderDate
        {
            get
            {
                return GetValue<DateTime?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region OrderProcessTrackingNr

        /// <summary>
        /// Bestellvorgang-Tracking Nr.
        /// </summary>
        [CrmProperty]
        public string OrderProcessTrackingNr
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region PartnerID

        [CrmProperty]
        public string PartnerID
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region PartnerName

        [CrmProperty]
        public string PartnerName
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(OrderProcessPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region Reseller

        ManyToOneRelationship<OrderProcess, Account> _reseller;
        public ManyToOneRelationship<OrderProcess, Account> Reseller
        {
            get
            {
                if (_reseller == null)
                {
                    _reseller = new ManyToOneRelationship<OrderProcess, Account>(this, CrmRelationshipNames.ResellerOrderProcess, "new_resellerid");
                    Observe(_reseller, true);
                }
                return _reseller;
            }
        }

        #endregion

        #region Subscriptions

        ManyToManyRelationship<OrderProcess, Subscription> _subscriptions;
        public ManyToManyRelationship<OrderProcess, Subscription> Subscriptions
        {
            get
            {
                if (_subscriptions == null)
                {
                    _subscriptions = new ManyToManyRelationship<OrderProcess, Subscription>(this, CrmRelationshipNames.SubscriptionOrderProcesses);
                    Observe(_subscriptions, true);
                }
                return _subscriptions;
            }
        }

        #endregion

        #region VoucherBlocks

        OneToManyRelationship<OrderProcess, VoucherBlock> _voucherBlocks;
        public OneToManyRelationship<OrderProcess, VoucherBlock> VoucherBlocks
        {
            get
            {
                if (_voucherBlocks == null)
                {
                    _voucherBlocks = new OneToManyRelationship<OrderProcess, VoucherBlock>(this, CrmRelationshipNames.OrderProcessVoucherBlocks);
                    Observe(_voucherBlocks, true);
                }
                return _voucherBlocks;
            }
        }

        #endregion

        #region XmlRaw

        [CrmProperty]
        public string XmlRaw
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

		#endregion

		#endregion

		#region Methoden

		#region GetOrderProcessProductGroup

        internal async Task<OrderProcessProductGroup> GetOrderProcessProductGroup()
		{
            if(_orderProcessProductGroup != null)
			{
                return _orderProcessProductGroup.Value;
            }
            var licenses = await CitaviLicenses.Get();
            var products = new List<Product>();
            var pricings = new List<Pricing>();

            foreach (var license in licenses)
            {
                var product = await license.Product.Get();
                products.Add(CrmCache.Products[product.Key]);
            }
            foreach (var license in licenses)
            {
                var princing = await license.Pricing.Get();
                pricings.Add(CrmCache.PricingsByKey[princing.Key]);
            }

            _orderProcessProductGroup = OrderProcessProductGroup.Home;

            if (products.Any(p => p.IsSqlServerProduct))
            {
                _orderProcessProductGroup = OrderProcessProductGroup.DbServer;
            }
            else if (pricings.Any(p => p.IsBusinessOrAcacemicPricing()))
            {
                _orderProcessProductGroup = OrderProcessProductGroup.Business;
            }

            return _orderProcessProductGroup.Value;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(OrderProcessTrackingNr))
            {
                return OrderProcessTrackingNr;
            }

            return base.ToString();
        }

        #endregion

        #endregion

        #region DataContract

        #region DataContractLicenseContactKey

        string _dataContractLicenseContactKey;

        public string DataContractLicenseContactKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractLicenseContactKey))
                {
                    return _dataContractLicenseContactKey;
                }

                _dataContractLicenseContactKey = GetAliasedValue<string>(CrmRelationshipNames.ContactOrderProcessLC, CrmEntityNames.Contact, ContactPropertyId.Key);
                return _dataContractLicenseContactKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.ContactOrderProcessLC, CrmEntityNames.Contact, ContactPropertyId.Key, value);
                _dataContractLicenseContactKey = value;
            }
        }

        #endregion

        #endregion

        #region Statische Methoden

        public static async Task<IEnumerable<OrderProcess>> Get(Contact contact, CrmDbContext context)
        {
            var query = new QueryExpression(CrmEntityNames.OrderProcess);
            query.AddCondition("_new_licensecontactid_value", ConditionOperator.Equal, contact.Id);
            var result = await context.RetrieveMultiple<OrderProcess>(query);
            if (result != null &&
                result.Any())
            {
                return result;
            }
            return Enumerable.Empty<OrderProcess>();
        }

        #endregion
    }
}
