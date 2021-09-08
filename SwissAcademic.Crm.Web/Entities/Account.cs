using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.Account)]
    [DataContract]
    public class Account
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public Account()
            :
            base(CrmEntityNames.Account)
        {

        }

        #endregion

        #region Eigenschaften

        #region Address1_City

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string Address1_City
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

        #region CampusCities

        /// <summary>
        /// Standorte
        /// </summary>
        [CrmProperty]
        [DataMember]
        public string CampusCities
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

        #region CBPartnerId

        [CrmProperty]
        public string CBPartnerId
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

        #region CBUserName

        [CrmProperty]
        public string CBUserName
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

        #region CBPartnerTyp

        [CrmProperty]
        public AccountPartnerType? CBPartnerTyp
        {
            get
            {

                return GetValue<AccountPartnerType?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region CleverbrigeAffiliateId

        [CrmProperty]
        public string CleverbrigeAffiliateId
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

        #region CampusContracts

        /// <summary>
        /// Verträge
        /// </summary>
        OneToManyRelationship<Account, CampusContract> _campusContracts;
        [ODataQuery("CampusContract")]
        public OneToManyRelationship<Account, CampusContract> CampusContracts
        {
            get
            {
                if (_campusContracts == null)
                {
                    _campusContracts = new OneToManyRelationship<Account, CampusContract>(this, CrmRelationshipNames.AccountCampusContract, "new_accountid");
                    Observe(_campusContracts, true);
                }
                return _campusContracts;
            }
        }

        #endregion

        #region DataCenter

        [CrmProperty]
        [DataMember]
        public DataCenter DataCenter
        {
			get
			{
				var dataCenter = GetValue<DataCenter?>();
				if (!dataCenter.HasValue)
				{
					return DataCenter.WestEurope;
				}
				return dataCenter.Value;
			}
			set
			{
				SetValue(value);
			}
		}

        #endregion

        #region EMailDomains

        OneToManyRelationship<Account, EmailDomain> _emailDomains;
        [ODataQuery("EMailDomain")]
        public OneToManyRelationship<Account, EmailDomain> EMailDomains
        {
            get
            {
                if (_emailDomains == null)
                {
                    _emailDomains = new OneToManyRelationship<Account, EmailDomain>(this, CrmRelationshipNames.AccountEMailDomain, "new_accountid");
                    Observe(_emailDomains, true);
                }
                return _emailDomains;
            }
        }

        #endregion

        #region ExclusiveCountries

        [CrmProperty]
        public string ExclusiveCountries
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

        #region IPRanges

        OneToManyRelationship<Account, IPRange> _ipRanges;
        public OneToManyRelationship<Account, IPRange> IPRanges
        {
            get
            {
                if (_ipRanges == null)
                {
                    _ipRanges = new OneToManyRelationship<Account, IPRange>(this, CrmRelationshipNames.AccountIPRange, "new_accountid");
                    Observe(_ipRanges, true);
                }
                return _ipRanges;
            }
        }

        #endregion

        #region Name

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string Name
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

        #region NewsletterContacts

        ManyToManyRelationship<Account, Contact> _newsletterContacts;
        public ManyToManyRelationship<Account, Contact> NewsletterContacts
        {
            get
            {
                if (_newsletterContacts == null)
                {
                    _newsletterContacts = new ManyToManyRelationship<Account, Contact>(this, CrmRelationshipNames.AccountNewsletterContact);
                    Observe(_newsletterContacts, true);
                }
                return _newsletterContacts;
            }
        }

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(AccountPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region SendCampusStatistic

        [CrmProperty]
        public bool SendCampusStatistic
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

        #region VoucherBlocks

        OneToManyRelationship<Account, VoucherBlock> _voucherBlocks;
        public OneToManyRelationship<Account, VoucherBlock> VoucherBlocks
        {
            get
            {
                if (_voucherBlocks == null)
                {
                    _voucherBlocks = new OneToManyRelationship<Account, VoucherBlock>(this, CrmRelationshipNames.AccountVoucherBlock, "new_accountid");
                    Observe(_voucherBlocks, true);
                }
                return _voucherBlocks;
            }
        }

        #endregion

        #endregion

        #region Methoden

        #region ToString

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }

            return base.ToString();
        }

        #endregion

        #endregion

        #region Statische Methoden

        #region GetResellerByExclusiveCountries

        public static async Task<string> GetAffiliateIdByExclusiveCountries(string countryTwoLetterIsoName, CrmDbContext context)
        {
            if (string.IsNullOrEmpty(countryTwoLetterIsoName))
            {
                return null;
            }

            if (context == null)
            {
                return null;
            }

            try
            {
                var xml = new Query.FetchXml.GetResellerByExclusiveCountries(countryTwoLetterIsoName).TransformText();
                var result = await context.FetchFirstOrDefault<Account>(xml);
                return result?.CleverbrigeAffiliateId;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat, property1: (nameof(TelemetryProperty.Description), countryTwoLetterIsoName));
                return null;
            }
        }

        #endregion

        #endregion
    }
}
