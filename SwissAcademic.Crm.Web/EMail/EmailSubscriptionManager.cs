using Newtonsoft.Json.Linq;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.KeyVaultUtils;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class EmailSubscriptionManager
    {
        #region Felder

        const string URL = "https://us4.api.mailchimp.com/3.0/";
        static HttpClient Client = new HttpClient();
        static bool _initialized;

        #endregion

        #region Eigenschaften



        #endregion

        #region Methoden

        #region CalculateMD5Hash
        static string CalculateMD5Hash(string input)
        {
#pragma warning disable CA5351 // Keine beschädigten kryptografischen Algorithmen verwenden
            using (var md5 = System.Security.Cryptography.MD5.Create())
#pragma warning restore CA5351 // Keine beschädigten kryptografischen Algorithmen verwenden
            {
                var inputBytes = Encoding.ASCII.GetBytes(input.ToLowerInvariant());
                var hash = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (var h in hash)
                {
                    sb.Append(h.ToString("X2"));
                }
                return sb.ToString();
            }
        }

        #endregion

        #region GetUserSubscriptions

        public async Task<UserSubscriptionStatus> GetUserSubscriptionStatus(Contact contact, string listId)
        {
            var md5 = CalculateMD5Hash(contact.EMailAddress1);
            using (var response = await Client.GetAsync($"lists/{listId}/members/{md5}"))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return UserSubscriptionStatus.notsubscribed;
                }
                var json = await response.Content.ReadAsAsync<JObject>();
                return json["status"].Value<string>().ParseEnum<UserSubscriptionStatus>();
            }
        }

        #endregion

        #region Initalize

        public async Task InitializeAsync()
        {
            if(_initialized)
            {
                return;
            }

            var apiKey = await AzureHelper.KeyVaultClient.GetSecretAsync(KeyVaultSecrets.ApiKeys.MailChimp);
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiKey);
            Client.BaseAddress = new Uri(URL);

            _initialized = true;
        }

        #endregion

        #region SubscribeNews

        public async Task<bool> SubscribeNews(Contact contact)
        {
            try
            {
                var emailSubscription = contact.Language == LanguageType.German ? EmailSubscription.NewsGerman : EmailSubscription.NewsEnglish;
                var status = await GetUserSubscriptionStatus(contact, emailSubscription.Id);
                if (status == UserSubscriptionStatus.subscribed)
                {
                    return true;
                }

                if (status == UserSubscriptionStatus.cleaned)
                {
                    return true;
                }

                await Subscribe(emailSubscription.Id, contact.EMailAddress1);

                foreach (var listIdInOtherLanguage in emailSubscription.ListIdsInOtherLanguages)
                {
                    var statusOtherLanguages = await GetUserSubscriptionStatus(contact, listIdInOtherLanguage);
                    if (statusOtherLanguages == UserSubscriptionStatus.subscribed)
                    {
                        await Unsubscribe(contact.EMailAddress1, listIdInOtherLanguage);
                    }
                }
                return true;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
                return false;
            }
        }

        public async Task<bool> Subscribe(string listId, string email)
        {
            var md5 = CalculateMD5Hash(email);
            var json = new
            {
                email_address = email,
                status = "subscribed"
            };
            HttpResponseMessage response = null;
            try
            {
                response = await Client.PostAsJsonAsync($"lists/{listId}/members", json);
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    response = await Client.PutAsJsonAsync($"lists/{listId}/members/{md5}", json);
                }
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Telemetry.TrackTrace($"Mailchimp failed with statuscode {response.StatusCode}: {listId}: {email}");
                }
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            finally
            {
                if(response != null)
                {
                    response.Dispose();
                }
            }
        }

        #endregion

        #region UnsubscribeNews

        public async Task UnsubscribeNews(Contact contact)
        {
            try
            {
                var emailSubscription = contact.Language == LanguageType.German ? EmailSubscription.NewsGerman : EmailSubscription.NewsEnglish;
                await Unsubscribe(contact.EMailAddress1, emailSubscription.Id);
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
            }
        }

        #endregion

        #region Unsubscribe

        public async Task<bool> Unsubscribe(string email, string listId)
        {
            if(string.IsNullOrEmpty(email))
            {
                throw new NotSupportedException("Email must no be null");
            }
            if (string.IsNullOrEmpty(listId))
            {
                throw new NotSupportedException("ListId must no be null");
            }
            try
            {
                var md5 = CalculateMD5Hash(email);
                var json = new
                {
                    email_address = email,
                    status = "unsubscribed"
                };
                using (var response = await Client.PutAsJsonAsync($"lists/{listId}/members/{md5}", json))
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Telemetry.TrackTrace($"Mailchimp failed with statuscode {response.StatusCode}: {listId}: {email}");
                    }
                    return response.StatusCode == System.Net.HttpStatusCode.OK;
                }
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
            }
            return false;
        }

        #endregion

        #endregion
    }

    public enum UserSubscriptionStatus
    {
        /// <summary>
        /// This address is on the list and ready to receive email. You can only send campaigns to ‘subscribed’ addresses.
        /// </summary>
        subscribed,
        /// <summary>
        /// This address used to be on the list but isn’t anymore.
        /// </summary>
        unsubscribed,
        /// <summary>
        /// This address requested to be added with double-opt-in but hasn’t confirmed their subscription yet.
        /// </summary>
        pending,
        /// <summary>
        /// This address bounced and has been removed from the list.
        /// </summary>
        cleaned,
        notsubscribed,
        error
    }

}
