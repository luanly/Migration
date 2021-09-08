using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SwissAcademic.Crm.Web.Authentication.MicrosoftAccount
{
    [ExcludeFromCodeCoverage]
    public static class MicrosoftAccountExtensions
    {
        public static AuthenticationBuilder AddMicrosoftAccount(this AuthenticationBuilder builder)
            => builder.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddMicrosoftAccount(this AuthenticationBuilder builder, Action<MicrosoftAccountOptions> configureOptions)
            => builder.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddMicrosoftAccount(this AuthenticationBuilder builder, string authenticationScheme, Action<MicrosoftAccountOptions> configureOptions)
            => builder.AddMicrosoftAccount(authenticationScheme, MicrosoftAccountDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddMicrosoftAccount(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<MicrosoftAccountOptions> configureOptions)
            => builder.AddOAuth<MicrosoftAccountOptions, MicrosoftAccountHandler>(authenticationScheme, displayName, configureOptions);
    }
}
