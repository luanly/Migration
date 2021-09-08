using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
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

namespace SwissAcademic.Crm.Web.Authentication.Google
{
    [ExcludeFromCodeCoverage]
    public static class GoogleExtensions
    {
        public static AuthenticationBuilder AddGoogleEx(this AuthenticationBuilder builder)
            => builder.AddGoogleEx(GoogleDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddGoogleEx(this AuthenticationBuilder builder, Action<GoogleOptions> configureOptions)
            => builder.AddGoogleEx(GoogleDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddGoogleEx(this AuthenticationBuilder builder, string authenticationScheme, Action<GoogleOptions> configureOptions)
            => builder.AddGoogleEx(authenticationScheme, GoogleDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddGoogleEx(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<GoogleOptions> configureOptions)
            => builder.AddOAuth<GoogleOptions, GoogleHandlerEx>(authenticationScheme, displayName, configureOptions);
    }

    [ExcludeFromCodeCoverage]
    public class GoogleHandlerEx
        :
        GoogleHandler
    {
        public GoogleHandlerEx(IOptionsMonitor<GoogleOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) :
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
