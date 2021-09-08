using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.KeyVaultUtils;
using SwissAcademic.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    internal class SendGridEmailClient
        :
        IEmailClient
    {
        #region Felder

        static string SendGridApiKey;

        #endregion

        #region Methoden

        public async Task<bool> DeleteBounces(string email)
        {
            var client = new SendGridClient(SendGridApiKey);

            try
            {
                var response = await client.RequestAsync(method: SendGridClient.Method.DELETE, urlPath: $"suppression/bounces/{email}");
                var text = await response.Body.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return true;
                }
                else if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    if (CrmConfig.IsUnittest)
                    {
                        throw new Exception($"DELETE delete suppression/bounces failed with statuscode '{response.StatusCode}'");
                    }
                    Telemetry.TrackTrace($"DELETE delete suppression/bounces failed with statuscode '{response.StatusCode}'. Reason: {text}", SeverityLevel.Error, property1: ("email", email));
                }
            }
            catch (Exception ignored)
            {
                if (CrmConfig.IsUnittest)
                {
                    throw;
                }
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return false;
        }

        public async Task<bool> DeleteBlocks(string email)
        {
            var client = new SendGridClient(SendGridApiKey);
            try
            {
                var response = await client.RequestAsync(method: SendGridClient.Method.DELETE, urlPath: $"suppression/blocks/{email}");
                var text = await response.Body.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return true;
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    Telemetry.TrackTrace($"DELETE delete suppression/blocks failed with statuscode '{response.StatusCode}'. Reason: {text}", SeverityLevel.Error, property1: ("email", email));
                }
                else if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    if (CrmConfig.IsUnittest)
                    {
                        throw new Exception($"DELETE delete suppression/bounces failed with statuscode '{response.StatusCode}'");
                    }
                    Telemetry.TrackTrace($"DELETE delete suppression/blocks failed with statuscode '{response.StatusCode}'. Reason: {text}", SeverityLevel.Error, property1: ("email", email));
                }
            }
            catch (Exception ignored)
            {
                if (CrmConfig.IsUnittest)
                {
                    throw;
                }
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return false;
        }

        public async Task<IEnumerable<EmailBounce>> GetBounces(string email)
        {
            var client = new SendGridClient(SendGridApiKey);
            try
            {
                var response = await client.RequestAsync(BaseClient.Method.GET, urlPath: $"suppression/bounces/{email}");
                var text = await response.Body.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<EmailBounce>>(text);
                }
                else
                {
                    Telemetry.TrackTrace($"GET suppression/bounces failed with statuscode '{response.StatusCode}'. Reason: {text}", SeverityLevel.Error, property1: ("email", email));
                }
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return null;
        }

        public async Task<IEnumerable<EmailBounce>> GetBlocks(string email)
        {
            var client = new SendGridClient(SendGridApiKey);
            try
            {
                var response = await client.RequestAsync(BaseClient.Method.GET, urlPath: $"suppression/blocks/{email}");
                var text = await response.Body.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<EmailBounce>>(text);
                }
                else
                {
                    Telemetry.TrackTrace($"GET suppression/blocks failed with statuscode '{response.StatusCode}'. Reason: {text}", SeverityLevel.Error, property1: ("email", email));
                }
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return null;
        }

        [ExcludeFromCodeCoverage]
        public async Task InitalizeAsync()
        {
            SendGridApiKey = await AzureHelper.KeyVaultClient.GetSecretAsync(KeyVaultSecrets.ApiKeys.SendGrid);
        }

        public async Task<IEnumerable<EmailBounce>> ListBounces(DateTimeOffset start, DateTimeOffset end)
        {
            var client = new SendGridClient(SendGridApiKey);

            var json = new JObject();
            json["start_time"] = start.ToUnixTimeSeconds();
            json["end_time"] = end.ToUnixTimeSeconds();

            var response = await client.RequestAsync(BaseClient.Method.GET, urlPath: "suppression/bounces", queryParams: json.ToString());
            var text = await response.Body.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<IEnumerable<EmailBounce>>(text);
            }
            else
            {
                Telemetry.TrackTrace($"GET suppression/bounces failed with statuscode '{response.StatusCode}'. Reason: {text}", SeverityLevel.Error);
            }
            return null;
        }

        public async Task<IEnumerable<EmailBounce>> ListBlocks(DateTimeOffset start, DateTimeOffset end)
        {
            var client = new SendGridClient(SendGridApiKey);

            var json = new JObject();
            json["start_time"] = start.ToUnixTimeSeconds();
            json["end_time"] = end.ToUnixTimeSeconds();

            var response = await client.RequestAsync(BaseClient.Method.GET, urlPath: "suppression/blocks", queryParams: json.ToString());
            var text = await response.Body.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<IEnumerable<EmailBounce>>(text);
            }
            else
            {
                Telemetry.TrackTrace($"GET suppression/blocks failed with statuscode '{response.StatusCode}'. Reason: {text}", SeverityLevel.Error);
            }
            return null;
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

                var client = new SendGridClient(SendGridApiKey);

                //Emails von Account haben immer Account als Absender. Replay ist dann der Contact
                var from = new EmailAddress(EmailClient.FromAddress, EmailClient.FromName);

                var to = new EmailAddress(mailAddress);
                var subject = email.Subject;
                var htmlContent = email.Description;
                var plainTextContent = "";
                if (!string.IsNullOrEmpty(email.PlainText))
                {
                    plainTextContent = email.PlainText;
                }
                var mail = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                foreach (var receiver in moreReceivers)
                {
                    mail.AddTo(new EmailAddress(receiver));
                }

                if (!string.IsNullOrEmpty(fromAddress))
                {
                    mail.ReplyTo = new EmailAddress(fromAddress, fromName);
                }
                
                //mail.SetBypassListManagement(true);

                mail.TrackingSettings = new TrackingSettings
                {
                    ClickTracking = new ClickTracking
                    {
                        Enable = false
                    },
                };

                if (!string.IsNullOrEmpty(tag))
                {
                    mail.AddCategory(tag);
                }

                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        var sendGridAttachment = new SendGrid.Helpers.Mail.Attachment();
                        sendGridAttachment.Content = attachment.BodyBase64;
                        sendGridAttachment.Filename = attachment.FileName;
                        sendGridAttachment.Type = attachment.MimeType;
                        mail.AddAttachment(sendGridAttachment);
                    }
                }

                var response = await client.SendEmailAsync(mail);
                var ok = response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK;
                if (!ok)
                {
                    Telemetry.TrackTrace($"SendGrid: Mail to {mailAddress} sent with StatusCode: {response.StatusCode}. Subject: {subject}", SeverityLevel.Error);
                }
                return ok;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat, property1: (nameof(TelemetryProperty.Description), mailAddress));
            }
            return false;
        }

        [ExcludeFromCodeCoverage]
        public async Task<bool> SendAsync(string body, string toAddress, string fromAddress, string fromName, string subject, bool isHtml, EmailAttachment emailAttachment, string tag)
        {
            try
            {
                if (!EmailClient.IsValid(toAddress))
                {
                    return true;
                }

                var client = new SendGridClient(SendGridApiKey);
                var from = new EmailAddress(fromAddress, fromName);
                var to = new EmailAddress(toAddress);
                var htmlContent = isHtml ? body : string.Empty;
                var plainTextContent = !isHtml ? body : string.Empty;

                var mail = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                mail.TrackingSettings = new TrackingSettings
                {
                    ClickTracking = new ClickTracking
                    {
                        Enable = false
                    },
                };
                if (!string.IsNullOrEmpty(tag))
                {
                    mail.AddCategory(tag);
                }
                if (emailAttachment != null)
                {
                    var attachment = new Attachment();
                    attachment.Content = emailAttachment.BodyBase64;
                    attachment.Filename = emailAttachment.FileName;
                    attachment.Type = emailAttachment.MimeType;
                    mail.AddAttachment(attachment);
                }
                var response = await client.SendEmailAsync(mail);
                var ok = response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK;
                if (!ok)
                {
                    Telemetry.TrackTrace($"SendGrid: Mail to {to} sent with StatusCode: {response.StatusCode}. Subject: {subject}", SeverityLevel.Error);
                }
                return ok;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat, property1: (nameof(TelemetryProperty.Description), toAddress));
            }
            return false;
        }

        public async Task<bool> SendBulkAsync(CrmEmail email, IEnumerable<Contact> contacts, string fromAddress, string fromName, string tag)
        {
            try
            {
                var tos = new List<EmailAddress>();
                var toAddresses = contacts.Select(c => c.EMailAddress1);
                foreach (var toAddress in toAddresses)
                {
                    if (!EmailClient.IsValid(toAddress))
                    {
                        return true;
                    }
                    tos.Add(new EmailAddress(toAddress));
                }
                if (!tos.Any())
                {
                    return true;
                }

                var client = new SendGridClient(SendGridApiKey);
                var from = new EmailAddress(fromAddress, fromName);

                var subject = email.Subject;
                var htmlContent = email.Description;
                var plainTextContent = "";
                if (!string.IsNullOrEmpty(email.PlainText))
                {
                    plainTextContent = email.PlainText;
                }

                var mail = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, plainTextContent, htmlContent);

                if (!string.IsNullOrEmpty(tag))
                {
                    mail.AddCategory(tag);
                }

                foreach (var personalizations in mail.Personalizations)
                {
                    var contactEmail = personalizations.Tos.First().Email;
                    var contact = contacts.First(c => string.Equals(c.EMailAddress1, contactEmail, StringComparison.InvariantCultureIgnoreCase));
                    personalizations.CustomArgs = new Dictionary<string, string>();
                    personalizations.CustomArgs[MessageKey.ContactId] = contact.Key;
                }

                mail.TrackingSettings = new TrackingSettings
                {
                    ClickTracking = new ClickTracking
                    {
                        Enable = false
                    },
                };
                var response = await client.SendEmailAsync(mail);
                var ok = response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK;
                if (!ok)
                {
                    Telemetry.TrackTrace($"SendGrid: Mail sent with StatusCode: {response.StatusCode}. Subject: {subject}", SeverityLevel.Error);
                }
                return ok;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
            }
            return false;
        }
        
        #endregion
    }

}
