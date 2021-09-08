using IdentityServer4.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Globalization;
using System;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class HttpContextExtensions
    {
        public const string CrmUser = "CrmUser";
        public const string Project = "Project";

        #region Set / Contains / Get

        public static void Set<T>(this IHttpContextAccessor httpContextAccessor, T value, string name) => Set(httpContextAccessor?.HttpContext, value, name);
        static void Set<T>(this HttpContext httpContext, T value, string name)
        {
            if (httpContext == null)
            {
                return;
            }

            httpContext.Items[name] = value;
        }

        static bool Contains(this IHttpContextAccessor httpContextAccessor, string name) => Contains(httpContextAccessor?.HttpContext, name);
        static bool Contains(this HttpContext httpContext, string name) => httpContext?.Items.ContainsKey(name) == true;

        public static T Get<T>(this IHttpContextAccessor httpContextAccessor, string name) => Get<T>(httpContextAccessor?.HttpContext, name);
        static T Get<T>(this HttpContext httpContext, string name)
        {
            if (httpContext?.Items?.ContainsKey(name) == false)
            {
                return default(T);
            }
            if(httpContext == null)
			{
                return default(T);
			}
            return (T)httpContext.Items[name];
        }

        #endregion

        #region CrmUser

        public static void SetCrmUser(this IHttpContextAccessor httpContextAccessor, CrmUser user) => Set(httpContextAccessor?.HttpContext, user, CrmUser);
        public static void SetCrmUser(this HttpContext httpContext, CrmUser user) => Set(httpContext, user, CrmUser);

        public static bool ContainsCrmUser(this IHttpContextAccessor httpContextAccessor) => Contains(httpContextAccessor?.HttpContext, CrmUser);
        public static bool ContainsCrmUser(this HttpContext httpContext) => Contains(httpContext, CrmUser);

        public static CrmUser GetCrmUser(this IHttpContextAccessor httpContextAccessor) => Get<CrmUser>(httpContextAccessor?.HttpContext, CrmUser);
        public static CrmUser GetCrmUser(this HttpContext httpContext) => Get<CrmUser>(httpContext, CrmUser);
        public static IPrincipal GetPrincipal(this IHttpContextAccessor httpContextAccessor)
        {
            var user = Get<CrmUser>(httpContextAccessor?.HttpContext, CrmUser);
            if(user != null)
            {
                return (ClaimsPrincipal)user;
            }
            return null;
        }

        #endregion

        #region ProjectEntry

        public static void SetProject(this IHttpContextAccessor httpContextAccessor, ProjectEntry projectEntry)
         => SetProject(httpContextAccessor.HttpContext, projectEntry);
        public static void SetProject(this HttpContext httpContext, ProjectEntry projectEntry)
        {
            Set(httpContext, projectEntry, Project);
            Set(httpContext, projectEntry.Key, MessageKey.ProjectKey);
        }

        public static void SetProjectKey(this HttpContext httpContext, string projectKey)
        {
            Set(httpContext, projectKey, MessageKey.ProjectKey);
        }

        public static ProjectEntry GetProject(this IHttpContextAccessor httpContextAccessor)
            => Get<ProjectEntry>(httpContextAccessor?.HttpContext, Project);

        public static string GetProjectKey(this IHttpContextAccessor httpContextAccessor)
            => Get<string>(httpContextAccessor?.HttpContext, MessageKey.ProjectKey);

        #endregion

        #region GetUserRegion

        #region GetRegion

        public static async Task<(RegionInfo Region, CrmUser User)> GetUserRegion(this IHttpContextAccessor httpContextAccessor, CrmDbContext context)
        {
            RegionInfo regionInfo = null;
            CrmUser user = null;
            try
            {
                if (httpContextAccessor.HttpContext.User.IsAuthenticated())
                {
                    var contactKey = httpContextAccessor.HttpContext.User.GetContactKey();
                    user = httpContextAccessor.GetCrmUser();
                    if (user == null)
                    {
                        user = await context.GetByKeyAsync(contactKey, updateCacheIfMissing: true);
                        if (user != null)
                        {
                            httpContextAccessor?.SetCrmUser(user);
                        }
                    }
                    if (!string.IsNullOrEmpty(user.Contact.Address1_Country))
                    {
                        CultureInfoUtility.TryGetRegionInfo(user.Contact.Address1_Country, out regionInfo);
                    }
                }
                if (regionInfo == null)
                {
                    var country = await ShopManager.GetClientRegionInfo(httpContextAccessor.HttpContext.Request.GetRemoteIpAddress());
#if DEBUG
                    country = "CH";
#endif
                    if (!string.IsNullOrEmpty(country))
                    {
                        CultureInfoUtility.TryGetRegionInfo(country, out regionInfo);
                    }
                }
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return (regionInfo, user);
        }

		#endregion

		#endregion

		#region SecureCookie

		public static void AddSecureResponseCookie(this HttpContext httpContext, string cookieName, string value, IAzureRegionResolver resolver, IDataProtector dataProtector)
         => AddSecureResponseCookie(httpContext, cookieName, value, resolver.GetCookieDomain(), dataProtector, DateTimeOffset.UtcNow.AddMinutes(5));
        public static void AddSecureResponseCookie(this HttpContext httpContext, string cookieName, string value, string domain, IDataProtector dataProtector, DateTimeOffset expiry)
        {
			if (string.IsNullOrEmpty(cookieName))
			{
                return;
			}
            var session_token = $"{value}|{expiry.ToUnixTimeMilliseconds()}";
            httpContext.Response.Cookies.Append(
            $"{cookieName}",
            $"{dataProtector.Protect(session_token).EncodeToBase64()}",
            new CookieOptions
            {
                Domain = domain,
                HttpOnly = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddMinutes(5)
            });
        }

        public static bool TryGetSecureRequestCookie(this HttpContext httpContext, string cookieName, IDataProtector dataProtector, out string value, bool deleteCookie = true)
        {
            value = string.Empty;
            if (httpContext.Request.Cookies.TryGetValue(cookieName, out var sessionTokenParameter))
            {
                var sessionToken = dataProtector.Unprotect(sessionTokenParameter.ToString().DecodeFrom64()).Split('|');
                var expiry = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(sessionToken[1]));
                if (expiry > DateTimeOffset.UtcNow)
                {
                    value = sessionToken[0];
                    if (deleteCookie)
                    {
                        httpContext.Response.Cookies.Delete(cookieName);
                    }
                }
            }
            return !string.IsNullOrEmpty(value);
        }

        #endregion
    }
}
