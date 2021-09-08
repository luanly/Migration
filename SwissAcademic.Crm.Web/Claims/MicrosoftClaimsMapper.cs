using IdentityModel;
using System.Collections.Generic;
using System.Security.Claims;

namespace SwissAcademic.Crm.Web
{
    internal class MicrosoftClaimsMapper
    {
        internal IEnumerable<Claim> Map(IEnumerable<Claim> msClaims)
        {
            var claims = new List<Claim>();

            var username = msClaims.GetClaimSave(JwtClaimTypes.Name);
            if (username != null)
            {
                claims.Add(new Claim(JwtClaimTypes.PreferredUserName, username.Value));
            }
            else
            {
                username = msClaims.GetClaimSave(JwtClaimTypes.PreferredUserName);
                if (username != null)
                {
                    claims.Add(username);
                }
            }
            var email = msClaims.GetClaimSave(JwtClaimTypes.Email);
            if (email != null)
            {
                claims.Add(email);
            }

            var gender = msClaims.GetClaimSave(JwtClaimTypes.Gender);
            if (gender != null)
            {
                claims.Add(gender);
            }

            var local = msClaims.GetClaimSave(JwtClaimTypes.Locale);
            if (local != null)
            {
                claims.Add(local);
            }

            var firstname = msClaims.GetClaimSave("first_name");
            if (firstname != null)
            {
                claims.Add(new Claim(JwtClaimTypes.GivenName, firstname.Value));
            }

            var lastname = msClaims.GetClaimSave("last_name");
            if (lastname != null)
            {
                claims.Add(new Claim(JwtClaimTypes.FamilyName, lastname.Value));
            }

            return claims;
        }
    }
}
