using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.Metadata;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Crm.Web.Authentication.Facebook;
using SwissAcademic.Crm.Web.Authentication.Google;
using SwissAcademic.Crm.Web.Authentication.MicrosoftAccount;
using SwissAcademic.KeyVaultUtils;
using SwissAcademic.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class IdentityProviders
    {
        static string facebookSecret;
        static string googleSecret;
        static string microsoftSecret;
        static string microsoftTiPSecret;

        static CrmShibbolethAuthenticationManager CrmShibbolethAuthentication = new CrmShibbolethAuthenticationManager();

        public const string Shibboleth = "Shibboleth";
        public const string Google = "Google";
        public const string Facebook = "Facebook";
        public const string Microsoft = "Microsoft";
        public const string TiPSuffix = "-TiP";

        public static readonly string[] All = new[]
        {
            Facebook,
            Google,
            Microsoft,
            Shibboleth
        };

        #region Methoden

        #region Configure

        public static void Configure(IServiceCollection services)
        {
            var authentication = services.AddAuthentication();
            var signInAsType = IdentityServerConstants.ExternalCookieAuthenticationScheme;

            #region Shibboleth

            var spOptions = new SPOptions();
            spOptions.EntityId = new EntityId(ConfigurationManager.AppSettings["ShibbolethEntityId"]);
            spOptions.Logger = new ShibbolethLogger();

            ///22.01.2018: OpenIDP nach Upgrade auf Version 22.0
            ///The signing algorithm http://www.w3.org/2000/09/xmldsig#rsa-sha1 is weaker than the minimum accepted http://www.w3.org/2001/04/xmldsig-more#rsa-sha256.
            ///If you want to allow this signing algorithm, use the minIncomingSigningAlgorithm configuration attribute.
            try
            {
                spOptions.MinIncomingSigningAlgorithm = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
            }
            catch (Exception ex)
            {
                try
                {
                    var list = typeof(System.Security.Cryptography.Xml.SignedXml).GetFields()
                              .Where(f => f.Name.StartsWith("XmlDsigRSASHA", StringComparison.Ordinal))
                              .Select(f => (string)f.GetRawConstantValue())
                              .OrderBy(f => f)
                              .ToList();
                    Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat, property1: (nameof(TelemetryProperty.Description), list.ToString("\r\n")));
                }
                catch (Exception ex2)
                {
                    Telemetry.TrackException(ex2, SeverityLevel.Error, ExceptionFlow.Eat);
                }
            }

            ShibbolethServiceProvider.Configure(spOptions);

            authentication.AddSaml2(options =>
            {
                options.SPOptions = spOptions;
                options.SignInScheme = signInAsType;
                ShibbolethIdentityProviderStore.Configure(options);

                options.Notifications.AcsCommandResultCreated = (result, samlResponse) =>
                {
                    if (samlResponse.Status == Sustainsys.Saml2.Saml2P.Saml2StatusCode.Success)
                    {
                        CrmShibbolethAuthentication.Authenticate(result, result.Principal);
                    }
                };

                options.Notifications.AuthenticationRequestCreated = (request, IdentityProvider, Options) =>
                {
                    if (Options.ContainsKey(ChallangeConstants.TestingInProduction))
                    {
                        Options.Remove(ChallangeConstants.TestingInProduction);
                        request.AssertionConsumerServiceUrl = GatewayHelper.UpdateShibbolethRedirectUri(request.AssertionConsumerServiceUrl, isTIP: true);
                    }
                    else
                    {
                        request.AssertionConsumerServiceUrl = GatewayHelper.UpdateShibbolethRedirectUri(request.AssertionConsumerServiceUrl, isTIP: false);
                    }
                };
            });

            #endregion

            #region Microsoft

            authentication.AddMicrosoftAccount(Microsoft, options =>
            {
                options.CallbackPath = "/identity/signin-microsoft";
                options.SignInScheme = signInAsType;
                options.ClientId = ConfigurationManager.AppSettings["IdentityProvider_Microsoft_ClientId"];
                options.ClientSecret = microsoftSecret;

                options.Events.OnRemoteFailure = ctx =>
                {
                    if (ctx.Failure != null)
                    {
                        Telemetry.TrackTrace(ctx.Failure.Message, SeverityLevel.Warning);
                    }
                    ctx.Response.Redirect("/");
                    ctx.HandleResponse();
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAuthorizationEndpoint = ctx =>
                {
                    ctx.RedirectUri = GatewayHelper.UpdateOAuthRedirectUri(ctx.RedirectUri, isTIP: false);
                    ctx.HttpContext.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            authentication.AddMicrosoftAccount($"{Microsoft}{TiPSuffix}", options =>
            {
                options.IsTiP = true;
                options.CallbackPath = "/identity/signin-microsoft";
                options.SignInScheme = signInAsType;
                options.ClientId = ConfigurationManager.AppSettings["TiP_IdentityProvider_Microsoft_ClientId"];
                options.ClientSecret = microsoftTiPSecret;
                options.Events.OnRemoteFailure = ctx =>
                {
                    if (ctx.Failure != null)
                    {
                        Telemetry.TrackTrace(ctx.Failure.Message, SeverityLevel.Warning);
                    }
                    ctx.Response.Redirect("/");
                    ctx.HandleResponse();
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAuthorizationEndpoint = ctx =>
                {
                    ctx.RedirectUri = GatewayHelper.UpdateOAuthRedirectUri(ctx.RedirectUri, isTIP: true);
                    ctx.HttpContext.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            #endregion

            #region Google

            authentication.AddGoogleEx("Google", options =>
            {
                options.CallbackPath = "/identity/signin-google";
                options.SignInScheme = signInAsType;
                options.ClientId = ConfigurationManager.AppSettings["IdentityProvider_Google_ClientId"];
                options.ClientSecret = googleSecret;

                options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
                options.ClaimActions.Clear();
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

                options.Events.OnRemoteFailure = ctx =>
                {
                    if (ctx.Failure != null)
                    {
                        Telemetry.TrackTrace(ctx.Failure.Message, SeverityLevel.Warning);
                    }
                    ctx.Response.Redirect("/");
                    ctx.HandleResponse();
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAuthorizationEndpoint = ctx =>
                {
                    ctx.RedirectUri = GatewayHelper.UpdateOAuthRedirectUri(ctx.RedirectUri, isTIP: ctx.Request.IsTestingInProductionRequest());
                    ctx.HttpContext.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            #endregion

            #region Facebook

            authentication.AddFacebookEx("Facebook", options =>
            {
                options.SignInScheme = signInAsType;
                options.CallbackPath = "/identity/signin-facebook";

                options.ClientId = ConfigurationManager.AppSettings["IdentityProvider_Facebook_ClientId"];
                options.ClientSecret = facebookSecret;
                options.Scope.Add("email");
                options.Events.OnRemoteFailure = ctx =>
                {
                    if (ctx.Failure != null)
                    {
                        Telemetry.TrackTrace(ctx.Failure.Message, SeverityLevel.Warning);
                    }
                    ctx.Response.Redirect("/");
                    ctx.HandleResponse();
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAuthorizationEndpoint = ctx =>
                {
                    ctx.RedirectUri = GatewayHelper.UpdateOAuthRedirectUri(ctx.RedirectUri, isTIP: ctx.Request.IsTestingInProductionRequest());
                    ctx.HttpContext.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            #endregion
        }

        #endregion

        #region InitializeAsync
        public static async Task InitializeAsync()
        {
            var facebookSecretTask = AzureHelper.KeyVaultClient.GetSecretAsync(KeyVaultSecrets.Secrets.Facebook);
            var googleSecretTask = AzureHelper.KeyVaultClient.GetSecretAsync(KeyVaultSecrets.Secrets.Google);
            var microsoftSecretTask = AzureHelper.KeyVaultClient.GetSecretAsync(KeyVaultSecrets.Secrets.Microsoft.FormatString(ConfigurationManager.AppSettings["SlotName"]));
            var microsoftTiPSecretTask = AzureHelper.KeyVaultClient.GetSecretAsync(KeyVaultSecrets.Secrets.Microsoft_TiP.FormatString(ConfigurationManager.AppSettings["SlotName"]));

            await Task.WhenAll(facebookSecretTask, googleSecretTask, microsoftSecretTask, microsoftTiPSecretTask);

            facebookSecret = facebookSecretTask.Result;
            googleSecret = googleSecretTask.Result;
            microsoftSecret = microsoftSecretTask.Result;
            microsoftTiPSecret = microsoftTiPSecretTask.Result;
        }

        #endregion

        #region IsShibboleth

        internal static bool IsShibboleth(string idp) => idp != IdentityProviders.Facebook &&
                                                         idp != IdentityProviders.Microsoft &&
                                                         idp != IdentityProviders.Google;

        #endregion

        #endregion
    }
}
