using Aspose.Words;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class Email
    {
        #region Felder

        CrmEmail _email;

        #endregion

        #region Konstruktor

        public Email(EmailTemplateType template)
        {
            TemplateType = template;
        }

        #endregion

        #region Eigenschaften

        #region Annotation

        public Annotation Annotation { get; private set; }

        #endregion

        #region TemplateType

        internal EmailTemplateType TemplateType { get; set; }

        #endregion

        #endregion

        #region Methoden

        #region SendAsync

        public Task<(CrmEmail email, Annotation annotation)> SendAsync(CrmUser receiver, string emailTo, CrmDbContext context, Dictionary<string, string> replacements, IEnumerable<EmailAttachment> attachments = null)
        {
            return SendAsync(receiver, emailTo, null, null, null, context, attachments, replacements);
        }
        public Task<(CrmEmail email, Annotation annotation)> SendAsync(CrmUser receiver, string emailTo, string tag, CrmDbContext context, Dictionary<string, string> replacements, IEnumerable<EmailAttachment> attachments = null)
        {
            return SendAsync(receiver, emailTo, null, null, tag, context, attachments, replacements);
        }
        public Task<(CrmEmail email, Annotation annotation)> SendAsync(CrmUser receiver, string emailTo, string emailFrom, string emailFromName, string tag, CrmDbContext context, Dictionary<string, string> replacements)
        {
            return SendAsync(receiver, emailTo, emailFrom, emailFromName, tag, context, null, replacements);
        }

        Task<(CrmEmail email, Annotation annotation)> SendAsync(CrmUser receiver, string emailTo, string emailFrom, string emailFromName, string tag, CrmDbContext context, IEnumerable<EmailAttachment> attachments, Dictionary<string, string> replacements)
        {
            return SendAsync(receiver, emailTo, emailFrom, emailFromName, tag, context, attachments, replacements == null ?
                null :
                replacements.ToDictionary(
                    item => item.Key,
                    item => (item.Value, string.IsNullOrEmpty(item.Value) ? null : System.Net.WebUtility.HtmlEncode(item.Value))));
        }

        async Task<(CrmEmail email, Annotation annotation)> SendAsync(CrmUser receiver, string emailTo, string emailFrom, string emailFromName, string tag, CrmDbContext context, IEnumerable<EmailAttachment> attachments, Dictionary<string, (string Text, string Html)> replacements)
        {
            try
            {
                (_email, Annotation) = EmailFormatter.Format(context, TemplateType, receiver.Contact, emailTo, replacements);

                if (CrmConfig.IsUnittest ||
                    emailTo.EndsWith(".noemail@citavi.com") ||
                    emailTo.EndsWith(".noemail@citavi2.com"))
                {
                    return (_email, Annotation);
                }

                await SendAsync(_email, emailTo, attachments, fromAddress: emailFrom, fromName: emailFromName, tag: tag);

                await context.SaveAsync();

                return (_email, Annotation);
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, property1: (nameof(TelemetryProperty.Description), $"SendCrmEmail \"{TemplateType}\": {emailTo}\r\nUser:{receiver.Key}"), severityLevel: SeverityLevel.Error, flow: ExceptionFlow.Eat);
            }

            return (null, null);
        }

        public Task SendAsync(string mailAddress, Dictionary<string, (string Text, string Html)> replacements, IEnumerable<EmailAttachment> attachments = null, string fromAddress = null, string fromName = null, string tag = null, params string[] moreReceivers)
        {
            using (var context = new CrmDbContext())
            {
                (var email, var annotation) = EmailFormatter.Format(context, TemplateType, new Contact(), mailAddress, replacements);
                return SendAsync(email, mailAddress, attachments, fromAddress, fromName, tag, moreReceivers);
            }
        }

        [ExcludeFromCodeCoverage]
        async Task<bool> SendAsync(CrmEmail email, string mailAddress, IEnumerable<EmailAttachment> attachments, string fromAddress = null, string fromName = null, string tag = null, params string[] moreReceivers)
                    => await EmailClient.SendAsync(email, mailAddress, attachments, fromAddress, fromName, tag, moreReceivers);

        #endregion

        #region SendVoucherGiftMailAsync

        public async Task SendVoucherGiftMailAsync(CrmUser receiver, string voucherCode)
        {
            if (CrmConfig.IsUnittest)
            {
                return;
            }

            var contact = receiver.Contact;
            var lng = contact.Language == LanguageType.German ? "de" : "en";
            using (var stream = new MemoryStream(Properties.Resources.ResourceManager.GetObject($"vouchers_gift_{lng}", CultureInfo.InvariantCulture) as byte[]))
            {
                var doc = new Document(stream);
                doc.MailMerge.Execute(new string[] { "Gutschein_Code" }, new object[] { voucherCode });

                using (var attachmentStream = new MemoryStream())
                {
                    doc.Save(attachmentStream, SaveFormat.Pdf);
                    attachmentStream.Seek(0, SeekOrigin.Begin);

                    var attachment = new EmailAttachment(attachmentStream, "Voucher.pdf", "application/pdf");

                    var url = UrlConstants.CitaviManual;
                    var prop = new Dictionary<string, string>();
                    prop.Add(CrmEmailTemplatePlaceholderConstants.Url, url);
                    using (var context = new CrmDbContext())
                    {
                        await SendAsync(receiver, contact.EMailAddress1, null, null, null, context, new List<EmailAttachment> { attachment }, prop);
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
