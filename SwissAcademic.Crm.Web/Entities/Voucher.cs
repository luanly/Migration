using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.Voucher)]
    [DataContract]
    public class Voucher
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public Voucher()
            :
            base(CrmEntityNames.Voucher)
        {

        }

        #endregion

        #region Eigenschaften

        #region Contact

        /// <summary>
        /// Eingelöst von
        /// </summary>
        ManyToOneRelationship<Voucher, Contact> _contact;
        public ManyToOneRelationship<Voucher, Contact> Contact
        {
            get
            {
                if (_contact == null)
                {
                    _contact = new ManyToOneRelationship<Voucher, Contact>(this, CrmRelationshipNames.ContactVoucher, "new_contactid");
                    Observe(_contact, true);
                }
                return _contact;
            }
        }

        #endregion

        #region License

        ManyToOneRelationship<Voucher, CitaviLicense> _license;
        public ManyToOneRelationship<Voucher, CitaviLicense> License
        {
            get
            {
                if (_license == null)
                {
                    _license = new ManyToOneRelationship<Voucher, CitaviLicense>(this, CrmRelationshipNames.LicenseVoucher, "new_citavilicenseid");
                    Observe(_license, true);
                }
                return _license;
            }
        }

        #endregion

        #region RedeemedOn

        /// <summary>
        /// Eingelöst am
        /// </summary>
        [CrmProperty]
        public DateTime? RedeemedOn
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

        #region PropertyEnumType

        static Type _properyEnumType = typeof(VoucherPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region VoucherBlock

        ManyToOneRelationship<Voucher, VoucherBlock> _voucherblock;
        public ManyToOneRelationship<Voucher, VoucherBlock> VoucherBlock
        {
            get
            {
                if (_voucherblock == null)
                {
                    _voucherblock = new ManyToOneRelationship<Voucher, VoucherBlock>(this, CrmRelationshipNames.VoucherVoucherBlock, "new_voucherblockid");
                    Observe(_voucherblock, true);
                }
                return _voucherblock;
            }
        }

        #endregion

        #region VoucherCode

        [CrmProperty]
        public string VoucherCode
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

        #region VoucherCodeInt

        [CrmProperty]
        public int? VoucherCodeInt
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

        #region VoucherCodePre

        /// <summary>
        /// Gutschein Code Preformatiert
        /// </summary>
        [CrmProperty]
        public string VoucherCodePre
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

        #region VoucherStatus

        [CrmProperty]
        public VoucherStatus? VoucherStatus
        {
            get
            {
                return GetValue<VoucherStatus?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #endregion

        #region Methoden

        #region ToString

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(VoucherCode))
            {
                return VoucherCode;
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
                if (!string.IsNullOrEmpty(_dataContractVoucherBlockKey))
                {
                    return _dataContractVoucherBlockKey;
                }

                _dataContractVoucherBlockKey = GetAliasedValue<string>(CrmRelationshipNames.VoucherVoucherBlock, CrmEntityNames.VoucherBlock, VoucherBlockPropertyId.Key);
                return _dataContractVoucherBlockKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.VoucherVoucherBlock, CrmEntityNames.VoucherBlock, VoucherBlockPropertyId.Key, value);
                _dataContractVoucherBlockKey = value;
            }
        }

        #endregion

        #endregion

        #region Statische Methoden

        public static async Task<VoucherStatus?> GetVoucherStatus(string voucherCode, CrmDbContext context)
        {
            var query = new Query.FetchXml.GetVoucherStatus(voucherCode).TransformText();
            var voucher = await context.FetchFirstOrDefault<Voucher>(query);
            if (voucher == null)
            {
                return null;
            }

            return voucher.VoucherStatus;
        }

        #endregion
    }
}
