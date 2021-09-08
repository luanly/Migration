using System;
using System.Linq;
using System.Net;

namespace SwissAcademic.Crm.Web
{
    public abstract class CitaviCookie
    {
        public abstract string Domain { get; set; }
        public abstract DateTime? Expires { get; set; }
        public abstract string Name { get; set; }
        public string Value { get; set; }


        public static Cookie FromString(string cookieString)
        {
            var arr = cookieString.Split(';');
            var secure = false;
            var httponly = false;
            string domain = null;
            string path = null;
            DateTime? expires = null;
            var cookieName = arr.First().Split('=')[0];
            var cookieValue = WebUtility.UrlDecode(arr.First().Split('=')[1].Trim());

            foreach (var s in arr.Skip(1))
            {
                var s2 = s.Split('=');
                switch (s2[0].Trim().ToLowerInvariant())
                {
                    case "domain":
                        domain = s2[1].Trim();
                        break;

                    case "expires":
                        expires = DateTime.Parse(s2[1].Trim());
                        break;

                    case "path":
                        path = s2[1].Trim();
                        break;

                    case "secure":
                        secure = true;
                        break;

                    case "httponly":
                        httponly = true;
                        break;
                }
            }

            var cookie = new Cookie(cookieName, cookieValue, path, domain);
            cookie.Secure = secure;
            cookie.HttpOnly = httponly;
            cookie.Expires = expires.GetValueOrDefault();

            return cookie;
        }
    }
}
