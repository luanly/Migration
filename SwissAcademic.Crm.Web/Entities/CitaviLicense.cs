using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Security;
using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.License)]
    [DataContract]
    public class CitaviLicense
        :
        CitaviCrmEntity
    {
        #region Ereignisse

        #region OnRelationshipChanged

        protected override void OnRelationshipChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var relationship = sender as ICrmRelationship;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    {
                        if (e.NewItems.Count > 0)
                        {
                            var newItem = e.NewItems[0];
                            switch (newItem)
                            {
                                case CampusContract campusContract:
                                    {
                                        DataContractCampusContractKey = campusContract.Key;
                                        DataContractCampusContractInfoWebsite = campusContract.InfoWebsite;
                                        DataContractRssFeedUrl = campusContract.RSSFeedUrl;
                                    }
                                    break;

                                case Product product:
                                    {
                                        DataContractProductKey = product.Key;
                                        DataContractProductName = product.CitaviProductName;
                                    }
                                    break;

                                case Pricing pricing:
                                    {
                                        DataContractPricingCode = pricing.PricingCode;
                                        DataContractPricingKey = pricing.Key;
                                    }
                                    break;

                                case LicenseType licenseType:
                                    {
                                        DataContractLicenseTypeCode = licenseType.LicenseCode;
                                        DataContractLicenseTypeKey = licenseType.Key;
                                    }
                                    break;

                                case Contact contact:
                                    {
                                        switch (relationship.RelationshipLogicalName)
                                        {
                                            case CrmRelationshipNames.ContactEndUserLicense:
                                                {
                                                    DataContractEndUserContactKey = contact?.Key;
                                                    DataContractEndUserContactName = contact?.FullName;
                                                    DataContractEndUserEmailAddress = contact?.EMailAddress1;
                                                    DataContractEndUserIsCrm4Contact = !string.IsNullOrEmpty(contact.Crm4Id);
                                                    DataContractEndUserIsVerified = contact?.IsVerified.GetValueOrDefault() == true;
                                                }
                                                break;

                                            case CrmRelationshipNames.ContactOwnerLicense:
                                                {
                                                    DataContractOwnerContactKey = contact.Key;
                                                    DataContractOwnerContactName = contact.FullName;
                                                }
                                                break;
                                        }
                                    }
                                    break;

                                case SubscriptionItem subscriptionItem:
                                    {
                                        DataContractSubscriptionItemKey = subscriptionItem.Key;
                                    }
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

        #endregion

        #endregion

        #region Konstruktor

        public CitaviLicense()
            :
            base(CrmEntityNames.License)
        {

        }

        #endregion

        #region Eigenschaften

        #region AllowUnlimitedReaders

        /// <summary>
        /// Wird nur in C5 benötigt. In C6 haben wir die Property ConcurrentReaderCount
        /// </summary>
        [CrmProperty]
        public bool? AllowUnlimitedReaders
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

        #region CampusContract

        ManyToOneRelationship<CitaviLicense, CampusContract> _campusContract;
        public ManyToOneRelationship<CitaviLicense, CampusContract> CampusContract
        {
            get
            {
                if (_campusContract == null)
                {
                    _campusContract = new ManyToOneRelationship<CitaviLicense, CampusContract>(this, CrmRelationshipNames.LicenseCampusContract, "new_campuscontractid");
                    Observe(_campusContract, true);
                }
                return _campusContract;
            }
        }

        #endregion

        #region CampusGroup

        [CrmProperty]
        [DataMember]
        public CampusGroup? CampusGroup
        {
            get
            {
                return GetValue<CampusGroup?>();
            }
            set
            {
                if (value == null)
                {
                    SetValue(SwissAcademic.Crm.Web.CampusGroup.Students);
                }
                else
                {
                    SetValue(value);
                }
            }
        }

        #endregion

        #region CitaviSpaceInMB

        [CrmProperty]
        [DataMember]
        public int? CitaviSpaceInMB
        {
            get
            {
                return GetValue<int?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region CitaviCampaign

        ManyToOneRelationship<CitaviLicense, CitaviCampaign> _citaviCampaign;
        public ManyToOneRelationship<CitaviLicense, CitaviCampaign> CitaviCampaign
        {
            get
            {
                if (_citaviCampaign == null)
                {
                    _citaviCampaign = new ManyToOneRelationship<CitaviLicense, CitaviCampaign>(this, CrmRelationshipNames.LicenseCitaviCampaign, "new_citavicampaignid");
                    Observe(_citaviCampaign, true);
                }
                return _citaviCampaign;
            }
        }

        #endregion

        #region CitaviKey

        [CrmProperty]
        [DataMember]
        public string CitaviKey
        {
            get
            {
                return GetValue<string>() ?? string.Empty;
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region CitaviLicenseName

        [CrmProperty]
        [DataMember]
        public string CitaviLicenseName
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

        #region CitaviMajorVersion

        int _citaviMajorVersion = -1;
        [DataMember]
        [CacheDataMember]
        public int CitaviMajorVersion
        {
            get
            {
                if (_citaviMajorVersion != -1)
                {
                    return _citaviMajorVersion;
                }

                if (string.IsNullOrEmpty(DataContractProductKey))
                {
                    return 0;
                }

                if (CrmCache.Products == null || CrmCache.Products.Count == 0)
                {
                    return -1;
                }

                _citaviMajorVersion = CrmCache.Products[DataContractProductKey].CitaviMajorVersion;
                return _citaviMajorVersion;
            }
            set
            {
                _citaviMajorVersion = value;
            }
        }

        #endregion

        #region ContractExtensionVoucherLicense

        [CrmProperty]
        public bool? ContractExtensionVoucherLicense
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

        #region ConcurrentReaderCount

        [CrmProperty]
        [DataMember]
        public int ConcurrentReaderCount
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

        #region EndUser

        ManyToOneRelationship<CitaviLicense, Contact> _endUser;
        public ManyToOneRelationship<CitaviLicense, Contact> EndUser
        {
            get
            {
                if (_endUser == null)
                {
                    _endUser = new ManyToOneRelationship<CitaviLicense, Contact>(this, CrmRelationshipNames.ContactEndUserLicense, CrmRelationshipLookupNames.LicenseEndUser);
                    Observe(_endUser, true);
                }
                return _endUser;
            }
        }

        #endregion

        #region ExpiryDate

        [CrmProperty]
        [DataMember]
        public DateTime? ExpiryDate
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

        #region FieldsOfStudy

        [CrmProperty]
        [DataMember]
        public string FieldsOfStudy
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

        #region Free

        [CrmProperty]
        [DataMember]
        public bool Free
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region InvoiceNumber

        /// <summary>
        /// Rechnungsnummer
        /// </summary>
        [CrmProperty]
        [DataMember]
        public string InvoiceNumber
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

        #region IsBetaLicense

        [CacheDataMember]
        public bool IsBetaLicense
        {
            get;
            set;
        }

        #endregion

        #region IsCampusLicense

        public bool IsCampusLicense
        {
            get
            {
                return DataContractPricingKey == SwissAcademic.Crm.Web.Pricing.None.Key &&
                       DataContractLicenseTypeKey == SwissAcademic.Crm.Web.LicenseType.Subscription.Key;
            }
        }

        #endregion

        #region IsCitaviSpace

        public bool IsCitaviSpace
        {
            get
            {
                if(!CrmConfig.IsShopWebAppSubscriptionAvailable)
				{
                    return false;
				}
                return DataContractProductKey == Crm.Web.Product.CitaviSpace.Key;
            }
        }

        #endregion

        #region IsCitaviWeb

        public bool IsCitaviWeb
        {
            get
            {
                if (!CrmConfig.IsShopWebAppSubscriptionAvailable)
                {
                    return false;
                }
                return DataContractProductKey == Crm.Web.Product.CitaviWeb.Key;
            }
        }

        #endregion

        #region IsVerified

        [DataMember]
        [CrmProperty]
        public bool IsVerified
        {
            get
            {
                //28.9.2016:
                //Wir haben nach der Migration von CRM4 -> CRM Online, alte Lizenzen OHNE diesen Wert
                //Die sind aber alle verified.
                //Ist kein Problem hier, da wir den Wert jetzt immer setzen.
                var value = GetValue<bool?>();
                return value.GetValueOrDefault(true);
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region IsOrganizationSettingsAdmin

        [CrmProperty]
        [DataMember]
        public bool IsOrganizationSettingsAdmin
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Language

        [CrmProperty]
        [DataMember]
        public int? Language
        {
            get
            {
                return GetValue<int?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region LicenseType

        ManyToOneRelationship<CitaviLicense, LicenseType> _licenseType;
        public ManyToOneRelationship<CitaviLicense, LicenseType> LicenseType
        {
            get
            {
                if (_licenseType == null)
                {
                    _licenseType = new ManyToOneRelationship<CitaviLicense, LicenseType>(this, CrmRelationshipNames.LicenseLicenseType, "new_licensetypid");
                    Observe(_licenseType, true);
                }
                return _licenseType;
            }
        }

        #endregion

        #region OrderMailSentOn

        [CrmProperty]
        public DateTime? OrderMailSentOn
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

        #region OrderCategory

        [CrmProperty]
        [DataMember]
        public OrderCategory? OrderCategory
        {
            get
            {
                return GetValue<OrderCategory?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region OrderDate

        [CrmProperty]
        [DataMember]
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

        #region OrderProcess

        ManyToOneRelationship<CitaviLicense, OrderProcess> _orderProcess;
        public ManyToOneRelationship<CitaviLicense, OrderProcess> OrderProcess
        {
            get
            {
                if (_orderProcess == null)
                {
                    _orderProcess = new ManyToOneRelationship<CitaviLicense, OrderProcess>(this, CrmRelationshipNames.LicenseOrderProcess, "new_orderprocessid");
                    Observe(_orderProcess, true);
                }
                return _orderProcess;
            }
        }

        #endregion

        #region OrganizationName

        [CrmProperty]
        [DataMember]
        public string OrganizationName
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

        #region Pricing

        ManyToOneRelationship<CitaviLicense, Pricing> _pricing;
        public ManyToOneRelationship<CitaviLicense, Pricing> Pricing
        {
            get
            {
                if (_pricing == null)
                {
                    _pricing = new ManyToOneRelationship<CitaviLicense, Pricing>(this, CrmRelationshipNames.LicensePricing, "new_pricingid");
                    Observe(_pricing, true);
                }
                return _pricing;
            }
        }

        Pricing _pricingResolved;
        [IgnoreDataMember]
        internal Pricing PricingResolved
        {
            get
            {
                if (_pricingResolved == null && !string.IsNullOrEmpty(DataContractPricingCode))
                {
                    _pricingResolved = CrmCache.PricingsByCode[DataContractPricingCode];
                }
                return _pricingResolved;

            }
        }

        #endregion

        #region Product

        ManyToOneRelationship<CitaviLicense, Product> _product;
        public ManyToOneRelationship<CitaviLicense, Product> Product
        {
            get
            {
                if (_product == null)
                {
                    _product = new ManyToOneRelationship<CitaviLicense, Product>(this, CrmRelationshipNames.LicenseProduct, "new_citaviproductid");
                    Observe(_product, true);
                }
                return _product;
            }
        }

        Product _productResolved;
        [IgnoreDataMember]
        internal Product ProductResolved
		{
			get
			{
                if(_productResolved == null && !string.IsNullOrEmpty(DataContractProductKey))
				{
                    _productResolved = CrmCache.Products[DataContractProductKey];
                }
                return _productResolved;

            }
		}

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(LicensePropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region Owner

        ManyToOneRelationship<CitaviLicense, Contact> _owner;
        public ManyToOneRelationship<CitaviLicense, Contact> Owner
        {
            get
            {
                if (_owner == null)
                {
                    _owner = new ManyToOneRelationship<CitaviLicense, Contact>(this, CrmRelationshipNames.ContactOwnerLicense, CrmRelationshipLookupNames.LicenseOwner);
                    Observe(_owner, true);
                }
                return _owner;
            }
        }

        #endregion

        #region ReadOnly

        [CrmProperty]
        [DataMember]
        public bool ReadOnly
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ServerAmount

        [CrmProperty]
        [DataMember]
        public int ServerAmount
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

        #region ServerConcurrent

        [CrmProperty]
        [DataMember]
        public bool? ServerConcurrent
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

        #region ServerId

        [CrmProperty]
        [DataMember]
        public string ServerId
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

        #region ServerXMLLicense
        /// <summary>
        /// Für Get: LoadServerXMLLicense() verwenden!
        /// </summary>
        [CrmProperty(NoCache=true)]
#pragma warning disable CA1044 // Eigenschaften dürfen nicht lesegeschützt sein
        public string ServerXMLLicense
#pragma warning restore CA1044 // Eigenschaften dürfen nicht lesegeschützt sein
        {
            private get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region SubscriptionItem

        ManyToOneRelationship<CitaviLicense, SubscriptionItem> _subscription;
        public ManyToOneRelationship<CitaviLicense, SubscriptionItem> SubscriptionItem
        {
            get
            {
                if (_subscription == null)
                {
                    _subscription = new ManyToOneRelationship<CitaviLicense, SubscriptionItem>(this, CrmRelationshipNames.SubscriptionItemLicenses, "new_subscriptionitemid");
                    Observe(_subscription, true);
                }
                return _subscription;
            }
        }

        #endregion

        #region SubscriptionStatus

        [CrmProperty]
        [DataMember]
        public LicenseSubscriptionStatus? SubscriptionStatus
        {
            get
            {
                return GetValue<LicenseSubscriptionStatus?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Voucher

        ManyToOneRelationship<CitaviLicense, Voucher> _voucher;
        public ManyToOneRelationship<CitaviLicense, Voucher> Voucher
        {
            get
            {
                if (_voucher == null)
                {
                    _voucher = new ManyToOneRelationship<CitaviLicense, Voucher>(this, CrmRelationshipNames.LicenseVoucher, "new_voucherid");
                    Observe(_voucher, true);
                }
                return _voucher;
            }
        }

        #endregion

        #region VoucherId

        [CrmProperty]
        public Guid VoucherId
        {
            get
            {
                return GetValue<Guid>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region VerificationKey

        [CrmProperty]
        public string VerificationKey
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

        #region VerificationStorage

        [CrmProperty]
        public string VerificationStorage
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

        #region VerificationKeySent

        [CrmProperty]
        public DateTime? VerificationKeySent
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

        #endregion

        #region Methoden

        #region Clone

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product">Produkt der geclonten Lizenz</param>
        /// <param name="pricing">Pricing der geclonten Lizenz</param>
        /// <param name="licenseType">LizenzType der geclonten Lizenz</param>
        /// <param name="attributesToClone">Wenn null, werden alle Properties geclont</param>
        /// <returns></returns>
        public async Task<CitaviLicense> CloneAsync(Product product,
                                   Pricing pricing,
                                   LicenseType licenseType,
                                   params LicensePropertyId[] attributesToClone)
        {
            var clone = Clone<CitaviLicense>(attributesToClone.Cast<Enum>().ToArray());
            if (product != null)
            {
                clone.Product.Set(product);
            }
            if (pricing != null)
            {
                clone.Pricing.Set(pricing);
            }
            if (licenseType != null)
            {
                clone.LicenseType.Set(licenseType);
            }

            if(DataContractEndUserId == Guid.Empty)
            {
                var enduser = await EndUser.Get();
                if (enduser != null)
                {
                    DataContractEndUserId = enduser.Id;
                }
            }
            if (DataContractOwnerId == Guid.Empty)
            {
                var owner = await Owner.Get();
                if (owner != null)
                {
                    DataContractOwnerId = owner.Id;
                }
            }

            if (DataContractEndUserId != Guid.Empty)
            {
                clone.EndUser.Set(new Contact() { Id = DataContractEndUserId });
            }
            if (DataContractOwnerId != Guid.Empty)
            {
                clone.Owner.Set(new Contact() { Id = DataContractOwnerId });
            }

            return clone;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="campusContract">Campusvertrag der geclonten Lizenz</param>
        /// <param name="product">Produkt der geclonten Lizenz</param>
        /// <param name="attributesToClone">Wenn null, werden alle Properties geclont</param>
        /// <returns></returns>
        public async Task<CitaviLicense> CloneAsync(CampusContract campusContract,
                                   Product product,
                                   params LicensePropertyId[] attributesToClone)
        {
            var clone = await CloneAsync(product, Web.Pricing.None, Web.LicenseType.Subscription, attributesToClone);
            clone.Product.Set(product);
            clone.CampusContract.Set(campusContract);
            return clone;
        }

        #endregion

        #region HasMissingValues

        internal bool HasMissingValues(string endUserContactKey)
        {
            if (DataContractEndUserContactKey != endUserContactKey)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(OrganizationName) &&
                !string.IsNullOrEmpty(CitaviLicenseName) &&
                !string.IsNullOrEmpty(CitaviKey))
            {
                return false;
            }

            if (CrmCache.Products[DataContractProductKey].IsSubscription)
            {
                return string.IsNullOrEmpty(OrganizationName) ||
                       string.IsNullOrEmpty(CitaviLicenseName);
            }
            else
            {
                return string.IsNullOrEmpty(CitaviKey) ||
                       string.IsNullOrEmpty(OrganizationName) ||
                       string.IsNullOrEmpty(CitaviLicenseName);
            }
        }

        #endregion

        #region LoadServerXMLLicense


        public async Task<string> LoadServerXMLLicenseAsync(CrmDbContext context)
        {
            var property = EntityNameResolver.ResolveAttributeName(CrmEntityNames.License, nameof(ServerXMLLicense));
            var result = await CrmWebApi.RetrieveProperty<string>(CrmEntityNames.License, Id, property, null);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            var product = CrmCache.Products[DataContractProductKey];
            ServerXMLLicense = new LicenseManager(context).CreateServerLicenseXml(this, product);
            await context.SaveAsync();
            return ServerXMLLicense;
        }

		#endregion

        #region ToString

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(DataContractProductName))
            {
                return DataContractProductName;
            }

            if (!string.IsNullOrEmpty(CitaviLicenseName))
            {
                return CitaviLicenseName;
            }

            return base.ToString();
        }

        #endregion

        #endregion

        #region Statische Methoden

        #region GetBetaLicense

        public static CitaviLicense GetBetaLicense(Contact contact)
        {
            try
            {
                var betaProduct = CrmConfig.BetaLicenseProduct;
                if (betaProduct == null)
                {
                    return null;
                }

                var license = new CitaviLicense();
                license.IsBetaLicense = true;
                license.EntityState = Web.EntityState.Created;
                license.CreateKey();
                license.CitaviKey = $"{LicenseManager.FormatLicenceKey(PasswordGenerator.LicenseKey.Generate())}{CrmConstants.CitaviBetaLicenseSuffix}";
                license.ExpiryDate = DateTime.UtcNow.AddDays(7);
                license.Pricing.Set(Crm.Web.Pricing.Standard);

                license.Product.Set(betaProduct);

                license.LicenseType.Set(Crm.Web.LicenseType.Purchase);
                license.IsVerified = true;
                license.CitaviLicenseName = contact.FullName;

                license.DataContractEndUserContactKey = contact.Key;
                license.DataContractEndUserContactName = contact.FullName;
                license.DataContractEndUserEmailAddress = contact.EMailAddress1;
                license.DataContractEndUserIsCrm4Contact = false;
                license.DataContractEndUserIsVerified = true;

                license.DataContractOwnerContactKey = contact.Key;
                license.DataContractOwnerContactName = contact.FullName;

                license.OrganizationName = "BETA-TEST";
                return license;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                return null;
            }
        }

        #endregion

        #endregion

        #region DataContract

        #region DataContractAccountKey

        string _dataContractAccountKey;

        [DataMember(Name = "AccountKey")]
        public string DataContractAccountKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractAccountKey))
                {
                    return _dataContractAccountKey;
                }

                _dataContractAccountKey = GetAliasedValue<string>(CrmRelationshipNames.AccountLicense, CrmEntityNames.Account, AccountPropertyId.Key);
                return _dataContractAccountKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.AccountLicense, CrmEntityNames.Account, AccountPropertyId.Key, value);
                _dataContractAccountKey = value;
            }
        }

        #endregion

        #region DataContractCitaviSpaceInGB


        [DataMember(Name = "CitaviSpaceInGB")]
        public float DataContractCitaviSpaceInGB
        {
            get
            {
				if (!CitaviSpaceInMB.HasValue)
				{
                    return 0;
				}
                return (float)CitaviSpaceInMB.Value / 1024;
            }
        }

        #endregion

        #region DataContractEndUserContactKey

        string _dataContractEndUserContactKey;

        [DataMember(Name = "EndUserContactKey")]
        public string DataContractEndUserContactKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractEndUserContactKey))
                {
                    return _dataContractEndUserContactKey;
                }

                _dataContractEndUserContactKey = GetAliasedValue<string>(CrmRelationshipNames.ContactEndUserLicense, CrmEntityNames.Contact, ContactPropertyId.Key);
                return _dataContractEndUserContactKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.ContactEndUserLicense, CrmEntityNames.Contact, ContactPropertyId.Key, value);
                _dataContractEndUserContactKey = value;
            }
        }

        #endregion

        #region DataContractEndUserContactName

        string _dataContractEndUserContactName;
        [DataMember(Name = "EndUserContactName")]
        public string DataContractEndUserContactName
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractEndUserContactName))
                {
                    return _dataContractEndUserContactName;
                }

                _dataContractEndUserContactName = GetAliasedValue<string>(CrmRelationshipNames.ContactEndUserLicense, CrmEntityNames.Contact, ContactPropertyId.FullName);
                return _dataContractEndUserContactName;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.ContactEndUserLicense, CrmEntityNames.Contact, ContactPropertyId.FullName, value);
                _dataContractEndUserContactName = value;
            }
        }

        #endregion

        #region DataContractEndUserEmailAddress

        string _dataContractEndUserEmailAddress;
        [DataMember(Name = "EndUserEmailAddress")]
        public string DataContractEndUserEmailAddress
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractEndUserEmailAddress))
                {
                    return _dataContractEndUserEmailAddress;
                }

                _dataContractEndUserEmailAddress = GetAliasedValue<string>(CrmRelationshipNames.ContactEndUserLicense, CrmEntityNames.Contact, ContactPropertyId.EMailAddress1);
                return _dataContractEndUserEmailAddress;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.ContactEndUserLicense, CrmEntityNames.Contact, ContactPropertyId.EMailAddress1, value);
                _dataContractEndUserEmailAddress = value;
            }
        }

        #endregion

        #region DataContractEndUserId

        Guid _dataContractEndUserId;
        public Guid DataContractEndUserId
        {
            get
            {
                if (_dataContractEndUserId != Guid.Empty)
                {
                    return _dataContractEndUserId;
                }

                _dataContractEndUserId = GetAliasedValue<Guid>(CrmRelationshipNames.ContactEndUserLicense, CrmEntityNames.Contact, ContactPropertyId.Id);
                return _dataContractEndUserId;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.ContactEndUserLicense, CrmEntityNames.Contact, ContactPropertyId.Id, value);
                _dataContractEndUserId = value;
            }
        }

        #endregion

        #region DataContractEndUserIsVerified

        bool? _dataContractEndUserIsVerified;

        [DataMember(Name = "EndUserIsVerified")]
        public bool DataContractEndUserIsVerified
        {
            get
            {
                if (_dataContractEndUserIsVerified.HasValue)
                {
                    return _dataContractEndUserIsVerified.Value;
                }

                _dataContractEndUserIsVerified = GetAliasedValue<bool>(CrmRelationshipNames.ContactEndUserLicense, CrmEntityNames.Contact, ContactPropertyId.IsVerified);
                return _dataContractEndUserIsVerified.Value;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.ContactEndUserLicense, CrmEntityNames.Contact, ContactPropertyId.IsVerified, value);
                _dataContractEndUserIsVerified = value;
            }
        }

        #endregion

        #region DataContractEndUserIsCrm4Contact

        bool? _endUserIsCrm4Contact;
        [DataMember(Name = "EndUserIsCrm4Contact")]
        public bool DataContractEndUserIsCrm4Contact
        {
            get
            {
                if (_endUserIsCrm4Contact == null)
                {
                    var crm4id = GetAliasedValue<string>(CrmRelationshipNames.ContactEndUserLicense, CrmEntityNames.Contact, "crm4id");
                    _endUserIsCrm4Contact = !string.IsNullOrEmpty(crm4id);
                }
                return _endUserIsCrm4Contact.GetValueOrDefault();

            }
            set
            {
                _endUserIsCrm4Contact = value;
            }
        }

        #endregion

        #region DataContractOwnerContactKey

        string _dataContractOwnerContactKey;
        [DataMember(Name = "OwnerContactKey")]
        public string DataContractOwnerContactKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractOwnerContactKey))
                {
                    return _dataContractOwnerContactKey;
                }

                _dataContractOwnerContactKey = GetAliasedValue<string>(CrmRelationshipNames.ContactOwnerLicense, CrmEntityNames.Contact, ContactPropertyId.Key);
                return _dataContractOwnerContactKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.ContactOwnerLicense, CrmEntityNames.Contact, ContactPropertyId.Key, value);
                _dataContractOwnerContactKey = value;
            }
        }

        #endregion

        #region DataContractOwnerId

        Guid _dataContractOwnerId;
        public Guid DataContractOwnerId
        {
            get
            {
                if (_dataContractOwnerId != Guid.Empty)
                {
                    return _dataContractOwnerId;
                }

                _dataContractOwnerId = GetAliasedValue<Guid>(CrmRelationshipNames.ContactOwnerLicense, CrmEntityNames.Contact, ContactPropertyId.Id);
                return _dataContractOwnerId;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.ContactOwnerLicense, CrmEntityNames.Contact, ContactPropertyId.Id, value);
                _dataContractOwnerId = value;
            }
        }

        #endregion

        #region DataContractOwnerContactName

        string _dataContractOwnerContactName;
        [DataMember(Name = "OwnerContactName")]
        public string DataContractOwnerContactName
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractOwnerContactName))
                {
                    return _dataContractOwnerContactName;
                }

                _dataContractOwnerContactName = GetAliasedValue<string>(CrmRelationshipNames.ContactOwnerLicense, CrmEntityNames.Contact, ContactPropertyId.FullName);
                return _dataContractOwnerContactName;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.ContactOwnerLicense, CrmEntityNames.Contact, ContactPropertyId.FullName, value);
                _dataContractOwnerContactName = value;
            }
        }

        #endregion

        #region DataContractLicenseTypeKey

        string _dataContractLicenseTypeKey;
        [CacheDataMember(Name = "LicenseTypeKey")]
        public string DataContractLicenseTypeKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractLicenseTypeKey))
                {
                    return _dataContractLicenseTypeKey;
                }

                _dataContractLicenseTypeKey = GetAliasedValue<string>(CrmRelationshipNames.LicenseLicenseType, CrmEntityNames.LicenseType, LicenseTypePropertyId.Key);
                return _dataContractLicenseTypeKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.LicenseLicenseType, CrmEntityNames.LicenseType, LicenseTypePropertyId.Key, value);
                _dataContractLicenseTypeKey = value;
            }
        }

        #endregion

        #region DataContractLicenseTypeCode

        string _dataContractLicenseTypeCode;

        [DataMember(Name = "LicenseTypeCode")]
        public string DataContractLicenseTypeCode
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractLicenseTypeCode))
                {
                    return _dataContractLicenseTypeCode;
                }

                _dataContractLicenseTypeCode = GetAliasedValue<string>(CrmRelationshipNames.LicenseLicenseType, CrmEntityNames.LicenseType, LicenseTypePropertyId.LicenseCode);
                return _dataContractLicenseTypeCode;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.LicenseLicenseType, CrmEntityNames.LicenseType, LicenseTypePropertyId.LicenseCode, value);
                _dataContractLicenseTypeCode = value;
            }
        }

        #endregion

        #region DataContractOrderKey

        string _dataContractOrderKey;

        [CacheDataMember(Name = "OrderKey")]
        public string DataContractOrderKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractOrderKey))
                {
                    return _dataContractOrderKey;
                }

                _dataContractOrderKey = GetAliasedValue<string>(CrmRelationshipNames.LicenseOrderProcess, CrmEntityNames.OrderProcess, OrderProcessPropertyId.Key);
                return _dataContractOrderKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.LicenseOrderProcess, CrmEntityNames.OrderProcess, OrderProcessPropertyId.Key, value);
                _dataContractOrderKey = value;
            }
        }

        #endregion

        #region DataContractRssFeedUrl

        string _dataContractRssFeedUrl;

        [DataMember(Name = "RssFeedUrl")]
        public string DataContractRssFeedUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractRssFeedUrl))
                {
                    return _dataContractRssFeedUrl;
                }

                _dataContractRssFeedUrl = GetAliasedValue<string>(CrmRelationshipNames.LicenseCampusContract, CrmEntityNames.CampusContract, CampusContractPropertyId.RssFeedUrl);
                return _dataContractRssFeedUrl;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.LicenseCampusContract, CrmEntityNames.CampusContract, CampusContractPropertyId.RssFeedUrl, value);
                _dataContractRssFeedUrl = value;
            }
        }

        #endregion

        #region DataContractPricingKey

        string _dataContractPricingKey;

        [CacheDataMember(Name = "PricingKey")]
        public string DataContractPricingKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractPricingKey))
                {
                    return _dataContractPricingKey;
                }

                _dataContractPricingKey = GetAliasedValue<string>(CrmRelationshipNames.LicensePricing, CrmEntityNames.Pricing, PricingPropertyId.Key);
                return _dataContractPricingKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.LicensePricing, CrmEntityNames.Pricing, PricingPropertyId.Key, value);
                _dataContractPricingKey = value;
            }
        }

        #endregion

        #region DataContractPricingCode

        string _dataContractPricingCode;

        [DataMember(Name = "PricingCode")]
        public string DataContractPricingCode
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractPricingCode))
                {
                    return _dataContractPricingCode;
                }

                _dataContractPricingCode = GetAliasedValue<string>(CrmRelationshipNames.LicensePricing, CrmEntityNames.Pricing, PricingPropertyId.PricingCode);
                return _dataContractPricingCode;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.LicensePricing, CrmEntityNames.Pricing, PricingPropertyId.PricingCode, value);
                _dataContractPricingCode = value;
            }
        }

        #endregion

        #region DataContractProductKey

        string _dataContractProductKey;

        [DataMember(Name = "ProductKey")]
        public string DataContractProductKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractProductKey))
                {
                    return _dataContractProductKey;
                }

                _dataContractProductKey = GetAliasedValue<string>(CrmRelationshipNames.LicenseProduct, CrmEntityNames.Product, ProductPropertyId.Key);
                return _dataContractProductKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.LicenseProduct, CrmEntityNames.Product, ProductPropertyId.Key, value);
                _dataContractProductKey = value;
            }
        }

        #endregion

        #region DataContractProductName

        string _dataContractProductName;

        [DataMember(Name = "ProductName")]
        public string DataContractProductName
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractProductName))
                {
                    return _dataContractProductName;
                }

                _dataContractProductName = GetAliasedValue<string>(CrmRelationshipNames.LicenseProduct, CrmEntityNames.Product, ProductPropertyId.CitaviProductName);
                return _dataContractProductName;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.LicenseProduct, CrmEntityNames.Product, ProductPropertyId.CitaviProductName, value);
                _dataContractProductName = value;
            }
        }

        #endregion

        #region DataContractCampusContractKey

        string _dataContractCampusContractKey;

        [DataMember(Name = "CampusContractKey")]
        public string DataContractCampusContractKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractCampusContractKey))
                {
                    return _dataContractCampusContractKey;
                }

                _dataContractCampusContractKey = GetAliasedValue<string>(CrmRelationshipNames.LicenseCampusContract, CrmEntityNames.CampusContract, CampusContractPropertyId.Key);
                return _dataContractCampusContractKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.LicenseCampusContract, CrmEntityNames.CampusContract, CampusContractPropertyId.Key, value);
                _dataContractCampusContractKey = value;
            }
        }

        #endregion

        #region DataContractCampusContractInfoWebsite

        string _dataContractCampusContractInfoWebsite;

        [DataMember(Name = "CampusContractInfoWebsite")]
        public string DataContractCampusContractInfoWebsite
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractCampusContractInfoWebsite))
                {
                    return _dataContractCampusContractInfoWebsite;
                }

                _dataContractCampusContractInfoWebsite = GetAliasedValue<string>(CrmRelationshipNames.LicenseCampusContract, CrmEntityNames.CampusContract, CampusContractPropertyId.InfoWebsite);
                return _dataContractCampusContractInfoWebsite;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.LicenseCampusContract, CrmEntityNames.CampusContract, CampusContractPropertyId.InfoWebsite, value);
                _dataContractCampusContractInfoWebsite = value;
            }
        }

        #endregion

        #region DataContractSubscriptionItemKey

        string _dataContractSubscriptionItemKey;

        [DataMember(Name = "SubscriptionItemKey")]
        public string DataContractSubscriptionItemKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractSubscriptionItemKey))
                {
                    return _dataContractSubscriptionItemKey;
                }
                _dataContractSubscriptionItemKey = GetAliasedValue<string>(CrmRelationshipNames.SubscriptionItemLicenses, CrmEntityNames.SubscriptionItem, SubscriptionItemPropertyId.Key);
                return _dataContractSubscriptionItemKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.SubscriptionItemLicenses, CrmEntityNames.SubscriptionItem, SubscriptionItemPropertyId.Key, value);
                _dataContractSubscriptionItemKey = value;
            }
        }

        #endregion

        #endregion
    }
}
