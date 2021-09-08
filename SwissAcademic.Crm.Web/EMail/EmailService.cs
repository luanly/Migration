using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class EmailService
    {
        #region Events

        public static event Action<EmailInfo> EmailSended;
        static void OnEmailSend(EmailTemplateType? e, string email, string verificationkey = null, string emailFrom = null, string plainText = null)
        {
            if (EmailSended == null)
            {
                return;
            }

            EmailSended(new EmailInfo
            {
                Type = e ?? EmailTemplateType.AccountCreatedVerifyEmail,
                EmailTo = email,
                EmailFrom = emailFrom,
                VerificationKey = verificationkey,
                PlainText = plainText
            });
        }

        #endregion

        #region Methoden


        #endregion

        #region Statische Methoden

        #region SendAssignLicenseMail

        public static async Task SendAssignLicenseMailAsync(CrmUser user, string email)
        {
            OnEmailSend(EmailTemplateType.OrderProcessLicensee, email);
            if (CrmConfig.IsUnittest)
            {
                return;
            }
            if (user == null)
            {
                throw new NotSupportedException("User must not be null");
            }
            var prop = new Dictionary<string, string>();
            prop.Add(MessageKey.ContactKey, user.Key);
            prop.Add(MessageKey.EmailTo, email);
            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.OrderProcessLicensee.ToString());
            await AzureHelper.AddQueueMessageAsync(user, prop, MessageKey.SendCrmEmail);
        }
        public static async Task SendAssignLicenseMail_CreatedAccount(CrmUser user, string email, string verificationKey)
        {
            await SendVerificationKeyMail(user, email, verificationKey, EmailTemplateType.AccountCreatedViaProcessOrderMailLicensee);
        }


        #endregion

        #region SendCampusContract Renewal Mails

        public static async Task<(CrmEmail email, Annotation annotation)> SendCampusContractExtensionVerifyLinkedEmailAccountMail(CrmUserManager userManager, CrmUser user, string email, CampusContract campusContract, EmailTemplateType templateType, Dictionary<string, string> prop)
        {
            var verificationKey = userManager.SetLinkedEmailVerificationKey(user, key: null, state: email);

            OnEmailSend(templateType, email, verificationKey);

            var para = "";
            if (!user.Contact.IsVerified.GetValueOrDefault(false))
            {
                para = "?registration=true";
            }
            var url = UrlBuilder.Combine(UrlConstants.Authority, UrlConstants.Web, UrlConstants.ConfirmEMailAddress, verificationKey, para);
            prop.Add(CrmEmailTemplatePlaceholderConstants.Url, url);
            prop.Add(CrmEmailTemplatePlaceholderConstants.AddOn, campusContract.SourceText);

            var mail = new Email(templateType);
            return await mail.SendAsync(user, user.Email, campusContract.Key, userManager.DbContext, prop);
        }

        public static async Task<(CrmEmail email, Annotation annotation)> SendCampusContractExtensionVerifyLicenseMail(CrmUserManager userManager, CrmUser user, CitaviLicense license, string emailAdress, CampusContract campusContract, EmailTemplateType templateType)
        {
            license.VerificationStorage = emailAdress;
            license.VerificationKeySent = DateTime.UtcNow;
            license.VerificationKey = Security.PasswordGenerator.WebKey.Generate();

            OnEmailSend(templateType, emailAdress, license.VerificationKey);

            var prop = new Dictionary<string, string>();

            var para = "";
            if (!user.Contact.IsVerified.GetValueOrDefault(false))
            {
                para = "?registration=true";
            }
            var url = UrlBuilder.Combine(UrlConstants.Authority, UrlConstants.Web, UrlConstants.ConfirmEMailAddressCampusLicense, license.VerificationKey, para);
            prop.Add(CrmEmailTemplatePlaceholderConstants.Url, url);
            prop.Add(CrmEmailTemplatePlaceholderConstants.AddOn, campusContract.SourceText);


            var mail = new Email(templateType);
            return await mail.SendAsync(user, emailAdress, campusContract.Key, userManager.DbContext, prop);
        }

        public static async Task<(CrmEmail email, Annotation annotation)> SendCampusContractExtensionVerifyShibbolethMail(CrmUserManager userManager, CrmUser user, string email, CampusContract campusContract, EmailTemplateType templateType)
        {
            OnEmailSend(templateType, email);

            var prop = new Dictionary<string, string>();
            var url = UrlBuilder.Combine(UrlConstants.Authority, UrlConstants.Web, UrlConstants.Campus, $"?accountKey={campusContract.AccountResolved.Key}");
            prop.Add(CrmEmailTemplatePlaceholderConstants.Url, url);
            prop.Add(CrmEmailTemplatePlaceholderConstants.AddOn, campusContract.SourceText);

            var mail = new Email(templateType);
            return await mail.SendAsync(user, user.Email, campusContract.Key, userManager.DbContext, prop);
        }

        public static async Task<(CrmEmail email, Annotation annotation)> SendCampusContractExtensionAccountLoginMail(CrmUserManager userManager, CrmUser user, string email, CampusContract campusContract, EmailTemplateType templateType, Dictionary<string, string> prop)
        {
            OnEmailSend(templateType, email);

            prop.Add(CrmEmailTemplatePlaceholderConstants.Url, UrlBuilder.Combine(UrlConstants.Authority, UrlConstants.Account));
            prop.Add(CrmEmailTemplatePlaceholderConstants.AddOn, campusContract.SourceText);

            var mail = new Email(templateType);

            return await mail.SendAsync(user, user.Email, campusContract.Key, userManager.DbContext, prop);
        }

        public static async Task<(CrmEmail email, Annotation annotation)> SendCampusContractExtensionInfoMail(CrmUserManager userManager, CrmUser user, string email, EmailTemplateType templateType, Dictionary<string, string> prop)
        {
            OnEmailSend(templateType, email);

            var mail = new Email(templateType);
            return await mail.SendAsync(user, user.Email, null, userManager.DbContext, prop);
        }

        public static async Task<(CrmEmail email, Annotation annotation)> SendBatchLicense(CrmDbContext context, CrmUser user, EmailTemplateType templateType, Dictionary<string, string> prop, IEnumerable<EmailAttachment> emailAttachments)
        {
            OnEmailSend(templateType, user.Email);

            var mail = new Email(templateType);
            return await mail.SendAsync(user, user.Email, null, context, prop, emailAttachments);
        }

        #endregion

        #region SendCampusNewsletter Mails

        public static async Task<(CrmEmail email, Annotation annotation)> SendCampusNewsletterMail(CrmUserManager userManager, CrmUser user, string email, CampusContract campusContract, EmailTemplateType templateType, Dictionary<string, string> prop)
        {
            OnEmailSend(templateType, email);
            var mail = new Email(templateType);

            return await mail.SendAsync(user, user.Email, campusContract.Key, userManager.DbContext, prop);
        }

        #endregion

        #region SendCloudSpaceMailAsync

        public static async Task SendCloudSpaceMailAsync(CrmUser user)
        {
            var emailTemplateType = user.Contact.CloudSpaceWarningSent == CloudSpaceWarningSentType.Exceeded ? EmailTemplateType.CloudSpaceExceeded : EmailTemplateType.CloudSpaceWarning;

            OnEmailSend(emailTemplateType, user.Email);

            if (CrmConfig.IsUnittest)
            {
                return;
            }

            if (CrmConfig.IsAlphaOrDev)
            {
                if (!user.Email.EndsWith("@citavi.com", StringComparison.InvariantCultureIgnoreCase))
                {
                    Telemetry.TrackDiagnostics($"Skip SendCloudSpaceMailAsync: {user.Email}");
                    return;
                }
            }

            var prop = new Dictionary<string, string>();

            prop.Add(MessageKey.ContactKey, user.Key);
            prop.Add(MessageKey.EmailTo, user.Email);
            prop.Add(MessageKey.EmailTemplateId, emailTemplateType.ToString());
            prop.Add(nameof(Contact.CloudSpaceWarningSent), user.Contact.CloudSpaceWarningSent.ToString());

            await AzureHelper.AddQueueMessageAsync(user, prop, MessageKey.SendCrmEmail);
        }

        #endregion

        #region SendSubscriptionLicenseReadOnlyAsync

        public static async Task SendSubscriptionLicenseReadOnlyAsync(CrmUser user)
        {
			if (!CrmConfig.IsShopWebAppSubscriptionAvailable)
			{
                return;
			}
            OnEmailSend(EmailTemplateType.SubscriptionLicenseReadOnly, user.Email);

            if (CrmConfig.IsUnittest)
            {
                return;
            }

            if (CrmConfig.IsAlphaOrDev)
            {
                if (!user.Email.EndsWith("@citavi.com", StringComparison.InvariantCultureIgnoreCase))
                {
                    Telemetry.TrackDiagnostics($"Skip SubscriptionLicenseReadOnly: {user.Email}");
                    return;
                }
            }

            var prop = new Dictionary<string, string>();

            prop.Add(MessageKey.ContactKey, user.Key);
            prop.Add(MessageKey.EmailTo, user.Email);
            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.SubscriptionLicenseReadOnly.ToString());

            await AzureHelper.AddQueueMessageAsync(user, prop, MessageKey.SendCrmEmail);
        }

        #endregion

        #region SendSubscriptionLicenseReadOnlyAsync

        public static async Task SendSubscriptionLicenseDeactivatedAsync(CrmUser user)
        {
            if (!CrmConfig.IsShopWebAppSubscriptionAvailable)
            {
                return;
            }
            OnEmailSend(EmailTemplateType.SubscriptionLicenseDeactivated, user.Email);

            if (CrmConfig.IsUnittest)
            {
                return;
            }

            if (CrmConfig.IsAlphaOrDev)
            {
                if (!user.Email.EndsWith("@citavi.com", StringComparison.InvariantCultureIgnoreCase))
                {
                    Telemetry.TrackDiagnostics($"Skip SubscriptionLicenseReadOnly: {user.Email}");
                    return;
                }
            }

            var prop = new Dictionary<string, string>();

            prop.Add(MessageKey.ContactKey, user.Key);
            prop.Add(MessageKey.EmailTo, user.Email);
            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.SubscriptionLicenseDeactivated.ToString());

            await AzureHelper.AddQueueMessageAsync(user, prop, MessageKey.SendCrmEmail);
        }

        #endregion

        #region SendConfirmProjectInviteMail

        public static async Task SendConfirmProjectInvitationMailAsync(CrmUser invitee, CrmUser inviter, ProjectRole projectRole, ProjectEntry projectEntry, string description = null)
        {
            OnEmailSend(EmailTemplateType.ConfirmProjectInvitation, invitee.Email, projectRole.ConfirmationKey);

            if (CrmConfig.IsUnittest)
            {
                return;
            }

            var prop = new Dictionary<string, string>();

            prop.Add(MessageKey.ContactKey, invitee.Key);

            prop.Add(MessageKey.EmailVerificationKey, projectRole.ConfirmationKey);
            prop.Add(MessageKey.EmailTo, invitee.Email);
            prop.Add(MessageKey.EmailFrom, inviter.Email);
            prop.Add(MessageKey.EmailProjectInvitationInviter, inviter.Contact.FullName);
            prop.Add(MessageKey.EmailProjectInvitationProjectName, projectEntry.Name);
            prop.Add(MessageKey.EmailProjectInvitationProjectRole, projectRole.ProjectRoleType.ToString());
            prop.Add(MessageKey.EmailProjectInvitationDescription, description ?? string.Empty);
            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.ConfirmProjectInvitation.ToString());

            await AzureHelper.AddQueueMessageAsync(invitee, prop, MessageKey.SendCrmEmail);
        }

        public static async Task SendConfirmProjectInvitationMailNewUser(CrmUser invitee, CrmUser inviter, ProjectRole projectRole, ProjectEntry projectEntry, string verificationKey, string description = null)
        {
            OnEmailSend(EmailTemplateType.ConfirmProjectInvitationNewUser, invitee.Email);

            if (CrmConfig.IsUnittest)
            {
                return;
            }

            var prop = new Dictionary<string, string>();

            prop.Add(MessageKey.ContactKey, invitee.Key);
            prop.Add(MessageKey.EmailTo, invitee.Email);
            prop.Add(MessageKey.EmailFrom, inviter.Email);
            prop.Add(MessageKey.EmailProjectInvitationInviter, inviter.Contact.FullName);
            prop.Add(MessageKey.EmailProjectInvitationProjectName, projectEntry.Name);
            prop.Add(MessageKey.EmailProjectInvitationProjectRole, projectRole.ProjectRoleType.ToString());
            prop.Add(MessageKey.EmailVerificationKey, verificationKey + "/" + projectEntry.Key);
            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.ConfirmProjectInvitationNewUser.ToString());
            prop.Add(MessageKey.EmailProjectInvitationDescription, description ?? string.Empty);

            await AzureHelper.AddQueueMessageAsync(invitee, prop, MessageKey.SendCrmEmail);
        }

        #endregion

        #region SendRemovedFromProjectEMailAsync

        public static async Task SendRemovedFromProjectEMailAsync(CrmUser removedUser, ProjectRole projectRole, CrmUser remover)
		{
            OnEmailSend(EmailTemplateType.UserRemovedFromProject, removedUser.Email);

            if (CrmConfig.IsUnittest)
            {
                return;
            }

            var prop = new Dictionary<string, string>();

            prop.Add(MessageKey.ContactKey, removedUser.Key);
            prop.Add(MessageKey.EmailTo, removedUser.Email);
            prop.Add(MessageKey.EmailFrom, remover.Email);
            prop.Add(MessageKey.EmailProjectInvitationInviter, remover.Contact.FullName);
            prop.Add(MessageKey.EmailProjectInvitationProjectName, projectRole.DataContractProjectName);
            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.UserRemovedFromProject.ToString());

            await AzureHelper.AddQueueMessageAsync(removedUser, prop, MessageKey.SendCrmEmail);
        }

        #endregion

        #region SendConfirmEmailAddressMail

        internal static async Task SendConfirmEmailAddressMail(CrmUser user, string email, string verificationKey)
        {
            await SendVerificationKeyMail(user, email, verificationKey, user.Contact.IsVerified.GetValueOrDefault() ? EmailTemplateType.ConfirmEmailAddressMail : EmailTemplateType.AccountCreatedVerifyEmail);
        }

        #endregion

        #region SendConfirmEmailAddressMailWithSignature

        internal static async Task SendConfirmEmailAddressMailWithSignature(CrmUser user, string email, string signature)
        {
            OnEmailSend(EmailTemplateType.ConfirmEmailAddressMailWithSignature, email, signature);

            if (CrmConfig.IsUnittest)
            {
                return;
            }

            var prop = new Dictionary<string, string>();
            prop.Add(MessageKey.ContactKey, user.Key);
            prop.Add(MessageKey.EmailVerificationKey, signature);
            prop.Add(MessageKey.EmailTo, email);
            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.ConfirmEmailAddressMailWithSignature.ToString());
            await AzureHelper.AddQueueMessageAsync(user, prop, MessageKey.SendCrmEmail);
        }

        #endregion

        #region SendConfirmEmailAddressCampusContractLicenseMail

        public static async Task SendConfirmEmailAddressCampusContractLicenseMail(CrmUser user, string email, string verificationKey)
        {
            await SendVerificationKeyMail(user, email, verificationKey, EmailTemplateType.ConfirmEmailAddressCampusContractLicenseMail);
        }

        #endregion

        #region SendLicenseWithdrawalMail

        public static Task SendLicenseWithdrawalMailAsync(CrmUser user, string email)
        {
            OnEmailSend(EmailTemplateType.LicenseWithdrawal, email);

            if (CrmConfig.IsUnittest)
            {
                return Task.CompletedTask;
            }

            var prop = new Dictionary<string, string>();
            prop.Add(MessageKey.ContactKey, user.Key);
            prop.Add(MessageKey.EmailTo, email);
            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.LicenseWithdrawal.ToString());
            return AzureHelper.AddQueueMessageAsync(user, prop, MessageKey.SendCrmEmail);
        }

        #endregion

        #region SendMergeAccountKeyMail

        public static Task SendMergeAccountKeyMailAsync(CrmUser loser, string loserEmailAddress, string verificationKey)
        {
            OnEmailSend(EmailTemplateType.AccountMerging, loserEmailAddress, verificationKey);

            if (CrmConfig.IsUnittest)
            {
                return Task.CompletedTask;
            }

            var prop = new Dictionary<string, string>();
            prop.Add(MessageKey.ContactKey, loser.Contact.Key);
            prop.Add(MessageKey.EmailVerificationKey, verificationKey);
            prop.Add(MessageKey.EmailTo, loserEmailAddress);
            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.AccountMerging.ToString());
            return AzureHelper.AddQueueMessageAsync(loser, prop, MessageKey.SendCrmEmail);
        }

        #endregion

        #region SendProcessOrderMail

        public static async Task SendProcessOrder_BillingMail_AccountCreated(CrmUser user, string email, string verificationKey, OrderProcessProductGroup productGroup)
        {
            switch (productGroup)
            {
                case OrderProcessProductGroup.Home:
                    {
                        await SendVerificationKeyMail(user, email, verificationKey, EmailTemplateType.AccountCreatedViaProcessOrderMailBilling_Home);
                    }
                    break;

                case OrderProcessProductGroup.Business:
                    {
                        await SendVerificationKeyMail(user, email, verificationKey, EmailTemplateType.AccountCreatedViaProcessOrderMailBilling_Business);
                    }
                    break;

                case OrderProcessProductGroup.DbServer:
                    {
                        await SendVerificationKeyMail(user, email, verificationKey, EmailTemplateType.AccountCreatedViaProcessOrderMailBilling_DbServer);
                    }
                    break;
            }
        }

        public static Task SendProcessOrder_BillingMail(CrmUser user, string email, OrderProcessProductGroup productGroup)
        {
            /**
                OrderProcessBilling_Home
                OrderProcessBilling_Business
                OrderProcessBilling_DbServer
            */

            var template = EmailTemplateType.OrderProcessBilling_Home;
            switch (productGroup)
            {
                case OrderProcessProductGroup.Home:
                    {
                        template = EmailTemplateType.OrderProcessBilling_Home;
                    }
                    break;

                case OrderProcessProductGroup.Business:
                    {
                        template = EmailTemplateType.OrderProcessBilling_Business;
                    }
                    break;

                case OrderProcessProductGroup.DbServer:
                    {
                        template = EmailTemplateType.OrderProcessBilling_DbServer;
                    }
                    break;
            }

            OnEmailSend(template, email);
            if (CrmConfig.IsUnittest)
            {
                return Task.CompletedTask;
            }
            var prop = new Dictionary<string, string>();
            prop.Add(MessageKey.ContactKey, user.Key);
            prop.Add(MessageKey.EmailTo, email);
            prop.Add(MessageKey.EmailTemplateId, template.ToString());
            return AzureHelper.AddQueueMessageAsync(user, prop, MessageKey.SendCrmEmail);
        }

        public static async Task SendProcessOrder_BillomatBillingMail(CrmUser user, string email, OrderProcess orderProcess, CrmDbContext context)
        {
            /**
                OrderProcessBilling_Home
                OrderProcessBilling_Business
                OrderProcessBilling_DbServer
            */
            var productGroup = await orderProcess.GetOrderProcessProductGroup();

            var template = EmailTemplateType.OrderProcessBilling_Home;
            switch (productGroup)
            {
                case OrderProcessProductGroup.Home:
                    {
                        template = EmailTemplateType.OrderProcessBilling_Billomat_Home;
                    }
                    break;

                case OrderProcessProductGroup.Business:
                    {
                        template = EmailTemplateType.OrderProcessBilling_Billomat_Business;
                    }
                    break;

                case OrderProcessProductGroup.DbServer:
                    {
                        template = EmailTemplateType.OrderProcessBilling_Billomat_DbServer;
                    }
                    break;
            }

            

            var prop = new Dictionary<string, string>();
            prop.Add(MessageKey.ContactKey, user.Key);
            prop.Add(CrmEmailTemplatePlaceholderConstants.InvoiceNumber, orderProcess.BillomatInvoiceNumber);
            prop.Add(CrmEmailTemplatePlaceholderConstants.CustomerOrderNumber, orderProcess.BillomatCustomerOrderNumber);

            var mail = new Email(template);
            prop.Add(CrmEmailTemplatePlaceholderConstants.Url, UrlConstants.Authority);

            var result = await mail.SendAsync(user, email, context, prop);

            OnEmailSend(template, email, plainText:result.email.PlainText);
        }

        #endregion

        #region SendPasswordResetMail

        public static async Task SendPasswordResetMail(CrmUser user, string email, string verificationkey)
        {
            OnEmailSend(EmailTemplateType.PasswordResetRequested, email, verificationkey);

            if (CrmConfig.IsUnittest)
            {
                return;
            }
            var prop = new Dictionary<string, string>();
            prop.Add(MessageKey.ContactKey, user.Key);
            prop.Add(MessageKey.EmailVerificationKey, verificationkey);
            prop.Add(MessageKey.EmailTo, email);
            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.PasswordResetRequested.ToString());
            await AzureHelper.AddQueueMessageAsync(user, prop, MessageKey.SendCrmEmail);
        }

        #endregion

        #region SendProjectRoleChangedMail

        public static Task SendProjectRoleChangedMailAsync(CrmUser projectMember, ProjectRole projectRole, CrmUser ownerOfManager)
        {
            OnEmailSend(EmailTemplateType.ProjectRoleChanged, projectMember.Email);

            if (CrmConfig.IsUnittest)
            {
                return Task.CompletedTask;
            }

            var prop = new Dictionary<string, string>();

            prop.Add(MessageKey.ContactKey, projectMember.Key);
            prop.Add(MessageKey.EmailTo, projectMember.Email);

            prop.Add(MessageKey.EmailProjectInvitationInviter, ownerOfManager.Contact.FullName);
            prop.Add(MessageKey.EmailProjectInvitationProjectName, projectRole.DataContractProjectName);
            prop.Add(MessageKey.EmailProjectInvitationProjectRole, projectRole.ProjectRoleType.ToString());

            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.ProjectRoleChanged.ToString());
            return AzureHelper.AddQueueMessageAsync(projectMember, prop, MessageKey.SendCrmEmail);
        }

        #endregion

        #region SendProjectDeletedMail

        public static Task SendProjectDeletedMailAsync(CrmUser owner, Contact member, string projectName)
        {
            OnEmailSend(EmailTemplateType.ProjectDeleted, projectName);

            if (CrmConfig.IsUnittest)
            {
                return Task.CompletedTask;
            }

            var prop = new Dictionary<string, string>();

            prop.Add(MessageKey.ContactKey, member.Key);
            prop.Add(MessageKey.EmailTo, member.EMailAddress1);

            prop.Add(MessageKey.EmailProjectInvitationInviter, owner.Contact.FullName);
            prop.Add(MessageKey.EmailProjectInvitationProjectName, projectName);

            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.ProjectDeleted.ToString());
            return AzureHelper.AddQueueMessageAsync(owner, prop, MessageKey.SendCrmEmail);
        }

        #endregion

        #region SendConfirmProjectInviteMail

        public static Task SendProjectRecoveryKeyAsync(CrmUser owner, ProjectEntry projectEntry)
        {
            OnEmailSend(EmailTemplateType.RecoverDeletedProject, owner.Email, projectEntry.RecoveryKey);

            if (CrmConfig.IsUnittest)
            {
                return Task.CompletedTask;
            }

            if (string.IsNullOrEmpty(owner.Email))
            {
                Telemetry.TrackTrace($"SendProjectRecoveryKeyAsync Email is null: {owner.Key}");
                return Task.CompletedTask;
            }
            var prop = new Dictionary<string, string>();

            prop.Add(MessageKey.ContactKey, owner.Key);

            prop.Add(MessageKey.EmailVerificationKey, projectEntry.RecoveryKey);
            prop.Add(MessageKey.EmailTo, owner.Email);
            prop.Add(MessageKey.ProjectName, projectEntry.Name);
            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.RecoverDeletedProject.ToString());

            return AzureHelper.AddQueueMessageAsync(owner, prop, MessageKey.SendCrmEmail);
        }

        #endregion

        #region SendVerificationKeyMail

        public static async Task SendVerificationKeyMail(CrmUser user, string email, string verificationKey, EmailTemplateType templateType)
        {
            OnEmailSend(templateType, email, verificationKey);

            if (CrmConfig.IsUnittest)
            {
                return;
            }

            var prop = new Dictionary<string, string>();
            prop.Add(MessageKey.ContactKey, user.Key);
            prop.Add(MessageKey.EmailVerificationKey, verificationKey);
            prop.Add(MessageKey.EmailTo, email);
            prop.Add(MessageKey.EmailTemplateId, templateType.ToString());
            await AzureHelper.AddQueueMessageAsync(user, prop, MessageKey.SendCrmEmail);
        }

        #endregion

        #region SendVoucherGiftMail

        public static Task SendVoucherGiftMailAsync(CrmUser licenseeUser, Voucher voucher) => SendVoucherGiftMailAsync(licenseeUser, voucher.VoucherCode);

        public static Task SendVoucherGiftMailAsync(CrmUser licenseeUser, string voucherCode)
        {
            OnEmailSend(EmailTemplateType.CitaviGiftCard, licenseeUser.Email);
            if (CrmConfig.IsUnittest)
            {
                return Task.CompletedTask;
            }

            var prop = new Dictionary<string, string>();
            prop.Add(MessageKey.ContactKey, licenseeUser.Contact.Key);
            prop.Add(MessageKey.EmailTo, licenseeUser.Email);
            prop.Add(MessageKey.EmailVerificationKey, voucherCode);
            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.CitaviGiftCard.ToString());
            return AzureHelper.AddQueueMessageAsync(licenseeUser, prop, MessageKey.SendCrmEmail);
        }


        #endregion

        #region SendConcurrentLicenseExceededMail

        public static Task SendConcurrentLicenseExceededMailAsync(IEnumerable<string> emails)
        {
            if (!emails.Any())
            {
                return Task.CompletedTask;
            }

            emails.ForEach(i => OnEmailSend(EmailTemplateType.ConcurrentLicenseExceeded, i));
            if (CrmConfig.IsUnittest)
            {
                return Task.CompletedTask;
            }

            var prop = new Dictionary<string, string>();

            prop.Add(MessageKey.EmailTo, emails.ElementAt(0));

            if (emails.Count() > 1)
            {
                List<string> emailCollection = new List<string>();
                for (int i = 1; i < emails.Count(); i++)
                {
                    emailCollection.Add(emails.ElementAt(i));
                }
                prop.Add(MessageKey.EmailToCollection, JsonConvert.SerializeObject(emailCollection));
            }
            prop.Add(MessageKey.EmailTemplateId, EmailTemplateType.ConcurrentLicenseExceeded.ToString());
            return AzureHelper.AddQueueMessageAsync(null, prop, MessageKey.SendEmail);
        }

        #endregion

        #region SendEmail

        public static async Task<bool> SendEmailAsync(EmailTemplateType emailTemplateType, IDictionary<string, string> properties, CrmDbContext context)
        {
            try
            {
                Telemetry.TrackDiagnostics($"SendEmailAsync: {emailTemplateType}");

                switch (emailTemplateType)
                {
                    case EmailTemplateType.AccountCreatedVerifyEmail:
                    case EmailTemplateType.AccountCreatedViaProcessOrderMailBilling_Home:
                    case EmailTemplateType.AccountCreatedViaProcessOrderMailBilling_DbServer:
                    case EmailTemplateType.AccountCreatedViaProcessOrderMailBilling_Business:
                    case EmailTemplateType.AccountCreatedViaProcessOrderMailLicensee:
                        {
                            var contactKey = properties[MessageKey.ContactKey];

                            var userManager = new CrmUserManager(context);
                            var user = await context.GetByKeyAsync(contactKey);
                            if (user == null)
                            {
                                Telemetry.TrackTrace($"SendEmailAsync: {emailTemplateType}. User not found: {contactKey}", SeverityLevel.Warning);
                                return false;
                            }
                            var vKey = await userManager.SetVerificationKeyForNewUserAsync(user);
                            await SendVerificationKeyMail(user, user.Email, vKey, emailTemplateType);
                            return true;
                        }

                    case EmailTemplateType.AccountMerging:
                        {
                            var contactKey = properties[MessageKey.ContactKey];
                            var loserEmailAddress = properties["LoserEmailAddress"];

                            var userManager = new CrmUserManager(context);
                            var result = await userManager.SendAccountMergingKeyAsync(loserEmailAddress, contactKey);
                            return result == MergeAccountResult.Success;
                        }

                    case EmailTemplateType.CitaviGiftCard:
                        {
                            var voucherKey = properties["VoucherKey"];

                            var voucher = await context.Get<Voucher>(voucherKey);
                            var voucherBlock = await voucher.VoucherBlock.Get();
                            var contact = await voucherBlock.Contact.Get(ContactPropertyId.Key);
                            var user = await context.GetByKeyAsync(contact.Key);
                            await SendVoucherGiftMailAsync(user, voucher.VoucherCode);
                            return true;
                        }

                    case EmailTemplateType.ConfirmEmailAddressMail:
                        {
                            var linkedEmailAccountKey = properties["LinkedEmailAccountKey"];

                            var userManager = new CrmUserManager(context);
                            var linkedEmailAccount = await context.Get<LinkedEmailAccount>(linkedEmailAccountKey);
                            var contact = await linkedEmailAccount.Contact.Get(ContactPropertyId.Key);
                            var user = await context.GetByKeyAsync(contact.Key);
                            var result = await userManager.SendVerificationKeyMailAsync(user, linkedEmailAccount.Email);
                            return result == SendVerificationKeyMailResult.OK;
                        }

                    case EmailTemplateType.ConfirmEmailAddressCampusContractLicenseMail:
                        {
                            var licenseKey = properties[MessageKey.LicenseKey];

                            var campusContractManager = new CampusContractManager(context);
                            var license = await context.Get<CitaviLicense>(licenseKey);
                            var contact = await license.EndUser.Get(ContactPropertyId.Key);
                            var user = await context.GetByKeyAsync(contact.Key);
                            var result = await campusContractManager.SetLicenseVerificationKeyAndSendMailAsync(user, licenseKey);
                            return result == SendVerificationKeyMailResult.OK;
                        }

                    case EmailTemplateType.LicenseWithdrawal:
                        {
                            var contactKey = properties[MessageKey.ContactKey];
                            var user = await context.GetByKeyAsync(contactKey);
                            if (user == null)
                            {
                                Telemetry.TrackTrace($"SendEmailAsync: {emailTemplateType}. User not found: {contactKey}", SeverityLevel.Warning);
                                return false;
                            }
                            await SendLicenseWithdrawalMailAsync(user, user.Email);
                            return true;
                        }

                    case EmailTemplateType.PasswordResetRequested:
                        {
                            var contactKey = properties[MessageKey.ContactKey];
                            var userManager = new CrmUserManager(context);
                            var user = await context.GetByKeyAsync(contactKey);
                            if (user == null)
                            {
                                Telemetry.TrackTrace($"SendEmailAsync: {emailTemplateType}. User not found: {contactKey}", SeverityLevel.Warning);
                                return false;
                            }
                            var result = await userManager.SendVerificationKeyResetPasswordAsync(user, user.Email);
                            return result == SendVerificationKeyMailResult.OK;
                        }

                    case EmailTemplateType.ProjectRoleChanged:
                        {
                            var projectRoleKey = properties[MessageKey.ProjectRoleKey];

                            var projectRole = await context.Get<ProjectRole>(projectRoleKey);
                            var projectEntry = await projectRole.Project.Get();
                            var contact = await projectRole.Contact.Get(ContactPropertyId.Key);
                            var user = await context.GetByKeyAsync(contact.Key);
                            var ownerContact = await projectEntry.GetProjectOwner(ContactPropertyId.Key);
                            var owner = await context.GetByKeyAsync(ownerContact.Key);
                            await SendProjectRoleChangedMailAsync(user, projectRole, owner);
                            return true;
                        }

                    case EmailTemplateType.OrderProcessBilling_Business:
                    case EmailTemplateType.OrderProcessBilling_DbServer:
                    case EmailTemplateType.OrderProcessBilling_Home:
                        {
                            var orderGroup = OrderProcessProductGroup.Home;
                            if (emailTemplateType == EmailTemplateType.OrderProcessBilling_Business)
                            {
                                orderGroup = OrderProcessProductGroup.Business;
                            }
                            else if (emailTemplateType == EmailTemplateType.OrderProcessBilling_DbServer)
                            {
                                orderGroup = OrderProcessProductGroup.DbServer;
                            }
                            var contactKey = properties[MessageKey.ContactKey];
                            var user = await context.GetByKeyAsync(contactKey);
                            if (user == null)
                            {
                                Telemetry.TrackTrace($"SendEmailAsync: {emailTemplateType}. User not found: {contactKey}", SeverityLevel.Warning);
                                return false;
                            }
                            await SendProcessOrder_BillingMail(user, user.Email, orderGroup);
                            return true;
                        }

                    case EmailTemplateType.OrderProcessLicensee:
                        {
                            var contactKey = properties[MessageKey.ContactKey];
                            var user = await context.GetByKeyAsync(contactKey);
                            if (user == null)
                            {
                                Telemetry.TrackTrace($"SendEmailAsync: {emailTemplateType}. User not found: {contactKey}", SeverityLevel.Warning);
                                return false;
                            }
                            await SendAssignLicenseMailAsync(user, user.Email);
                            return true;
                        }

                    case EmailTemplateType.RecoverDeletedProject:
                        {
                            var projectKey = properties[MessageKey.ProjectKey];
                            var projectManager = new ProjectManager(context);
                            var projectEntry = await context.Get<ProjectEntry>(projectKey);
                            if (projectEntry == null)
                            {
                                Telemetry.TrackTrace($"SendEmailAsync: {emailTemplateType}. ProjectEntry not found: {projectKey}", SeverityLevel.Warning);
                                return false;
                            }
                            var owner = await projectEntry.GetProjectOwner(ContactPropertyId.Key);
                            var user = await context.GetByKeyAsync(owner.Key);
                            await projectManager.SetRecoveryKeyAndSendMail(user, projectEntry);
                            return true;

                        }
                }
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat, property1: (nameof(TelemetryProperty.Description), emailTemplateType.ToString()));
            }
            return false;
        }

        public static async Task<bool> SendHtmlEmailAsync(string body, string toAddress, string subject, string fromAddress, string fromName, string tag = null)
        {
            OnEmailSend(null, toAddress, null, fromAddress);
            return await EmailClient.SendAsync(body, toAddress, fromAddress, fromName, subject, true, null, tag);
        }

        public static async Task<bool> SendEmailWithAttachementAsync(string body, string toAddress, string subject, bool isHtml, EmailAttachment attachment, string tag = null)
        {
            OnEmailSend(null, toAddress, subject);

            if (CrmConfig.IsUnittest)
            {
                return true;
            }
            if (toAddress.EndsWith(".noemail@citavi.com", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            if (toAddress.EndsWith(".noemail@citavi2.com", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return await EmailClient.SendAsync(body, toAddress, subject, isHtml, attachment, tag);
        }

        #endregion

        #endregion
    }

    public class EmailInfo
    {
        public EmailTemplateType Type { get; set; }
        public string EmailTo { get; set; }
        public string EmailFrom { get; set; }
        public string VerificationKey { get; set; }

        public string PlainText { get; set; }
    }
}
