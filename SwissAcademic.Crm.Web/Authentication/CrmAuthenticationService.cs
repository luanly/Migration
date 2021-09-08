using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SwissAcademic.ApplicationInsights;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class CrmAuthenticationService
    {
        #region Konstruktor

        public CrmAuthenticationService()
            :
            this(new CrmDbContext())
        {

        }
        public CrmAuthenticationService(CrmDbContext context)
        {
            DbContext = context;
            UserManager = new CrmUserManager(context);
        }

        #endregion

        #region Eigenschaften

        #region DbContext

        CrmDbContext DbContext { get; }

        #endregion

        #region UserManager

        CrmUserManager UserManager { get; }

        #endregion

        #endregion

        #region Methoden

        #region AuthenticateExternalAsync

        public async Task<(CrmUser User, string UserProviderSubject, string Provider)> AuthenticateExternalAsync(AuthenticateResult ctx, IHttpContextAccessor httpContextAccessor, string clientId = null)
        {
            if (ctx?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }
            var externalUser = ctx.Principal;
            var provider = ctx.Properties.Items[ChallangeConstants.Scheme];
            var claims = ctx.Principal.Claims.ToList();
            var shibbolethIssuer = claims.GetClaimSave(SAML2ClaimTypes.ShibbolethIssuer);
            
            var remoteIPAddress = ctx.Properties.Items[ChallangeConstants.RemoteIpAddress];

            if (shibbolethIssuer != null)
            {
                provider = shibbolethIssuer.Value;
            }
            var userIdClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);
            if (userIdClaim == null)
            {
                userIdClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            }
            if (userIdClaim == null)
            {
                throw new Exception("Unknown userid");
            }
            claims.Remove(userIdClaim);
            var userId = userIdClaim.Value;
            await using (var tslock = new TableStorageLock(userId))
            {
                if (!await tslock.TryEnter())
                {
                    if (CrmConfig.IsUnittest)
                    {
                        return (null, null, null);
                    }
                    throw new RateLimitException();
                }

                CrmUser user;
                if (provider == "aad")
                {
                    var citaviKey = await AzureB2CObjectIdCache.GetAsync(userId);
                    user = await DbContext.GetByKeyAsync(citaviKey);
                    if (user == null)
                    {
                        throw new NotSupportedException("Azure B2C User not found");
                    }
                }
                else
                {
                    user = await DbContext.GetByLinkedAccountAsync(provider, userId);
                }
                if (user == null)
                {
                    user = await UserManager.CreateUserFromExternalProviderAsync(provider, userId, claims, saveAndRemoveUserFromCache: false, remoteIPAddress: remoteIPAddress);
                }

                var campusContractManager = new CampusContractManager(DbContext);
                if (shibbolethIssuer != null &&
                    !user.IsNew())
                {
                    await campusContractManager.AddOrUpdateLicensesFromShibbolethClaimsAsync(user, claims);
                }

                if (httpContextAccessor != null)
                {
                    var requestIp = httpContextAccessor.HttpContext.Request.GetRemoteIpAddress();
                    await campusContractManager.UpadateLicensesAfterLoginAsync(user, requestIp);
                    UserManager.UpdateUserLanguage(user, httpContextAccessor.HttpContext.Request.Headers.GetBrowserPreferedLanguage());
                }
                if (!string.IsNullOrEmpty(clientId) && clientId != ClientIds.Web)
                {
                    user.SetLastLogin(clientId);
                }
                user.UpdateLastLoginInfoOnLinkedEntities(ActiveLoginAccountType.IdentityProvider, provider);
                await UserManager.DbContext.SaveAsync();
                await CrmUserCache.RemoveAsync(user);
                return (user, userId, provider);
            }
        }

        #endregion

        #region AuthenticateLocalAsync

        public async Task<CrmUser> AuthenticateLocalAsync(ResourceOwnerPasswordValidationContext ctx)
        {
            var clientId = "";
            if (ctx.Request != null)
            {
                clientId = ctx.Request.ClientId;
                if(string.IsNullOrEmpty(clientId) && ctx.Request.Client != null)
				{
                    clientId = ctx.Request.Client.ClientId;
                }
            }
            return await AuthenticateLocalAsync(ctx, string.Empty, null, clientId);
        }
        public async Task<CrmUser> AuthenticateLocalAsync(ResourceOwnerPasswordValidationContext ctx, IHttpContextAccessor httpContextAccessor, string clientId)
        {
            return await AuthenticateLocalAsync(ctx, httpContextAccessor.HttpContext.Request.GetRemoteIpAddress(), httpContextAccessor.HttpContext.Request.Headers.GetBrowserPreferedLanguage(), clientId);
        }
        async Task<CrmUser> AuthenticateLocalAsync(ResourceOwnerPasswordValidationContext ctx, string requestIp, LanguageType? language, string clientId)
        {
            CrmUser user = null;
            await using (var tslock = new TableStorageLock(ctx.UserName))
            {
                if (!await tslock.TryEnter())
                {
                    if (CrmConfig.IsUnittest)
                    {
                        return null;
                    }
                    throw new RateLimitException();
                }
                try
                {

                    UserManager.DbContext.SuspendUpdateUser = true;

                    var result = await UserManager.AuthenticateAsync(ctx.UserName, ctx.Password);
                    user = result.User;
                    if (result.Success)
                    {
                        var name = user.Username;

                        UserManager.UpdateUserLanguage(user, language);

                        var ccManager = new CampusContractManager(UserManager.DbContext);

                        await ccManager.UpadateLicensesAfterLoginAsync(user, requestIp, saveChanges: false);

                        if (!string.IsNullOrEmpty(clientId) && clientId != ClientIds.Web)
                        {
                            user.SetLastLogin(clientId);
                        }

                        var subject = user.Key;
                        var claims = Enumerable.Empty<Claim>();
                        ctx.Result = new GrantValidationResult(subject: subject, authenticationMethod: OidcConstants.AuthenticationMethods.Password, authTime: DateTime.UtcNow, claims: claims);
                    }
                    else if (user != null)
                    {
                        var linkedEMailAccount = user.GetLinkedEmailAccount(ctx.UserName);
                        if (linkedEMailAccount != null &&
                           !linkedEMailAccount.IsVerified.GetValueOrDefault())
                        {
                            ctx.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, AuthenticateResultConstants.LoginNotAllowedLinkedEMailAccountIsNotVerified);
                            Telemetry.TrackTrace(AuthenticateResultConstants.LoginNotAllowedLinkedEMailAccountIsNotVerified);
                            return user;
                        }
                    }
                    else
                    {
                        ctx.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "invalid_credential");
                    }

                    if (user != null)
                    {
                        if (!user.IsLoginAllowed)
                        {
                            ctx.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, AuthenticateResultConstants.AccountIsNotAllowedToLogin);
                            Telemetry.TrackDiagnostics(AuthenticateResultConstants.AccountIsNotAllowedToLogin, property1: ("ContactKey: ", user.Key));
                        }

                        if (user.IsAccountClosed)
                        {
                            ctx.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, AuthenticateResultConstants.AccountIsClosed);
                            Telemetry.TrackDiagnostics(AuthenticateResultConstants.AccountIsClosed, property1: ("ContactKey: ", user.Key));
                        }

                        if (user.FailedLoginCount >= CrmConfig.AccountLockoutFailedLoginAttempts)
                        {
                            ctx.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, AuthenticateResultConstants.AccountLockoutFailedLoginAttempts);
                            Telemetry.TrackDiagnostics(AuthenticateResultConstants.AccountLockoutFailedLoginAttempts, property1: ("ContactKey: ", user.Key));
                        }
                    }
                }
                finally
                {
                    UserManager.DbContext.SuspendUpdateUser = false;
                    if (user != null)
                    {
                        await UserManager.DbContext.SaveAsync(user);
                    }
                }
            }
            return user;
        }

        #endregion

        #region CreateAuthenticationProperties

        public static AuthenticationProperties CreateAuthenticationProperties(string redirectUri, string returnUrl, string scheme, HttpContext httpContext)
		{
            var props = new AuthenticationProperties()
            {
                RedirectUri = redirectUri,
                Items =
                    {
                        { ChallangeConstants.ReturnUrl, returnUrl },
                        { ChallangeConstants.Scheme, scheme },
                    }
            };

            props.Items.Add(ChallangeConstants.RemoteIpAddress, httpContext.Request.GetRemoteIpAddress());

            return props;
        }

        #endregion

        #region ProcessAddExternalAccountAsync

        public async Task<(bool success, string redirect)> ProcessAddExternalAccountAsync(string contactKey, AuthenticateResult result)
        {
            try
            {
                if (!result.Succeeded)
                {
                    return (false, "/");
                }
                if (!result.Principal.Identity.IsAuthenticated)
                {
                    return (false, "/");
                }
                if (string.IsNullOrEmpty(contactKey))
                {
                    Telemetry.TrackTrace($"ProcessAddExternalAccount: ContactKey is missing.", SeverityLevel.Warning);
                    return (false, "/");
                }
                var identityProviderId = result.Ticket.Properties.Items[ChallangeConstants.Scheme];

                var claims = result.Principal.Claims;
                var email = claims.GetFirstValue(JwtClaimTypes.Email, SAML2ClaimTypes.Email, ClaimTypes.Email);
                var userIdClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);
                if (userIdClaim == null)
                {
                    userIdClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                }
                if (userIdClaim == null)
                {
                    Telemetry.TrackTrace($"ProcessAddExternalAccount: UserIdClaim is missing.", SeverityLevel.Warning);
                    return (false, "/");
                }

                await using (var tslock = new TableStorageLock(contactKey))
                {
                    if (!await tslock.TryEnter())
                    {
                        if (CrmConfig.IsUnittest)
                        {
                            return (false, "/");
                        }
                        throw new RateLimitException();
                    }
                    var shibbolethIssuer = claims.GetClaimSave(SAML2ClaimTypes.ShibbolethIssuer);
                    if (shibbolethIssuer != null)
                    {
                        identityProviderId = shibbolethIssuer.Value;
                    }
                    using (var dbContext = new CrmDbContext())
                    {
                        var userManager = new CrmUserManager(dbContext);
                        var user = await userManager.GetByKeyAsync(contactKey);

                        var existingUser = await dbContext.GetByLinkedAccountAsync(identityProviderId, userIdClaim.Value);
                        if (existingUser != null && existingUser.Id != user.Id)
                        {
                            //#2064
                            if (CrmIdentityProviders.IsShibbolethProvider(identityProviderId))
                            {
                                if (string.IsNullOrEmpty(existingUser.Email))
                                {
                                    Telemetry.TrackTrace($"ProcessAddExternalAccountAsync Shibboleth alread exists without email -> Merge Accounts. {existingUser.Key}|{user.Key}");
                                    await DbContext.MergeAsync(existingUser, user);
                                    return (true, "/");
                                }
                            }
                            Telemetry.TrackTrace($"LinkedAccount already exists: {existingUser.Key}", SeverityLevel.Warning);
                            return (false, $"/account?epap={identityProviderId}&email={existingUser.Email}");
                        }

                        if (!string.IsNullOrEmpty(email))
                        {
                            existingUser = await dbContext.GetByEmailAsync(email);
                            if (existingUser != null && existingUser.Id != user.Id)
                            {
                                Telemetry.TrackTrace($"LinkedAccount already exists: {existingUser.Key} (via email {email})", SeverityLevel.Warning);
                                return (false, $"/account?epap={identityProviderId}&email={email}");
                            }
                        }

                        if (user.Contact.IsKeyContact)
                        {
                            throw new Exception(AuthenticateResultConstants.ContactUpdateFailedIsKeyContact);
                        }

                        await userManager.AddOrUpdateLinkedAccountAsync(user, identityProviderId, userIdClaim.Value);

                        if (!string.IsNullOrEmpty(email))
                        {
                            user.AddLinkedEMailAccount(email, true);
                            await userManager.AddLinkEmail(user.Contact.AzureB2CId, email);
                        }

                        var campusContractManager = new CampusContractManager(dbContext);
                        if (CrmIdentityProviders.IsShibbolethProvider(identityProviderId))
                        {
                            await campusContractManager.AddOrUpdateLicensesFromShibbolethClaimsAsync(user, claims);
                        }
                        else
                        {
                            await campusContractManager.AddLicensesFromEmailVerficationAsync(user, email);
                        }

                        await userManager.DbContext.SaveAndUpdateUserCacheAsync(user);
                    }

                    return (true, "/");
                }
            }
            catch (Exception ex) when (ex.Message != AuthenticateResultConstants.ContactUpdateFailedIsKeyContact)
            {
                Telemetry.TrackException(ex, flow: ExceptionFlow.Eat);
            }
            return (false, "/");
        }

        #endregion

        #region RevokeUserAccess

        /// <summary>
        /// Alle AccessTokens & Cookies des User werden entfernt
        /// User.IsLoginAllowed auf false gestellt
        /// </summary>
        /// <param name="contactKey"></param>
        /// <returns></returns>
        public async Task RevokeUserAccess(string contactKey)
        {
            var user = await DbContext.GetByKeyAsync(contactKey);

            if (user == null)
            {
                throw new NotSupportedException($"ContactKey not found: {contactKey}");
            }

            await RevokeUserAccessTokensAndCookies(contactKey);

            user.IsLoginAllowed = false;
            await DbContext.SaveAsync();
            await CrmUserCache.RemoveAsync(user);
        }

        #endregion

        #region RevokeUserAccessTokensAndCookies

        /// <summary>
        /// Alle AccessTokens & Cookies des Users werden entfernt
        /// </summary>
        /// <param name="contactKey"></param>
        /// <returns></returns>
        public static async Task RevokeUserAccessTokensAndCookies(string contactKey)
        {
            if (contactKey == null)
            {
                throw new NotSupportedException($"{nameof(contactKey)} must not be null");
            }

            await CookieStore.Instance.RemoveAllAsync(contactKey);
            await PersistedGrantStore.Instance.RemoveAllAsync2(contactKey);
        }

        /// <summary>
        /// Alle AccessTokens & Cookies des Users werden entfernt, ausser der aktiven BrowserSession
        /// </summary>
        /// <param name="contactKey"></param>
        /// <returns></returns>
        public static async Task RevokeUserAccessTokensAndCookiesExpectCurrentSession(CrmUser user, IHttpContextAccessor httpContextAccessor)
        {
            var accessToken = httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            var cookie = httpContextAccessor.HttpContext.Request.Cookies[IdentityServerConstants.DefaultCheckSessionCookieName];

            var referenceTokenStore = httpContextAccessor.HttpContext.RequestServices.GetService(typeof(IdentityServer4.Stores.IReferenceTokenStore)) as ReferenceTokenStore;
            var accessTokenHash = string.IsNullOrEmpty(accessToken) ? string.Empty : referenceTokenStore.KeyToHash(accessToken);

            await RevokeUserAccessTokensAndCookiesExpectCurrentSession(user, accessTokenHash, cookie);
        }

        /// <summary>
        /// Alle AccessTokens & Cookies des Users werden entfernt, ausser sessionAccessToken & sessionCookie
        /// </summary>
        /// <param name="contactKey"></param>
        /// <returns></returns>
        internal static async Task RevokeUserAccessTokensAndCookiesExpectCurrentSession(CrmUser user, string sessionAccessToken, string sessionCookie)
        {
            await CookieStore.Instance.RemoveAllAsync(user.Key, sessionCookie);
            await PersistedGrantStore.Instance.RemoveAllAsync2(user.Key, sessionAccessToken);
        }

        #endregion

        #endregion
    }
}
