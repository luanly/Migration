using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.LicenseFile)]
    [DataContract]
    public class LicenseFile
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public LicenseFile()
            :
            base(CrmEntityNames.LicenseFile)
        {

        }

        #endregion

        #region Eigenschaften

        #region Account

        ManyToOneRelationship<LicenseFile, Account> _account;
        public ManyToOneRelationship<LicenseFile, Account> Account
        {
            get
            {
                if (_account == null)
                {
                    _account = new ManyToOneRelationship<LicenseFile, Account>(this, CrmRelationshipNames.AccountLicenseFile, "new_accountid");
                    Observe(_account, true);
                }
                return _account;
            }
        }

        #endregion

        #region Ammount

        [CrmProperty]
        public int Ammount
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

        #region CampusContract

        ManyToOneRelationship<LicenseFile, CampusContract> _campusContract;
        public ManyToOneRelationship<LicenseFile, CampusContract> CampusContract
        {
            get
            {
                if (_campusContract == null)
                {
                    _campusContract = new ManyToOneRelationship<LicenseFile, CampusContract>(this, CrmRelationshipNames.CampusContractLicenseFile, "new_campuscontractid");
                    Observe(_campusContract, true);
                }
                return _campusContract;
            }
        }

        #endregion

        #region Contacts

        ManyToManyRelationship<LicenseFile, Contact> _contacts;
        public ManyToManyRelationship<LicenseFile, Contact> Contacts
        {
            get
            {
                if (_contacts == null)
                {
                    _contacts = new ManyToManyRelationship<LicenseFile, Contact>(this, CrmRelationshipNames.LicenseFileContacts);
                    Observe(_contacts, true);
                }
                return _contacts;
            }
        }

        #endregion

        #region ExportAccessFiles

        [CrmProperty]
        public bool ExportAccessFiles
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

        #region License

        ManyToOneRelationship<LicenseFile, CitaviLicense> _license;
        public ManyToOneRelationship<LicenseFile, CitaviLicense> License
        {
            get
            {
                if (_license == null)
                {
                    _license = new ManyToOneRelationship<LicenseFile, CitaviLicense>(this, CrmRelationshipNames.LicenseLicenseFile, "new_citavilicenseopid");
                    Observe(_license, true);
                }
                return _license;
            }
        }

        #endregion

        #region LicenseFileType

        [CrmProperty]
        public LicenseFileType? LicenseFileType
        {
            get
            {
                return GetValue<LicenseFileType?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(LicenseFilePropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region ShortInfo

        [CrmProperty]
        public string ShortInfo
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

        #region Voucherblock

        ManyToOneRelationship<LicenseFile, VoucherBlock> _voucherblock;
        public ManyToOneRelationship<LicenseFile, VoucherBlock> Voucherblock
        {
            get
            {
                if (_voucherblock == null)
                {
                    _voucherblock = new ManyToOneRelationship<LicenseFile, VoucherBlock>(this, CrmRelationshipNames.VoucherBlockLicenseFile, "new_voucherblockid");
                    Observe(_voucherblock, true);
                }
                return _voucherblock;
            }
        }

        #endregion

        #endregion
    }
}
