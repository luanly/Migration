using IdentityModel;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SwissAcademic.Crm.Web
{
    internal class ShibbolethClaimsMapper
    {
        internal IEnumerable<Claim> Map(IEnumerable<Claim> msClaims)
        {
            var list = new List<Claim>();
            foreach (var claim in msClaims.ToList())
            {
                var mapped = Map(claim);
                if (mapped == null)
                {
                    continue;
                }

                list.Add(mapped);
            }
            return list;
        }

        internal Claim Map(Claim claim)
        {
            switch (claim.Type)
            {
                #region CommonName

                case SAML1ClaimTypes.CommonName:
                case SAML2ClaimTypes.CommonName:
                    return new Claim(JwtClaimTypes.Name, claim.Value, claim.ValueType, claim.Issuer);

                #endregion

                #region DisplayName

                case SAML1ClaimTypes.DisplayName:
                case SAML2ClaimTypes.DisplayName:
                    return new Claim(JwtClaimTypes.Name, claim.Value, claim.ValueType, claim.Issuer);

                #endregion

                #region Email

                case SAML1ClaimTypes.Email:
                case SAML2ClaimTypes.Email:
                    return new Claim(JwtClaimTypes.Email, claim.Value, claim.ValueType, claim.Issuer);

                #endregion

                #region GivenName

                case SAML1ClaimTypes.GivenName:
                case SAML2ClaimTypes.GivenName:
                    return new Claim(JwtClaimTypes.GivenName, claim.Value, claim.ValueType, claim.Issuer);

                #endregion

                #region ShibbolethIssuer

                case SAML2ClaimTypes.ShibbolethIssuer:
                    return claim;

                #endregion

                #region Surname

                case SAML1ClaimTypes.Surname:
                case SAML2ClaimTypes.Surname:
                    return new Claim(JwtClaimTypes.FamilyName, claim.Value, claim.ValueType, claim.Issuer);

                    #endregion
            }
            return null;
        }
    }
}
