using IdentityServer4.Models;
using System.Collections.Generic;
using System.Threading;

namespace SwissAcademic.Crm.Web
{
    //https://leastprivilege.com/2016/12/01/new-in-identityserver4-resource-based-configuration/

    public class ScopeStore
    {
        public const string WebApiSecret = "4AA914A3-A575-4CDD-8674-2DCC00449620";

        #region Eigenschaften

        #region ApiResource

        //Allowed Scopes bei Client für "Api" - Zugänge. 
        //

        IEnumerable<ApiResource> _apiResources;
        public IEnumerable<ApiResource> ApiResources => LazyInitializer.EnsureInitialized(ref _apiResources, () =>
        {
            return new[]
            {
                new ApiResource
                {
                    Name= ApiResourceNames.WebApi,
                    ApiSecrets =
                    {
                        new Secret(WebApiSecret.Sha512())
                    },
                    Scopes =
                    {
                        CitaviScopes.WebApi
                    }
                }
            };
        });

        #endregion

        #region ApiScopes

        public IEnumerable<ApiScope> ApiScopes
        {
            get
            {
                return new ApiScope[]
                {
                    new ApiScope
                    {
                        Name = CitaviScopes.WebApi
                    }
                };
            }
        }

        #endregion

        #region IdentityResource

        //Allowed Scopes bei Clients
        //Bsp. hinzufügen von Email bei Allowed Scopes bei Client. ProfileService hat dann den EmailClaim in den RequestedClaims

        IEnumerable<IdentityResource> _identityResources;
        public IEnumerable<IdentityResource> IdentityResources => LazyInitializer.EnsureInitialized(ref _identityResources, () =>
        {
            return new[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource
                {
                    Name = CitaviScopes.Project,
                    ShowInDiscoveryDocument = false,
                    UserClaims = new[]
                    {
                        CitaviClaimTypes.Project
                    }
                },
                new IdentityResource
                {
                    Name = "unittests",
                    ShowInDiscoveryDocument = false,
                    UserClaims = new[]
                    {
                        CitaviClaimTypes.Project
                    }
                },
				//new IdentityResource
				//{
				//	Name = CitaviScopes.CrmInternal,
				//	ShowInDiscoveryDocument = false,
				//}
			};

        });

        #endregion

        #endregion

        public static ScopeStore Default { get; } = new ScopeStore();
    }
}
