using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SwissAcademic.Crm.Web
{
    internal static class SAML2ClaimTypes
    {
        #region Felder

        static Regex _personScopedAffiliationRgx = new Regex("(?<VAL>.+)@");

        #endregion

        #region Eigenschaften

        #region CommonName

        /// <summary>
        /// Common name. Typically represented in LBNL LDAP directory as "First Middle Last"
        /// </summary>
        internal const string CommonName = "urn:oid:2.5.4.3";

        #endregion

        #region DisplayName

        internal const string DisplayName = "urn:oid:2.16.840.1.113730.3.1.241";

        #endregion

        #region Email

        internal const string Email = "urn:oid:0.9.2342.19200300.100.1.3";

        #endregion

        #region GivenName

        internal const string GivenName = "urn:oid:2.5.4.42";

        #endregion

        #region Pairwise ID

        /// <summary>
        /// <para>This is a long-lived, non-re-assignable, uni-directional identifier suitable as a unique external key specific
        /// to particular applications.Its value for a given subject depends on the relying party to whom it is given,
        /// preventing unrelated systems from using it as a basis for correlation.</para>
        /// </summary>
        internal const string PairwiseSubjectIdentifier = "urn:oasis:names:tc:SAML:attribute:pairwise-id";

        #endregion

        #region PersonAffiliation

        internal const string PersonAffiliation = "urn:oid:1.3.6.1.4.1.5923.1.1.1.1";

        #endregion

        #region PersonScopedAffiliation

        /// <summary>
        /// Multiple values of the form value@domain, where domainis (typically) a DNS-like subdomain representing the organization or sub-organization of the affiliation (e.g., "osu.edu")
        /// </summary>
        internal const string PersonScopedAffiliation = "urn:oid:1.3.6.1.4.1.5923.1.1.1.9";

        #endregion

        #region PersonEntitlement

        /// <summary>
        /// URI (entweder URL oder URN), das Rechte der Person an speziellen Ressourcen anzeigt
        /// </summary>

        internal const string PersonEntitlement = "urn:oid:1.3.6.1.4.1.5923.1.1.1.7";

        #endregion

        #region PersonTargetedID

        /// <summary>
        /// <para>eduPersonTargetedID offers a way for pairings of identity providers and service providers to share unique,
        /// persistent identifiers about people in a way that avoids the privacy loss that would come from the use of a single,
        /// globally unique and persistent identifier for a given person.</para>
        /// </summary>
        internal const string PersonTargetedID = "urn:oid:1.3.6.1.4.1.5923.1.1.1.10";

        #endregion

        #region PersonUniqueId

        /// <summary>
        /// <para>A long-lived, non re-assignable, omnidirectional identifier
        /// suitable for use as a unique external key by applications.
        /// International version of the swissEduPerson Unique ID.</para>
        /// </summary>
        internal const string PersonUniqueId = "urn:oid:1.3.6.1.4.1.5923.1.1.1.13";

        #endregion

        #region PersonalUniqueCode

        /// <summary>
        /// <para>schacPersonalUniqueCode: Matrikelnummer</para>
        /// </summary>
        internal const string PersonalUniqueCode = "urn:oid:1.3.6.1.4.1.25178.1.2.14";

        #endregion

        #region PersistantNameIDFormat

        public const string PersistantNameIDFormat = "urn:oasis:names:tc:saml:2.0:nameid-format:persistent";

        #endregion

        #region ShibbolethIssuer

        /// <summary>
        /// Wird als hilfs-claim benötigt, da IdentityServer den Claim.Issuer auf Provider (=Shibboleth) setzt. Dann wissen wir nicht mehr, woher die Anmeldung kommt.
        /// </summary>
        internal const string ShibbolethIssuer = "shibboleth-issuer";

        #endregion

        #region Surname

        internal const string Surname = "urn:oid:2.5.4.4";

        #endregion

        #endregion

        #region Methoden

        #region ParseAffiliationClaim

        internal static IEnumerable<PersonAffiliationType> ParseAffiliationClaims(IEnumerable<Claim> claims)
        {
            var list = new List<PersonAffiliationType>();
            foreach (var claim in claims)
            {
                var affiliation = ParseAffiliationClaim(claim);
                if (affiliation == PersonAffiliationType.None)
                {
                    continue;
                }

                list.Add(affiliation);
            }
            return list.Distinct();
        }

        internal static PersonAffiliationType ParseAffiliationClaim(Claim claim)
        {
            var affiliation = PersonAffiliationType.None;
            if (claim.Type == PersonAffiliation ||
                claim.Type == SAML1ClaimTypes.PersonAffiliation)
            {
                //library-walk-in -> librarywalkin
                Enum.TryParse(claim.Value.Replace("-", string.Empty), true, out affiliation);
            }
            else if (claim.Type == PersonScopedAffiliation ||
                     claim.Type == SAML1ClaimTypes.PersonScopedAffiliation)
            {
                var affiliationValue = _personScopedAffiliationRgx.Match(claim.Value).Groups["VAL"].Value.Replace("-", string.Empty);
                Enum.TryParse(affiliationValue, true, out affiliation);
            }
            return affiliation;
        }

        internal static IEnumerable<string> GetAffiliationClaims(Claim claim)
        {
            var affiliation = new List<string>();
            if (claim.Type == PersonAffiliation ||
                claim.Type == SAML1ClaimTypes.PersonAffiliation)
            {
                affiliation.Add(claim.Value);
            }
            else if (claim.Type == PersonScopedAffiliation ||
                     claim.Type == SAML1ClaimTypes.PersonScopedAffiliation)
            {
                affiliation.Add(claim.Value);
            }
            return affiliation;
        }

        #endregion

        #endregion
    }

    internal static class SAML1ClaimTypes
    {
        #region CommonName

        /// <summary>
        /// Common name. Typically represented in LBNL LDAP directory as "First Middle Last"
        /// </summary>
        internal const string CommonName = "urn:mace:dir:attribute-def:cn";

        #endregion

        #region DisplayName

        internal const string DisplayName = "urn:mace:dir:attribute-def:displayName";

        #endregion

        #region Email

        internal const string Email = "urn:mace:dir:attribute-def:mail";

        #endregion

        #region EmployeeNumber

        internal const string EmployeeNumber = "urn:mace:dir:attribute-def:employeeNumber";

        #endregion

        #region GivenName

        internal const string GivenName = "urn:mace:dir:attribute-def:givenName";

        #endregion

        #region PersonalUniqueCode

        internal const string PersonalUniqueCode = "urn:mace:terena.org:schac:attribute-def:schacPersonalUniqueCode";

        #endregion

        #region PersonTargetedID

        internal const string PersonTargetedID = "urn:mace:dir:attribute-def:eduPersonTargetedID";

        #endregion

        #region PersonAffiliation

        internal const string PersonAffiliation = "urn:mace:dir:attribute-def:eduPersonAffiliation";

        #endregion

        #region PersonScopedAffiliation

        /// <summary>
        /// Multiple values of the form value@domain, where domainis (typically) a DNS-like subdomain representing the organization or sub-organization of the affiliation (e.g., "osu.edu")
        /// </summary>
        internal const string PersonScopedAffiliation = "urn:mace:dir:attribute-def:eduPersonScopedAffiliation";

        #endregion

        #region Surname

        internal const string Surname = "urn:mace:dir:attribute-def:sn";

        #endregion

        #region UID

        /// <summary>
        /// "LDAP" uid; the user's central LDAP directory username.
        /// </summary>
        internal const string UID = "urn:mace:dir:attribute-def:uid";

        #endregion
    }
}
