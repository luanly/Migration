using Microsoft.IdentityModel.Tokens.Saml2;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Saml2P;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

namespace SwissAcademic.Crm.Web
{
    //   bei RUB Bochum gibt es kein "NameIdentifier" (eigenlich schon, nur nicht diesen)
    //   -> ProcessAuthenticationStatement NullReferenceException

    //Try/Catch und ignorieren, wenn null (bekommen den korrekten NI später).

    [ExcludeFromCodeCoverage]
    public class CrmSaml2SecurityTokenHandler
        :
        Saml2PSecurityTokenHandler
    {
        public CrmSaml2SecurityTokenHandler()
           :
           base()
        {
        }

        protected override void ProcessAuthenticationStatement(Microsoft.IdentityModel.Tokens.Saml2.Saml2AuthenticationStatement statement, ClaimsIdentity subject, string issuer)
        {
            if (statement.AuthenticationContext != null)
            {
                statement.AuthenticationContext.DeclarationReference = null;
            }
            try
            {
                base.ProcessAuthenticationStatement(statement, subject, issuer);
                return;
            }
            catch (Exception) { }

            if (statement.SessionIndex != null)
            {
                var nameIdClaim = subject.FindFirst(ClaimTypes.NameIdentifier);
                if (nameIdClaim != null)
                {
                    subject.AddClaim(
                        new Claim(
                            Saml2ClaimTypes.LogoutNameIdentifier,
                            DelimitedString.Join(
                                nameIdClaim.Properties.GetValueOrEmpty(ClaimProperties.SamlNameIdentifierNameQualifier),
                                nameIdClaim.Properties.GetValueOrEmpty(ClaimProperties.SamlNameIdentifierSPNameQualifier),
                                nameIdClaim.Properties.GetValueOrEmpty(ClaimProperties.SamlNameIdentifierFormat),
                                nameIdClaim.Properties.GetValueOrEmpty(ClaimProperties.SamlNameIdentifierSPProvidedId),
                                nameIdClaim.Value),
                            null,
                            issuer));
                }
                subject.AddClaim(
                    new Claim(Saml2ClaimTypes.SessionIndex, statement.SessionIndex, null, issuer));
            }
        }
    }

    [ExcludeFromCodeCoverage]
    static class DelimitedString
    {
        const string DelimiterString = ",";
        const char DelimiterChar = ',';

        // Use a single / as escape char, avoid \ as that would require
        // all escape chars to be escaped in the source code...
        const char EscapeChar = '/';
        const string EscapeString = "/";

        /// <summary>
        /// Join strings with a delimiter and escape any occurence of the
        /// delimiter and the escape character in the string.
        /// </summary>
        /// <param name="strings">Strings to join</param>
        /// <returns>Joined string</returns>
        public static string Join(params string[] strings)
        {
            return string.Join(
                DelimiterString,
                strings.Select(
                    s => s
                    .Replace(EscapeString, EscapeString + EscapeString)
                    .Replace(DelimiterString, EscapeString + DelimiterString)));
        }

        /// <summary>
        /// Split strings delimited strings, respecting if the delimiter
        /// characters is escaped.
        /// </summary>
        /// <param name="source">Joined string from <see cref="Join(string[])"/></param>
        /// <returns>Unescaped, split strings</returns>
        public static string[] Split(string source)
        {
            var result = new List<string>();

            int segmentStart = 0;
            for (int i = 0; i < source.Length; i++)
            {
                bool readEscapeChar = false;
                if (source[i] == EscapeChar)
                {
                    readEscapeChar = true;
                    i++;
                }

                if (!readEscapeChar && source[i] == DelimiterChar)
                {
                    result.Add(UnEscapeString(
                        source.Substring(segmentStart, i - segmentStart)));
                    segmentStart = i + 1;
                }

                if (i == source.Length - 1)
                {
                    result.Add(UnEscapeString(source.Substring(segmentStart)));
                }
            }

            return result.ToArray();
        }

        static string UnEscapeString(string src)
        {
            return src.Replace(EscapeString + DelimiterString, DelimiterString)
                .Replace(EscapeString + EscapeString, EscapeString);
        }
    }

    [ExcludeFromCodeCoverage]
    static class DictionaryExtensions
    {
        public static string GetValueOrEmpty<T>(this IDictionary<T, string> dictionary, T key)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            return "";
        }
    }
}