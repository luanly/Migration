using IdentityModel;
using System.Collections.Generic;
using System.Security.Claims;

namespace SwissAcademic.Crm.Web
{
    internal class GoogleClaimsMapper
    {
        internal IEnumerable<Claim> Map(IEnumerable<Claim> googleClaims)
        {
            var claims = new List<Claim>();

            var username = googleClaims.GetClaimSave(ClaimTypes.Name);
            if (username != null)
            {
                claims.Add(new Claim(JwtClaimTypes.PreferredUserName, username.Value));
            }

            var email = googleClaims.GetClaimSave(ClaimTypes.Email);
            if (email != null)
            {
                claims.Add(new Claim(JwtClaimTypes.Email, email.Value));
            }

            var firstname = googleClaims.GetClaimSave(ClaimTypes.GivenName);
            if (firstname != null)
            {
                claims.Add(new Claim(JwtClaimTypes.GivenName, firstname.Value));
            }

            var gender = googleClaims.GetClaimSave(ClaimTypes.Gender);
            if (gender != null)
            {
                claims.Add(new Claim(JwtClaimTypes.Gender, gender.Value));
            }

            var lastname = googleClaims.GetClaimSave(ClaimTypes.Surname);
            if (lastname != null)
            {
                claims.Add(new Claim(JwtClaimTypes.FamilyName, lastname.Value));
            }

            return claims;
        }
    }
}
