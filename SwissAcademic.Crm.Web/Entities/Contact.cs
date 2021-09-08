using SwissAcademic.Azure;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.Contact)]
    [DataContract]
    public class Contact
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public Contact()
            :
            base(CrmEntityNames.Contact)
        {

        }

        #endregion

        #region Eigenschaften

        #region Address1_Fax

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string Address1_Fax
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

        #region Address1 Line1

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string Address1_Line1
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

        #region Address1 Line2

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string Address1_Line2
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

        #region Address1_PostalCode

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string Address1_PostalCode
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

        #region Address1_Country

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string Address1_Country
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

        #region Address1_Composite

        [CrmProperty(IsBuiltInAttribute = true, NoCache = true)]
        public string Address1_Composite
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

        #region Address1_StateOrProvince

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string Address1_StateOrProvince
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

        #region Address1_Telephone1

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string Address1_Telephone1
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

        #region Address1_Telephone2

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string Address1_Telephone2
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

        #region AllowLargeImports

        [CrmProperty]
        [DataMember]
        public bool AllowLargeImports
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

        #region CampusBenefitEligibility

        [CrmProperty]
        [DataMember]
        public CampusBenefitEligibilityType? CampusBenefitEligibility
        {
            get
            {
                return GetValue<CampusBenefitEligibilityType?>();
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                SetValue(value);
            }
        }

        #endregion

        #region CloudSpaceWarningSent

        [CrmProperty]
        [DataMember]
        public CloudSpaceWarningSentType CloudSpaceWarningSent
        {
            get
            {
                return GetValue<CloudSpaceWarningSentType>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ClosedDate

        [CrmProperty]
        public DateTime? ClosedDate
        {
            get
            {
                return GetValue<DateTime?>();
            }
            set
            {
                if (value == DateTime.MinValue)
                {
                    value = null;
                }

                SetValue(value);
            }
        }

        #endregion

        #region ContactCreatedByOrdermail

        [CrmProperty]
        public bool ContactCreatedByOrdermail
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

        #region EMailAddress

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string EMailAddress1
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

        #region EmailDomain

        [CrmProperty]
        public string EmailDomain
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

        #region EndUserLicenses

        OneToManyRelationship<Contact, CitaviLicense> _endUserlicenses;
        public OneToManyRelationship<Contact, CitaviLicense> EndUserLicenses
        {
            get
            {
                if (_endUserlicenses == null)
                {
                    _endUserlicenses = new OneToManyRelationship<Contact, CitaviLicense>(this, CrmRelationshipNames.ContactEndUserLicense, "new_enduserid");
                    Observe(_endUserlicenses, true);
                }
                return _endUserlicenses;
            }
        }

        #endregion

        #region EntityImage

        [CrmProperty(NoCache = true, IsBuiltInAttribute = true)]
        public byte[] EntityImage
        {
            get
            {
                return GetValue<byte[]>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region FailedLoginCount

        [CrmProperty]
        public int? FailedLoginCount
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

        #region FailedPasswordResetCount

        [CrmProperty]
        public int? FailedPasswordResetCount
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

        #region FailedRedeemVoucherCount

        [CrmProperty]
        public int FailedRedeemVoucherCount
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

        #region FirstName

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string FirstName
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

        #region Firm

        [CrmProperty]
        [DataMember]
        public string Firm
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

        #region FullName

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(FirstName) &&
                    !string.IsNullOrEmpty(LastName))
                {
                    return string.Concat(FirstName, " ", LastName);
                }
                else if (!string.IsNullOrEmpty(LastName))
                {
                    return LastName;
                }
                else if (!string.IsNullOrEmpty(FirstName))
                {
                    return FirstName;
                }
                else if (!string.IsNullOrEmpty(EMailAddress1))
                {
                    return EMailAddress1;
                }
                else
                {
                    return Resources.Strings.UsernameMissing;
                }
            }
        }

        #endregion

        #region GenderCode

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public GenderCodeType? GenderCode
        {
            get
            {
                return GetValue<GenderCodeType?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region HashedPassword

        [CrmProperty]
        public string HashedPassword
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

        #region HasPassword

        [DataMember]
        public bool HasPassword
        {
            get
            {
                return !string.IsNullOrEmpty(HashedPassword);
            }
        }

        #endregion

        #region HasUserSettingsWE

        [CrmProperty]
        public bool HasUserSettingsWE
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

        #region IsKeyContact

        [DataMember]
        [CrmProperty]
        public bool IsKeyContact
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

        #region IsLoginAllowed

        [CrmProperty]
        public bool? IsLoginAllowed
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

        #region IsSasAdmin

        [CrmProperty]
        public bool IsSasAdmin
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

        #region IsVerified

        [CrmProperty]
        public bool? IsVerified
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

        #region Language

        /// <summary>
        /// Set Language -> Contact.ChangeLanguage()
        /// </summary>
        [CrmProperty]
        [DataMember]
        public LanguageType? Language
        {
            get
            {
                var optionSet = GetValue<LanguageType?>();
                if (optionSet == null)
                {
                    return LanguageType.English;
                }

                return optionSet.Value;
            }
            private set
            {
                if (value == null)
                {
                    value = LanguageType.English;
                }

                SetValue(value);
            }
        }

        public string LanguageTwoDigits
        {
            get
            {
                if (Language == null)
                {
                    return "en";
                }

                switch (Language)
                {
                    case LanguageType.French:
                        return "fr";

                    case LanguageType.German:
                        return "de";

                    case LanguageType.Italian:
                        return "it";

                    case LanguageType.Polish:
                        return "pl";

                    case LanguageType.Portuguese:
                        return "pt";

                    case LanguageType.Spanish:
                        return "es";

                    default:
                    case LanguageType.English:
                        return "en";
                }
            }
        }

        #endregion

        #region LastName

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string LastName
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

        #region LastModifiedOn

        [CrmProperty]
        [DataMember]
        public DateTime? LastModifiedOn
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

        #region LastFailedLogin

        [CrmProperty]
        public DateTime? LastFailedLogin
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

        #region LastFailedPasswordReset

        [CrmProperty]
        public DateTime? LastFailedPasswordReset
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

        #region LastFailedRedeemVoucher

        [CrmProperty]
        public DateTime? LastFailedRedeemVoucher
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

        #region LastLogin

        /// <summary>
        /// Last Desktop Login
        /// </summary>
        [CrmProperty]
        [DataMember]
        public DateTime? LastLogin
        {
            get
            {
                return GetValue<DateTime?>();
            }
            private set
            {
                SetValue(value);
            }
        }

        #endregion

        #region LastLoginCitaviWeb

        /// <summary>
        /// Last Web Login
        /// </summary>
        [CrmProperty]
        [DataMember]
        public DateTime? LastLoginCitaviWeb
        {
            get
            {
                return GetValue<DateTime?>();
            }
            private set
            {
                SetValue(value);
            }
        }

        #endregion

        #region LastLoginWordAssistant

        /// <summary>
        /// Last Assistant Login
        /// </summary>
        [CrmProperty]
        [DataMember]
        public DateTime? LastLoginWordAssistant
        {
            get
            {
                return GetValue<DateTime?>();
            }
            private set
            {
                SetValue(value);
            }
        }

        #endregion

        #region LastLoginPicker

        /// <summary>
        /// Last Picker Login
        /// </summary>
        [CrmProperty]
        [DataMember]
        public DateTime? LastLoginPicker
        {
            get
            {
                return GetValue<DateTime?>();
            }
            private set
            {
                SetValue(value);
            }
        }

        #endregion

        #region LinkedAccounts

        OneToManyRelationship<Contact, LinkedAccount> _linkedAccounts;
        public OneToManyRelationship<Contact, LinkedAccount> LinkedAccounts
        {
            get
            {
                if (_linkedAccounts == null)
                {
                    _linkedAccounts = new OneToManyRelationship<Contact, LinkedAccount>(this, CrmRelationshipNames.ContactLinkedAccount);
                    Observe(_linkedAccounts, true);
                }
                return _linkedAccounts;
            }
        }

        #endregion

        #region LinkedEMailAccounts

        OneToManyRelationship<Contact, LinkedEmailAccount> _linkedEMailAccounts;
        public OneToManyRelationship<Contact, LinkedEmailAccount> LinkedEMailAccounts
        {
            get
            {
                if (_linkedEMailAccounts == null)
                {
                    _linkedEMailAccounts = new OneToManyRelationship<Contact, LinkedEmailAccount>(this, CrmRelationshipNames.ContactLinkedEMailAccount);
                    Observe(_linkedEMailAccounts, true);
                }
                return _linkedEMailAccounts;
            }
        }

        #endregion

        #region MergeAccountVerificationKey

        [CrmProperty]
        public string MergeAccountVerificationKey
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

        #region MergeAccountVerificationKeySent

        [CrmProperty]
        [DataMember]
        public DateTime? MergeAccountVerificationKeySent
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

        #region NewsletterAccounts

        ManyToManyRelationship<Contact, Account> _newsletterAccounts;
        public ManyToManyRelationship<Contact, Account> NewsletterAccounts
        {
            get
            {
                if (_newsletterAccounts == null)
                {
                    _newsletterAccounts = new ManyToManyRelationship<Contact, Account>(this, CrmRelationshipNames.AccountNewsletterContact);
                    Observe(_newsletterAccounts, true);
                }
                return _newsletterAccounts;
            }
        }

        #endregion

        #region NickName

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string NickName
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

        #region OwnerLicenses

        /// <summary>
        /// Bestelle Lizenzen
        /// </summary>
        OneToManyRelationship<Contact, CitaviLicense> _ownerlicenses;
        public OneToManyRelationship<Contact, CitaviLicense> OwnerLicenses
        {
            get
            {
                if (_ownerlicenses == null)
                {
                    _ownerlicenses = new OneToManyRelationship<Contact, CitaviLicense>(this, CrmRelationshipNames.ContactOwnerLicense, "new_contactid");
                    Observe(_ownerlicenses, true);
                }
                return _ownerlicenses;
            }
        }

        #endregion

        #region PasswordChanged

        [CrmProperty]
        public DateTime? PasswordChanged
        {
            get
            {
                return GetValue<DateTime?>();
            }
            set
            {
                if (value == DateTime.MinValue)
                {
                    value = null;
                }

                SetValue(value);
            }
        }

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(ContactPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region ProjectRoles

        OneToManyRelationship<Contact, ProjectRole> _projectRoles;
        public OneToManyRelationship<Contact, ProjectRole> ProjectRoles
        {
            get
            {
                if (_projectRoles == null)
                {
                    _projectRoles = new OneToManyRelationship<Contact, ProjectRole>(this, CrmRelationshipNames.ContactProjectRole);
                    Observe(_projectRoles, true);
                }
                return _projectRoles;
            }
        }

        #endregion

        #region ResellerAffiliateId

        [CrmProperty]
        [DataMember]
        public string ResellerAffiliateId
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

        #region RequiresPasswordReset

        [CrmProperty]
        public bool? RequiresPasswordReset
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

        #region Salutation

        [CrmProperty(IsBuiltInAttribute = true)]
        [DataMember]
        public string Salutation
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

        #region Subscriptions

        OneToManyRelationship<Contact, Subscription> _subscriptions;
        /// <summary>
        /// Subscription wo dieser Contact Owner ist
        /// </summary>
        public OneToManyRelationship<Contact, Subscription> Subscriptions
        {
            get
            {
                if (_subscriptions == null)
                {
                    _subscriptions = new OneToManyRelationship<Contact, Subscription>(this, CrmRelationshipNames.SubscriptionContact, "new_subscriptionid");
                    Observe(_subscriptions, true);
                }
                return _subscriptions;
            }
        }

        #endregion

        #region SoftBounceCounter

        [CrmProperty]
        public int? SoftBounceCounter
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

        #region TitelDefinedByContact

        [CrmProperty]
        [DataMember]
        public string TitelDefinedByContact
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Length > 99)
                {
                    value = value.Substring(0, 99);
                }
                SetValue(value);
            }
        }

        #endregion

        #region ZuoraAccountId

        [CrmProperty]
        [DataMember]
        public string ZuoraAccountId
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

        #region AzureB2CId

        [CrmProperty]
        [DataMember]
        public string AzureB2CId { get; set; }

        #endregion

        #endregion

        #region Methoden

        #region BuildLicenseName

        internal string BuildLicenseName()
        {
            return FullName;
        }

        #endregion

        #region ChangeLanguage

        public void ChangeLanguage(LanguageType? language)
        {
            Language = language;
        }

        #endregion

        #region GetCultureInfo

        internal CultureInfo GetCultureInfo()
        {
            return new CultureInfo((int)Language);
        }

		#endregion

		#region SetLastLogin

        public bool SetLastLogin(string clientId, DateTime? dateTime = null)
		{
			if (dateTime == null)
			{
                dateTime = DateTime.UtcNow.Date;
			}

			switch (clientId)
			{
                case ClientIds.Desktop:
                    if(LastLogin == dateTime)
					{
                        return false;
					}
                    LastLogin = dateTime;
                    break;

                case ClientIds.WebWordAddIn:
                    if (LastLoginWordAssistant == dateTime)
                    {
                        return false;
                    }
                    LastLoginWordAssistant = dateTime;
                    break;

                case ClientIds.EdgePicker:
                case ClientIds.GoogleChromePicker:
                case ClientIds.FirefoxPicker:
                    if (LastLoginPicker == dateTime)
                    {
                        return false;
                    }
                    LastLoginPicker = dateTime;
                    break;

                case ClientIds.Web:
                    if (LastLoginCitaviWeb == dateTime)
                    {
                        return false;
                    }
                    LastLoginCitaviWeb = dateTime;
                    break;

                default:
                    return false;
            }
            return true;
		}

		#endregion

		#region ToString

		[ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(FullName))
            {
                return FullName;
            }

            return base.ToString();
        }

        #endregion

        #endregion
    }
}
