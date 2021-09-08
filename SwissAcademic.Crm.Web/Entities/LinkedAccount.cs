using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.LinkedAccount)]
    [DataContract]
    public class LinkedAccount
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public LinkedAccount()
            :
            base(CrmEntityNames.LinkedAccount)
        {

        }

        #endregion

        #region Eigenschaften

        #region Contact

        ManyToOneRelationship<LinkedAccount, Contact> _contact;
        public ManyToOneRelationship<LinkedAccount, Contact> Contact
        {
            get
            {
                if (_contact == null)
                {
                    _contact = new ManyToOneRelationship<LinkedAccount, Contact>(this, CrmRelationshipNames.ContactLinkedAccount);
                    Observe(_contact, true);
                }
                return _contact;
            }
        }

        #endregion

        #region EduPersonEntitlement

        [CrmProperty]
        public string EduPersonEntitlement
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

        #region EduPersonScopedAffiliation

        [CrmProperty]
        public string EduPersonScopedAffiliation
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

        #region IdentityProviderId

        [CrmProperty]
        [DataMember]
        public string IdentityProviderId
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

        #region LastModifiedOn

        [CrmProperty]
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

        #region Name

        [CrmProperty(IsPrimaryAttribute = true)]
        public string Name
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                SetValue(value);
            }
        }

        #endregion

        #region NameIdentifier

        [CrmProperty]
        public string NameIdentifier
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

        static Type _properyEnumType = typeof(LinkedAccountPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

		#endregion

		#endregion

		#region Methoden

		#region UpdateShibbolethProperties

		internal void UpdateShibbolethProperties(IEnumerable<Claim> claims)
        {
            try
            {
                EduPersonEntitlement = claims.Where(c => c.Type == SAML2ClaimTypes.PersonEntitlement).Select(c => c.Value).ToString(";");
                EduPersonScopedAffiliation = claims.SelectMany(c => SAML2ClaimTypes.GetAffiliationClaims(c)).ToString(";");
            }
            catch(Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
            }
        }

        #endregion

        #region ToString

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Name;
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

                _dataContractContactKey = GetAliasedValue<string>(CrmRelationshipNames.ContactLinkedAccount, CrmEntityNames.Contact, ContactPropertyId.Key);
                return _dataContractContactKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.ContactLinkedAccount, CrmEntityNames.Contact, ContactPropertyId.Key, value);
                _dataContractContactKey = value;

            }
        }

        #endregion

        #region DataContractIdentityProvider

        [DataMember(Name = "IdentityProvider")]
        public string DataContractIdentityProvider
        {
            get
            {
                return Name;
            }
        }

        #endregion

        #endregion
    }
}
