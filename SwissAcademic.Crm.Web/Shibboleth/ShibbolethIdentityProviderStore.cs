using Sustainsys.Saml2.AspNetCore2;
using SwissAcademic.ApplicationInsights;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class ShibbolethIdentityProviderStore
    {
        #region Eigenschaften

        public static Saml2Options AuthenticationOptions { get; private set; }

        public static List<string> Countries
        {
            get
            {
                return (from fed in _federations
                        select fed.Region.TwoLetterISORegionName).Distinct().ToList();
            }
        }
        public static ConcurrentDictionary<string, string> IdpNames { get; } = new ConcurrentDictionary<string, string>();
        internal static List<ShibbolethFederation> _federations { get; } = new List<ShibbolethFederation>();

        #endregion

        #region Methoden

        #region Configure

        internal static void Configure(Saml2Options authServiceOptions)
        {
            AuthenticationOptions = authServiceOptions;
        }

        #endregion

        #region GetDisplayName

        public static string GetDisplayName(string idpId)
        {
            if (!IdpNames.ContainsKey(idpId))
            {
                Telemetry.TrackTrace($"ShibbolethIdentityProviderName not found: {idpId}", SeverityLevel.Warning);
                return idpId;
            }
            return IdpNames[idpId];
        }

        #endregion

        #region RegisterIdentityProvider

        internal static void RegisterShibbolethFederation(ShibbolethFederation federation)
        {
            var existing = _federations.FirstOrDefault(i => i.MetadataUrl == federation.MetadataUrl);
            if (existing != null)
            {
                _federations.Remove(existing);
            }
            _federations.Add(federation);

            foreach (var identityProvider in federation.IdentityProviders)
            {
                var id = identityProvider.EntityId.Id;
                var name = federation.IdentityProviderNames[id];

                //Unis in AT. Da gibt es nur "prod" und keine Testumgebung.
                if (!CrmConfig.IsAlphaOrDev ||
                     CrmConfig.IsUnittest)
                {
                    if (name.Contains("(Test)"))
                    {
                        continue;
                    }
                }

                if (name.Length > 99)
                {
                    name = name.Substring(0, 99);
                }

                if (AuthenticationOptions.IdentityProviders.TryGetValue(identityProvider.EntityId, out _))
                {
                    AuthenticationOptions.IdentityProviders[identityProvider.EntityId] = identityProvider;
                }
                else
                {
                    AuthenticationOptions.IdentityProviders.Add(identityProvider);
                }
                IdpNames.AddOrUpdate(id, name, (s1, s2) => name);
            }
        }

        #endregion

        #endregion
    }
}
