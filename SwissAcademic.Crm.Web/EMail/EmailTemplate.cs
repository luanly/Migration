using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SwissAcademic.Crm.Web
{
    public class EmailTemplate
    {
        #region EmailTemplate

        public EmailTemplate(EmailTemplateType templateType)
        {
            TemplateType = templateType;

            switch (TemplateType)
            {
                case EmailTemplateType.BulkEmail:
                    HtmlTemplate = Properties.Resources.BulkEmailTemplate;
                    IsNoButtonTemplate = true;
                    break;
                
                case EmailTemplateType.AccountMerging:
                case EmailTemplateType.ConfirmEmailAddressMailWithSignature:
                case EmailTemplateType.CampusContractExtensionInfoMail:
                case EmailTemplateType.CampusNewsletterStatisticMail:
                case EmailTemplateType.CitaviGiftCard:
                case EmailTemplateType.LicenseWithdrawal:
                case EmailTemplateType.UserRemovedFromProject:
                case EmailTemplateType.ProjectRoleChanged:
                case EmailTemplateType.ProjectDeleted:
                case EmailTemplateType.OrderProcessBilling_Home:
                case EmailTemplateType.OrderProcessBilling_Business:
                case EmailTemplateType.OrderProcessBilling_DbServer:
                case EmailTemplateType.OrderProcessBilling_Billomat_Home:
                case EmailTemplateType.OrderProcessBilling_Billomat_Business:
                case EmailTemplateType.OrderProcessBilling_Billomat_DbServer:
                    {
                        HtmlTemplate = Properties.Resources.EmailWithoutButtonTemplate;
                        IsNoButtonTemplate = true;
                    }
                    break;

                case EmailTemplateType.SendFormattedReferences:
                    {
                        HtmlTemplate = Properties.Resources.EmailBibliographyTemplate;
                        IsNoButtonTemplate = true;
                    }
                    break;


                default:
                    HtmlTemplate = Properties.Resources.EmailButtonTemplate;
                    break;
            }

            if (templateType != EmailTemplateType.BulkEmail)
            {
                PlainTextTemplate = Properties.Resources.EmailPlainTextTemplate;
            }
            else
            {
                PlainTextTemplate = Properties.Resources.BulkEmailPlainTextTemplate;
            }


            NoDesclaimer = TemplateType == EmailTemplateType.ConfirmProjectInvitation ||
                           TemplateType == EmailTemplateType.ConfirmProjectInvitationNewUser ||
                           TemplateType == EmailTemplateType.ProjectRoleChanged ||
                           TemplateType == EmailTemplateType.ProjectDeleted;

            var isCampusContractExtension = TemplateType.ToString().StartsWith(nameof(EmailTemplateType.CampusContractExtension));

            var rm = Resources.Strings.ResourceManager;
            var res = "";
            foreach (int culture in Enum.GetValues(typeof(LanguageType)))
            {
                var cultureInfo = CultureInfo.GetCultureInfo(culture);
                var dict = new Dictionary<string, string>();

                foreach (var placeholder in Enum.GetNames(typeof(EMailPlaceholderType)).ToList())
                {
                    res = rm.GetString($"CRM_Email_{templateType.ToString()}_{placeholder.ToLowerInvariant()}", cultureInfo);
                    if (placeholder == nameof(EMailPlaceholderType.Subject))
                    {
                        Subject.Add((LanguageType)culture, res);
                    }
                    dict.Add($"##{placeholder.ToLowerInvariant()}", res);
                }

                res = rm.GetString(nameof(Resources.Strings.CRM_Email_regards_firstline), cultureInfo);
                dict.Add("##regards_firstline", res);

                res = rm.GetString(nameof(Resources.Strings.CRM_Email_address), cultureInfo);
                dict.Add("##address", res);

                res = rm.GetString(nameof(Resources.Strings.CRM_Email_disclaimer), cultureInfo);
                dict.Add("##disclaimer", res);

                res = rm.GetString(nameof(Resources.Strings.CRM_Email_regards_secondline), cultureInfo);
                dict.Add("##regards_secondline", res);

                res = rm.GetString(nameof(Resources.Strings.CRM_Email_info_plaintext_link), cultureInfo);
                dict.Add("##info_plaintext_link", res);

                res = rm.GetString(nameof(Resources.Strings.CRM_Email_CampusContractExtensionRenewalEmail), cultureInfo);
                dict.Add(CrmEmailTemplatePlaceholderConstants.RenewalEmail, res);

                res = rm.GetString(nameof(Resources.Strings.CRM_Email_CampusContractExtensionRenewalIp), cultureInfo);
                dict.Add(CrmEmailTemplatePlaceholderConstants.RenewalIP, res);

                res = rm.GetString(nameof(Resources.Strings.CRM_Email_CampusContractExtensionRenewalVoucher), cultureInfo);
                dict.Add(CrmEmailTemplatePlaceholderConstants.RenewalVoucher, res);

                dict.Add(CrmEmailTemplatePlaceholderConstants.HelpGetOldVersion, CrmEmailHelpManualUrlsEx.CampusLicenseHelpGetOldKey);

                res = rm.GetString(nameof(Resources.Strings.CRM_Email_Salutation), cultureInfo);
                Saluation.Add((LanguageType)culture, res);

                Description.Add((LanguageType)culture, dict);

                if (isCampusContractExtension)
                {
                    res = rm.GetString(nameof(Resources.Strings.CRM_Email_Delete_Account), cultureInfo);
                    dict["##last_paragraph"] = res;
                }
            }
        }

        #endregion

        #region Eigenschaften

        #region Description

        public Dictionary<LanguageType, Dictionary<string, string>> Description { get; } = new Dictionary<LanguageType, Dictionary<string, string>>();

        #endregion

        #region IsNoButtonTemplate

        public bool IsNoButtonTemplate { get; private set; }

        #endregion

        #region EmailTemplateType

        public EmailTemplateType TemplateType { get; private set; }

        #endregion

        #region NoDesclaimer

        public bool NoDesclaimer { get; private set; }

        #endregion

        #region HtmlTemplate

        public string HtmlTemplate { get; private set; }

        #endregion

        #region Saluation

        public Dictionary<LanguageType, string> Saluation { get; } = new Dictionary<LanguageType, string>();

        #endregion

        #region Subject

        public Dictionary<LanguageType, string> Subject { get; } = new Dictionary<LanguageType, string>();

        #endregion

        #region PlainTextTemplate

        public string PlainTextTemplate { get; private set; }

        #endregion

        #endregion
    }

    public enum EMailPlaceholderType
    {
        Button,
        Firstline,
        First_Paragraph,
        Last_Paragraph,
        Second_Paragraph,
        Subject
    }
}
