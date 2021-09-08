using Microsoft.AspNetCore.Http;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace SwissAcademic.Crm.Web
{
    public static class HttpResponseExtensions
    {
        public static void AddCitaviRegistrationCookie(this HttpResponse response, string clientId)
		{
            var cookie = new CitaviRegistrationCookie(clientId);
            var option = new CookieOptions();
            option.Domain = AzureRegionResolver.Instance.GetCookieDomain();
            option.Expires = cookie.Expires;
            option.HttpOnly = true;
            option.Secure = true;
            option.SameSite = SameSiteMode.Lax;
            response.Cookies.Append(cookie.Name, cookie.Value, option);
        }

        public static void DeleteCitaviRegistrationCookie(this HttpResponse response)
		{
			var option = new CookieOptions();
            option.Domain = AzureRegionResolver.Instance.GetCookieDomain();
            option.HttpOnly = true;
			option.Secure = true;
			option.SameSite = SameSiteMode.Lax;
            response.Cookies.Delete(CookieNames.RegistrationInfo, option);
		}
    }
}
