using Microsoft.Azure.Documents.SystemFunctions;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.Metadata;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SwissAcademic.Crm.Web
{
    internal static class ShibbolethMetadataLoader
    {
        #region Classes

        class ExtendedMetadataSerializer : MetadataSerializer
        {
            private ExtendedMetadataSerializer(Sustainsys.Saml2.Metadata.SecurityTokenSerializer serializer)
                : base(serializer)
            { }

            private ExtendedMetadataSerializer() { }

            private static ExtendedMetadataSerializer readerInstance =
                new ExtendedMetadataSerializer();

            /// <summary>
            /// Use this instance for reading metadata. It uses custom extensions
            /// to increase feature support when reading metadata.
            /// </summary>
            public static ExtendedMetadataSerializer ReaderInstance
            {
                get
                {
                    return readerInstance;
                }
            }

            private static ExtendedMetadataSerializer writerInstance =
                new ExtendedMetadataSerializer();

            public static ExtendedMetadataSerializer WriterInstance
            {
                get
                {
                    return writerInstance;
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Method is only called by base class no validation needed.")]
            protected override void WriteCustomAttributes<T>(XmlWriter writer, T source)
            {
                if (typeof(T) == typeof(EntityDescriptor))
                {
                    writer.WriteAttributeString("xmlns", "saml2", null, Saml2Namespaces.Saml2Name);
                }
            }
        }

		#endregion

		#region Felder

		static Uri SAML2Protocol = new Uri("urn:oasis:names:tc:SAML:2.0:protocol");
        static HttpClient Client = new HttpClient();
        internal static BlobStorageRepository Repo;

        #endregion

        #region Methoden

        internal static string BuildCacheKey(Uri uri)
		{
            return $"{uri.Host.Replace(".", "_").ToLowerInvariant()}.xml";
		}

        internal static async Task Initialize() 
        {
            Repo = new BlobStorageRepository();
            await Repo.InitializeAsync(AzureConstants.ShibollethCacheBlobContainer);
        }

        public static Dictionary<string, XDocument> Raw = new Dictionary<string, XDocument>();

        internal static async Task<IEnumerable<Tuple<IdentityProvider, string>>> LoadAsync(Uri metadataUrl, SPOptions options, CancellationToken token, bool saveToStorage = true)
        {
            IEnumerable<Tuple<IdentityProvider, string>> list = null;
            var cacheKey = BuildCacheKey(metadataUrl);
            try
            {
                using (var stream = await Client.GetStreamAsync(metadataUrl))
                {
                    using (var ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);

                        ms.Seek(0, System.IO.SeekOrigin.Begin);
                        list = Load(ms, options);

                        if (saveToStorage)
                        {
                            ms.Seek(0, System.IO.SeekOrigin.Begin);
                            await Repo.AddOrUpdateAsync(ms.ToArray(), cacheKey);
                        }

#if DEBUG
                        ms.Seek(0, System.IO.SeekOrigin.Begin);
                        Raw.Add(metadataUrl.ToString(), await XDocument.LoadAsync(ms, LoadOptions.None, default));
#endif
                    }
                }
            }
            catch(Exception ex)
			{
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
                return await LoadFromCacheAsync(metadataUrl, options);
			}
            return list;
        }

        internal static async Task<IEnumerable<Tuple<IdentityProvider, string>>> LoadFromCacheAsync(Uri metadataUrl, SPOptions options)
		{
            IEnumerable<Tuple<IdentityProvider, string>> list = null;
            var cacheKey = BuildCacheKey(metadataUrl);
            var fromCache = await Repo.GetAsync<byte[]>(cacheKey);
            if (fromCache != null)
            {
                using (var stream = new MemoryStream(fromCache))
                {
                    list = Load(stream, options);
                }
            }
            return list;
        }

        internal static Tuple<IdentityProvider, string> LoadIdpMetadata(XmlReader r, SPOptions options)
        {
            string entityId = "";
            try
            {
                entityId = r.GetAttribute("entityID");

                if (Environment.Build == BuildType.Alpha)
                {
                    //Duplicate Keys
                    if (entityId == "https://sp.brockhaus-wissensservice.com/shibboleth")
                    {
                        return null;
                    }

                    if (entityId == "https://ubsrvappl5.ub.tu-berlin.de/shibboleth")
                    {
                        return null;
                    }

                    if (entityId == "https://api-staging.class.games/")
                    {
                        return null;
                    }

                    if (entityId == "https://pqshibboleth.aa1.proquest.com/shibboleth")
                    {
                        return null;
                    }

                    if (entityId == "https://pqshibboleth.aa1.proquest.com:9443/shibboleth")
                    {
                        return null;
                    }

                    if (entityId == "https://test-sp.ph-ludwigsburg.de/shibboleth")
                    {
                        return null;
                    }
                }

                var descriptor = ExtendedMetadataSerializer.ReaderInstance.ReadMetadata(r) as EntityDescriptor;

                foreach (var element in descriptor.Extensions)
                {
                    if (!element.Name.Contains("EntityAttributes"))
                    {
                        continue;
                    }

                    foreach (XmlElement child in element.ChildNodes)
                    {
                        foreach (XmlNode attributeValue in child.ChildNodes)
                        {
                            if (attributeValue.InnerText.Contains("hide-from-discovery"))
                            {
                                return null;
                            }
                        }
                    }
                }

                if (!descriptor.RoleDescriptors.Any())
                {
                    return null;
                }

                var spssDesc = descriptor.RoleDescriptors.Where(i => i is IdpSsoDescriptor);
                if (!spssDesc.Any())
                {
                    return null;
                }

                if (!spssDesc.Any((i) => i.ProtocolsSupported.Contains(SAML2Protocol)))
                {
                    return null;
                }

                var idp = new IdentityProvider(new Sustainsys.Saml2.Metadata.EntityId(entityId), options)
                {
                    AllowUnsolicitedAuthnResponse = false
                };

                idp.ReadMetadata(descriptor);
                if (descriptor.Organization == null)
                {
                    return null;
                }

                var name = descriptor.Organization.DisplayNames.FirstOrDefault().Name;
                return new Tuple<IdentityProvider, string>(idp, name);
            }
            catch
            {

            }
            return null;
        }

        static IEnumerable<Tuple<IdentityProvider, string>> Load(Stream stream, SPOptions options)
		{
            var list = new List<Tuple<IdentityProvider, string>>();

            using (var r = XmlReader.Create(stream))
            {
                while (r.ReadToElement())
                {
                    if (r.LocalName != "EntityDescriptor")
                    {
                        continue;
                    }
                    var item = LoadIdpMetadata(r, options);
                    if (item == null)
                    {
                        continue;
                    }

                    list.Add(item);
                }
            }
            return list;
        }

        #endregion
    }
}