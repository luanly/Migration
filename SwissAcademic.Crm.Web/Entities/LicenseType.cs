using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.LicenseType)]
    [DataContract]
    public class LicenseType
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public LicenseType()
            :
            base(CrmEntityNames.LicenseType)
        {

        }

        #endregion

        #region Eigenschaften

        #region Account

        ManyToOneRelationship<LicenseType, Account> _account;
        public ManyToOneRelationship<LicenseType, Account> Account
        {
            get
            {
                if (_account == null)
                {
                    _account = new ManyToOneRelationship<LicenseType, Account>(this, CrmRelationshipNames.AccountLicenseType, "organizationid");
                    Observe(_account, true);
                }
                return _account;
            }
        }

        #endregion

        #region License

        OneToManyRelationship<LicenseType, CitaviLicense> _license;
        public OneToManyRelationship<LicenseType, CitaviLicense> License
        {
            get
            {
                if (_license == null)
                {
                    _license = new OneToManyRelationship<LicenseType, CitaviLicense>(this, CrmRelationshipNames.LicenseLicenseType);
                    Observe(_license, true);
                }
                return _license;
            }
        }

        #endregion

        #region LicenseCode

        [CrmProperty]
        [DataMember]
        public string LicenseCode
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

        #region LicenseTypeName

        [CrmProperty]
        [DataMember]
        public string LicenseTypeName
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

        static Type _properyEnumType = typeof(LicenseTypePropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region VoucherBlock

        OneToManyRelationship<LicenseType, VoucherBlock> _voucherBlock;
        public OneToManyRelationship<LicenseType, VoucherBlock> VoucherBlock
        {
            get
            {
                if (_voucherBlock == null)
                {
                    _voucherBlock = new OneToManyRelationship<LicenseType, VoucherBlock>(this, CrmRelationshipNames.LicenseTypeVoucherBlock, "new_licensetypId");
                    Observe(_voucherBlock, true);
                }
                return _voucherBlock;
            }
        }

        #endregion

        #endregion

        #region Methoden

        #region ToString

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(LicenseTypeName))
            {
                return LicenseTypeName;
            }

            return base.ToString();
        }

        #endregion

        #endregion

        #region DataContract

        #region DataContractVoucherBlockKey

        string _dataContractVoucherBlockKey;
        [DataMember(Name = "VoucherBlockKey")]
        public string DataContractVoucherBlockKey
        {
            get
            {
                if (string.IsNullOrEmpty(_dataContractVoucherBlockKey))
                {
                    return _dataContractVoucherBlockKey;
                }

                _dataContractVoucherBlockKey = GetAliasedValue<string>(CrmRelationshipNames.LicenseTypeVoucherBlock, CrmEntityNames.VoucherBlock, VoucherBlockPropertyId.Key);
                return _dataContractVoucherBlockKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.LicenseTypeVoucherBlock, CrmEntityNames.VoucherBlock, VoucherBlockPropertyId.Key, value);
                _dataContractVoucherBlockKey = value;
            }
        }

        #endregion

        #endregion

        #region Statische Eigenschaften

        #region None

        public static LicenseType None => CrmCache.LicenseTypesByCode["None"];

        #endregion

        #region Purchase

        public static LicenseType Purchase => CrmCache.LicenseTypesByCode["pur"];

        #endregion

        #region Subscription

        public static LicenseType Subscription => CrmCache.LicenseTypesByCode["sub"];

        #endregion

        #endregion
    }
}
