using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SwissAcademic.Crm.Web
{
    public static class EmailFormatter
    {
        #region Felder

        static Dictionary<string, List<(LanguageType Language, string Text)>> _customMailTexts = new Dictionary<string, List<(LanguageType, string)>>();
        static List<EmailTemplate> _templates = new List<EmailTemplate>();

        #endregion

        #region Methoden

        #region Format

        public static (CrmEmail email, Annotation annotation) Format(CrmDbContext context, EmailTemplateType templateType, Contact contact, string emailAddress, Dictionary<string, string> replacements = null)
        {
            return Format(context, templateType, contact, emailAddress, replacements == null ?
                null :
                replacements.ToDictionary(item => item.Key,
                    item => (Text: item.Value,
                    Html: string.IsNullOrEmpty(item.Value) ? null : System.Net.WebUtility.HtmlEncode(item.Value))));
        }

        public static (CrmEmail email, Annotation annotation) Format(CrmDbContext context, EmailTemplateType templateType, Contact contact, string emailAddress, Dictionary<string, (string Text, string Html)> replacements = null)
        {
            var template = _templates.First(i => i.TemplateType == templateType);
            var language = contact.Language ?? LanguageType.English;

            //Nicht via Context erstellen - wir wollen die Email nicht mehr speichern
            var email = new CrmEmail();
            var id = Guid.NewGuid();
            email.ActivityId = id;
            email.Id = id;

            var descHtml = template.HtmlTemplate;
            var descPlain = template.PlainTextTemplate;
            
            var subject = template.Subject[language];

            foreach (var r in template.Description[language])
            {
                switch (r.Value)
                {
                    case null:
                    case "":
                        {
                            descHtml = descHtml.Replace(r.Key, r.Value);
                            descPlain = descPlain.Replace("\r\n" + r.Key + "\r\n", r.Value);
                        }
                        break;

                    case "##dynamicHtml":
                    case "##dynamicText":
                        break;

                    default:
                        {
                            if((r.Key == CrmEmailTemplatePlaceholderConstants.RenewalEmail &&
                               !replacements.ContainsKey(CrmEmailTemplatePlaceholderConstants.RenewalEmail)) ||

                               (r.Key == CrmEmailTemplatePlaceholderConstants.RenewalIP &&
                               !replacements.ContainsKey(CrmEmailTemplatePlaceholderConstants.RenewalIP)) ||

                               (r.Key == CrmEmailTemplatePlaceholderConstants.RenewalVoucher &&
                               !replacements.ContainsKey(CrmEmailTemplatePlaceholderConstants.RenewalVoucher)))
                            {
                                descHtml = descHtml.Replace(r.Key, string.Empty);
                                descPlain = descPlain.Replace(r.Key, string.Empty);
                            }

                            if (r.Key == "##disclaimer" &&
                                template.NoDesclaimer)
                            {
                                descHtml = descHtml.Replace(r.Key, string.Empty);
                                descPlain = descPlain.Replace(r.Key, string.Empty);
                            }
                            else
                            {
                                var val = r.Value;
                                val = Regex.Replace(val, "<(\\w*)>", "REP_$1_1");
                                val = Regex.Replace(val, "</(\\w*)>", "REP_$1_2");
                                var html = System.Net.WebUtility.HtmlEncode(val);
                                if (html.Contains("\r\n"))
                                {
                                    html = html.Replace("\r\n", "<br>");
                                }
                                html = Regex.Replace(html, "REP_(\\w*)_1", "<$1>");
                                html = Regex.Replace(html, "REP_(\\w*)_2", "</$1>");
                                descHtml = descHtml.Replace(r.Key, html);

                                var plain = r.Value;
                                plain = Regex.Replace(plain, "<.+?>", "");
                                descPlain = descPlain.Replace(r.Key, plain);
                            }
                        }
                        break;
                }
            }

            if (replacements != null)
            {
                foreach (var r in replacements)
                {
                    if (r.Key == CrmEmailTemplatePlaceholderConstants.AddOn || r.Key == CrmEmailTemplatePlaceholderConstants.News)
                    {
                        //Ist HTML beim "abfüllen" - muss wieder dekodiert werden
                        descHtml = descHtml.Replace(r.Key, System.Net.WebUtility.HtmlDecode(r.Value.Html ?? string.Empty));
                        descPlain = descPlain.Replace(r.Key, Regex.Replace(r.Value.Text ?? string.Empty, "<.+?>", ""));
                    }
                    else
                    {
                        descHtml = descHtml.Replace(r.Key, r.Value.Html ?? string.Empty);
                        descPlain = descPlain.Replace(r.Key, r.Value.Text ?? string.Empty);
                    }
                    if (!string.IsNullOrEmpty(subject))
                    {
                        subject = subject.Replace(r.Key, r.Value.Text);
                    }
                }
            }

            var saluation = template.Saluation[language];
            if (string.IsNullOrEmpty(contact.FirstName))
            {
                saluation = string.Empty;
            }
            else
            {
                saluation = string.Format(saluation, contact.FirstName);
            }

            descHtml = descHtml.Replace(CrmEmailTemplatePlaceholderConstants.Email, System.Net.WebUtility.HtmlEncode(emailAddress));
            descHtml = descHtml.Replace(CrmEmailTemplatePlaceholderConstants.Salutation, System.Net.WebUtility.HtmlEncode(saluation));
            descHtml = descHtml.Replace(CrmEmailTemplatePlaceholderConstants.NewLine, "<br>");

            descPlain = descPlain.Replace(CrmEmailTemplatePlaceholderConstants.Email, System.Net.WebUtility.HtmlEncode(emailAddress));
            descPlain = descPlain.Replace(CrmEmailTemplatePlaceholderConstants.Salutation, System.Net.WebUtility.HtmlEncode(saluation));
            descPlain = descPlain.Replace(CrmEmailTemplatePlaceholderConstants.NewLine, "\r\n");

            if (template.IsNoButtonTemplate)
            {
                descPlain = descPlain.Replace(CrmEmailTemplatePlaceholderConstants.Url + "\r\n", string.Empty);
                descPlain = descPlain.Replace($"\r\n##{EMailPlaceholderType.Button.ToString().ToLowerInvariant()}:\r\n", string.Empty);
            }

            descHtml = Regex.Replace(descHtml, "(<br>){2,}", "<br><br>");
            descPlain = Regex.Replace(descPlain, "(\r\n){2,}", "\r\n\r\n");

            email.Description = descHtml;
            email.Subject = subject;
            email.PlainText = descPlain;

            if (context != null)
            {
                var annotation = context.Create<Annotation>();
                annotation.ContactId = contact.Id.ToString();
                annotation.Subject = subject;
                annotation.NoteText = $"Sent on: {DateTime.UtcNow.ToString("r")}";
                return (email, annotation);
            }

            return (email, null);
        }

        #endregion

        #region Initialize

        public static void Initialize()
        {
            foreach (var e in Enum.GetValues(typeof(EmailTemplateType)).Cast<EmailTemplateType>())
            {
                _templates.Add(new EmailTemplate(e));
            }

            var rm = Resources.Strings.ResourceManager;
            _customMailTexts[nameof(Resources.Strings.CRM_Email_ConfirmProjectInvitationDescriptionPrefix)] = new List<(LanguageType, string)>();
            foreach (int culture in Enum.GetValues(typeof(LanguageType)))
            {
                var cultureInfo = CultureInfo.GetCultureInfo(culture);
                var res = rm.GetString(nameof(Resources.Strings.CRM_Email_ConfirmProjectInvitationDescriptionPrefix), cultureInfo);
                _customMailTexts[nameof(Resources.Strings.CRM_Email_ConfirmProjectInvitationDescriptionPrefix)].Add(((LanguageType)culture, res));
            }
        }

        #endregion

        #region GetLanguageSpecificText

        public static string GetLanguageSpecificText(string resxname, LanguageType? language)
        {
            try
            {
                if (language == null)
                {
                    language = LanguageType.English;
                }

                var res = _customMailTexts[resxname].FirstOrDefault(item => item.Language == language).Text;
                return res ?? string.Empty;
            }
            catch
            {
                return "";
            }
        }

        #endregion

        #endregion
    }
}
