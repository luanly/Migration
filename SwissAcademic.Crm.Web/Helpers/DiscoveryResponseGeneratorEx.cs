using IdentityServer4.Configuration;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
	public class DiscoveryResponseGeneratorEx
		:
		DiscoveryResponseGenerator
	{
        public DiscoveryResponseGeneratorEx(
            IdentityServerOptions options,
            IResourceStore resourceStore,
            IKeyMaterialService keys,
            ExtensionGrantValidator extensionGrants,
            ISecretsListParser secretParsers,
            IResourceOwnerPasswordValidator resourceOwnerValidator,
            ILogger<DiscoveryResponseGenerator> logger)
            :
            base(options, resourceStore, keys, extensionGrants, secretParsers, resourceOwnerValidator, logger)
        {
            
        }

        public override Task<Dictionary<string, object>> CreateDiscoveryDocumentAsync(string baseUrl, string issuerUri)
        {
            baseUrl = UrlConstants.Authority.EnsureTrailingSlash();
            return base.CreateDiscoveryDocumentAsync(baseUrl, issuerUri);
        }
	}
}
