using System.Diagnostics.CodeAnalysis;

namespace SwissAcademic.Crm.Web.Authentication.MicrosoftAccount
{
    [ExcludeFromCodeCoverage]
    public static class MicrosoftAccountDefaults
    {
        public const string AuthenticationScheme = "Microsoft";

        public static readonly string DisplayName = "Microsoft";

        // https://developer.microsoft.com/en-us/graph/docs/concepts/auth_v2_user
        public static readonly string AuthorizationEndpoint = "https://login.live.com/oauth20_authorize.srf";

        public static readonly string TokenEndpoint = "https://login.live.com/oauth20_token.srf";

        public static readonly string UserInformationEndpoint = "https://apis.live.net/v5.0/me";
    }
}
