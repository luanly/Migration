using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class CrmAccessTokenClient
        :
        IDisposable
    {
        #region Felder

        bool IsDisposed;
        const string AuthenticateHeader = "WWW-Authenticate";
        const string Bearer = "bearer";
        const string AuthorityKey = "authorization_uri";
        const string ResourceKey = "resource_id";

        static Task RenewAccessTokenTask;
        int RenewAccessTokenDelay = 0;
        HttpClient Client;
        CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        string TokenEndPoint;
        string ResourceId;
        HttpClient TokenClient;
        HttpClientHandler ClientHandler = new HttpClientHandler { UseCookies = false, CheckCertificateRevocationList = true };
        FormUrlEncodedContent AuthorizeContent;
        IEnumerable<KeyValuePair<string, string>> AuthorizeContentValueCollection;

        #endregion

        #region Eigenschaften

        public string AccessToken { get; private set; }
        public string UserName { get; private set; }

        #endregion

        #region Methoden

        #region Authorize

        async Task Authorize()
        {
            HttpResponseMessage responseResult;
            try
            {
                responseResult = await TokenClient.PostAsync(TokenEndPoint, AuthorizeContent);
            }
            catch (ObjectDisposedException)
            {
                AuthorizeContent = new FormUrlEncodedContent(AuthorizeContentValueCollection);
                responseResult = await TokenClient.PostAsync(TokenEndPoint, AuthorizeContent);
            }
            try
            {
                var json = Newtonsoft.Json.Linq.JObject.Parse(await responseResult.Content.ReadAsStringAsync());
                RenewAccessTokenDelay = json.Value<int>("expires_in") - 300;
                AccessToken = json.Value<string>("access_token");
            }
            finally
            {
                if(responseResult != null)
                {
                    responseResult.Dispose();
                }
            }
        }

        #endregion

        #region Connect
        public async Task Connect(string url, string user, string pass, string clientId, bool autoRenewAccessToken = true)
        {
            UserName = user;

            var api = "api/data/v9.0/";
            Client = new HttpClient(ClientHandler);
            Client.BaseAddress = new Uri(url);
            Client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            Client.DefaultRequestHeaders.Add("OData-Version", "4.0");
            Client.DefaultRequestHeaders.Add("If-None-Match", "");
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            TokenClient = new HttpClient();
            using (var msg = new HttpRequestMessage(HttpMethod.Get, UrlBuilder.Combine(url, api)))
            {
                using (var response = await TokenClient.SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();

                    var authenticateHeader = response.Headers.GetValues(AuthenticateHeader).FirstOrDefault().Trim();
                    authenticateHeader = authenticateHeader.Substring(Bearer.Length).Trim();
                    var authenticateHeaderItems = authenticateHeader.Split(',')
                                                                    .Select(s => s.Split('='))
                                                                    .ToDictionary(r => r[0].Trim(), r => r[1].Trim());

                    ResourceId = authenticateHeaderItems[ResourceKey];
                    TokenEndPoint = authenticateHeaderItems[AuthorityKey].Replace("/authorize", "/token");

                    AuthorizeContentValueCollection = new[] {
                    new KeyValuePair<string,string>("username",UserName),
                    new KeyValuePair<string,string>("password",pass),
                    new KeyValuePair<string,string>("grant_type","password"),
                    new KeyValuePair<string,string>("client_id",clientId),
                    new KeyValuePair<string,string>("resource",ResourceId),
                };

                    AuthorizeContent = new FormUrlEncodedContent(AuthorizeContentValueCollection);

                    await Authorize();
                    if (autoRenewAccessToken)
                    {
                        RenewAccessTokenTask = RenewAccessToken();
                    }
                }
            }
        }

        #endregion

        #region Disconnect

        public void Disconnect()
        {
            CancellationTokenSource.Cancel(false);
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                ClientHandler.Dispose();
                TokenClient.Dispose();
                Client.Dispose();
                CancellationTokenSource.Dispose();
                AuthorizeContent?.Dispose();
            }

            IsDisposed = true;
        }

        #endregion

        #region RenewAccessToken

        async Task RenewAccessToken()
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(RenewAccessTokenDelay));
                    await Authorize();
                }
                catch (Exception ex)
                {
                    ex.Data["UserName"] = UserName;
                    RenewAccessTokenDelay = 60;
                    Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
                }
            }
            Client = null;
        }

        #endregion

        #endregion
    }
}
