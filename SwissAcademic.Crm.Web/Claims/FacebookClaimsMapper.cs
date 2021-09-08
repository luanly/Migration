
using IdentityModel;
using System.Collections.Generic;
using System.Security.Claims;

namespace SwissAcademic.Crm.Web
{
    internal class FacebookClaimsMapper
    {
        internal IEnumerable<Claim> Map(IEnumerable<Claim> facebookClaims)
        {
            var claims = new List<Claim>();

            var username = facebookClaims.GetClaimSave(JwtClaimTypes.Name);
            if (username != null)
            {
                claims.Add(new Claim(JwtClaimTypes.PreferredUserName, username.Value));
            }

            var email = facebookClaims.GetClaimSave(JwtClaimTypes.Email);
            if (email != null)
            {
                claims.Add(email);
            }

            var local = facebookClaims.GetClaimSave(JwtClaimTypes.Locale);
            if (local != null)
            {
                claims.Add(local);
            }

            var firstname = facebookClaims.GetClaimSave(JwtClaimTypes.GivenName);
            if (firstname != null)
            {
                claims.Add(firstname);
            }

            var lastname = facebookClaims.GetClaimSave(JwtClaimTypes.FamilyName);
            if (lastname != null)
            {
                claims.Add(lastname);
            }

            var middlename = facebookClaims.GetClaimSave(JwtClaimTypes.MiddleName);
            if (middlename != null)
            {
                claims.Add(middlename);
            }

            var gender = facebookClaims.GetClaimSave(JwtClaimTypes.Gender);
            if (gender != null)
            {
                claims.Add(gender);
            }

            if ((firstname == null || lastname == null) && username != null)
            {
                var m = username.Value.Split(' ');
                if (m.Length == 3)
                {
                    claims.Add(new Claim(JwtClaimTypes.GivenName, m[0] + " " + m[1]));
                    claims.Add(new Claim(JwtClaimTypes.FamilyName, m[2]));
                }
                else if (m.Length == 2)
                {
                    claims.Add(new Claim(JwtClaimTypes.GivenName, m[0]));
                    claims.Add(new Claim(JwtClaimTypes.FamilyName, m[1]));
                }
                else if (m.Length == 1)
                {
                    claims.Add(new Claim(JwtClaimTypes.FamilyName, m[0]));
                }
            }

            return claims;
        }
    }
}
