using IdentityServer4.Models;
using SwissAcademic.Authorization;
using SwissAcademic.Azure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web.Authorization
{
    partial class AuthorizationManager
    {
        public async Task<(bool Ok, ProjectEntry projectEntry)> CheckProjectDataAccessAsync(CrmUser user, IDictionary<object, object> items, AuthAction action, string projectKey, string clientVersion = null, string clientId = null)
        { 
            var clientVersionAuthResource = clientVersion == null ? null : AuthResource.ClientVersion(clientVersion);

            var context = new SwissAcademic.Authorization.AuthorizationContext(user,
                items,
                action,
                AuthResource.ProjectData,
                 AuthResource.ProjectKey(projectKey),
                 clientVersionAuthResource);

            return await CheckProjectDataAccessAsync(context, clientId);
        }

        async Task<(bool Ok, ProjectEntry projectEntry)> CheckProjectDataAccessAsync(AuthorizationContext context, string clientId)
        {
            ProjectEntry projectEntry;
            bool ok;

            (ok, projectEntry) = await CheckProjectCompatibilityAsync(context);

            if (!ok)
            {
                return (false, null);
            }

            var projectRole = await GetProjectRoleAsync(context);
            if (projectRole == null)
            {
                return (NotFound(), null);
            }

            if (!projectRole.Confirmed)
            {
                return (ProjectRoleNotConfirmed(), null);
            }

            switch (context.Actions["ActionType"])
            {
                case nameof(AuthAction.Create):
                case nameof(AuthAction.Delete):
                case nameof(AuthAction.Update):
                    {
						if (!string.IsNullOrEmpty(clientId))
						{
                            var user = await context.GetCrmUserAsync(null);
                            var hasWriteAccess = user.HasWriteLicense(clientId);
							if (!hasWriteAccess)
							{
                                return (ProjectMissingWritePermission(), null);
                            }
						}

                        switch (projectRole.ProjectRoleType)
                        {
                            case ProjectRoleType.Author:
                            case ProjectRoleType.Manager:
                            case ProjectRoleType.Owner:
                                return (true, projectEntry);
                        }
                        return (ProjectMissingWritePermission(), null);
                    }

                case nameof(AuthAction.Read):
                    {
                        if (!string.IsNullOrEmpty(clientId))
                        {
                            var user = await context.GetCrmUserAsync(null);
                            var hasWriteAccess = user.HasReadLicense(clientId);
                            if (!hasWriteAccess)
                            {
                                return (ProjectMissingReadPermission(), null);
                            }
                        }
                        switch (projectRole.ProjectRoleType)
                        {
                            case ProjectRoleType.Author:
                            case ProjectRoleType.Manager:
                            case ProjectRoleType.Reader:
                            case ProjectRoleType.Owner:
                                return (true, projectEntry);
                        }
                    }
                    break;
            }

            return (false, null);
        }
    }
}
