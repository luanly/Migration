#if Web
using SwissAcademic;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;

namespace System.Security.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        public const string CitaviKey = "citavikey";

        public static void AddClaim(this ClaimsPrincipal claimsPrincipal, Claim claim)
        {
            claimsPrincipal.Identities.First().AddClaim(claim);
        }
        public static void AddClaim(this ClaimsPrincipal claimsPrincipal, string type, string value)
        {
            claimsPrincipal.Identities.First().AddClaim(new Claim(type, value));
        }

        public static void AddOrUpdateClaim(this ClaimsPrincipal claimsPrincipal, string type, string value)
        {
            var identity = claimsPrincipal.Identities.First();
            var existingClaim = identity.Claims.FirstOrDefault(i => i.Type == type);
            if (existingClaim != null) identity.RemoveClaim(existingClaim);
            identity.AddClaim(new Claim(type, value));
        }

        public static void RemoveClaim(this ClaimsPrincipal claimsPrincipal, Claim claim)
        {
            var identity = claimsPrincipal.Identities.FirstOrDefault();
            if (identity == null) return;
            identity.RemoveClaim(claim);
        }

        public static string GetContactKey(this IPrincipal principal)
        {
            var contactKey = ((ClaimsPrincipal)principal).Claims.FirstOrDefault(i => i.Type == "sub")?.Value;
            return contactKey;
        }
        public static string GetContactKey(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.FirstOrDefault(i => i.Type == "sub")?.Value;
        }

        public static string GetClaim(this IPrincipal principal, string claimType)
        {
            if (principal == null) return null;
            return ((ClaimsPrincipal)principal).Claims.FirstOrDefault(i => i.Type == claimType)?.Value;
        }

        public static void RemoveClaimsByType(this ClaimsPrincipal principal, string claimType)
        {
            if (string.IsNullOrEmpty(claimType)) return;
            var identity = principal.Identities.FirstOrDefault();
            if (identity == null) return;

            var allClaims = principal.Claims.Where(i => i.Type == claimType).ToList();

            allClaims.ForEach(i => identity.RemoveClaim(i));
        }
    }
}

#endif
