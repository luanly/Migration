using Sustainsys.Saml2;
using Sustainsys.Saml2.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class ShibbolethFederation
    {
        #region Konstruktor

        public ShibbolethFederation(string metadataUrl, RegionInfo region, SPOptions options)
        {
            MetadataUrl = metadataUrl;
            Options = options;
            Region = region;
        }

        #endregion

        #region Eigenschaften

        public IEnumerable<IdentityProvider> IdentityProviders { get; private set; }

        public Dictionary<string, string> IdentityProviderNames { get; private set; }

        internal string MetadataUrl { get; private set; }

        SPOptions Options { get; set; }

        public RegionInfo Region { get; private set; }

        #endregion

        #region Methoden

        void Add(IEnumerable<Tuple<IdentityProvider, string>> metadata)
        {
            var l = new List<IdentityProvider>();
            IdentityProviderNames = new Dictionary<string, string>();
            foreach (var item in metadata)
            {
                if (!CrmConfig.IsAlphaOrDev ||
                     CrmConfig.IsUnittest)
                {
                    if (IdentityProviderNames.ContainsKey(item.Item1.EntityId.Id))
                    {
                        continue;
                    }

                    if (item.Item2.Contains("(Test)"))
                    {
                        continue;
                    }
                }
                if (CrmConfig.IsAlphaOrDev)
                {
                    IdentityProviderNames[item.Item1.EntityId.Id] = $"{item.Item2} ({item.Item1.EntityId.Id})";
                }
                else
                {
                    IdentityProviderNames[item.Item1.EntityId.Id] = item.Item2;
                }
                l.Add(item.Item1);
            }
            IdentityProviders = l;
        }

        public async Task LoadAsync(CancellationToken token, bool useCache = false)
        {
            IEnumerable<Tuple<IdentityProvider, string>> metadata = null;
            if (useCache)
            {
                metadata = await ShibbolethMetadataLoader.LoadFromCacheAsync(new Uri(MetadataUrl), Options);
            }

			if(metadata == null || !metadata.Any())
            { 
                metadata = await ShibbolethMetadataLoader.LoadAsync(new Uri(MetadataUrl), Options, token);
            }
            Add(metadata);
        }

        #endregion
    }
}
