using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    public static class ClaimsExtensions
    {
        public static string GetFirstValue(this IEnumerable<Claim> claims, params string[] types)
        {
            if (claims == null)
            {
                return null;
            }

            foreach (var type in types)
            {
                var claim = claims.FirstOrDefault(i => i.Type == type);
                if (claim == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(claim.Value))
                {
                    return claim.Value;
                }
            }
            return null;
        }

        public static Claim GetClaimSave(this IEnumerable<Claim> claims, string type)
        {
            var result = claims?.FirstOrDefault(i => i.Type == type);
            if (result != null)
            {
                return result;
            }

            return claims?.FirstOrDefault(i => i.Type == $"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{type}");
        }

        public static string GetClientId(this IEnumerable<Claim> claims)
        {
            var result = claims?.FirstOrDefault(i => i.Type == "client_id");
            if (result != null)
            {
                return result.Value;
            }
            return string.Empty;
        }
    }
}