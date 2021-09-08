using IdentityModel;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace SwissAcademic.Crm.Web
{
    public class CrmClaimsToUserMapper
    {
        public void Map(CrmUser user, IEnumerable<Claim> userClaims)
        {
            if (userClaims == null)
            {
                return;
            }
            if (!userClaims.Any())
            {
                Telemetry.TrackTrace("UserClaims are empty", SeverityLevel.Warning);
                return;
            }

            var contact = user.Contact;
            var issuer = userClaims.First().Issuer.ToLowerInvariant();
            IEnumerable<Claim> claims = null;
            var dic = new Dictionary<string, object>();
            var s = new StringBuilder($"Claims from {issuer}:");
            foreach (var claim in userClaims)
            {
                dic[claim.Type] = claim.Value;
            }

            claims = Map(issuer, userClaims);

            var email = claims.GetFirstValue(JwtClaimTypes.Email, SAML2ClaimTypes.Email, ClaimTypes.Email);
            if (email == null)
            {
                email = userClaims.GetFirstValue(JwtClaimTypes.Email, SAML2ClaimTypes.Email, ClaimTypes.Email);
            }

            var firstname = claims.GetFirstValue(JwtClaimTypes.GivenName, ClaimTypes.GivenName);
            if (firstname == null)
            {
                firstname = userClaims.GetFirstValue(JwtClaimTypes.GivenName, ClaimTypes.GivenName);
            }

            var lastname = claims.GetFirstValue(JwtClaimTypes.FamilyName, ClaimTypes.Surname);
            if (lastname == null)
            {
                lastname = userClaims.GetFirstValue(JwtClaimTypes.FamilyName, ClaimTypes.Surname);
            }

            var gender = claims.GetFirstValue(JwtClaimTypes.Gender, ClaimTypes.Gender);
            if (gender == null)
            {
                gender = userClaims.GetFirstValue(JwtClaimTypes.Gender, ClaimTypes.Gender);
            }

            if (!string.IsNullOrEmpty(gender))
            {
                switch (gender.ToLowerInvariant())
                {
                    case "male":
                        contact.GenderCode = GenderCodeType.Male;
                        break;

                    case "female":
                        contact.GenderCode = GenderCodeType.Female;
                        break;

                    default:
                        contact.GenderCode = GenderCodeType.Unknown;
                        break;
                }
            }
            contact.FirstName = firstname;
            contact.LastName = lastname;

            if (!string.IsNullOrEmpty(email))
            {
                user.AddLinkedEMailAccount(email, true);
            }
            var local = claims.GetFirstValue(JwtClaimTypes.Locale, ClaimTypes.Locality);
            if (local == null)
            {
                local = claims.GetFirstValue(JwtClaimTypes.Locale, ClaimTypes.Locality);
            }

            if (!string.IsNullOrEmpty(local))
            {
                try
                {
                    if (Enum.TryParse<LanguageType>(local, out var lng))
                    {
                        contact.ChangeLanguage(lng);
                    }
                }
                catch (Exception ignored)
                {
                    Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                }
            }
        }

        public IEnumerable<Claim> Map(string provider, IEnumerable<Claim> providerClaims)
        {
            switch (provider.ToLowerInvariant())
            {
                case "facebook":
                    {
                        return new FacebookClaimsMapper().Map(providerClaims);
                    }

                case "google":
                    {
                        return new GoogleClaimsMapper().Map(providerClaims);
                    }

                case "microsoft":
                    {
                        return new MicrosoftClaimsMapper().Map(providerClaims);
                    }

                default:
                    {
                        return new ShibbolethClaimsMapper().Map(providerClaims);
                    }

            }
        }
    }
}
