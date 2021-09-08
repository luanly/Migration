using Newtonsoft.Json;
using SwissAcademic.Azure;
using SwissAcademic.Crm.Web.Query.FetchXml;
using SwissAcademic.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.ProjectRole)]
    [DataContract]
    public class ProjectRole
        :
        CitaviCrmEntity
    {
        #region Ereignisse

        #region OnRelationshipChanged

        protected override void OnRelationshipChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    {
                        if (e.NewItems.Count == 1)
                        {
                            var newItem = e.NewItems[0];
                            if (newItem is ProjectEntry)
                            {
                                _dataContractProjectEntry = newItem as ProjectEntry;
                            }
                            else if (newItem is Contact)
                            {
                                var contact = newItem as Contact;
                                DataContractContactKey = contact.Key;
                            }
                        }
                    }
                    break;
            }
        }

        #endregion

        #endregion

        #region Konstruktor

        public ProjectRole()
            :
            base(CrmEntityNames.ProjectRole)
        {

        }

        #endregion

        #region Eigenschaften

        #region Confirmed

        [CrmProperty]
        [DataMember]
        public bool Confirmed
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

        #region ConfirmationKey

        [CrmProperty]
        public string ConfirmationKey
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

        #region ConfirmationKeySent

        [CrmProperty]
        [DataMember]
        public DateTime? ConfirmationKeySent
        {
            get
            {
                return GetValue<DateTime>();
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

        #region ConfirmationKeyStorage

        [CrmProperty]
        public string ConfirmationKeyStorage
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

        #region Contact

        ManyToOneRelationship<ProjectRole, Contact> _contact;
        public ManyToOneRelationship<ProjectRole, Contact> Contact
        {
            get
            {
                if (_contact == null)
                {
                    _contact = new ManyToOneRelationship<ProjectRole, Contact>(this, CrmRelationshipNames.ContactProjectRole);
                    Observe(_contact, true);
                }
                return _contact;
            }
        }

        #endregion

        #region ProjectRoleType

        [CrmProperty]
        [DataMember]
        public ProjectRoleType? ProjectRoleType
        {
            get
            {
                return GetValue<ProjectRoleType?>();
            }
            set
            {
                SetValue(value);
                Name = value.ToString();
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

        #region Project

        ManyToOneRelationship<ProjectRole, ProjectEntry> _project;
        public ManyToOneRelationship<ProjectRole, ProjectEntry> Project
        {
            get
            {
                if (_project == null)
                {
                    _project = new ManyToOneRelationship<ProjectRole, ProjectEntry>(this, CrmRelationshipNames.ProjectProjectRole);
                    Observe(_project, true);
                }
                return _project;
            }
        }

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(ProjectRolePropertyId);
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

        #region ToString

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (ProjectRoleType != null)
            {
                if (!string.IsNullOrEmpty(DataContractProjectName))
                {
                    return $"{ProjectRoleType.ToString()} in {DataContractProjectName}";
                }

                return ProjectRoleType.ToString();
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

                _dataContractContactKey = GetAliasedValue<string>(CrmRelationshipNames.ContactProjectRole, CrmEntityNames.Contact, ContactPropertyId.Key);
                return _dataContractContactKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.ContactProjectRole, CrmEntityNames.Contact, ContactPropertyId.Key, value);
                _dataContractContactKey = value;
            }
        }

        #endregion

        #region DataContractProjectKey

        ProjectEntry _dataContractProjectEntry;

        [DataMember(Name = "ProjectKey")]
        public string DataContractProjectKey
        {
            get
            {
                if (_dataContractProjectEntry != null)
                {
                    return _dataContractProjectEntry.Key;
                }

                return GetAliasedValue<string>(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.Key);
            }
            set
            {
                if (_dataContractProjectEntry != null)
                {
                    _dataContractProjectEntry.Key = value;
                }
                SetAliasedValue(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.Key, value);
            }
        }

        #endregion

        #region DataContractProjectDataSource

        [CacheDataMember(Name = "ProjectDataSource")]
        public string DataContractProjectDataSource
        {
            get
            {
                if (_dataContractProjectEntry != null)
                {
                    return _dataContractProjectEntry.DataSource;
                }

                return GetAliasedValue<string>(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.DataSource);
            }
            set
            {
                if (_dataContractProjectEntry != null)
                {
                    _dataContractProjectEntry.DataSource = value;
                }
                SetAliasedValue(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.DataSource, value);
            }
        }

        #endregion

        #region DataContractProjectMinClientVersion

        [DataMember(Name = "ProjectMinClientVersion")]
        public Version DataContractProjectMinClientVersion
        {
            get
            {
                if (_dataContractProjectEntry != null)
                {
                    return _dataContractProjectEntry.MinClientVersion;
                }

                return GetAliasedValue<Version>(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.MinClientVersion);
            }
            set
            {
                if (_dataContractProjectEntry != null)
                {
                    _dataContractProjectEntry.MinClientVersion = value;
                }
                SetAliasedValue(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.MinClientVersion, value);
            }
        }

        #endregion

        #region DataContractProjectOnlineStatus

        [DataMember(Name = "ProjectOnlineStatus")]
        public ProjectOnlineStatus? DataContractProjectOnlineStatus
        {
            get
            {
                if (_dataContractProjectEntry != null)
                {
                    return _dataContractProjectEntry.OnlineStatus;
                }

                var val = GetAliasedValue<ProjectOnlineStatus?>(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.OnlineStatus);
                if (val == null)
                {
                    return null;
                }

                return val.Value;
            }
            set
            {
                if (_dataContractProjectEntry != null)
                {
                    _dataContractProjectEntry.OnlineStatus = value.GetValueOrDefault();
                }
                if (value == null)
                {
                    SetAliasedValue(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.OnlineStatus, value);
                }
                else
                {
                    SetAliasedValue(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.OnlineStatus, value);
                }
            }
        }

        #endregion

        #region DataContractProjectDeletedOn

        [DataMember(Name = "ProjectDeletedOn")]
        public DateTime? DataContractProjectDeletedOn
        {
            get
            {
                if (_dataContractProjectEntry != null)
                {
                    return _dataContractProjectEntry.DeletedOn;
                }

                return GetAliasedValue<DateTime?>(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.DeletedOn);
            }
            set
            {
                if (_dataContractProjectEntry != null)
                {
                    _dataContractProjectEntry.DeletedOn = value;
                }
                SetAliasedValue(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.DeletedOn, value);
            }
        }

        #endregion

        #region DataContractProjectName

        [DataMember(Name = "ProjectName")]
        public string DataContractProjectName
        {
            get
            {
                if (_dataContractProjectEntry != null)
                {
                    return _dataContractProjectEntry.Name;
                }

                return GetAliasedValue<string>(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.Name);
            }
            set
            {
                if (_dataContractProjectEntry != null)
                {
                    _dataContractProjectEntry.Name = value;
                }
                SetAliasedValue(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.Name, value);
            }
        }

        #endregion

        #region DataContractProjectDataCenter

        [DataMember(Name = "ProjectDataCenter")]
        public DataCenter DataContractProjectDataCenter
        {
            get
            {
                if (_dataContractProjectEntry != null)
                {
                    return _dataContractProjectEntry.DataCenter;
                }

                var dataCenter = GetAliasedValue<DataCenter?>(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.DataCenter);
				if (!dataCenter.HasValue)
				{
                    return DataCenter.WestEurope;
				}
                return dataCenter.Value;
            }
            set
            {
                if (_dataContractProjectEntry != null)
                {
                    _dataContractProjectEntry.DataCenter = value;
                }
                SetAliasedValue(CrmRelationshipNames.ProjectProjectRole, CrmEntityNames.Project, ProjectEntryPropertyId.DataCenter, value);
            }
        }

        #endregion

        #region DataContractColorSchemeIdentifier

        [DataMember(Name = "ColorSchemeIdentifier")]
        public ColorSchemeIdentifier? DataContractColorSchemeIdentifier
        {
            get; set;
        }

        #endregion

        #endregion

        #region Statische Methoden

        #region GetProjectRoleType

        public static async Task<ProjectRole> Get(string projectKey, string contactKey)
        {
            using (var context = new CrmDbContext())
            {
                return await Get(projectKey, contactKey, context);
            }
        }
        public static async Task<ProjectRole> Get(string projectKey, string contactKey, CrmDbContext context)
        {
            var xml = new Query.FetchXml.GetProjectRole(contactKey, projectKey).TransformText();
            var result = await context.FetchFirstOrDefault<ProjectRole>(xml);
            return result;
        }

        /// <summary>
        /// Hier werden auch die "DeletedOn"-Projekte zurückgegeben
        /// Diese sind bei User.ProjectRoles nicht vorhanden
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ProjectRole>> GetUserProjectRoles(Guid contactId, CrmDbContext context)
        {
            var xml = new Query.FetchXml.GetUserProjectRoles(contactId).TransformText();
            var result = await context.Fetch(FetchXmlExpression.Create<Contact>(xml));
            return new CrmSet(result).ProjectRoles;
        }

        public static async Task<ProjectRole> Get(string projectRoleKey)
        {
            using (var context = new CrmDbContext())
            {
                return await Get(projectRoleKey, context);
            }
        }
        public static async Task<ProjectRole> Get(string projectRoleKey, CrmDbContext context)
        {
            var xml = new Query.FetchXml.GetProjectRoleByKey(projectRoleKey).TransformText();
            var result = await context.FetchFirstOrDefault<ProjectRole>(xml);
            return result;
        }


        public static async Task<ProjectRole> GetByConfirmationKey(string confirmationKey)
        {
            using (var context = new CrmDbContext())
            {
                return await GetByConfirmationKey(confirmationKey, context);
            }
        }
        public static async Task<ProjectRole> GetByConfirmationKey(string confirmationKey, CrmDbContext context)
        {
            var query = new GetProjectRoleByConfirmationKey(confirmationKey).TransformText();
            var result = (await context.Fetch<ProjectRole>(query))?.FirstOrDefault();
            return result;
        }

        #endregion

        #endregion
    }
}
