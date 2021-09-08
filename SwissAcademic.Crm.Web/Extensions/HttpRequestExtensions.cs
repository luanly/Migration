using Microsoft.AspNetCore.Http;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace SwissAcademic.Crm.Web
{
    public static class HttpRequestExtensions
    {
        public static bool IsTestingInProductionRequest(this HttpRequest request)
         => !string.IsNullOrEmpty(GetTestingInProductionRoutingName(request));

        public static string GetTestingInProductionRoutingName(this HttpRequest request)
        {
            if (request == null)
            {
                return null;
            }
            try
            {
                if (request.Cookies.TryGetValue(CookieNames.TestingInProduction, out var tip))
                {
                    if (string.IsNullOrEmpty(tip))
                    {
                        return null;
                    }
                    if (string.Equals(tip, "self", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return null;
                    }
                    if (request.Query.TryGetValue("x-ms-routing-name", out var route))
                    {
                        if (string.Equals(route, "self", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return null;
                        }
                        return route;
                    }
                    return tip;
                }
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Warning, ExceptionFlow.Eat);
            }
            return null;
        }
        public static string GetLanguageCode(this HttpRequest request)
        {
            if (request.Headers.TryGetValue(MessageKey.LanguageCode, out var value))
            {
                return value;
            }

            return null;
        }

        public static void SetClientVersion(this HttpRequest request, string clientVersion)
		{
            if (string.IsNullOrEmpty(clientVersion))
            {
                return;
            }
            request.Headers.Add(MessageKey.ClientVersion, clientVersion);
        }
        public static string GetClientVersion(this HttpRequest request)
        {
            if (request.Headers.TryGetValue(MessageKey.ClientVersion, out var value))
            {
                return value;
            }

            return null;
        }
        public static string GetRemoteIpAddress(this HttpRequest request)
        {
            if (request.Headers.TryGetValue(HttpHeaderNames.OriginalIPAddress, out var val))
            {
                try
                {
                    //85.5.111.47:33371,20.76.43.173, 20.67.118.85:2944
                    var ipAddressWithPort = val.ToString().Split(':')[0].Trim();
                    if (IPAddress.TryParse(ipAddressWithPort, out var ipAddress))
                    {
                        return ipAddress.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Telemetry.TrackException(ex, SeverityLevel.Warning, ExceptionFlow.Eat);
                }
            }
            return request.HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        public static LanguageType? GetBrowserPreferedLanguage(this IHeaderDictionary headers)
        {
            try
            {
                if (headers == null)
                {
                    return null;
                }

                if (!headers.Any(k => k.Key == "Accept-Language"))
                {
                    return null;
                }

                var lngHeader = headers["Accept-Language"];
                return ParseHeaderLanguage(lngHeader);
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
            }
            return null;
        }
        public static LanguageType? ParseHeaderLanguage(string lngHeader)
        {
            try
            {
                var languages = lngHeader.Split(new[] { ',' })
                                         .Select(a => StringWithQualityHeaderValue.Parse(a))
                                         .Select(a => new StringWithQualityHeaderValue(a.Value.Split('-')[0].ToLowerInvariant(),
                                             a.Quality.GetValueOrDefault(1)))
                                         .OrderByDescending(a => a.Quality);

                foreach (var lng in languages)
                {
                    switch (lng.Value)
                    {
                        case "de":
                            return LanguageType.German;

                        case "en":
                            return LanguageType.English;

                        case "fr":
                            return LanguageType.French;

                        case "it":
                            return LanguageType.Italian;

                        case "pl":
                            return LanguageType.Polish;

                        case "pt":
                            return LanguageType.Portuguese;

                        case "es":
                            return LanguageType.Spanish;

                        default:
                            continue;
                    }
                }
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
            }

            return null;
        }
        public static LanguageType? ParseLanguage(string language)
        {
            switch (language)
            {
                case "de":
                    return LanguageType.German;

                case "en":
                    return LanguageType.English;

                case "fr":
                    return LanguageType.French;

                case "it":
                    return LanguageType.Italian;

                case "pl":
                    return LanguageType.Polish;

                case "pt":
                    return LanguageType.Portuguese;

                case "es":
                    return LanguageType.Spanish;
            }

            return null;
        }

        public static bool IsInternetExplorer(this HttpRequest request)
        {
            var userAgent = request.Headers["User-Agent"];
            if (string.IsNullOrEmpty(userAgent))
            {
                return true;
            }
            return userAgent.ToString().Contains("MSIE") || userAgent.ToString().Contains("Trident");
        }

        public static bool IsPickerOrAssistantLoginRequest(this HttpRequest request)
        {
            try
            {
                var path = request.Path.ToString();
                return path.Contains(ClientIds.WebWordAddIn) ||
                       path.Contains(ClientIds.EdgePicker) ||
                       path.Contains(ClientIds.GoogleChromePicker) ||
                       path.Contains(ClientIds.FirefoxPicker);
            }
            catch { }
            return false;
        }
        public static string GetRequestHost(this HttpRequest request)
        {
            if (request.Headers.TryGetValue(HttpHeaderNames.OriginalHostHeaderName, out var requestHost))
            {
                return requestHost;
            }
            return request.Host.Value.Replace("https://", string.Empty);
        }

        static List<string> LegacyDomains = new List<string>
        {
            "alphacitaviweb-dev.citavi.com",
            "citaviweb2.citavi.com",
            "citaviweb-staging.citavi.com",
        };
        public static bool IsLegacyRequest(this HttpRequest request, out string host)
        {
            host = GetRequestHost(request);
            return LegacyDomains.Contains(host);
        }
    }
}
