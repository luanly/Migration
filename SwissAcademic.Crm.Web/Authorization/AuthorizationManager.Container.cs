using SwissAcademic.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web.Authorization
{
    partial class AuthorizationManager
    {
        async Task<bool> CheckContainerAccessAsync(AuthorizationContext context)
        {
            var resource = context.Resources.First().Value;

            switch (resource)
            {
                case nameof(AuthResource.ProjectContainer):
                    {
                        if (!context.Resources.ContainsKey(nameof(AuthResource.ProjectKey)))
                        {
                            context.Resources.Add(nameof(AuthResource.ProjectKey), context.Resources[nameof(AuthResource.ContainerName)]);
                        }

                        switch (context.Actions.First().Value)
                        {
                            case nameof(AuthAction.Read):
                                return (await CheckProjectDataAccessAsync(context, null)).Ok;
                        }
                    }
                    break;

                case nameof(AuthResource.TemporaryContainerRead):
                    {
                        switch (context.Actions.First().Value)
                        {
                            case nameof(AuthAction.Read):
                                return true;
                        }
                    }
                    break;

                case nameof(AuthResource.TemporaryContainerWrite):
                    return true;

                case nameof(AuthResource.UserContainer):
                    {
                        if (!context.Resources.ContainsKey(nameof(AuthResource.ContactKey)))
                        {
                            context.Resources.Add(nameof(AuthResource.ContactKey), context.Resources[nameof(AuthResource.ContainerName)]);
                        }


                        switch (context.Actions.First().Value)
                        {
                            case nameof(AuthAction.Read):
                                return CheckUserSettingsAccess(context);
                        }
                    }
                    break;
            }

            return false;
        }
    }
}
