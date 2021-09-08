using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class GatewayHelper
    {
        public static string UpdateOAuthRedirectUri(string oAuthUri, bool isTIP)
        {
            var uri = new Uri(oAuthUri);
            var baseUri = uri.GetComponents(UriComponents.Scheme | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);
            var items = QueryHelpers.ParseQuery(uri.Query);
            var redirectUrl = items["redirect_uri"];

            var uriBuilder = new UriBuilder(redirectUrl);
            uriBuilder.Host = new Uri(isTIP ? ConfigurationManager.AppSettings["TiP_RootAuthority"] : UrlConstants.Authority).Host;
            items["redirect_uri"] = uriBuilder.Uri.ToString();

            var qb = new QueryBuilder(items.SelectMany(x => x.Value, (c, v) => new KeyValuePair<string, string>(c.Key, v)).ToList());

            return baseUri + qb.ToQueryString().ToString();
        }

        public static string UpdateRedirectUri(string redirectUri, bool isTIP)
        {
            var uriBuilder = new UriBuilder(redirectUri);
            uriBuilder.Host = new Uri(isTIP ? ConfigurationManager.AppSettings["TiP_RootAuthority"] : UrlConstants.Authority).Host;
            return uriBuilder.Uri.ToString();
        }

        public static Uri UpdateShibbolethRedirectUri(Uri assertionConsumerServiceUrl, bool isTIP)
        {
            var uriBuilder = new UriBuilder(assertionConsumerServiceUrl);
            uriBuilder.Host = new Uri(isTIP ? ConfigurationManager.AppSettings["TiP_RootAuthority"] : UrlConstants.Authority).Host;
            return uriBuilder.Uri;
        }
    }
}
