using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web.Authorization
{
    partial class AuthorizationManager
    {
        #region CheckSettingsAccessAsync

        async Task<bool> CheckSettingsAccessAsync(SwissAcademic.Authorization.AuthorizationContext context)
        {
            var resource = context.Resources["ResourceType"];

            switch (resource)
            {
                case nameof(AuthResource.ProjectSettings):
                    return (await CheckProjectDataAccessAsync(context, null)).Ok;

                case nameof(AuthResource.ProjectUserSettings):
                    return await CheckProjectUserSettingsAccessAsync(context);

                case nameof(AuthResource.UserSettings):
                    return CheckUserSettingsAccess(context);
            }

            return false;
        }

        #endregion

        #region CheckProjectUserSettingsAccessAsync

        async Task<bool> CheckProjectUserSettingsAccessAsync(SwissAcademic.Authorization.AuthorizationContext context)
        {
            (var result, _) = await CheckProjectDataAccessAsync(context, null);
            if (!result)
            {
                return false;
            }

            result = CheckUserSettingsAccess(context);
            if (!result)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region CheckUserSettingsAccess

        bool CheckUserSettingsAccess(SwissAcademic.Authorization.AuthorizationContext context)
        {
            return context.Resources[nameof(AuthResource.ContactKey)].Equals(context.Principal.GetContactKey(), StringComparison.Ordinal);
        }

        #endregion
    }
}
