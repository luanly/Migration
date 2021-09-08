using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web.Authentication.MicrosoftAccount
{
    [ExcludeFromCodeCoverage]
    public class MicrosoftAccountHandler : OAuthHandler<MicrosoftAccountOptions>
    {
        public MicrosoftAccountHandler(IOptionsMonitor<MicrosoftAccountOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        { 
            
        }

        public override Task<bool> ShouldHandleRequestAsync()
        {
            if(Options.CallbackPath == Request.Path)
            {
                if(Request.IsTestingInProductionRequest())
                {
                    if (Options.IsTiP)
                    {
                        return Task.FromResult(true);
                    }

                    return Task.FromResult(false);
                }
                
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            if(tokens == null)
            {
                throw new NotSupportedException("OAuthTokenResponse must not be null");
            }

            using (var request = new HttpRequestMessage(HttpMethod.Get, Options.UserInformationEndpoint))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

                using (var response = await Backchannel.SendAsync(request, Context.RequestAborted))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"An error occurred when retrieving Microsoft user information ({response.StatusCode}). Please check if the authentication information is correct and the corresponding Microsoft Account API is enabled.");
                    }
                    var text = await response.Content.ReadAsStringAsync();
                    var payload = JsonDocument.Parse(text).RootElement;

                    var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, payload);
                    
                    context.RunClaimActions();
                    //foreach (var action in Options.ClaimActions)
                    //{
                    //    action.Run(payload, context.Identity, Options.ClaimsIssuer ?? Scheme.Name);
                    //}

                    await Events.CreatingTicket(context);

                    context.Properties.Items[ChallangeConstants.Scheme] = IdentityProviders.Microsoft;

                    return new AuthenticationTicket(context.Principal, context.Properties, IdentityProviders.Microsoft);
                }
            }
        }

        protected override Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
        {
            context = new OAuthCodeExchangeContext(context.Properties, context.Code, GatewayHelper.UpdateRedirectUri(context.RedirectUri, Options.IsTiP));
            return base.ExchangeCodeAsync(context);
        }
    }
}
