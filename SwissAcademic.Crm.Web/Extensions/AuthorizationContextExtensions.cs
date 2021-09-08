using SwissAcademic.Authorization;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class AuthorizationContextExtensions
    {
        public static async Task<CrmUser> GetCrmUserAsync(this AuthorizationContext authorizationContext, CrmDbContext context)
        {
            if(authorizationContext.Principal is CrmUser)
			{
                return authorizationContext.Principal as CrmUser;
            }

            var contactKey = System.Security.Claims.ClaimsPrincipalExtensions.GetContactKey(authorizationContext.Principal);
            CrmUser crmUser = null;
            if (authorizationContext.Items.ContainsKey(HttpContextExtensions.CrmUser))
            {
                crmUser = authorizationContext.Items[HttpContextExtensions.CrmUser] as CrmUser;
            }
            if (crmUser != null &&
                contactKey != crmUser.Key)
            {
                crmUser = null;
            }
            if (crmUser == null)
            {
                if (context == null)
                {
                    using (var ctx = new CrmDbContext())
                    {
                        crmUser = await ctx.GetByKeyAsync(contactKey, updateCacheIfMissing: true);
                    }
                }
                else
                {
                    crmUser = await context.GetByKeyAsync(contactKey, updateCacheIfMissing: true);
                }

                if (crmUser != null)
                {
                    authorizationContext.Items[HttpContextExtensions.CrmUser] = crmUser;
                }
            }
            return crmUser;
        }

        public static async Task<ProjectEntry> GetProjectAsync(this AuthorizationContext authorizationContext)
        {
            var projectKey = authorizationContext.Resources[nameof(SwissAcademic.Crm.Web.Authorization.AuthResource.ProjectKey)];
            if (authorizationContext.Items.ContainsKey(HttpContextExtensions.Project))
            {
                var projectEntry = authorizationContext.Items[HttpContextExtensions.Project] as ProjectEntry;
                if (projectEntry.Key == projectKey)
                {
                    return projectEntry;
                }
            }
            var p = await CrmCache.Projects.GetAsync(projectKey);
            if (p != null)
            {
                authorizationContext.Items[HttpContextExtensions.Project] = p;
            }
            return p;
        }
    }
}
