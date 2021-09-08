using SwissAcademic.ApplicationInsights;
using SwissAcademic.Authorization;
using SwissAcademic.Azure;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web.Authorization
{
    public partial class AuthorizationManager
        :
        AuthorizationManagerBase
    {
        #region Constructors

        public AuthorizationManager()
        {
        }

        #endregion

        #region Properties

        #region Instance

        public static AuthorizationManager Instance { get; } = new AuthorizationManager();

        #endregion

        #endregion

        #region Methods

        #region CheckAccessAsync

        public override async Task<bool> CheckAccessAsync(SwissAcademic.Authorization.AuthorizationContext context)
        {
            if (!RequireAuthentication(context))
            {
                return Forbid();
            }
            var crmUser = await context.GetCrmUserAsync(null);
            if (crmUser == null)
            {
                return Forbid();
            }
            if (!crmUser.IsLoginAllowed)
            {
                return Forbid();
            }
            var result = false;

            var resource = context.Resources["ResourceType"];

            switch (resource)
            {
                case nameof(AuthResource.CrmContact):
                case nameof(AuthResource.CrmProjectEntry):
                case nameof(AuthResource.CrmProjectRole):
                case nameof(AuthResource.CrmLinkedAccount):
                case nameof(AuthResource.CrmLinkedEmailAccount):
                case nameof(AuthResource.CrmCampusOrganizationSetting):
                    using (var dbContext = new CrmDbContext())
                    {
                        result = await CheckCrmAccessAsync(dbContext, context);
                    }
                    break;

                case nameof(AuthResource.ProjectData):
                    (result, _) = await CheckProjectDataAccessAsync(context, null);
                    break;

                case nameof(AuthResource.ProjectContainer):
                case nameof(AuthResource.TemporaryContainerRead):
                case nameof(AuthResource.TemporaryContainerWrite):
                case nameof(AuthResource.UserContainer):
                    result = await CheckContainerAccessAsync(context);
                    break;

                case nameof(AuthResource.ProjectSettings):
                case nameof(AuthResource.ProjectUserSettings):
                case nameof(AuthResource.UserSettings):
                    result = await CheckSettingsAccessAsync(context);
                    break;
            }

            if (result == false)
            {
                return Forbid();
            }

            return true;
        }

        #endregion

        #region CheckProjectCompatibility

        async Task<(bool Ok, ProjectEntry ProjectEntry)> CheckProjectCompatibilityAsync(SwissAcademic.Authorization.AuthorizationContext context)
        {
            var projectKey = context.Resources[nameof(AuthResource.ProjectKey)];
            if (string.IsNullOrEmpty(projectKey))
            {
                throw new NotSupportedException("ProjectKey must not be null");
            }

            var projectEntry = await context.GetProjectAsync();

            if (projectEntry == null)
            {
                Telemetry.TrackTrace($"Project not found: {projectKey}", SeverityLevel.Warning);
                return (NotFound(), null);
            }
            if (projectEntry.OnlineStatus != ProjectOnlineStatus.Online)
            {
                Telemetry.TrackTrace($"Project not Online ({projectEntry.OnlineStatus}): {projectKey}", SeverityLevel.Warning);
                return (NotFound(), null);
            }
            if (projectEntry.DeletedOn != null)
            {
                Telemetry.TrackTrace($"Project deleted ({projectEntry.DeletedOn}): {projectKey}", SeverityLevel.Warning);
                return (NotFound(), null);
            }

            if (projectEntry.MinClientVersion == null)
            {
                return (true, projectEntry);
            }
            if (projectEntry.MinClientVersion == ProjectEntry.MinProjectVersion)
            {
                return (true, projectEntry);
            }

            if (!context.Resources.ContainsKey(nameof(AuthResource.ClientVersion)))
            {
                return (true, projectEntry);
            }

            var clientVersion = context.Resources[nameof(AuthResource.ClientVersion)];
            if (string.IsNullOrEmpty(clientVersion))
            {
                return (true, projectEntry);
            }

            return (new Version(clientVersion) >= projectEntry.MinClientVersion, projectEntry);

        }

        #endregion

        #region GetProjectRole

        async Task<ProjectRole> GetProjectRoleAsync(SwissAcademic.Authorization.AuthorizationContext context)
        {
            var contactKey = context.Principal.GetContactKey();
            if (string.IsNullOrEmpty(contactKey))
            {
                Telemetry.TrackTrace($"Principal is not authorized.", SeverityLevel.Warning);
                return null;
            }
            var projectKey = context.Resources[nameof(AuthResource.ProjectKey)];

            var crmUser = await context.GetCrmUserAsync(null);
            if (crmUser == null)
            {
                Telemetry.TrackTrace($"User not found. {projectKey}/{contactKey}", SeverityLevel.Warning);
                return null;
            }
            if (!crmUser.IsLoginAllowed)
            {
                Telemetry.TrackTrace($"User IsLoginAllowed false. {projectKey}/{contactKey}", SeverityLevel.Warning);
                return null;
            }

            var existing = crmUser.ProjectRoles.FirstOrDefault(i => i.DataContractProjectKey == projectKey);
            if (existing != null)
            {
                return existing;
            }

            ProjectRoleType? projectRoleType;

            var projectRole = await ProjectRole.Get(projectKey, contactKey);

            if (projectRole != null)
            {
                projectRoleType = projectRole.ProjectRoleType.Value;
                crmUser.ProjectRoles.Add(projectRole);
                await CrmUserCache.AddOrUpdateAsync(crmUser);
            }
            else
            {
                Telemetry.TrackTrace($"ProjectRole not found. {projectKey}/{contactKey}", SeverityLevel.Warning);
            }
            return projectRole;
        }

        #endregion

        #endregion
    }
}
