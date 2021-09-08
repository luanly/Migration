using SwissAcademic.Azure;
using System;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.OrganizationSetting)]
    [DataContract]
    public class OrganizationSetting
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public OrganizationSetting()
            :
            base(CrmEntityNames.OrganizationSetting)
        {

        }

        #endregion

        #region Eigenschaften

        #region AccountName

        [DataMember]
        public string AccountName { get; set; }

        #endregion

        #region BlobContainerUrl

        [CrmProperty]
        [DataMember]
        public string BlobContainerUrl
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

        #region BlobName

        [CrmProperty]
        [DataMember]
        public string BlobName
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

        #region CampusContract

        ManyToOneRelationship<OrganizationSetting, CampusContract> _campusContract;
        public ManyToOneRelationship<OrganizationSetting, CampusContract> CampusContract
        {
            get
            {
                if (_campusContract == null)
                {
                    _campusContract = new ManyToOneRelationship<OrganizationSetting, CampusContract>(this, CrmRelationshipNames.OrganizationSettingsCampusContract, "new_campuscontractid");
                    Observe(_campusContract, true);
                }
                return _campusContract;
            }
        }

        #endregion

        #region Consent

        [CrmProperty]
        public OrganizationSettingConsent? Consent
        {
            get
            {
                return GetValue<OrganizationSettingConsent?>();
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

        #region Description

        [CrmProperty]
        [DataMember]
        public string Description
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

        #region IsPublic

        [CrmProperty]
        [DataMember]
        public bool IsPublic
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

        #region ModifiedOn

        [DataMember]
        [CrmProperty(IsBuiltInAttribute = true)]
        public new DateTime ModifiedOn
        {
            get
            {
                return base.ModifiedOn;
            }
            set
            {
                base.ModifiedOn = value;
            }
        }

        #endregion

        #region Name

        [CrmProperty]
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

        #region PropertyEnumType

        static Type _properyEnumType = typeof(OrganizationSettingPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region SasToken

        [DataMember]
        public string SasToken
        {
            get;
            set;
        }

        #endregion

        #region UpdatedOn

        [CrmProperty]
        [DataMember]
        public DateTime UpdatedOn
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

        #endregion

        #region DataContract

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

                _dataContractCampusContractKey = GetAliasedValue<string>(CrmRelationshipNames.OrganizationSettingsCampusContract, CrmEntityNames.CampusContract, CampusContractPropertyId.Key);
                return _dataContractCampusContractKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.OrganizationSettingsCampusContract, CrmEntityNames.CampusContract, CampusContractPropertyId.Key, value);
                _dataContractCampusContractKey = value;
            }
        }

        #endregion

        #endregion
    }
}
