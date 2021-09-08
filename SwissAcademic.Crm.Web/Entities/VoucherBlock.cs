using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.VoucherBlock)]
    [DataContract]
    public class VoucherBlock
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public VoucherBlock()
            :
            base(CrmEntityNames.VoucherBlock)
        {

        }

        #endregion

        #region Eigenschaften

        #region Account

        ManyToOneRelationship<VoucherBlock, Account> _account;
        public ManyToOneRelationship<VoucherBlock, Account> Account
        {
            get
            {
                if (_account == null)
                {
                    _account = new ManyToOneRelationship<VoucherBlock, Account>(this, CrmRelationshipNames.AccountVoucherBlock, "new_accountid");
                    Observe(_account, true);
                }
                return _account;
            }
        }

        #endregion

        #region BlockNumber

        [CrmProperty]
        public string BlockNumber
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

        #region CampusContractVoucher

        /// <summary>
        /// Campus Vertrag Gutschein Block ?
        /// </summary>
        [CrmProperty]
        [DataMember]
        public bool CampusContractVoucher
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

        #region CampusUseVoucherBlockProduct

        /// <summary>
        /// Gutscheinblock Produkt nutzen
        /// </summary>
        [CrmProperty]
        [DataMember]
        public bool CampusUseVoucherBlockProduct
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

        #region CitaviSpaceInGB

        [CrmProperty]
        [DataMember]
        public int? CitaviSpaceInGB
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

        #region CleverBridgeOrderNr

        /// <summary>
        /// Cleverbridge Bestellnummer
        /// </summary>
        [CrmProperty]
        public string CbOrderNummer
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

        #region CreatedOn

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember(Name = "CreatedOn")]
        public new DateTime CreatedOn
        {
            get
            {
                return GetValue<DateTime>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Contact

        /// <summary>
        /// Beantragt von
        /// </summary>
        ManyToOneRelationship<VoucherBlock, Contact> _purchaser;
        public ManyToOneRelationship<VoucherBlock, Contact> Contact
        {
            get
            {
                if (_purchaser == null)
                {
                    _purchaser = new ManyToOneRelationship<VoucherBlock, Contact>(this, CrmRelationshipNames.ContactVoucherBlock, "new_contactid");
                    Observe(_purchaser, true);
                }
                return _purchaser;
            }
        }

        #endregion

        #region LicenseType

        ManyToOneRelationship<VoucherBlock, LicenseType> _licenseType;
        public ManyToOneRelationship<VoucherBlock, LicenseType> LicenseType
        {
            get
            {
                if (_licenseType == null)
                {
                    _licenseType = new ManyToOneRelationship<VoucherBlock, LicenseType>(this, CrmRelationshipNames.VoucherBlockLicenseType, "new_licensetypid");
                    Observe(_licenseType, true);
                }
                return _licenseType;
            }
        }

        #endregion

        #region NumberOfVouchers

        [CrmProperty]
        [DataMember]
        public int NumberOfVouchers
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

        #region OrderProcess

        ManyToOneRelationship<VoucherBlock, OrderProcess> _orderProcess;
        public ManyToOneRelationship<VoucherBlock, OrderProcess> OrderProcess
        {
            get
            {
                if (_orderProcess == null)
                {
                    _orderProcess = new ManyToOneRelationship<VoucherBlock, OrderProcess>(this, CrmRelationshipNames.OrderProcessVoucherBlocks, "new_orderprocessid");
                    Observe(_orderProcess, true);
                }
                return _orderProcess;
            }
        }

        #endregion

        #region Pricing

        ManyToOneRelationship<VoucherBlock, Pricing> _pricing;
        public ManyToOneRelationship<VoucherBlock, Pricing> Pricing
        {
            get
            {
                if (_pricing == null)
                {
                    _pricing = new ManyToOneRelationship<VoucherBlock, Pricing>(this, CrmRelationshipNames.PricingVoucherBlock, "new_pricingid");
                    Observe(_pricing, true);
                }
                return _pricing;
            }
        }

        #endregion

        #region Product

        ManyToOneRelationship<VoucherBlock, Product> _product;
        public ManyToOneRelationship<VoucherBlock, Product> Product
        {
            get
            {
                if (_product == null)
                {
                    _product = new ManyToOneRelationship<VoucherBlock, Product>(this, CrmRelationshipNames.VoucherBlockProduct, "new_citaviproductid");
                    Observe(_product, true);
                }
                return _product;
            }
        }

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(VoucherBlockPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region ShowInLicenseManagement

        [CrmProperty]
        public bool? ShowInLicenseManagement
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

        #region TempDeact

        [CrmProperty]
        public bool? TempDeact
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

        #region Vouchers

        OneToManyRelationship<VoucherBlock, Voucher> _vouchers;
        public OneToManyRelationship<VoucherBlock, Voucher> Vouchers
        {
            get
            {
                if (_vouchers == null)
                {
                    _vouchers = new OneToManyRelationship<VoucherBlock, Voucher>(this, CrmRelationshipNames.VoucherVoucherBlock, "new_voucherblockid");
                    Observe(_vouchers, true);
                }
                return _vouchers;
            }
        }

        #endregion

        #region VoucherValidityInMonths

        /// <summary>
        /// Gutschein Laufzeit in Monaten
        /// </summary>
        [CrmProperty]
        [DataMember]
        public int? VoucherValidityInMonths
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

        #endregion

        #region Methoden

        #region CreateVouchers

        public async Task<IEnumerable<Voucher>> CreateVouchers()
            => await CreateVouchers(Context);

        public async Task<IEnumerable<Voucher>> CreateVouchers(CrmDbContext context)
        {
            var vouchers = new List<Voucher>();
            var voucherManager = new VoucherManager(context);

            for(var i = 0; i < NumberOfVouchers; i++)
            {
                vouchers.Add(await voucherManager.CreateVoucher(this));
            }

            return vouchers;
        }

        #endregion

        #region ToString

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(BlockNumber))
            {
                return BlockNumber;
            }

            return base.ToString();
        }

        #endregion

        #endregion

        #region DataContract

        #region DataContractAccountKey

        string _dataContractAccountKey;
        [CacheDataMember(Name = "AccountKey")]
        public string DataContractAccountKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractAccountKey))
                {
                    return _dataContractAccountKey;
                }

                _dataContractAccountKey = GetAliasedValue<string>(CrmRelationshipNames.AccountVoucherBlock, CrmEntityNames.Account, ProductPropertyId.Key);
                return _dataContractAccountKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.AccountVoucherBlock, CrmEntityNames.Account, ProductPropertyId.Key, value);
                _dataContractAccountKey = value;
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

                _dataContractCampusContractKey = GetAliasedValue<string>(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.CampusContract, ProductPropertyId.Key);
                return _dataContractCampusContractKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.CampusContract, ProductPropertyId.Key, value);
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

                _dataContractCampusContractInfoWebsite = GetAliasedValue<string>(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.CampusContract, CampusContractPropertyId.InfoWebsite);
                return _dataContractCampusContractInfoWebsite;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.CampusContract, CampusContractPropertyId.InfoWebsite, value);
                _dataContractCampusContractInfoWebsite = value;
            }
        }

        #endregion

        #region DataContractCampusContractContractDuration

        [DataMember(Name = "CampusContractContractDuration")]
        public DateTime? DataContractCampusContractContractDuration
        {
            get
            {
                return GetAliasedValue<DateTime?>(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.CampusContract, CampusContractPropertyId.ContractDuration);
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.CampusContract, CampusContractPropertyId.ContractDuration, value);
            }
        }

        #endregion

        #region DataContractCampusContractProductKeys

        [Obsolete("CCs haben neu mehrere Produkte. Sobald das FrontEnd damit umgehen kann, löschen")]
        [DataMember(Name = "CampusContractProductKey")]
        public string DataContractCampusContractProductKey
        {
            get => DataContractCampusContractProductKeys.FirstOrDefault();
        }

        [DataMember(Name = "CampusContractProductKeys")]
        public IEnumerable<string> DataContractCampusContractProductKeys
        {
            get
            {
                if (string.IsNullOrEmpty(DataContractCampusContractKey))
                {
                    if (string.IsNullOrEmpty(DataContractProductKey))
                    {
                        return Enumerable.Empty<string>();
                    }
                    return new[] { DataContractProductKey };
                }
                if (!CrmCache._campusContracts.ContainsKey(DataContractCampusContractKey))
                {
                    return Enumerable.Empty<string>();
                }

                return CrmCache._campusContracts[DataContractCampusContractKey].ProductsResolved.Select(p => p.Key);
            }
        }

        #endregion

        #region DataContractCampusContractProductNames

        [Obsolete("CCs haben neu mehrere Produkte. Sobald das FrontEnd damit umgehen kann, löschen")]
        [DataMember(Name = "CampusContractProductName")]
        public string DataContractCampusContractProductName
        {
            get => DataContractCampusContractProductNames.FirstOrDefault();
        }

        [DataMember(Name = "CampusContractProductNames")]
        public IEnumerable<string> DataContractCampusContractProductNames
        {
            get
            {
                if (string.IsNullOrEmpty(DataContractCampusContractKey))
                {
                    if (string.IsNullOrEmpty(DataContractProductName))
                    {
                        return Enumerable.Empty<string>();
                    }
                    return new[] { DataContractProductName };
                }
                if (!CrmCache._campusContracts.ContainsKey(DataContractCampusContractKey))
                {
                    return Enumerable.Empty<string>();
                }

                return CrmCache._campusContracts[DataContractCampusContractKey].ProductsResolved.Select(p => p.CitaviProductName);
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

                _dataContractProductKey = GetAliasedValue<string>(CrmRelationshipNames.VoucherBlockProduct, CrmEntityNames.Product, ProductPropertyId.Key);
                return _dataContractProductKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.VoucherBlockProduct, CrmEntityNames.Product, ProductPropertyId.Key, value);
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

                _dataContractProductName = GetAliasedValue<string>(CrmRelationshipNames.VoucherBlockProduct, CrmEntityNames.Product, ProductPropertyId.CitaviProductName);
                return _dataContractProductName;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.VoucherBlockProduct, CrmEntityNames.Product, ProductPropertyId.CitaviProductName, value);
                _dataContractProductName = value;
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

                _dataContractLicenseTypeKey = GetAliasedValue<string>(CrmRelationshipNames.VoucherBlockLicenseType, CrmEntityNames.LicenseType, LicenseTypePropertyId.Key);
                return _dataContractLicenseTypeKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.VoucherBlockLicenseType, CrmEntityNames.LicenseType, LicenseTypePropertyId.Key, value);
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

                _dataContractLicenseTypeCode = GetAliasedValue<string>(CrmRelationshipNames.VoucherBlockLicenseType, CrmEntityNames.LicenseType, LicenseTypePropertyId.LicenseCode);
                return _dataContractLicenseTypeCode;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.VoucherBlockLicenseType, CrmEntityNames.LicenseType, LicenseTypePropertyId.LicenseCode, value);
                _dataContractLicenseTypeCode = value;
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

                _dataContractPricingKey = GetAliasedValue<string>(CrmRelationshipNames.PricingVoucherBlock, CrmEntityNames.Pricing, PricingPropertyId.Key);
                return _dataContractPricingKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.PricingVoucherBlock, CrmEntityNames.Pricing, PricingPropertyId.Key, value);
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

                _dataContractPricingCode = GetAliasedValue<string>(CrmRelationshipNames.PricingVoucherBlock, CrmEntityNames.Pricing, PricingPropertyId.PricingCode);
                return _dataContractPricingCode;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.PricingVoucherBlock, CrmEntityNames.Pricing, PricingPropertyId.PricingCode, value);
                _dataContractPricingCode = value;
            }
        }

        #endregion

        #endregion
    }
}
