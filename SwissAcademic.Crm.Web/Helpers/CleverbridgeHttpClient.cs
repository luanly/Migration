using SwissAcademic.KeyVaultUtils;
using SwissAcademic.Security;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    internal static class CleverbridgeHttpClient
    { 
        public static async Task InitializeAsync()
        {
            var cred = (await AzureHelper.KeyVaultClient.GetSecretAsync(KeyVaultSecrets.Credentials.CleverbridgeApi)).Split(';');

            ApiCredentials =  new NetworkCredential(cred[0], cred[1]);

            var handler = new HttpClientHandler { Credentials = ApiCredentials, CheckCertificateRevocationList = true };
            Instance = new HttpClient(handler);
            Instance.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        public static NetworkCredential ApiCredentials { get; private set; }

        internal static HttpClient Instance { get; private set; }
    }
}
