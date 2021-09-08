using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web.Authentication.Facebook
{
    [ExcludeFromCodeCoverage]
    public static class FacebookExtensions
    {
        public static AuthenticationBuilder AddFacebookEx(this AuthenticationBuilder builder)
            => builder.AddFacebookEx(FacebookDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddFacebookEx(this AuthenticationBuilder builder, Action<FacebookOptions> configureOptions)
            => builder.AddFacebookEx(FacebookDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddFacebookEx(this AuthenticationBuilder builder, string authenticationScheme, Action<FacebookOptions> configureOptions)
            => builder.AddFacebookEx(authenticationScheme, FacebookDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddFacebookEx(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<FacebookOptions> configureOptions)
            => builder.AddOAuth<FacebookOptions, FacebookHandlerEx>(authenticationScheme, displayName, configureOptions);
    }

    [ExcludeFromCodeCoverage]
    public class FacebookHandlerEx
        :
        FacebookHandler
    {
        public FacebookHandlerEx(IOptionsMonitor<FacebookOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) :
            base(options, logger, encoder, clock)
        {

        }

        protected override Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
        {
            context = new OAuthCodeExchangeContext(context.Properties, context.Code, GatewayHelper.UpdateRedirectUri(context.RedirectUri, isTIP: Request.IsTestingInProductionRequest()));
            return base.ExchangeCodeAsync(context);
        }
    }
}
