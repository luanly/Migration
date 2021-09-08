using IdentityServer4.Models;
using IdentityServer4.Stores;
using SwissAcademic.KeyVaultUtils;
using SwissAcademic.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace SwissAcademic.Crm.Web
{
    public class ClientStore
        :
        IClientStore
    {
        #region Eigenschaften

        public IEnumerable<Client> Clients { get; private set; }
        string CitaviCRMClientSecret { get; set; }

        public IEnumerable<Client> Assistants => Clients.Where(c => c.ClientId == ClientIds.WebWordAddIn || c.ClientId == ClientIds.GoogleDocsAddIn);
        public IEnumerable<Client> Pickers => Clients.Where(c => c.ClientId == ClientIds.FirefoxPicker || c.ClientId == ClientIds.GoogleChromePicker || c.ClientId == ClientIds.EdgePicker);

        #endregion

        #region Methoden

        #region GetDisplayName

        public string GetDisplayName(string clientId) => Clients.First(i => i.ClientId == clientId).ClientName;

        #endregion

        #region FindClientById

        public Client FindClientById(string clientId)
        {
            var query =
               from client in Clients
               where client.ClientId == clientId && client.Enabled
               select client;

            return query.SingleOrDefault();
        }

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            var query =
               from client in Clients
               where client.ClientId == clientId && client.Enabled
               select client;

            return Task.FromResult(query.SingleOrDefault());
        }

        #endregion

        #region Initialize

        public async Task InitializeAsync()
        {
            var clients = new List<Client>();

            #region Citavi Web Client

            var client = new Client
            {
                Enabled = true,
                ClientName = "Citavi Web Client",
                ClientId = ClientIds.Web,
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string>
                    {
                        CitaviScopes.WebApi,
                        StandardScopes.OpenId,
                        StandardScopes.Profile,
                        StandardScopes.Email
                    },
                PostLogoutRedirectUris = new[]
                {
                    "/"
                },
                RedirectUris = new List<string>
                    {
                        UrlBuilder.Combine(UrlConstants.Authority, UrlConstants.Account),
                        UrlBuilder.Combine(UrlConstants.Authority, "scripts/_silentrenew.html"),
                        UrlBuilder.Combine(ConfigurationManager.AppSettings["TiP_RootAuthority"], UrlConstants.Account),
                        UrlBuilder.Combine(ConfigurationManager.AppSettings["TiP_RootAuthority"], "scripts/_silentrenew.html"),
                    },
                AllowedCorsOrigins = new List<string>
                {
                    UrlConstants.Authority,
                    ConfigurationManager.AppSettings["TiP_RootAuthority"],
                },
                AlwaysIncludeUserClaimsInIdToken = true,
                AccessTokenLifetime = (int)TimeSpan.FromHours(8).TotalSeconds,
                AccessTokenType = AccessTokenType.Reference,
                AllowAccessTokensViaBrowser = true,
            };
            
            foreach(var dataCenter in AzureRegionResolver.Instance.DataCenters)
			{
                var authority = AzureRegionResolver.Instance.GetAuthority(dataCenter);
                client.RedirectUris.Add(UrlBuilder.Combine(authority, UrlConstants.Account));
                client.RedirectUris.Add(UrlBuilder.Combine(authority, "scripts/_silentrenew.html"));
                client.AllowedCorsOrigins.Add(authority);
            }
            
            clients.Add(client);

            #endregion

            #region Citavi Desktop Client

            var redirectUris = new List<string>
                    {
                        UrlConstants.DesktopClientCustomURILoginRedirect,
                        UrlBuilder.Combine(UrlConstants.Authority, UrlConstants.Web, UrlConstants.DesktopLoginRedirect),
                        UrlBuilder.Combine(ConfigurationManager.AppSettings["TiP_RootAuthority"], UrlConstants.Web, UrlConstants.DesktopLoginRedirect),
                    };

            switch (Environment.Build)
            {
                case BuildType.Alpha:
                    redirectUris.Add(UrlBuilder.Combine("https://alphacitaviweb-dev.citavi.com/", UrlConstants.Web, UrlConstants.DesktopLoginRedirect));
                    break;

                case BuildType.Beta:
                    redirectUris.Add(UrlBuilder.Combine("https://citaviweb2.citavi.com/", UrlConstants.Web, UrlConstants.DesktopLoginRedirect));
                    redirectUris.Add(UrlBuilder.Combine("https://citaviweb-staging.citavi.com/", UrlConstants.Web, UrlConstants.DesktopLoginRedirect));
                    break;

                case BuildType.Release:
                    redirectUris.Add(UrlBuilder.Combine("https://citaviweb-staging.citavi.com/", UrlConstants.Web, UrlConstants.DesktopLoginRedirect));
                    break;
            }

            clients.Add(new Client
            {
                Enabled = true,
                ClientName = "Citavi Desktop Client",
                ClientId = ClientIds.Desktop,
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string>
                    {
                        CitaviScopes.WebApi,
                        StandardScopes.OpenId,
                        StandardScopes.Profile,
                        StandardScopes.Email
                    },
                RedirectUris = redirectUris,
                ClientSecrets = new List<Secret>
                    {
                         new Secret(ClientSecrets.CitaviDesktopClientSecret.Sha512())
                    },
                AlwaysIncludeUserClaimsInIdToken = true,
                AccessTokenLifetime = (int)TimeSpan.FromDays(365 * 2).TotalSeconds,
                AllowAccessTokensViaBrowser = true,
                AccessTokenType = AccessTokenType.Reference
            });

            #endregion

            #region Citavi Mobile Client

            clients.Add(new Client
            {
                Enabled = true,
                ClientName = "Citavi Mobile Client",
                ClientId = ClientIds.Mobile,
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string>
                    {
                        CitaviScopes.WebApi,
                        StandardScopes.OpenId,
                        StandardScopes.Profile,
                        StandardScopes.Email
                    },
                RedirectUris = new List<string>
                    {
                        UrlBuilder.Combine(UrlConstants.MobileAppScheme, UrlConstants.MobileClientLoginRedirect),
                    },
                ClientSecrets = new List<Secret>
                    {
                         new Secret(ClientSecrets.CitaviMobileClientSecret.Sha512())
                    },
                AccessTokenLifetime = (int)TimeSpan.FromDays(365 * 2).TotalSeconds,
                AccessTokenType = AccessTokenType.Reference,
                AllowAccessTokensViaBrowser = true,
                AllowOfflineAccess = true
            });

            #endregion

            #region Citavi Web WordAddIn Client

            var WebWordAddInAuthorities = new string[]
            {
                "https://alphawordassistant.citavi.com",
                "https://wordassistant-beta.citavi.com",
                "https://wordassistant.citavi.com",
            };

            var owai_PostLogoutRedirectUris = new List<string>();
            var owai_RedirectUris = new List<string>();
            var owai_AllowedCorsOrigins = new List<string>();
            foreach (var authority in WebWordAddInAuthorities)
            {
                owai_AllowedCorsOrigins.Add(authority);

                owai_RedirectUris.AddRange(new string[]{
                    UrlBuilder.Combine(authority, "app/web/authorize_local.html"),             //Via Dialog-Api 1.1 (im Web nicht verfügbar)
                    UrlBuilder.Combine(authority, "app/web/authorize_web.html"),               //Via window.open
                    UrlBuilder.Combine(authority, "app/web/authorize.html"),
                    UrlBuilder.Combine(authority, "app/web/_silentrenew.html"),
                    UrlBuilder.Combine(authority, "app/home.html"),
				});

                owai_PostLogoutRedirectUris.Add(UrlBuilder.Combine(authority, "app/web/logout.html"));
            }

            if(Environment.Build == BuildType.Alpha)
            {
                var webWordAddIn_Localhost = "https://localhost:5001";
                owai_RedirectUris.Add(UrlBuilder.Combine(webWordAddIn_Localhost, "app/web/authorize_local.html"));   //Via Dialog-Api 1.1 (im Web nicht verfügbar)
                owai_RedirectUris.Add(UrlBuilder.Combine(webWordAddIn_Localhost, "app/web/authorize_web.html"));     //Via window.open
                owai_RedirectUris.Add(UrlBuilder.Combine(webWordAddIn_Localhost, "app/web/authorize.html"));
                owai_RedirectUris.Add(UrlBuilder.Combine(webWordAddIn_Localhost, "app/web/_silentrenew.html"));
                owai_RedirectUris.Add(UrlBuilder.Combine(webWordAddIn_Localhost, "app/home.html"));

                owai_PostLogoutRedirectUris.Add(UrlBuilder.Combine(webWordAddIn_Localhost, "app/web/logout.html"));
                owai_AllowedCorsOrigins.Add(webWordAddIn_Localhost);
            }

            clients.Add(new Client
            {
                Enabled = true,
                ClientName = "Citavi Web WordAddIn Client",
                ClientId = ClientIds.WebWordAddIn,
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string>
                    {
                        CitaviScopes.WebApi,
                        StandardScopes.OpenId,
                        StandardScopes.Profile,
                    },
                AllowedCorsOrigins = owai_AllowedCorsOrigins,
                RedirectUris = owai_RedirectUris,
                PostLogoutRedirectUris = owai_PostLogoutRedirectUris,
                AccessTokenLifetime = (int)TimeSpan.FromHours(24).TotalSeconds,
                AllowAccessTokensViaBrowser = true,
                AccessTokenType = AccessTokenType.Reference,
            });

            #endregion

            #region Citavi Web GoogleDocsAddIn Client

            clients.Add(new Client
            {
                Enabled = true,
                ClientName = "Citavi Web GoogleDocsAddIn Client",
                ClientId = ClientIds.GoogleDocsAddIn,
                ClientSecrets = new List<Secret>
                    {
                         new Secret("RaLRGS5Gu@%UN7NV8IgR6!rQN3WP26".Sha512())
                    },
                AllowedGrantTypes = GrantTypes.Code,
                RequireConsent = false,
                AllowedScopes = new List<string>
                    {
                        CitaviScopes.WebApi,
                        StandardScopes.OfflineAccess,
                        StandardScopes.OpenId,
                        StandardScopes.Profile,
                    },
                AllowedCorsOrigins = new List<string>
                    {
                        "https://googleusercontent.com",
                        "https://script.google.com"
                    },
                RedirectUris = new List<string>
                    {
                        "https://script.google.com/macros/d/M_0PKVwRnPiP7eJxczFWOnoHvfU5ZDxYj/usercallback"
                    },
                AccessTokenLifetime = (int)TimeSpan.FromHours(24).TotalSeconds,
                AccessTokenType = AccessTokenType.Reference,
                AllowAccessTokensViaBrowser = true,
                AllowOfflineAccess = true
            });

            #endregion

            #region Chrome Picker

            clients.Add(new Client
            {
                Enabled = true,
                ClientName = "Google Chrome Picker",
                ClientId = ClientIds.GoogleChromePicker,
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string>
                    {
                        CitaviScopes.WebApi,
                        StandardScopes.OpenId,
                        StandardScopes.Profile
                    },
                AllowedCorsOrigins = new List<string>
				{
                        "https://nkhagnfgobknphgoholmfbcaappgfngg.chromiumapp.org",
                        "https://fneidadefimoalbgjnfgkcjpclpkbadf.chromiumapp.org",
                        "https://ohgndokldibnndfnjnagojmheejlengn.chromiumapp.org",
                        "https://eaandldnbchhjimdfnaagaaidgebplgj.chromiumapp.org",
                        "https://ndahmgeoecpnplkdnejnidmbbahoamkc.chromiumapp.org",
                },
                RedirectUris = new List<string>
                    {
                        "https://nkhagnfgobknphgoholmfbcaappgfngg.chromiumapp.org",
                        "https://fneidadefimoalbgjnfgkcjpclpkbadf.chromiumapp.org",
                        "https://ohgndokldibnndfnjnagojmheejlengn.chromiumapp.org",
                        "https://eaandldnbchhjimdfnaagaaidgebplgj.chromiumapp.org",
                        "https://ndahmgeoecpnplkdnejnidmbbahoamkc.chromiumapp.org",

                        "chrome-extension://nkhagnfgobknphgoholmfbcaappgfngg/web/_sr.html",
                        "chrome-extension://fneidadefimoalbgjnfgkcjpclpkbadf/web/_sr.html",
                        "chrome-extension://ohgndokldibnndfnjnagojmheejlengn/web/_sr.html",
                        "chrome-extension://eaandldnbchhjimdfnaagaaidgebplgj/web/_sr.html",
                        "chrome-extension://ndahmgeoecpnplkdnejnidmbbahoamkc/web/_sr.html",
                    },
                PostLogoutRedirectUris = new List<string>
                    {
                        "https://nkhagnfgobknphgoholmfbcaappgfngg.chromiumapp.org",
                        "https://fneidadefimoalbgjnfgkcjpclpkbadf.chromiumapp.org",
                        "https://ohgndokldibnndfnjnagojmheejlengn.chromiumapp.org",
                        "https://eaandldnbchhjimdfnaagaaidgebplgj.chromiumapp.org",
                        "https://ndahmgeoecpnplkdnejnidmbbahoamkc.chromiumapp.org",

                        "chrome-extension://nkhagnfgobknphgoholmfbcaappgfngg/",
                        "chrome-extension://fneidadefimoalbgjnfgkcjpclpkbadf/",
                        "chrome-extension://ohgndokldibnndfnjnagojmheejlengn/",
                        "chrome-extension://eaandldnbchhjimdfnaagaaidgebplgj/",
                        "chrome-extension://ndahmgeoecpnplkdnejnidmbbahoamkc/",
                    },
                AccessTokenLifetime = (int)TimeSpan.FromHours(24).TotalSeconds,
                AllowAccessTokensViaBrowser = true,
                AccessTokenType = AccessTokenType.Reference,
            });

            #endregion

            #region Edge Picker

            clients.Add(new Client
            {
                Enabled = true,
                ClientName = "Microsoft Edge Picker",
                ClientId = ClientIds.EdgePicker,
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string>
                    {
                        CitaviScopes.WebApi,
                        StandardScopes.OpenId,
                        StandardScopes.Profile
                    },
                AllowedCorsOrigins = new List<string>
                    {
                        "https://fneidadefimoalbgjnfgkcjpclpkbadf.chromiumapp.org",
                        "https://mielbhbkcliienpdicphhecpodcaeefg.chromiumapp.org",
                        "https://oojiepblieajfgppbooofighlmmabmjo.chromiumapp.org",
                        "https://iodenooodmoenkmdaobjmjanhcgdagln.chromiumapp.org",
                    },
                RedirectUris = new List<string>
                    {
                        "https://fneidadefimoalbgjnfgkcjpclpkbadf.chromiumapp.org",
                        "https://mielbhbkcliienpdicphhecpodcaeefg.chromiumapp.org",
                        "https://oojiepblieajfgppbooofighlmmabmjo.chromiumapp.org",
                        "https://iodenooodmoenkmdaobjmjanhcgdagln.chromiumapp.org",

                        "chrome-extension://fneidadefimoalbgjnfgkcjpclpkbadf/web/_sr.html",
                        "chrome-extension://mielbhbkcliienpdicphhecpodcaeefg/web/_sr.html",
                        "chrome-extension://oojiepblieajfgppbooofighlmmabmjo/web/_sr.html",
                        "chrome-extension://iodenooodmoenkmdaobjmjanhcgdagln/web/_sr.html",
                    },
                PostLogoutRedirectUris = new List<string>
                    {
                        "https://fneidadefimoalbgjnfgkcjpclpkbadf.chromiumapp.org",
                        "https://mielbhbkcliienpdicphhecpodcaeefg.chromiumapp.org",
                        "https://oojiepblieajfgppbooofighlmmabmjo.chromiumapp.org",
                        "https://iodenooodmoenkmdaobjmjanhcgdagln.chromiumapp.org",

                        "chrome-extension://fneidadefimoalbgjnfgkcjpclpkbadf/",
                        "chrome-extension://mielbhbkcliienpdicphhecpodcaeefg/",
                        "chrome-extension://oojiepblieajfgppbooofighlmmabmjo/",
                        "chrome-extension://iodenooodmoenkmdaobjmjanhcgdagln/",
                    },
                AccessTokenLifetime = (int)TimeSpan.FromHours(24).TotalSeconds,
                AllowAccessTokensViaBrowser = true,
                AccessTokenType = AccessTokenType.Reference,
            });

            #endregion

            #region Firefox Picker

            clients.Add(new Client
            {
                Enabled = true,
                ClientName = "Firefox Picker",
                ClientId = ClientIds.FirefoxPicker,
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string>
                    {
                        CitaviScopes.WebApi,
                        StandardScopes.OpenId,
                        StandardScopes.Profile
                    },
                AllowedCorsOrigins = new List<string>
                    {
                        "https://4809aec236656fcbe688f735278214f28a2c7c65.extensions.allizom.org",
                        "https://2affd432e29a46fc5ab767bd90034608a12432c0.extensions.allizom.org",
                        "https://903f720d0bb83b9fe4420442276ab01775c71daa.extensions.allizom.org",
                    },
                RedirectUris = new List<string>
                    {
                        "https://4809aec236656fcbe688f735278214f28a2c7c65.extensions.allizom.org",
                        "https://2affd432e29a46fc5ab767bd90034608a12432c0.extensions.allizom.org",
                        "https://903f720d0bb83b9fe4420442276ab01775c71daa.extensions.allizom.org",

                        "moz-extension://76eb1632-d2e9-4fff-8726-fdedaa0a745e/web/_sr.html",
                        "moz-extension://f62cee52-3d4b-495a-96f3-3e0b388c1793/web/_sr.html",
                        "moz-extension://7d762b66-0f0b-496e-b858-c21c8bd61ac8/web/_sr.html",
                    },
                PostLogoutRedirectUris = new List<string>
                    {
                        "https://4809aec236656fcbe688f735278214f28a2c7c65.extensions.allizom.org",
                        "https://2affd432e29a46fc5ab767bd90034608a12432c0.extensions.allizom.org",
                        "https://903f720d0bb83b9fe4420442276ab01775c71daa.extensions.allizom.org",

                        "moz-extension://76eb1632-d2e9-4fff-8726-fdedaa0a745e/",
                        "moz-extension://f62cee52-3d4b-495a-96f3-3e0b388c1793/",
                        "moz-extension://7d762b66-0f0b-496e-b858-c21c8bd61ac8/",
                    },
                AccessTokenLifetime = (int)TimeSpan.FromHours(24).TotalSeconds,
                AllowAccessTokensViaBrowser = true,
                AccessTokenType = AccessTokenType.Reference,
            });

            #endregion

            #region Citavi UnitTest Client

            clients.Add(new Client
            {
                Enabled = true,
                ClientName = "Citavi Test Client",
                ClientId = ClientIds.UnitTest,
                AllowedScopes = new List<string>
                    {
                        CitaviScopes.WebApi,
                        StandardScopes.OpenId,
                        StandardScopes.Profile,
                        CitaviScopes.Project,
                        StandardScopes.Email,
                        "unittests",
                    },
                ClientSecrets = new List<Secret>
                    {
                         new Secret(ClientSecrets.CitaviUnitTestClientSecret.Sha512())
                    },
                AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                RequireConsent = false,
                AccessTokenType = AccessTokenType.Reference,
                AccessTokenLifetime = (int)TimeSpan.FromDays(365 * 2).TotalSeconds,
            });

            #endregion

            #region UseResponse  Client

            clients.Add(new Client
            {
                Enabled = true,
                ClientName = "UseResponse Client",
                ClientId = ClientIds.UseResponse,
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,

                AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId,
                        StandardScopes.Profile,
                        StandardScopes.Email
                    },
                AllowedCorsOrigins = new List<string>
                    {
                        "https://umbraco.citavi.com",
                        "https://help.citavi.com"
                    },
                RedirectUris = new List<string>
                    {
                       "https://help.citavi.com/citavi/login"
                    },
                PostLogoutRedirectUris = new List<string>
                    {
                        "https://help.citavi.com/logout"
                    },
                AlwaysIncludeUserClaimsInIdToken = true,
                AccessTokenType = AccessTokenType.Reference,
            });


            #endregion

            #region Keylight  Client

            clients.Add(new Client
            {
                Enabled = true,
                ClientName = "Keylight Client",
                ClientId = ClientIds.Keylight,
                ClientSecrets = new List<Secret>
                    {
                         new Secret(ClientSecrets.KeylightClientSecret.Sha512())
                    },
                AllowedGrantTypes = GrantTypes.Code,
                RequireConsent = false,
                RequirePkce = false,
                AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId,
                        StandardScopes.Profile,
                        StandardScopes.Email
                    },
                AllowedCorsOrigins = new List<string>
                {
                    "https://www.myqsrportal.com",
                    "https://sso-sandbox.subscription-suite.io",
                    "https://qsr-sandbox.subscription-suite.io",
                    "https://qsr-citavi-sandbox.subscription-suite.io",
                    "https://sso.subscription-suite.io"
                },
                RedirectUris = new List<string>
                    {
                       "https://www.myqsrportal.com",
                       "https://sso-sandbox.subscription-suite.io/auth/realms/qsr/broker/citavi/endpoint",
                       "https://sso-sandbox.subscription-suite.io/auth/realms/qsr/broker/citavi/endpoint/logout_response",
                       "https://sso.subscription-suite.io/auth/realms/qsr/broker/citavi/endpoint",
                       "https://sso.subscription-suite.io/auth/realms/qsr/broker/citavi/endpoint/logout_response",

                       "https://sso-sandbox.subscription-suite.io ",
                       "https://qsr-sandbox.subscription-suite.io",
                       "https://qsr-citavi-sandbox.subscription-suite.io",
                       "https://sso.subscription-suite.io",
                    },
                PostLogoutRedirectUris = new List<string>
                    {
                        "https://www.myqsrportal.com",

                        "https://sso-sandbox.subscription-suite.io ",
                        "https://qsr-sandbox.subscription-suite.io",
                        "https://qsr-citavi-sandbox.subscription-suite.io",
                        "https://sso.subscription-suite.io",

                        "https://sso-sandbox.subscription-suite.io/auth/realms/qsr/broker/citavi/endpoint",
                        "https://sso-sandbox.subscription-suite.io/auth/realms/qsr/broker/citavi/endpoint/logout_response",
                        "https://sso.subscription-suite.io/auth/realms/qsr/broker/citavi/endpoint",
                        "https://sso.subscription-suite.io/auth/realms/qsr/broker/citavi/endpoint/logout_response",

                    },
                AlwaysIncludeUserClaimsInIdToken = true,
                AccessTokenType = AccessTokenType.Reference,
            });


            #endregion

            #region ZenDesk  Client

            clients.Add(new Client
			{
				Enabled = true,
				ClientName = "ZenDesk Client",
				ClientId = ClientIds.ZenDesk,
				AllowedGrantTypes = GrantTypes.Implicit,
				RequireConsent = false,
				AccessTokenLifetime = (int)TimeSpan.FromMinutes(5).TotalSeconds,
				AllowedScopes = new List<string>
					{
						StandardScopes.OpenId
					},
				RedirectUris = new List<string>
					{
						UrlBuilder.Combine(UrlConstants.Authority, UrlConstants.Web, UrlConstants.ZenDeskLoginRedirect),
						UrlBuilder.Combine(ConfigurationManager.AppSettings["TiP_RootAuthority"], UrlConstants.Web, UrlConstants.ZenDeskLoginRedirect),
					},
				AccessTokenType = AccessTokenType.Reference,
			});


			#endregion

			#region SciFlow

			if (Environment.Build == BuildType.Alpha)
            {
                clients.Add(new Client
                {
                    Enabled = true,
                    ClientName = "SciFlow",
                    ClientId = ClientIds.SciFlow,
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = false,
                    AllowedScopes = new List<string>
                    {
                        CitaviScopes.WebApi,
                        StandardScopes.OpenId,
                        StandardScopes.Profile
                    },
                    AllowedCorsOrigins = new List<string>
                    {
                        "http://localhost:5060/",
                        "https://citavi.staging.sciflow.de/"
                    },
                    RedirectUris = new List<string>
                    {
                        "http://localhost:5060/api/references/citavi/callback",
                        "https://citavi.staging.sciflow.de/api/references/citavi/callback"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:5060/",
                        "https://citavi.staging.sciflow.de"
                    },
                    AccessTokenLifetime = (int)TimeSpan.FromHours(24).TotalSeconds,
                    AllowAccessTokensViaBrowser = true,
                    AccessTokenType = AccessTokenType.Reference,
                });

            }

            #endregion

            Clients = clients;
        }

        #endregion

        #endregion

        public static ClientStore Instance { get; private set; } 

        public static async Task InitializeInstanceAsync()
        {
            Instance = new ClientStore();
            await Instance.InitializeAsync();
        }
    }
}
