using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

namespace SwissAcademic.Crm.Web.Authentication.MicrosoftAccount
{
    [ExcludeFromCodeCoverage]
    public class MicrosoftAccountOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="MicrosoftAccountOptions"/>.
        /// </summary>
        public MicrosoftAccountOptions()
        {
            CallbackPath = new PathString("/signin-microsoft");
            AuthorizationEndpoint = MicrosoftAccountDefaults.AuthorizationEndpoint;
            TokenEndpoint = MicrosoftAccountDefaults.TokenEndpoint;
            UserInformationEndpoint = MicrosoftAccountDefaults.UserInformationEndpoint;
            Scope.Add("wl.basic");
            Scope.Add("wl.emails");

            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            ClaimActions.MapJsonKey("name", "name");
            ClaimActions.MapJsonKey("given_name", "first_name");
            ClaimActions.MapJsonKey("family_name", "last_name");
            ClaimActions.MapJsonSubKey("email", "emails", "account");
            ClaimActions.MapJsonSubKey("email", "emails", "preferred");
        }

        public bool IsTiP { get; set; }
    }
}
