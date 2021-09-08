using Newtonsoft.Json;
#if !Web
using SwissAcademic.Net;
#endif
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace SwissAcademic.Azure
{
    public class CloudAlertInfo
    {
        public Version VersionFrom { get; set; }
        public Version VersionTo { get; set; }
        public bool Internal { get; set; }
        public int ProjectType { get; set; }

#region GetCloudAlert

        public static async Task<CloudAlertInfo> GetCloudAlertAsync()
        {
#if !Web
            if (!NetworkInformation.IsNetworkAvailable) return null;
#endif

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var data = await httpClient.GetStringAsync(ConfigurationManager.AppSettings["CloudAlertDownloadUrl"]);
                    if (!string.IsNullOrEmpty(data))
                    {
                        return JsonConvert.DeserializeObject<CloudAlertInfo>(data);
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }

#endregion
    }
}
