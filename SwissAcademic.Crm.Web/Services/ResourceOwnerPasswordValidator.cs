using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class ResourceOwnerPasswordValidator
        :
        IResourceOwnerPasswordValidator
    {
        IHttpContextAccessor _httpContextAccessor;

        public ResourceOwnerPasswordValidator(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var authService = new CrmAuthenticationService();
            await authService.AuthenticateLocalAsync(context, _httpContextAccessor, null);
        }
    }
}
