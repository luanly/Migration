using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class HttpContextAccessorFactory
    {
        //Ich verstehe es nicht ganz, aber hier können wir nicht mit async await arbeiten
        //Bsp: public static async Task<IHttpContextAccessor> Create(Dictionary<string, string> properties)
        //Das führt dazu, dass der HTTPContext bei Aufrufen null ist
        //Weil HttpContext isn't thread safe ?!? HttpContext DARF NICHT in einem async/await Methode erstellt werden


        public static IHttpContextAccessor Create()
        {
            var hc = new HttpContextAccessor();
            hc.HttpContext = new DefaultHttpContext();
            return hc;
        }

        /// <summary>
        /// Only for Unittests or if we need access pd-storage of an other datacenter
        /// </summary>
        /// <param name="user"></param>
        /// <param name="projectEntry"></param>
        /// <param name="ProjectDataStorage"></param>
        /// <returns></returns>
        public static IHttpContextAccessor Create(CrmUser user, ProjectEntry projectEntry, object projectDataStorage)
        {
            var hc = new HttpContextAccessor();
            hc.HttpContext = new DefaultHttpContext();
            hc.SetProject(projectEntry);
            hc.SetCrmUser(user);
            hc.HttpContext.Items.Add("ProjectData", projectDataStorage);
            return hc;
        }

        public static async Task<IHttpContextAccessor> Populate(this IHttpContextAccessor hc, Dictionary<string, string> properties)
        {
            foreach (var prop in properties)
            {
                hc.HttpContext.Items.Add(prop.Key, prop.Value);
            }

            if (properties.ContainsKey(MessageKey.ProjectKey))
            {
                var projectEntry = await CrmCache.Projects.GetAsync(properties[MessageKey.ProjectKey]);
                if (projectEntry != null)
                {
                    hc.SetProject(projectEntry);
                }
            }
            if (properties.ContainsKey(MessageKey.ContactKey))
            {
                var user = await CrmUserCache.GetAsync(properties[MessageKey.ContactKey]);
                if (user != null)
                {
                    hc.HttpContext.User = user;
                    hc.SetCrmUser(user);
                }
            }

            if (properties.ContainsKey(MessageKey.ClientVersion))
            {
                hc.HttpContext.Request.Headers.TryAdd(MessageKey.ClientVersion, new Microsoft.Extensions.Primitives.StringValues(properties[MessageKey.ClientVersion]));
            }

            return hc;
        }
    }
}
