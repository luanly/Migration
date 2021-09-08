using Aspose.Words.Drawing;
using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using System;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.Project)]
    [DataContract]
    public class ProjectEntry
        :
        CitaviCrmEntity
    {
        #region Felder

        internal static Version MinProjectVersion = new Version(1, 0, 0, 0);

        #endregion

        #region Konstruktor

        public ProjectEntry()
            :
            base(CrmEntityNames.Project)
        {

        }

        #endregion

        #region Eigenschaften

        #region DataCenter

        [CrmProperty]
        [DataMember]
        public DataCenter DataCenter
        {
            get
            {
                var optionSet = GetValue<DataCenter?>();
                if (optionSet == null)
                {
                    return DataCenter.WestEurope;
                }
                return optionSet.Value;
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region DataSource

        [CrmProperty]
        public string DataSource
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

        #region DeletedOn

        [CrmProperty]
        [DataMember]
        public DateTime? DeletedOn
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

        #region InitialCatalog

        [CrmProperty]
        public string InitialCatalog
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

        #region InitialCatalogBeforeMigration

        [CrmProperty]
        public string InitialCatalogBeforeMigration
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

        #region MinClientVersion

        [CrmProperty]
        [DataMember]
        public Version MinClientVersion
        {
            get
            {
                var val = GetValue<string>();
                if (string.IsNullOrEmpty(val))
                {
                    return MinProjectVersion;
                }

                return new Version(val);
            }
            set
            {
                if (value == null)
                {
                    SetValue<Version>(null);
                }
                else
                {
                    SetValue(value.ToString(4));
                }
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

        #region OnlineStatus

        [CrmProperty]
        [DataMember]
        public ProjectOnlineStatus OnlineStatus
        {
            get
            {
                var optionSet = GetValue<ProjectOnlineStatus?>();
                if (optionSet == null)
                {
                    return ProjectOnlineStatus.Online;
                }

                return optionSet.Value;
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ProjectRoles

        OneToManyRelationship<ProjectEntry, ProjectRole> _projectRoles;
        [ODataQuery("ProjectRole")]
        public OneToManyRelationship<ProjectEntry, ProjectRole> ProjectRoles
        {
            get
            {
                if (_projectRoles == null)
                {
                    _projectRoles = new OneToManyRelationship<ProjectEntry, ProjectRole>(this, CrmRelationshipNames.ProjectProjectRole);
                    Observe(_projectRoles, true);
                }
                return _projectRoles;
            }
        }

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(ProjectEntryPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region ProjectLastModifiedOn

        /// <summary>
        /// Tag der letzte Änderungen am Citavi Project
        /// </summary>
        [CrmProperty]
        [DataMember]
        public DateTime? ProjectLastModifiedOn
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

        #region RecoveryKey

        [CrmProperty]
        public string RecoveryKey
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

        #region ShortName

        string _shortName;
        public string ShortName
        {
            get
            {
                if (string.IsNullOrEmpty(_shortName))
                {
                    if (Name.Length > 50)
                    {
                        _shortName = Name.TruncateAtWordBoundary(50, true);
                    }
                    else
                    {
                        _shortName = Name;
                    }

                    if (_shortName.Length > 50)
                    {
                        _shortName = Name.Truncate(50, true);
                    }
                }
                return _shortName;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region GetProjectOwner

        public async Task<Contact> GetProjectOwner(params Enum[] contactPropertyIds)
        {
            var projectRoles = await ProjectRoles.Get(ProjectRolePropertyId.ProjectRoleType);
            foreach (var projectRole in projectRoles)
            {
                if (projectRole.ProjectRoleType == Azure.ProjectRoleType.Owner)
                {
                    return await projectRole.Contact.Get(contactPropertyIds);
                }
            }
            return null;
        }

        #endregion

        #region SetRecoveryKey

        public void SetRecoveryKey()
        {
            DeletedOn = DateTime.UtcNow;
            RecoveryKey = Security.PasswordGenerator.WebKey.Generate();
        }

        #endregion

        #region ToString

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Key))
            {
                return Key;
            }

            return base.ToString();
        }

        #endregion

        #region UpdateProjectLastModified

        public async Task UpdateProjectLastModified()
		{
            using(var context = new CrmDbContext())
			{
                await UpdateProjectLastModified(context);
            }
		}
        public async Task UpdateProjectLastModified(CrmDbContext context)
		{
            try
            {
                if (ProjectLastModifiedOn != DateTime.UtcNow.Date)
                {
                    if (_context == null)
                    {
                        context.Attach(this);
                    }
                    ProjectLastModifiedOn = DateTime.UtcNow.Date;
                    var updateCache = CrmCache.Projects.AddOrUpdateAsync(this);
                    var saveChanges = context.SaveAsync();
                    await Task.WhenAll(updateCache, saveChanges);
                }
            }
            catch(Exception ignored)
			{
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
			}
		}

        #endregion

        #endregion

        #region DataContract

        #region DataCenterShortName

        [DataMember]
        public string DataCenterShortName => AzureRegionResolver.Instance.GetShortName(DataCenter);

        #endregion

        #region DataContractOwnerContactKey

        string _dataContractOwnerContactKey;

        [DataMember(Name = "OwnerContactKey")]
        public string DataContractOwnerContactKey
        {
            get
            {
                return _dataContractOwnerContactKey;
            }
            set
            {
                _dataContractOwnerContactKey = value;
            }
        }

        #endregion

        #endregion
    }
}
