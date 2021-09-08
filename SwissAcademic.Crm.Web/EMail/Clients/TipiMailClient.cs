using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Crm.Web.EMail.Clients.TipiMail;
using SwissAcademic.KeyVaultUtils;
using SwissAcademic.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    internal class TipiMailClient
        :
        IEmailClient
    {
        #region Felder

        static HttpClient Client;
        const string BaseUrl = "https://api.tipimail.com/v1/";

        #endregion

        #region Methoden

        public async Task<bool> AddBounces(string email)
        {
            var json = new JObject();
            json["type"] = "bounces";
            json["email"] = email;

            var response = await Client.PostAsJsonAsync("blacklists", json);
            var responseText = await response.Content.ReadAsStringAsync();
            var success = responseText.Contains("success");
            if (!success)
            {
                var token = JsonConvert.DeserializeObject<JObject>(responseText);
                token["email"] = email;
                Telemetry.TrackTrace("AddBounces failed", SeverityLevel.Error, property1: ("status", token));
            }
            return success;
        }

        public async Task<bool> DeleteBounces(string email)
        {
            var response = await Client.DeleteAsync($"blacklists/bounces/{email}");
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return true;
            }
            var responseText = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<JObject>(responseText);
            token["email"] = email;
            Telemetry.TrackTrace("DeleteBounces failed", SeverityLevel.Error, property1: ("status", token));

            return false;
        }

        public async Task<IEnumerable<EmailBounce>> GetBounces(string email)
        {
            var list = new List<EmailBounce>();

            var response = await Client.GetAsync($"blacklists/bounces/{email}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return list;
            }
            var responseText = await response.Content.ReadAsStringAsync();
            var bounces = JObject.Parse(responseText);
            if (bounces.ContainsKey("error"))
            {
                bounces["email"] = email;
                Telemetry.TrackTrace("GetBounces failed", SeverityLevel.Error, property1: ("status", bounces));
                return list;
            }

            var emailBounce = new EmailBounce();
            emailBounce.Created = bounces["createdDate"].Value<long>();
            emailBounce.Email = bounces["email"].Value<string>();
            emailBounce.Reason = bounces["blacklist"].Value<string>();
            list.Add(emailBounce);

            return list;
        }

        [ExcludeFromCodeCoverage]
        public async Task InitalizeAsync()
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri(BaseUrl);

            var key = (await AzureHelper.KeyVaultClient.GetSecretAsync(KeyVaultSecrets.ApiKeys.TipiMail)).Split(':');

            Client.DefaultRequestHeaders.Add("X-Tipimail-ApiUser", key[0]);
            Client.DefaultRequestHeaders.Add("X-Tipimail-ApiKey", key[1]);
        }

        public async Task<IEnumerable<EmailBounce>> ListBounces(int limit = 500, int pageNumber = 0)
        {
            var list = new List<EmailBounce>();

            var json = new JObject();
            json["type"] = "bounces";
            json["pageSize"] = limit;
            json["page"] = pageNumber;

            var response = await Client.PostAsJsonAsync("blacklists/list", json);
            var responseText = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseText))
            {
                return list;
            }

            try
            {
                var bounces = JObject.Parse(responseText);

                foreach (var bounce in bounces["bounces"])
                {
                    var emailBounce = new EmailBounce();
                    emailBounce.Created = bounce["createdDate"].Value<long>();
                    emailBounce.Email = bounce["email"].Value<string>();
                    emailBounce.Id = bounce["id"].Value<string>();
                    emailBounce.Reason = "bounces";
                    list.Add(emailBounce);
                }
            }
            catch(Exception ex)
			{
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
			}

            return list;
        }

        [ExcludeFromCodeCoverage]
        public async Task<bool> SendAsync(CrmEmail email, string mailAddress, IEnumerable<EmailAttachment> attachments, string fromAddress, string fromName, string tag, params string[] moreReceivers)
        {
            try
            {
                if (!EmailClient.IsValid(mailAddress))
                {
                    return true;
                }

                var tipiEmail = new TipiMailEmail();

                tipiEmail.To.Add(new TipiMailEmailAddress(mailAddress, string.Empty));

                tipiEmail.Message.From = new TipiMailEmailAddress(EmailClient.FromAddress, EmailClient.FromName);
                tipiEmail.Message.Subject = email.Subject;
                tipiEmail.Message.Html = email.Description;
                tipiEmail.Message.Text = email.PlainText;

                if (!string.IsNullOrEmpty(fromAddress))
                {
                    tipiEmail.Message.ReplyTo = new TipiMailEmailAddress(fromAddress, fromName);
                }
                
                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        var sendGridAttachment = new TipiMailContent();
                        sendGridAttachment.Content = attachment.BodyBase64;
                        sendGridAttachment.FileName = attachment.FileName;
                        sendGridAttachment.ContentType = attachment.MimeType;
                        tipiEmail.Message.Attachments.Add(sendGridAttachment);
                    }
                }

                var response = await Client.PostAsJsonAsync("messages/send", tipiEmail);
                var status = await response.Content.ReadAsStringAsync();
                var success = status.Contains("success");
                if (!success)
                {
                    var token = JsonConvert.DeserializeObject<JObject>(status);
                    token["email"] = mailAddress;
                    Telemetry.TrackTrace("Send email failed", SeverityLevel.Error, property1: ("status", token));
                }
                return success;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat, property1: (nameof(TelemetryProperty.Description), mailAddress));
            }
            return false;
        }

        [ExcludeFromCodeCoverage]
        public async Task<bool> SendAsync(string body, string toAddress, string fromAddress, string fromName, string subject, bool isHtml, EmailAttachment attachment, string tag)
        {
            try
            {
                if (!EmailClient.IsValid(toAddress))
                {
                    return true;
                }

                var tipiEmail = new TipiMailEmail();

                tipiEmail.To.Add(new TipiMailEmailAddress(toAddress, string.Empty));

                tipiEmail.Message.Subject = subject;
                tipiEmail.Message.Html = isHtml ? body : string.Empty;
                tipiEmail.Message.Text = !isHtml ? body : string.Empty;
                tipiEmail.Message.From = new TipiMailEmailAddress(fromAddress, fromName);

                if (!string.IsNullOrEmpty(tag))
                {
                    tipiEmail.AddTag(tag);
                }

                if (attachment != null)
                {
                    var sendGridAttachment = new TipiMailContent();
                    sendGridAttachment.Content = attachment.BodyBase64;
                    sendGridAttachment.FileName = attachment.FileName;
                    sendGridAttachment.ContentType = attachment.MimeType;
                    tipiEmail.Message.Attachments.Add(sendGridAttachment);
                }
                
                var response = await Client.PostAsJsonAsync("messages/send", tipiEmail);
                var status = await response.Content.ReadAsStringAsync();
                var success = status.Contains("success");
                if (!success)
                {
                    var token = JsonConvert.DeserializeObject<JObject>(status);
                    token["email"] = toAddress;
                    Telemetry.TrackTrace("Send email failed", SeverityLevel.Error, property1: ("status", token));
                }
                return success;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat, property1: (nameof(TelemetryProperty.Description), toAddress));
            }
            return false;
        }

        public async Task<bool> SendBulkAsync(CrmEmail email, IEnumerable<Contact> contacts, string fromAddress, string fromName, string tag)
        {
            var tos = new List<TipiMailEmailAddress>();
            var toAddresses = contacts.Select(c => c.EMailAddress1);

            foreach (var toAddress in toAddresses)
            {
                if (!EmailClient.IsValid(toAddress))
                {
                    return true;
                }
                tos.Add(new TipiMailEmailAddress(toAddress, string.Empty));
            }

            if (!tos.Any())
            {
                return true;
            }

            var tipiEmail = new TipiMailEmail();

            tipiEmail.Message.From = new TipiMailEmailAddress(fromAddress, fromName);
            tipiEmail.Message.Subject = email.Subject;
            tipiEmail.Message.Html = email.Description;
            tipiEmail.Message.Text = email.PlainText;

            foreach (var to in tos)
            {
                tipiEmail.To.Add(to);
            }

            tipiEmail.EnableBulkMail();

            if (!string.IsNullOrEmpty(tag))
            {
                tipiEmail.AddTag(tag);
            }

            var response = await Client.PostAsJsonAsync("messages/send", tipiEmail);
            var status = await response.Content.ReadAsStringAsync();
            var success = status.Contains("success");
            if (!success)
            {
                var token = JsonConvert.DeserializeObject<JObject>(status);
                token["subject"] = email.Subject;
                Telemetry.TrackTrace("Send email failed", SeverityLevel.Error, property1: ("status", token));
            }
            return success;
        }

		public Task<IEnumerable<EmailBounce>> GetBlocks(string email) => throw new NotImplementedException();
		public Task<IEnumerable<EmailBounce>> ListBounces(DateTimeOffset start, DateTimeOffset end) => throw new NotImplementedException();
		public Task<IEnumerable<EmailBounce>> ListBlocks(DateTimeOffset start, DateTimeOffset end) => throw new NotImplementedException();
        public Task<bool> DeleteBlocks(string email) => Task.FromResult(true);

		#endregion
	}
}
