using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class PrincipalExtensions
    {
        public async static Task<CrmUser> GetCrmUserAsync(this IPrincipal principal)
        {
            var contactKey = principal?.GetContactKey();
            if (string.IsNullOrEmpty(contactKey))
            {
                return null;
            }

            using (var context = new CrmDbContext())
            {
                return await context.GetByKeyAsync(contactKey);
            }
        }
    }
}