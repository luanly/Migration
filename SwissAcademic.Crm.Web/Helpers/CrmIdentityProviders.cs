using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.Metadata;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class CrmIdentityProviders
    {
        public const string Shibboleth = "Shibboleth";
        public const string Google = "Google";
        public const string Facebook = "Facebook";
        public const string Microsoft = "Microsoft";
        public const string TiPSuffix = "-TiP";

        public static readonly string[] All = new[]
        {
            Facebook,
            Google,
            Microsoft,
            Shibboleth
        };

        #region Methoden

        #region GetDisplayName

        public static string GetDisplayName(string idpId)
        {
            switch (idpId)
            {
                case IdentityProviderNames.Facebook:
                case IdentityProviderNames.Google:
                case IdentityProviderNames.Microsoft:
                case IdentityProviderNames.Yahoo:
                    return idpId;

                default:
                    return ShibbolethIdentityProviderStore.GetDisplayName(idpId);
            }
        }

        #endregion

        #region IsShibbolethProvider

        public static bool IsShibbolethProvider(string idpId)
        {
            if (string.IsNullOrEmpty(idpId))
            {
                return false;
            }

            switch (idpId)
            {
                case IdentityProviderNames.Facebook:
                case IdentityProviderNames.Google:
                case IdentityProviderNames.Microsoft:
                case IdentityProviderNames.Yahoo:
                    return false;

                default:
                    return true;
            }
        }

        #endregion

        #endregion
    }
}
