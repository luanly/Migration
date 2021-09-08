using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class BulkEmailService
    {
        #region Konstanten

        public const string KeyMissingExceptionMessage = "BulkMailQuery must contains new_key";
        public const string LanguageMissingExceptionMessage = "BulkMailQuery must contains new_language";
        public const string EmailMissingExceptionMessage = "BulkMailQuery must contains emailaddress1";
        public const string EmailMissingExceptionMessage2 = "BulkMailQuery must contains select attribute emailaddress1";

        #endregion

        #region Konstruktor

        public BulkEmailService(CrmDbContext dbContext)
        {
            DbContext = dbContext;
        }

        #endregion

        #region Eigenschaften

        CrmDbContext DbContext { get; }

        #endregion

        #region Methoden

        #region ExecuteQuery

        async Task<IEnumerable<Contact>> ExecuteQuery(BulkMailQuery bulkMailQuery)
        {
            ExpressionBase expression;
            var result = new List<Contact>();

            if (bulkMailQuery.FetchXml.StartsWith(BulkMailQuery.ODataQueryPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                expression = new ODataExpression(bulkMailQuery.FetchXml.Substring(BulkMailQuery.ODataQueryPrefix.Length));
            }
            else
            {
                expression = new FetchXmlExpression(CrmEntityNames.Contact, bulkMailQuery.FetchXml);
            }

            do
            {
                result.AddRange(await DbContext.Fetch<Contact>(expression, observe: false));
            }
            while (expression.HasMoreResults);

            return result;
        }

        #endregion

        #region Preview

        public async Task<BulkMailQueryPreview> Preview(BulkMailQuery bulkMailQuery, int maxContactPreview = 5)
        {
            var preview = new BulkMailQueryPreview();
            try
            {
                Validate(bulkMailQuery);
                var contacts = await ExecuteQuery(bulkMailQuery);
                preview.Contacts = contacts.Take(maxContactPreview).Select(c => (BulkMailQueryPreviewContact)c);
                preview.ReceiverCount += contacts.Count();
                preview.Success = true;
            }
            catch(Exception ex)
            {
                preview.Success = false;
                preview.ExceptionMessage = ex.Message;
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return preview;
        }

        #endregion

        #region Send

        public async Task<BulkEmailResult> Send(Delivery delivery, string tag = null)
        {
            if (delivery == null)
            {
                throw new NotSupportedException("Delivery must not be null.");
            }

            var bulkMailQuery = await delivery.Query.Get(EntityPropertySets.BulkMailQuery);
            
            Validate(bulkMailQuery);

            var template = await delivery.Template.Get(EntityPropertySets.BulkMailTemplate);
            if (template == null)
            {
                throw new NotSupportedException("BulkMailTemplate must not be null.");
            }

            var result = new BulkEmailResult();
            try
            {
                var contacts = await ExecuteQuery(bulkMailQuery);
                result.Recievers = contacts;

                await Send(template, contacts.Where(c => c.Language == LanguageType.German), LanguageType.German, tag);
                await Send(template, contacts.Where(c => c.Language != LanguageType.German), LanguageType.English, tag);

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ExceptionMessage = ex.Message;
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return result;
        }

        public async Task Send(BulkMailTemplate mailTemplate, IEnumerable<Contact> contacts, LanguageType languageType, string tag = null)
        {
            if(mailTemplate == null)
            {
                throw new NotSupportedException("BulkMailTemplate must not be null");
            }
            if(!contacts.Any())
            {
                return;
            }
            const int MaxRecievers = 1000;

            var replacement = new Dictionary<string, string>
            {
                [CrmEmailTemplatePlaceholderConstants.AddOn] = languageType == LanguageType.German ? mailTemplate.MailTextDE : mailTemplate.MailTextEN
            };
            var (email, _) = EmailFormatter.Format(null, EmailTemplateType.BulkEmail, contacts.First(), string.Empty, replacement);
            email.Subject = languageType == LanguageType.German ? mailTemplate.SubjectDE : mailTemplate.SubjectEN;
            
            var index = 0;
            IEnumerable<Contact> list;
            do
            {
                list = contacts.Skip(index).Take(MaxRecievers);
                await EmailClient.SendBulkAsync(email, list, tag);
                index += MaxRecievers;
            }
            while (index < contacts.Count());
        }

        #endregion

        #region Validate

        void Validate(BulkMailQuery bulkMailQuery)
        {
            if (bulkMailQuery == null)
            {
                throw new NotSupportedException("BulkMailQuery must not be null");
            }
            if (string.IsNullOrEmpty(bulkMailQuery.FetchXml))
            {
                throw new NotSupportedException("BulkMailQuery.FetchXml must not be null.");
            }
            if (!bulkMailQuery.FetchXml.Contains("new_key"))
            {
                throw new NotSupportedException(KeyMissingExceptionMessage);
            }
            if (!bulkMailQuery.FetchXml.Contains("new_language"))
            {
                throw new NotSupportedException(LanguageMissingExceptionMessage);
            }
            if (!bulkMailQuery.FetchXml.Contains("emailaddress1"))
            {
                throw new NotSupportedException(EmailMissingExceptionMessage);
            }
            if (!bulkMailQuery.FetchXml.StartsWith(BulkMailQuery.ODataQueryPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!bulkMailQuery.FetchXml.Contains("<attribute name=\"emailaddress1\""))
                {
                    throw new NotSupportedException(EmailMissingExceptionMessage2);
                }
            }
        }

        #endregion

        #endregion
    }

    public class BulkEmailResult
    {
        public string ExceptionMessage { get; set; }
        public IEnumerable<Contact> Recievers { get; set; }

        public bool Success { get; set; }
    }
}
