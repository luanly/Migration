using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.LinkedEmailAccount)]
    [DataContract]
    public class LinkedEmailAccount
        :
        CitaviCrmEntity,
        IHasVerificationData
    {
        #region Konstruktor

        public LinkedEmailAccount()
            :
            base(CrmEntityNames.LinkedEmailAccount)
        {

        }

        #endregion

        #region Eigenschaften

        #region BounceStatus

        /// <summary>
        /// Wenn > 0 sind Bounces vorhanden. 
        /// </summary>
        [CrmProperty]
        public int BounceStatus
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

        #region Contact

        ManyToOneRelationship<LinkedEmailAccount, Contact> _contact;
        public ManyToOneRelationship<LinkedEmailAccount, Contact> Contact
        {
            get
            {
                if (_contact == null)
                {
                    _contact = new ManyToOneRelationship<LinkedEmailAccount, Contact>(this, CrmRelationshipNames.ContactLinkedEMailAccount, CrmRelationshipLookupNames.ContactLinkedEmailAccount);

                    Observe(_contact, true);
                }
                return _contact;
            }
        }

        #endregion

        #region Email

        [CrmProperty(IsPrimaryAttribute = true)]
        [DataMember]
        public string Email
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

        #region IsCampusEmail

        [DataMember]
        public bool IsCampusEmail
        {
            get;
            set;
        }

        #endregion

        #region IsVerified

        [CrmProperty]
        [DataMember]
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

        #region BounceStatus

        [CrmProperty]
        public string LastBounceReason
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

        #region LastLogin

        [CrmProperty]
        public DateTime? LastLogin
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

        static Type _properyEnumType = typeof(LinkedEmailAccountPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
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

        #region VerificationCampusGroup

        /// <summary>
        /// Mit dieser CampusGroup hat sich der Kunde angemeldet
        /// </summary>
        [CrmProperty]
        [DataMember]
        public CampusGroup? VerificationCampusGroup
        {
            get
            {
                return GetValue<CampusGroup?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region VerificationIPAddress

        /// <summary>
        /// Wir bei der Anmeldung für die Lizenzen benötigt
        /// </summary>
        [CrmProperty]
        public string VerificationIPAddress
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
        [DataMember]
        public DateTime? VerificationKeySent
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

        #region VerificationPurpose

        [CrmProperty]
        public VerificationKeyPurpose? VerificationPurpose
        {
            get
            {
                return GetValue<VerificationKeyPurpose?>();
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

        #region VerificationVocherCode

        /// <summary>
        /// Wir bei der Anmeldung für die Lizenzen benötigt
        /// </summary>
        [CrmProperty]
        public string VerificationVocherCode
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

        #region ClearVerifcationData

        internal void ClearVerifcationData()
        {
            VerificationVocherCode = null;
            VerificationIPAddress = null;
            VerificationKey = null;
            VerificationPurpose = null;
            VerificationKeySent = null;
            VerificationStorage = null;
        }

		#endregion

		#region IsEqual

        public bool IsEqual(string email)
		{
            return string.Equals(email, Email, StringComparison.InvariantCultureIgnoreCase);
		}

        #endregion

        #region ToString

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Email))
            {
                return Email;
            }

            return base.ToString();
        }

        #endregion

        #endregion

        #region DataContract

        #region DataContractContactKey

        string _dataContractContactKey;

        [DataMember(Name = "ContactKey")]
        public string DataContractContactKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractContactKey))
                {
                    return _dataContractContactKey;
                }

                _dataContractContactKey = GetAliasedValue<string>(CrmRelationshipNames.ContactLinkedEMailAccount, CrmEntityNames.Contact, ContactPropertyId.Key);
                return _dataContractContactKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.ContactLinkedEMailAccount, CrmEntityNames.Contact, ContactPropertyId.Key, value);
                _dataContractContactKey = value;

            }
        }

        #endregion

        #region DataContractCreatedOn

        [DataMember(Name = "CreatedOn")]
        public DateTime DataContractCreatedOn
        {
            get
            {
                return CreatedOn;
            }
            set
            {
                CreatedOn = value;
            }
        }

        #endregion

        #endregion
    }
}
