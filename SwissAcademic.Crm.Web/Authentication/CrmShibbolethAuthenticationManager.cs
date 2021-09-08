using Sustainsys.Saml2.WebSso;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;

namespace SwissAcademic.Crm.Web
{
    public class CrmShibbolethAuthenticationManager
    {
        static List<string> DefaultNameIdentifierTypes = new List<string>
        {
            SAML2ClaimTypes.PersistantNameIDFormat,
            SAML2ClaimTypes.PersonTargetedID,
            SAML2ClaimTypes.PairwiseSubjectIdentifier
        };

        #region Authenticate

        public ClaimsPrincipal Authenticate(CommandResult result, ClaimsPrincipal incomingPrincipal)
        {
            var idp = result?.RelayData?.TryGetStringValue("idp");
            var personIdClaim = FindNameIdentifier(incomingPrincipal.Claims);
            if (personIdClaim == null)
            {
                var claimInfo = new StringBuilder("PersonTargetedID is missing\r\n");
                var issuer = incomingPrincipal.Claims.FirstOrDefault()?.Issuer;
                if (issuer != null)
                {
                    claimInfo.AppendLine($"Issuer: {issuer}");
                }
                foreach (var claim in incomingPrincipal.Claims)
                {
                    claimInfo.AppendLine($"{claim.Type}: {claim.Value}");
                }

                if (result != null)
                {
                    Telemetry.TrackDiagnostics(claimInfo.ToString());
                    result.Location = new Uri(UrlConstants.ShibbolethPersonIdMissing, UriKind.Relative);
                    result.HandledResult = true;
                    return incomingPrincipal;
                }
                throw new NotSupportedException();
            }

            var nameIdentifier = incomingPrincipal.Claims.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier);
            if (nameIdentifier != null)
            {
                incomingPrincipal.RemoveClaim(nameIdentifier);
            }

            var severityLevel = Environment.Build == BuildType.Alpha ? SeverityLevel.Warning : SeverityLevel.Information;

            Telemetry.TrackTrace($"Shibboleth authenticate: NameIdentifier: {personIdClaim.Value} , Issuer: {incomingPrincipal.Claims.First().Issuer}", severityLevel: severityLevel);
            foreach (var claim in incomingPrincipal.Claims)
            {
                Telemetry.TrackTrace($"Shibboleth-Claims: Type: {claim.Type} , Value: {claim.Value}", severityLevel: severityLevel);
            }

            incomingPrincipal.AddClaim(personIdClaim);
            incomingPrincipal.AddClaim(new Claim(SAML2ClaimTypes.ShibbolethIssuer, incomingPrincipal.Claims.First().Issuer));

            return incomingPrincipal;
        }

        #endregion

        #region FindNameIdentifier

        Claim FindNameIdentifier(IEnumerable<Claim> claims)
        {
            foreach(var nid in DefaultNameIdentifierTypes)
            {
                switch (nid)
                {
                    case SAML2ClaimTypes.PersistantNameIDFormat:
                        {
                            var nameIdentifier = claims.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier);
                            if (nameIdentifier?.Properties.Any(i => i.Value.ToLowerInvariant() == SAML2ClaimTypes.PersistantNameIDFormat) == true)
                            {
                                return nameIdentifier;
                            }
                        }
                        break;

                    case SAML2ClaimTypes.PersonTargetedID:
                        {
                            var personID = ParseEduPersonTargetId(claims.FirstOrDefault(i => i.Type == SAML2ClaimTypes.PersonTargetedID)?.Value);
                            if (personID != null)
                            {
                                return personID;
                            }

                            personID = ParseEduPersonTargetId(claims.FirstOrDefault(i => i.Type == SAML1ClaimTypes.PersonTargetedID)?.Value);
                            if (personID != null)
                            {
                                return personID;
                            }
                        }
                        break;

                    case SAML2ClaimTypes.PairwiseSubjectIdentifier:
                        {
                            var pairwiseSubjectIdentifier = claims.FirstOrDefault(i => i.Type == SAML2ClaimTypes.PairwiseSubjectIdentifier)?.Value;
                            if (!string.IsNullOrEmpty(pairwiseSubjectIdentifier))
                            {
                                return new Claim(ClaimTypes.NameIdentifier, pairwiseSubjectIdentifier);
                            }
                        }
                        break;
                }
            }
            

            //var personalUniqueCode = claims.FirstOrDefault(i => i.Type == SAML2ClaimTypes.PersonalUniqueCode)?.Value;
            //if (!string.IsNullOrEmpty(personalUniqueCode)) return new Claim(ClaimTypes.NameIdentifier, personalUniqueCode);

            //personalUniqueCode = claims.FirstOrDefault(i => i.Type == SAML1ClaimTypes.PersonalUniqueCode)?.Value;
            //if (!string.IsNullOrEmpty(personalUniqueCode)) return new Claim(ClaimTypes.NameIdentifier, personalUniqueCode);

            //var personUniqueCode = claims.FirstOrDefault(i => i.Type == SAML2ClaimTypes.PersonUniqueId)?.Value;
            //if (!string.IsNullOrEmpty(personUniqueCode)) return new Claim(ClaimTypes.NameIdentifier, personUniqueCode);

            return null;
        }

        #endregion

        #region ParseEduPersonTargetId

        Claim ParseEduPersonTargetId(string claim)
        {
            if (string.IsNullOrEmpty(claim))
            {
                return null;
            }

            var element = XElement.Parse(claim);
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName.ToLowerInvariant())
                {
                    case "format":
                        {
                            if (attribute.Value.ToLowerInvariant() == SAML2ClaimTypes.PersistantNameIDFormat)
                            {
                                return new Claim(ClaimTypes.NameIdentifier, element.Value);
                            }
                        }
                        break;
                    case "namequalifier":
                    case "spnamequalifier":
                    case "saml2":
                        break;

                }
            }
            return null;
        }

        #endregion
    }
}
