using Aspose.Words.Drawing;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Client = Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using SwissAcademic.Crm.Web.Authorization;
using SwissAcademic.KeyVaultUtils;
using SwissAcademic.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public partial class CrmUserManager
    {
        #region Felder

        static readonly string[] UglyBase64 = { "+", "/", "=" };
        AzureB2CManager _azureB2CManager;

        #endregion

        #region Konstruktor

        public CrmUserManager(CrmDbContext dbContext)
        {
            DbContext = dbContext;
        }

        #endregion

        #region Eigenschaften

        #region DbContext

        public CrmDbContext DbContext { get; set; }

        #endregion

        #endregion

        #region Methoden

        #region AddEmail

        internal async Task<LinkedEmailAccount> AddEmail(string userKey, string email)
        {
            var user = await GetByKeyAsync(userKey);
            if (user == null)
            {
                var execption = new ArgumentException($"User with key '{userKey}' not found");
                Telemetry.TrackException(execption);
            }
            return await AddEmail(user, email);
        }

        public async Task<LinkedEmailAccount> AddEmail(CrmUser user, string email, bool ignoredAccessCheck = false)
        {
            email = email.RemoveNonStandardWhitespace();

            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            if (!ignoredAccessCheck)
            {
                await AuthorizationManager.Instance.CheckAccessAsync(user.Principal, AuthAction.Create, AuthResource.CrmLinkedEmailAccount, AuthResource.LinkedEmailAccount(email));
            }

            await ValidateEmail(user, email);

            var linkedEmailAccount = user.AddLinkedEMailAccount(email);

            var vkey = SetVerificationKey(user, VerificationKeyPurpose.ChangeEmail, state: email);
            await DbContext.SaveAndUpdateUserCacheAsync(user);
            await EmailService.SendConfirmEmailAddressMail(user, email, vkey);
            return linkedEmailAccount;
        }

        public async Task<VerifyEmailSignatureResult> AddEmailWithSignatureCheck(CrmUser user, string email, string signature)
        {
            if (user == null)
            {
                throw new NullReferenceException("User must not be null");
            }

            email = email.RemoveNonStandardWhitespace();
            if (string.IsNullOrEmpty(email))
            {
                throw new NullReferenceException("email must not be null or empty");
            }

            if (string.IsNullOrEmpty(signature))
            {
                throw new NullReferenceException("signature must not be null or empty");
            }

            await using (var tslock = new TableStorageLock(email))
            {
                if (!await tslock.TryEnter())
                {
                    return VerifyEmailSignatureResult.Error;
                }

                LinkedEmailAccount linkedEmailAccount;

                if (!await SignatureUtility.VerifyLinkedEmailAccountSignature(user, email, signature))
                {
                    var signatureValidationException = new SignatureValidationException(user);
                    if (CrmConfig.IsUnittest)
                    {
                        throw signatureValidationException;
                    }
                    Telemetry.TrackException(signatureValidationException, SeverityLevel.Error, ExceptionFlow.Eat);
                    return VerifyEmailSignatureResult.Error;
                }

                if (!CrmUserAccountValidation.EmailIsValid(email))
                {
                    throw new NotSupportedException($"Email '{email}' is not vaild");
                }

                linkedEmailAccount = user.CrmLinkedEMailAccounts.FirstOrDefault(i => string.Equals(i.Email, email, StringComparison.InvariantCultureIgnoreCase));
                if (linkedEmailAccount != null)
                {
                    linkedEmailAccount.IsVerified = true;
                }
                else if (await EmailExistsAsync(email))
                {
                    return VerifyEmailSignatureResult.EmailPresentMerge;
                }
                else
                {
                    await AuthorizationManager.Instance.CheckAccessAsync(user.Principal, AuthAction.Create, AuthResource.CrmLinkedEmailAccount, AuthResource.LinkedEmailAccount(email));
                    linkedEmailAccount = user.AddLinkedEMailAccount(email, true);
                }
                var existingCrm4User = await DbContext.FetchFirstOrDefault<Contact>(new Query.FetchXml.GetCrm4Contact(email).TransformText());
                if (existingCrm4User != null &&
                    existingCrm4User.Key != user.Key)
                {
                    await DbContext.SaveAndUpdateUserCacheAsync(user);
                    await DbContext.MergeCrm4ContactAsync(existingCrm4User, user);
                    await CrmUserCache.RemoveAsync(user);
                    user = await DbContext.GetByKeyAsync(user.Key);
                }

                await new CampusContractManager(DbContext).AddLicensesFromEmailVerficationAsync(user, email);
                await DbContext.SaveAndUpdateUserCacheAsync(user);

                // Add link email to Azure B2C
                await AddLinkEmail(user.Contact.AzureB2CId, email);
                return VerifyEmailSignatureResult.Success;
            }
        }

        #endregion

        #region AddOrUpdateLinkedAccountAsync
        public async Task AddOrUpdateLinkedAccountAsync(CrmUser user, string provider, string id)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (string.IsNullOrWhiteSpace(provider))
            {
                throw new ArgumentNullException("provider");
            }
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }

            var otherUser = await DbContext.GetByLinkedAccountAsync(provider, id);
            if (otherUser != null && otherUser.Id != user.Id)
            {
                throw new ValidationException(MembershipRebootConstants.LinkedAccountAlreadyInUse);
            }

            var linked = user.CrmLinkedAccounts.Where(x => x.IdentityProviderId == provider && x.NameIdentifier == id).SingleOrDefault();
            if (linked == null)
            {
                linked = DbContext.Create<LinkedAccount>();
                linked.IdentityProviderId = provider;
                linked.NameIdentifier = id;
                linked.LastLogin = DateTime.UtcNow;
                user.AddLinkedAccount(linked);

                // Add link account to Azure B2C
                await AddLinkAccount(user.Contact.AzureB2CId, provider, id);
            }
            else
            {
                linked.LastLogin = DateTime.UtcNow;
            }
        }

        #endregion

        #region AuthenticateAsync

        public async Task<(CrmUser User, bool Success)> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (null, false);
            }

            var account = await DbContext.GetByEmailAsync(username);
            if (account == null)
            {
                return (null, false);
            }

            var result = await AuthenticateAsync(account, password);
            return (account, result);
        }

        async Task<bool> AuthenticateAsync(CrmUser user, string password)
        {
            var success = await VerifyPasswordAsync(user, password);

            if (success)
            {
                try
                {
                    if (!user.IsLoginAllowed)
                    {
                        return false;
                    }

                    if (user.IsAccountClosed)
                    {
                        return false;
                    }

                    if (CrmConfig.RequireAccountVerification &&
                        !user.IsAccountVerified)
                    {
                        return false;
                    }
                }
                finally
                {
                    await DbContext.SaveAsync(user);
                }
            }

            return success;
        }

        #endregion

        #region CreateUser

        public async Task<CrmUser> CreateUser(string firstname, string lastname, string title, string email, string password, LanguageType? language, string remoteIPAddress = null, bool isVerfied = false)
        {
            await using (var tslock = new TableStorageLock(email))
            {
                if (!await tslock.TryEnter())
                {
                    if (CrmConfig.IsUnittest)
                    {
                        return null;
                    }
                    throw new RateLimitException();
                }
                email = email.RemoveNonStandardWhitespace();
                //Bei CRM4 Kontakten wird ein neuer Kontakt angelegt. Bei der Bestätigung wird dann der CRM4 Kontakt mit dem neuen Kontakt gemergt.

                var user = await CreateUser(password, email);
                var contact = user.Contact;
                contact.FirstName = firstname;
                contact.LastName = lastname;
                contact.TitelDefinedByContact = title;
                contact.ChangeLanguage(language ?? LanguageType.English);
                contact.EMailAddress1 = email;
                contact.DataCenter = await AzureRegionResolver.Instance.GetDataCenter(remoteIPAddress);

                if (!isVerfied)
                {
                    var key = SetVerificationKey(user, VerificationKeyPurpose.ChangeEmail, state: email);
                    await EmailService.SendConfirmEmailAddressMail(user, email, key);
                }
				else
				{
                    user.IsAccountVerified = true;
                    user.IsLoginAllowed = true;
                    user.CrmLinkedEMailAccounts[0].IsVerified = true;
				}
                if (!await user.CreateUserSettingsIfNotExistsAsync(DbContext))
                {
                    await DbContext.SaveAndUpdateUserCacheAsync(user);
                }
                await DbContext.SaveAndUpdateUserCacheAsync(user);
                
                return user;
            }
        }

        public async Task<CrmUser> CreateUserWithCampusInfoAsync(CreateCampusUserAccountInfo info, LanguageType? language, bool isStudent, bool checkEmailExists, string remoteIPAddress = null)
        {
            await using (var tslock = new TableStorageLock(info.Email))
            {
                if (!await tslock.TryEnter())
                {
                    if (CrmConfig.IsUnittest)
                    {
                        return null;
                    }
                    throw new RateLimitException();
                }
                var email = info.Email;
                email = email.RemoveNonStandardWhitespace();
                if (checkEmailExists &&
                    await EmailExistsAsync(email))
                {
                    var exception = new InvalidOperationException($"'{email}' already exists");
                    Telemetry.TrackException(exception);
                }
                var contact = DbContext.Create<Contact>();
                var user = new CrmUser(contact);
                contact.DataCenter = await AzureRegionResolver.Instance.GetDataCenter(remoteIPAddress);
                contact.ChangeLanguage(language ?? LanguageType.English);
                contact.EMailAddress1 = email;
                var linkedEmailAccount = user.AddLinkedEMailAccount(email);
                linkedEmailAccount.VerificationCampusGroup = isStudent ? CampusGroup.Students : CampusGroup.Faculty;
                if (info.IPRangeCheck)
                {
                    if (string.IsNullOrEmpty(info.IP))
                    {
                        var exception = new ArgumentException($"{nameof(info.IP)} is null or empty");
                        Telemetry.TrackException(exception);
                    }
                    linkedEmailAccount.VerificationIPAddress = info.IP;
                }
                if (info.VoucherCheck)
                {
                    if (string.IsNullOrEmpty(info.VoucherCode))
                    {
                        var exception = new ArgumentException($"{nameof(info.VoucherCode)} is null or empty");
                        Telemetry.TrackException(exception);
                    }
                    linkedEmailAccount.VerificationVocherCode = info.VoucherCode;
                }

                var key = SetVerificationKey(user, VerificationKeyPurpose.ChangeEmail, state: email);
                await DbContext.SaveAndUpdateUserCacheAsync(user);
                await EmailService.SendConfirmEmailAddressMail(user, email, key);
                return user;
            }
        }

        //Anmeldung via VoucherCode ohne CampusContract
        public async Task<CrmUser> CreateUserWithVoucherCodeAsync(string email, string voucherCode, LanguageType? language, bool checkEmailExists, string remoteIPAddress = null)
        {
            await using (var tslock = new TableStorageLock(email))
            {
                if (!await tslock.TryEnter())
                {
                    if (CrmConfig.IsUnittest)
                    {
                        return null;
                    }
                    throw new RateLimitException();
                }
                email = email.RemoveNonStandardWhitespace();
                if (checkEmailExists &&
                    await EmailExistsAsync(email))
                {
                    var exception = new InvalidOperationException($"'{email}' already exists");
                    Telemetry.TrackException(exception);
                }
                var contact = DbContext.Create<Contact>();
                var user = new CrmUser(contact);
                contact.ChangeLanguage(language ?? LanguageType.English);
                contact.EMailAddress1 = email;
                contact.DataCenter = await AzureRegionResolver.Instance.GetDataCenter(remoteIPAddress);
                var linkedEmailAccount = user.AddLinkedEMailAccount(email);
                linkedEmailAccount.VerificationVocherCode = voucherCode;

                var key = SetVerificationKey(user, VerificationKeyPurpose.ChangeEmail, state: email);
                if (!await user.CreateUserSettingsIfNotExistsAsync(DbContext))
                {
                    await DbContext.SaveAndUpdateUserCacheAsync(user);
                }
                await EmailService.SendConfirmEmailAddressMail(user, email, key);
                return user;
            }
        }

        async Task<CrmUser> CreateUser(string password, string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("email must not be null");
            }

            if (await EmailExistsAsync(email))
            {
                throw new NotSupportedException($"Email already exists: {email}");
            }

            if (!CrmUserAccountValidation.EmailIsValid(email))
            {
                var ex = new ArgumentException($"Email is not vaild:{email}");
                ex.TreatAsWarning();
                throw ex;
            }

            var contact = DbContext.Create<Contact>();
            var user = new CrmUser(contact);
            var now = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(password))
            {
                ValidatePassword(user, password);
                user.HashedPassword = password != null ? CrmConfig.Crypto.HashPassword(password, CrmConfig.PasswordHashingIterationCount) : null;
                user.PasswordChanged = password != null ? now : (DateTime?)null;
            }
            
            user.Email = email;
            user.Created = now;
            user.LastUpdated = now;
           
            user.IsAccountVerified = false;
            user.IsLoginAllowed = true;

            return user;
        }

        internal async Task<CrmUser> CreateUserFromExternalProviderAsync(string provider, string providerAccountId, IEnumerable<Claim> claims, bool saveAndRemoveUserFromCache = true, string remoteIPAddress = null)
        {
            try
            {
                var email = claims.GetFirstValue(JwtClaimTypes.Email, SAML2ClaimTypes.Email, ClaimTypes.Email);
                if (!string.IsNullOrEmpty(email))
                {
                    email = email.RemoveNonStandardWhitespace();
                }

                var user = await DbContext.GetByEmailAsync(email);
                if (user == null)
                {
                    var contact = DbContext.Create<Contact>();
                    contact.IsVerified = true;
                    contact.IsLoginAllowed = true;
                    contact.DataCenter = await AzureRegionResolver.Instance.GetDataCenter(remoteIPAddress);
                    user = new CrmUser(contact);
                    await user.CreateUserSettingsIfNotExistsAsync(DbContext, saveChanges: false);
                    new CrmClaimsToUserMapper().Map(user, claims);
                }
                else if (CrmIdentityProviders.IsShibbolethProvider(provider))
                {
                    //User existiert bereits, hat aber den Shib-Account nicht (Shib-Claims enthalten User-Email)
                    //Wir erlauben nur bei EternalEmail eine "Übernahme" des bestehenden Kontakts
                    var allowShibbolethLogin = CrmCache.CampusContracts.Any(cc => cc.ShibbolethEntityId == provider &&
                                                                                  cc.EternalEmail == YesNoOptionSet.Yes);

                    //#2056
                    //08.08.2019:
                    //Wir erlauben nun wieder die Übernahme von Account via Shibboleth. Egal ob keine EternalEmail.
                    //Ich lass den Code aber drin. In einem Jahr ist das dann ev. wieder nötig ;-)
                    //Betrifft 2 Unittests: 
                    //Shibboleth_NewAccountFromExternalProviderButEmailAlreadyExists_NotOk
                    //Shibboleth_NewAccountFromExternalProviderButEmailAlreadyExists_NotOk_LoginHint

                    allowShibbolethLogin = true;

                    if (!user.Contact.IsVerified.GetValueOrDefault())
                    {
                        //Bei einem "nicht verifizierten" Account erlauben wir die Übernahme von Shibboleth
                        allowShibbolethLogin = true;
                    }

                    if (!allowShibbolethLogin)
                    {
                        Telemetry.TrackTrace($"CreateUserFromExternalProvider: User already extists with this email (shibboleth: {provider})");
                        var ex = new Exception(AuthenticateResultConstants.NewAccountFromExternalProviderButEmailAlreadyExists);
                        ex.Data[nameof(Contact.EMailAddress1)] = email;
                        ex.Data[nameof(user.Key)] = user.Key;
                        ex.Data[nameof(SeverityLevel)] = "Warning";
                        throw ex;
                    }
                }

                if (!string.IsNullOrEmpty(user.VerificationKey))
                {
                    var linkedEmail = user.CrmLinkedEMailAccounts.First(i => i.VerificationKey == user.VerificationKey);
                    linkedEmail.ClearVerifcationData();
                    linkedEmail.IsVerified = true;
                    user.Contact.IsVerified = true;
                    user.Contact.IsLoginAllowed = true;
                }

                if (user.Contact.IsKeyContact)
                {
                    throw new Exception(AuthenticateResultConstants.ContactUpdateFailedIsKeyContact);
                }
                if (!string.IsNullOrEmpty(email))
                {
                    var existingCrm4User = await DbContext.FetchFirstOrDefault<Contact>(new Query.FetchXml.GetCrm4Contact(email).TransformText());
                    if (existingCrm4User != null &&
                        existingCrm4User.Key != user.Key &&
                       !existingCrm4User.IsVerified.GetValueOrDefault(false))
                    {
                        //Es handelt sich um einen alten CRM4 - Kontakt.
                        //Wir mergen diesen alten Kontakt mit dem aktuellen User
                        await DbContext.SaveAndUpdateUserCacheAsync(user);
                        await DbContext.MergeCrm4ContactAsync(existingCrm4User, user);
                        await user.Load(DbContext, attachToContext: true, loadContext: UserLoadContexts.Licenses);
                        if (!user.HasMissingValues())
                        {
                            await UpdateExistingLicensesForNewUserAsync(user, false);
                        }
                    }
                }

                await AddOrUpdateLinkedAccountAsync(user, provider, providerAccountId);

                var campusContractManager = new CampusContractManager(DbContext);
                if (CrmIdentityProviders.IsShibbolethProvider(provider))
                {
                    await campusContractManager.AddOrUpdateLicensesFromShibbolethClaimsAsync(user, claims);
                }

                await campusContractManager.AddLicensesFromEmailVerficationAsync(user, email);
                new ProjectManager(DbContext).ConfirmInvitationAfterAccountCreation(user);
                if (saveAndRemoveUserFromCache)
                {
                    await DbContext.SaveAsync();
                    await CrmUserCache.RemoveAsync(user);
                }
                return user;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
            }
            return null;
        }

        internal CrmUser CreateUserFromCleverbridgeOrder(Cleverbridge.ContactType cleverbridgeContact)
		{
            var email = cleverbridgeContact.Email.RemoveNonStandardWhitespace();
            var contact = DbContext.Create<Contact>();
            contact.Address1_City = cleverbridgeContact.City;
            contact.Address1_Country = cleverbridgeContact.CountryId.ToString();
            contact.Address1_PostalCode = cleverbridgeContact.PostalCode;
            contact.Address1_StateOrProvince = cleverbridgeContact.State;
            contact.Address1_Line1 = cleverbridgeContact.Street1;
            contact.Address1_Line2 = cleverbridgeContact.Street2;
            contact.Address1_Fax = cleverbridgeContact.Fax;
            contact.Address1_Telephone1 = cleverbridgeContact.Phone1;
            contact.Address1_Telephone2 = cleverbridgeContact.Phone2;
            contact.FirstName = cleverbridgeContact.Firstname;
            contact.LastName = cleverbridgeContact.Lastname;
            contact.ChangeLanguage(cleverbridgeContact.LanguageResolved);
            contact.Salutation = cleverbridgeContact.Salutation;
            contact.TitelDefinedByContact = cleverbridgeContact.Title;
            contact.Firm = cleverbridgeContact.Company;
            

            switch (cleverbridgeContact.SalutationId)
            {
                case Cleverbridge.SalutationIdType.MIS:
                case Cleverbridge.SalutationIdType.MS_:
                case Cleverbridge.SalutationIdType.MRS:
                    contact.GenderCode = GenderCodeType.Female;
                    break;

                case Cleverbridge.SalutationIdType.MR_:
                    contact.GenderCode = GenderCodeType.Male;
                    break;
            }

            var user = new CrmUser(contact);
            user.AddLinkedEMailAccount(email);
            contact.ContactCreatedByOrdermail = true;

            return user;
        }

        #endregion

        #region CancelVerificationAsync

        public async Task CancelVerificationAsync(string verificationKey)
        {
            var vKey = CrmConfig.Crypto.Hash(verificationKey);
            var user = await DbContext.GetByVerificationKeyAsync(vKey);
            if (user.CrmLinkedEMailAccounts.Count == 1)
            {
                throw new NotSupportedException();
            }
            var linkedEMailAddress = user.CrmLinkedEMailAccounts.FirstOrDefault(i => i.VerificationKey == vKey);
            if (linkedEMailAddress == null)
            {
                var exception = new ArgumentException($"LinkedEMailAccount '{verificationKey}' not found");
                Telemetry.TrackException(exception);
            }
            if (linkedEMailAddress.VerificationPurpose == VerificationKeyPurpose.ChangeEmail)
            {
                user.RemoveLinkedEMailAccount(linkedEMailAddress);
                DbContext.Delete(linkedEMailAddress);
            }
            if (user.CrmLinkedEMailAccounts.Count == 0)
            {
                DbContext.Delete(user.Contact);
            }
            await DbContext.SaveAsync();
            await CrmUserCache.RemoveAsync(user);
        }

		#endregion

		#region ChangePrimaryEmailAddress

        public async Task ChangePrimaryEmailAddress(string contactKey, string newPrimaryEmailLinkedAccountKey)
		{
            if (string.IsNullOrEmpty(contactKey))
            {
                throw new NotSupportedException($"{nameof(contactKey)} must not be null or empty");
            }

            var user = await DbContext.GetByKeyAsync(contactKey);

            if (user == null)
			{
                throw new NotSupportedException($"{nameof(user)} must not be null");
			}
            if(string.IsNullOrEmpty(newPrimaryEmailLinkedAccountKey))
			{
                throw new NotSupportedException($"{nameof(newPrimaryEmailLinkedAccountKey)} must not be null or empty");
            }
            var linkedEmailAccount = user.CrmLinkedEMailAccounts.FirstOrDefault(linkedEmailAccount => string.Equals(linkedEmailAccount.Key, newPrimaryEmailLinkedAccountKey, StringComparison.InvariantCultureIgnoreCase));
            if (linkedEmailAccount == null)
            {
                throw new NotSupportedException($"{nameof(linkedEmailAccount)} must not be null. {newPrimaryEmailLinkedAccountKey} / {user.Key}");
            }
            if(!linkedEmailAccount.IsVerified.GetValueOrDefault())
			{
                throw new NotSupportedException($"{nameof(linkedEmailAccount)} must be verified. {newPrimaryEmailLinkedAccountKey} / {user.Key}");
            }
            user.Contact.EMailAddress1 = linkedEmailAccount.Email;
            await DbContext.SaveAndUpdateUserCacheAsync(user);
        }

		#endregion

		#region ChangePassword

		/// <summary>
		/// Wenn HttpContextAccessor gesetzt ist, werden alle Cookies und AccessTokens des Users revoked, ausser der aktuellen Session
		/// Wenn HttpContextAccessor nicht gesetzt ist, werden alle Cookies und AccessTokens des User revoked (Bsp. Passwort vergessen, da ist man nicht angemeldet)
		/// </summary>
		/// <param name="user"></param>
		/// <param name="oldPassword"></param>
		/// <param name="newPassword"></param>
		/// <param name="httpContextAccessor"></param>
		/// <returns></returns>
		public async Task ChangePasswordAsync(CrmUser user, string oldPassword, string newPassword, IHttpContextAccessor httpContextAccessor)
        {
            if (String.IsNullOrWhiteSpace(oldPassword))
            {
                throw new ValidationException(MembershipRebootConstants.InvalidOldPassword);
            }
            if (String.IsNullOrWhiteSpace(newPassword))
            {
                throw new ValidationException(MembershipRebootConstants.InvalidNewPassword);
            }

            if (!await VerifyPasswordAsync(user, oldPassword))
            {
                throw new ValidationException(MembershipRebootConstants.InvalidOldPassword);
            }

            ValidatePassword(user, newPassword);
            await SetPasswordAsync(user, newPassword);

            foreach(var linkedEmailAccount in user.CrmLinkedEMailAccounts)
			{
                if(!string.IsNullOrWhiteSpace(linkedEmailAccount.VerificationKey) &&
                    linkedEmailAccount.VerificationPurpose == VerificationKeyPurpose.ResetPassword)
				{
                    linkedEmailAccount.VerificationKey = string.Empty;
                }
			}
            await DbContext.SaveAsync(user);

            if (httpContextAccessor != null)
            {
                await CrmAuthenticationService.RevokeUserAccessTokensAndCookiesExpectCurrentSession(user, httpContextAccessor);
            }
			else
			{
                await CrmAuthenticationService.RevokeUserAccessTokensAndCookies(user.Key);
            }
        }

        #endregion

        #region ChangePasswordFromResetKey

        public async Task<CrmUser> ChangePasswordFromResetKeyAsync(string verificationKey, string newPassword, IHttpContextAccessor httpContextAccessor)
        {
            CrmUser user;
            if (String.IsNullOrWhiteSpace(verificationKey))
            {
                var exception = new ValidationException($"Invalid {nameof(verificationKey)}: '{verificationKey}'");
                Telemetry.TrackException(exception);
            }

            if (String.IsNullOrWhiteSpace(newPassword))
            {
                var exception = new ValidationException($"Invalid {nameof(newPassword)}: '{newPassword}'");
                Telemetry.TrackException(exception);
            }
            await using (var tslock = new TableStorageLock(verificationKey))
            {
                if (!await tslock.TryEnter())
                {
                    if (CrmConfig.IsUnittest)
                    {
                        return null;
                    }
                    throw new RateLimitException();
                }
                user = await GetByVerificationKeyAsync(verificationKey);
                if (user == null)
                {
                    return null;
                }

                ValidatePassword(user, newPassword);

                if (!user.Contact.IsVerified.GetValueOrDefault())
                {
                    return null;
                }

                if (!user.IsLoginAllowed)
                {
                    return null;
                }

                if (user.IsAccountClosed)
                {
                    return null;
                }

                if (!IsVerificationKeyValid(user, VerificationKeyPurpose.ResetPassword, verificationKey))
                {
                    return null;
                }

                ClearVerificationKey(user);
                await SetPasswordAsync(user, newPassword);
                await DbContext.SaveAsync(user);

                if (httpContextAccessor != null)
                {
                    await CrmAuthenticationService.RevokeUserAccessTokensAndCookiesExpectCurrentSession(user, httpContextAccessor);
                }
                else
                {
                    await CrmAuthenticationService.RevokeUserAccessTokensAndCookies(user.Key);
                }

                return user;
            }
        }

        #endregion

        #region CheckHasTooManyRecentPasswordFailures
        bool CheckHasTooManyRecentPasswordFailures(CrmUser account)
        {
            var result = false;
            if (CrmConfig.AccountLockoutFailedLoginAttempts <= account.FailedLoginCount)
            {
                result = account.LastFailedLogin >= DateTime.UtcNow.Subtract(CrmConfig.AccountLockoutDuration);
                if (!result)
                {
                    // if we're past the lockout window, then reset to zero
                    account.FailedLoginCount = 0;
                }
            }

            if (result)
            {
                account.FailedLoginCount++;
            }

            return result;
        }

        #endregion

        #region ClearVerificationKey
        static void ClearVerificationKey(CrmUser account)
        {
            account.VerificationKey = null;
            account.VerificationPurpose = null;
            account.VerificationKeySent = null;
            account.VerificationStorage = null;
        }

        #endregion

        #region EmailExists

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            email = email.RemoveNonStandardWhitespace();

            await using (var tslock = new TableStorageLock(email))
            {
                if (!await tslock.TryEnter())
                {
					if (CrmConfig.IsUnittest)
					{
                        return false;
					}
                    throw new RateLimitException();
                }
                var result = await DbContext.Fetch(FetchXmlExpression.Create<Contact>(new Query.FetchXml.EmailExists(email).TransformText()));

                if (result == null || !result.Any())
                {
                    return false;
                }

                return true;
            }
        }

        #endregion

        #region EmailExistsButNotVerified

        public async Task<bool> EmailExistsButNotVerified(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            email = email.RemoveNonStandardWhitespace();

            var result = await DbContext.Get<LinkedEmailAccount>(LinkedEmailAccountPropertyId.Email, email, LinkedEmailAccountPropertyId.IsVerified);
            if (result == null)
            {
                return false;
            }

            return !result.IsVerified.GetValueOrDefault();
        }

        #endregion

        #region GetBy

        public Task<CrmUser> GetByEmailAsync(string email) => DbContext.GetByEmailAsync(email);
        public Task<CrmUser> GetByKeyAsync(string key) => DbContext.GetByKeyAsync(key);
        public async Task<CrmUser> GetByVerificationKeyAsync(string vKey)
        {
            if (String.IsNullOrWhiteSpace(vKey))
            {
                return null;
            }

            vKey = CrmConfig.Crypto.Hash(vKey);
            await using (var tslock = new TableStorageLock(vKey))
            {
                if (!await tslock.TryEnter())
                {
                    if (CrmConfig.IsUnittest)
                    {
                        return null;
                    }
                    throw new RateLimitException();
                }
                return await DbContext.GetByVerificationKeyAsync(vKey);
            }
        }

        #endregion

        #region GetByLicenseVerificationKey

        public async Task<CrmUser> GetByLicenseVerificationKey(string key)
        {
            await using (var tslock = new TableStorageLock(key))
            {
                if (!await tslock.TryEnter())
                {
					if (CrmConfig.IsUnittest)
					{
                        return null;
					}
                    throw new RateLimitException();
                }
                var xml = new Query.FetchXml.GetContactByLicenseVerificationKey(key).TransformText();
                var result = await DbContext.Fetch(FetchXmlExpression.Create<CitaviLicense>(xml), observe: true);
                if (!result.Any())
                {
                    return null;
                }

                var resultSet = new CrmSet(result);
                var license = resultSet.Licenses.FirstOrDefault();
                if (license == null)
                {
                    return null;
                }

                var user = await DbContext.GetByKeyAsync(license.DataContractEndUserContactKey);
                await user.Load(DbContext, attachToContext: true, loadContext: UserLoadContexts.Licenses);
                return user;
            }
        }

		#endregion

		#region IsVerificationKeySendAllowed

        internal bool IsVerificationKeySendAllowed(LinkedEmailAccount linkedEmailAccount, VerificationKeyPurpose verificationKeyPurpose)
		{
            if(linkedEmailAccount.VerificationPurpose != verificationKeyPurpose)
			{
                return true;
			}
			if (!linkedEmailAccount.VerificationKeySent.HasValue)
			{
                return true;
			}
            return DateTime.UtcNow > linkedEmailAccount.VerificationKeySent.Value.Add(CrmConfig.VerificationKeyLockoutDuration);
        }

        #endregion

        #region IsVericationKeyExpired

        public bool IsVericationKeyExpired(CrmUser user, string verificationKey, VerificationKeyPurpose purpose)
        {
            return !IsVerificationKeyValid(user, purpose, verificationKey);
        }

        #endregion

        #region IsVerificationPurposeValid
        bool IsVerificationPurposeValid(CrmUser account, VerificationKeyPurpose purpose)
        {
            if (account.VerificationPurpose != purpose)
            {
                return false;
            }

            if (IsVerificationKeyStale(account))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region IsVerificationKeyStale

        bool IsVerificationKeyStale(CrmUser account)
        {
            if (account.VerificationKeySent == null)
            {
                return true;
            }

            if (account.VerificationKeySent < DateTime.UtcNow.Subtract(CrmConfig.VerificationKeyLifetime))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region MergeAccountAsync

        public async Task<MergeAccountResult> MergeAccountAsync(string userKey, string mergeVerificationKey)
        {
            var winner = await GetByKeyAsync(userKey);

            await AuthorizationManager.Instance.CheckAccessAsync(winner.Principal, AuthAction.Merge, AuthResource.CrmContact);

            var key = CrmConfig.Crypto.Hash(mergeVerificationKey);
            var loser = await DbContext.GetByMergeVerificationKey(key);

            if (loser == null ||
                loser.IsAccountClosed ||
                loser.Contact.IsKeyContact)
            {
                return MergeAccountResult.VerificationKeyNotFound;
            }

            if (loser.Contact.MergeAccountVerificationKeySent + CrmMembershipRebootConstants.MergeVerificationKeyLifetime < DateTime.UtcNow)
            {
                return MergeAccountResult.VerificationKeyExpired;
            }

            if(!loser.IsAccountVerified)
			{
                //In diesem Fall können wir den Account noch verifzieren. Der MergeKey ging an die Primäre - Email Adresse
                //Ansonsten wird der unverifizierte Email-Account übernommen und muss dann nochmals verifziert werden
                //#4637
                var linkedEmailAccount = loser.CrmLinkedEMailAccounts.First(i => string.Equals(i.Email, loser.Email, StringComparison.InvariantCultureIgnoreCase));
                if (linkedEmailAccount != null)
                {
                    linkedEmailAccount.IsVerified = true;
                    await DbContext.SaveAsync();
                }
            }

            await DbContext.MergeAsync(loser, winner);
            return MergeAccountResult.Success;
        }

        #endregion

        #region ResetPassword

        public async Task ResetPasswordAsync(CrmUser account)
        {
            await SendVerificationKeyResetPasswordAsync(account, account.Email);
        }

        #endregion

        #region RemoveLinkedEMailAccount

        public async Task RemoveLinkedEMailAccount(CrmUser user, LinkedEmailAccount linked, bool authCheck = true, IHttpContextAccessor httpContextAccessor = null)
        {
            if (linked == null)
            {
                throw new NullReferenceException(nameof(LinkedEmailAccount));
            }

            if (authCheck)
            {
                if (httpContextAccessor != null)
                {
                    await AuthorizationManager.Instance.CheckAccessAsync(user.Principal, httpContextAccessor.HttpContext.Items, AuthAction.Delete, AuthResource.CrmLinkedEmailAccount, AuthResource.LinkedEmailAccount(linked.Email));
                }
                else
                {
                    await AuthorizationManager.Instance.CheckAccessAsync(user.Principal, AuthAction.Delete, AuthResource.CrmLinkedEmailAccount, AuthResource.LinkedEmailAccount(linked.Email));
                }
            }
           
            if (user.CrmLinkedEMailAccounts.Count == 1)
            {
                var exception = new ArgumentException("Cant remove last LinkedEmailAccount");
                Telemetry.TrackException(exception);
            }

            if (user.Email.Equals(linked.Email, StringComparison.InvariantCultureIgnoreCase))
            {
                var exception = new ArgumentException($"Cant remove primary LinkedEmailAccount. {linked.Email}|{user.Contact.Key}");
                Telemetry.TrackException(exception);
            }

            user.RemoveLinkedEMailAccount(linked);

            //Nicht bestätige Lizenzen zu diesem Email-Account löschen.
            foreach (var license in user.Licenses.ToList())
            {
                if (!license.IsVerified &&
                   license.VerificationStorage == linked.Email)
                {
                    DbContext.Delete(license);
                }
            }
            DbContext.Delete(linked);

            //Remove Link Email from Azure B2C
            await InitializeAzureB2CManager();
            await _azureB2CManager.RemoveLinkEmail(user.Contact.AzureB2CId, linked.Email);

            await DbContext.SaveAndUpdateUserCacheAsync(user);
        }

        #endregion

        #region RemoveLinkedAccount

        public async Task RemoveLinkedAccount(CrmUser user, LinkedAccount linked, IHttpContextAccessor httpContextAccessor = null)
        {
            if (linked == null)
            {
                throw new NullReferenceException(nameof(LinkedAccount));
            }

            if (httpContextAccessor != null)
            {
                await AuthorizationManager.Instance.CheckAccessAsync(user.Principal, httpContextAccessor.HttpContext.Items, AuthAction.Delete, AuthResource.CrmLinkedAccount, AuthResource.LinkedAccount(linked.Key));
            }
            else
            {
                await AuthorizationManager.Instance.CheckAccessAsync(user.Principal, AuthAction.Delete, AuthResource.CrmLinkedAccount, AuthResource.LinkedAccount(linked.Key));
            }

            user.RemoveLinkedAccount(linked);

            //Remove Link Email from Azure B2C
            await InitializeAzureB2CManager();
            await _azureB2CManager.RemoveLinkAccount(user.Contact.AzureB2CId, linked.IdentityProviderId, linked.NameIdentifier);

            await DbContext.SaveAndUpdateUserCacheAsync(user);
        }

        #endregion

        #region RecordInvalidLoginAttempt

        void RecordInvalidLoginAttempt(CrmUser account)
        {
            account.LastFailedLogin = DateTime.UtcNow;
            if (account.FailedLoginCount <= 0)
            {
                account.FailedLoginCount = 1;
            }
            else
            {
                account.FailedLoginCount++;
            }
        }

        #endregion

        #region SetPasswordAsync

        public async Task SetPasswordAsync(CrmUser account, string password)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            if (String.IsNullOrWhiteSpace(password))
            {
                throw new ValidationException(MembershipRebootConstants.InvalidPassword);
            }

            account.HashedPassword = CrmConfig.Crypto.HashPassword(password, CrmConfig.PasswordHashingIterationCount);
            account.PasswordChanged = DateTime.UtcNow;
            account.RequiresPasswordReset = false;
            account.FailedLoginCount = 0;

            await DbContext.SaveAsync(account);
        }

        #endregion

        #region SetVerificationKey

        public string SetLinkedEmailVerificationKey(CrmUser user, string key = null, string state = null)
        {
            return SetVerificationKey(user, VerificationKeyPurpose.ChangeEmail, key, state);
        }
        string SetVerificationKey(CrmUser user, VerificationKeyPurpose purpose, string key = null, string state = null)
        {
            if (purpose == VerificationKeyPurpose.ChangeEmail)
            {
                var linkedEMailAccount = user.CrmLinkedEMailAccounts.First(i => string.Equals(i.Email, state, StringComparison.InvariantCultureIgnoreCase));
                user.SetVerificationData(linkedEMailAccount);
            }
            else if (purpose == VerificationKeyPurpose.ResetPassword)
            {
                var email = state ?? user.Email;
                var linkedEMailAccount = user.CrmLinkedEMailAccounts.First(i => string.Equals(i.Email, state, StringComparison.InvariantCultureIgnoreCase));
                user.SetVerificationData(linkedEMailAccount);
            }

            if (key == null)
            {
                key = StripUglyBase64(CrmConfig.Crypto.GenerateSalt());
            }

            user.VerificationKey = CrmConfig.Crypto.Hash(key);
            user.VerificationPurpose = purpose;
            user.VerificationKeySent = DateTime.UtcNow;
            user.VerificationStorage = state;

            return key;
        }

        #endregion

        #region SetVerificationKeyForNewUser

        internal async Task<string> SetVerificationKeyForNewUserAsync(CrmUser user)
        {
            var linkedAccount = user.CrmLinkedEMailAccounts.First();
            var verificationKey = SetVerificationKey(user, VerificationKeyPurpose.ChangeEmail, state: linkedAccount.Email);
            await DbContext.SaveAndUpdateUserCacheAsync(user);
            return verificationKey;
        }

        #endregion

        #region SendVerificationKeyMail

        public async Task<SendVerificationKeyMailResult> SendVerificationKeyMailAsync(CrmUser user, string email)
        {
            email = email.RemoveNonStandardWhitespace();

            var existing = user.GetLinkedEmailAccount(email);

            if (existing == null)
            {
                Telemetry.TrackTrace($"'{email}' not found", SeverityLevel.Information);
                return SendVerificationKeyMailResult.EMailNotFound;
            }
            if (existing.IsVerified.Value)
            {
                Telemetry.TrackTrace($"{email} is verified", SeverityLevel.Information);
                return SendVerificationKeyMailResult.AlreadyVerified;
            }

            await ValidateEmail(user, email);

            var key = SetVerificationKey(user, VerificationKeyPurpose.ChangeEmail, state: email);

            await DbContext.SaveAsync(user);
            await EmailService.SendConfirmEmailAddressMail(user, email, key);

            return SendVerificationKeyMailResult.OK;
        }

        #endregion

        #region SendConfirmEmailAddressMailWithSignature

        public async Task<SendVerificationKeyMailResult> SendConfirmEmailAddressMailWithSignature(CrmUser user, string email)
        {
            //CRM_Email_ConfirmEmailAddressMailWithSignature_first_paragraph
            //CRM_Email_ConfirmEmailAddressMailWithSignature_subject

            email = email.RemoveNonStandardWhitespace();

            var existing = user.GetLinkedEmailAccount(email);

            if (existing != null && 
                existing.IsVerified.Value)
            {
                Telemetry.TrackTrace($"{email} is verified", SeverityLevel.Information);
                return SendVerificationKeyMailResult.AlreadyVerified;
            }

            if (!CrmUserAccountValidation.EmailIsValid(email))
            {
                return SendVerificationKeyMailResult.EmailIsNotValid;
            }

            var result = await CrmUserAccountValidation.EmailMustNotAlreadyExistsOthenThanUser(user.Contact, email, DbContext);
            if (result != null)
            {
                return SendVerificationKeyMailResult.EmailAlreadyExists;
            }

            var signature = await SignatureUtility.CreateLinkedEmailAccountSignature(user, email);

            await EmailService.SendConfirmEmailAddressMailWithSignature(user, email, signature);

            return SendVerificationKeyMailResult.OK;
        }

        #endregion

        #region SendVerificationKeyResetPassword

        public async Task<SendVerificationKeyMailResult> SendVerificationKeyResetPasswordAsync(CrmUser user, string email)
        {
            email = email.RemoveNonStandardWhitespace();

            var existing = user.GetLinkedEmailAccount(email);

            if (existing == null)
            {
                Telemetry.TrackDiagnostics($"'{email}' not found ({user.Key})");
                return SendVerificationKeyMailResult.EMailNotFound;
            }

			if (!IsVerificationKeySendAllowed(existing, VerificationKeyPurpose.ResetPassword))
			{
                Telemetry.TrackDiagnostics($"'{email}' already sent ({user.Key})");
                return SendVerificationKeyMailResult.OK;
            }

            await using (var tslock = new TableStorageLock(email))
            {
                if (!await tslock.TryEnter())
                {
                    if (CrmConfig.IsUnittest)
                    {
                        return SendVerificationKeyMailResult.EMailNotFound;
                    }
                    throw new RateLimitException();
                }

                await ValidateEmail(user, email);

                var key = SetVerificationKey(user, VerificationKeyPurpose.ResetPassword, state: email);
                await DbContext.SaveAsync(user);
                await EmailService.SendPasswordResetMail(user, email, key);

                return SendVerificationKeyMailResult.OK;
            }
        }

        #endregion

        #region SendAccountMergingKeyAsync

        public async Task<MergeAccountResult> SendAccountMergingKeyAsync(string loserEmailAddress, string userKey)
        {
            var winner = await GetByKeyAsync(userKey);
            if (winner == null ||
                winner.IsAccountClosed ||
                !winner.IsLoginAllowed ||
                !winner.IsAccountVerified)
            {
                return MergeAccountResult.UserNotFound;
            }

            loserEmailAddress = loserEmailAddress.RemoveNonStandardWhitespace();
            var loser = await DbContext.GetByEmailAsync(loserEmailAddress);
            //#4637
            //Prüfungen IsVerified entfernen.
            if (loser == null ||
                loser.IsAccountClosed)
            {
                return MergeAccountResult.UserNotFound;
            }

            if (winner.Key == loser.Key)
            {
                return MergeAccountResult.ErrorSameUser;
            }

            var email = loser.GetLinkedEmailAccount(loser.Email);
            //#4637
            //Prüfungen IsVerified entfernen.
            if (email == null)
            {
                return MergeAccountResult.UserNotFound;
            }

            var verificationKey = StripUglyBase64(CrmConfig.Crypto.GenerateSalt());
            loser.Contact.MergeAccountVerificationKey = CrmConfig.Crypto.Hash(verificationKey);
            loser.Contact.MergeAccountVerificationKeySent = DateTime.UtcNow;
            await EmailService.SendMergeAccountKeyMailAsync(loser, loserEmailAddress, verificationKey);
            await DbContext.SaveAndUpdateUserCacheAsync(loser);
            return MergeAccountResult.Success;
        }

        #endregion

        #region StripUglyBase64

        string StripUglyBase64(string s)
        {
            if (s == null)
            {
                return s;
            }

            foreach (var ugly in UglyBase64)
            {
                s = s.Replace(ugly, "");
            }
            return s;
        }

        #endregion

        #region TryAddUnverifiedEmailAccountFromOtherAccount

        /// <summary>
        /// Übernimmt die *unverifizierter* Email-Adresse von einem anderen Account.<para></para>
        /// Unverifizierter Account wird deaktiviert<para></para>
        /// Verifizierter Account verliehrt diese Email-Adresse
        /// </summary>
        public async Task<(bool Result, LinkedEmailAccount Added)> TryAddUnverifiedEmailAccountFromOtherAccount(CrmUser user, string email)
        {
            var existingUser = await DbContext.GetByEmailAsync(email);
            var linkedEmailAccountFromOtherUser = existingUser.CrmLinkedEMailAccounts.First(la => la.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
            if (linkedEmailAccountFromOtherUser.IsVerified.GetValueOrDefault())
            {
                return (false, null);
            }
            if (existingUser.Key == user.Key)
            {
                //Verifzierungsmail nochmals senden
                await SendVerificationKeyMailAsync(user, email);
                return (false, null);
            }
            if (existingUser.Contact.IsVerified.GetValueOrDefault())
            {
                if (existingUser.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                {
                    //Darf nie passieren: Verifizierter Account mit *nicht verfizierter* Email
                    return (false, null);
                }
                await RemoveLinkedEMailAccount(existingUser, linkedEmailAccountFromOtherUser, authCheck: false);
            }
            else
            {
                //Alle Projekt-Einladungen dem neuen User zuweisen
                //Die Projekt-Einladungen gingen allen an diese Email-Adresse
                foreach (var projectRole in existingUser.ProjectRoles)
                {
                    projectRole.Contact.Set(user.Contact);
                }
                foreach (var license in existingUser.Licenses)
                {
                    if (license.DataContractOwnerContactKey == existingUser.Key)
                    {
                        license.Owner.Set(user.Contact);
                    }
                    if (license.DataContractEndUserContactKey == existingUser.Key)
                    {
                        license.EndUser.Set(user.Contact);
                    }
                }
                foreach (var orderProcess in await OrderProcess.Get(existingUser.Contact, DbContext))
                {
                    orderProcess.LicenseContact.Set(user.Contact);
                }

                foreach (var la in existingUser.CrmLinkedEMailAccounts)
                {
                    la.Deactivate();
                }

                foreach (var la in existingUser.CrmLinkedAccounts)
                {
                    la.Deactivate();
                }

                //Dies ist ein nicht verifzierter Account. Können wir gefahrlos deaktivieren
                existingUser.Deactivate();
            }

            //Zuerst speichern. Sollte was schiefgehen, ist es halt so, aber wir erzeugen keine Duplikate
            await DbContext.SaveAsync();
            var linkedEmailAccount = user.AddLinkedEMailAccount(email);
            await SendVerificationKeyMailAsync(user, email);
            await DbContext.SaveAsync();
            await CrmUserCache.RemoveAsync(existingUser);
            await CrmUserCache.RemoveAsync(user);
            return (true, linkedEmailAccount);
        }


        #endregion

        #region UpdateExistingLicensesForNewUser

        public async Task UpdateExistingLicensesForNewUserAsync(CrmUser user, bool removeUserFromCache)
        {
            var licenseManager = new LicenseManager(DbContext);
            foreach (var license in user.Licenses)
            {
                if (license.DataContractEndUserContactKey != user.Key)
                {
                    continue;
                }

                var prod = CrmCache.Products.Where(i => i.Key == license.DataContractProductKey).FirstOrDefault();
                if (prod.Value == null)
                {
                    continue;
                }

                if (prod.Value.IsSqlServerProduct)
                {
                    continue;
                }

                if (license.CitaviMajorVersion < 5)
                {
                    continue;
                }

                //#21910
                if (!string.IsNullOrEmpty(license.Crm4Id) &&
                    license.VoucherId != Guid.Empty)
                {
                    continue;
                }

                licenseManager.UpdateLicenseKey(user, license);
            }

            await DbContext.SaveAndUpdateUserCacheAsync(user);
            if (user.Licenses.Any())
            {
                await user.Load(DbContext, attachToContext: true, loadContext: UserLoadContexts.Licenses);
            }

            if (removeUserFromCache)
            {
                await CrmUserCache.RemoveAsync(user);
            }
        }

        #endregion

        #region UpdateAsync

        public Task UpdateAsync(CrmUser user) => DbContext.SaveAsync(user);

        #endregion

        #region UpdateUserImageAsync

        public async Task UpdateUserImageAsync(CrmUser user, byte[] image)
        {
            try
            {
                await DbContext.SetUserImageAsync(user.Contact, image);
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Warning, ExceptionFlow.Eat);
            }
        }
        internal async Task UpdateUserImageAsync(string contactKey, byte[] image)
        {
            var user = await DbContext.GetByKeyAsync(contactKey);
            if (user == null)
            {
                return;
            }

            await UpdateUserImageAsync(user, image);
        }

        #endregion

        #region UpdateUserEmail

        public async Task<bool> UpdateUserEmail(CrmUser user)
        {
            var existing = user.GetLinkedEmailAccount(user.Email);
            if (existing != null)
            {
                if (!existing.IsVerified.Value)
                {
                    await SendVerificationKeyMailAsync(user, user.Email);
                }
                await DbContext.SaveAsync(user);
                return existing.IsVerified.Value;
            }
            else
            {
                await AddEmail(user, user.Email);
                return false;
            }
        }

        #endregion

        #region UpdateLanguage

        internal void UpdateUserLanguage(CrmUser user, LanguageType? lng)
        {
            if (lng == null)
            {
                return;
            }

            if (user.Contact.Language == lng)
            {
                return;
            }

            user.Contact.ChangeLanguage(lng);
        }

        #endregion

        #region ValidateEmail

        public async Task ValidateEmail(CrmUser user, string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("email must not be null");
            }

            if (!CrmUserAccountValidation.EmailIsValid(email))
            {
                var ex = new ArgumentException($"{email} is not valid");
                ex.TreatAsWarning();
                throw ex;
            }

            var result = await CrmUserAccountValidation.EmailMustNotAlreadyExistsOthenThanUser(user.Contact, email, DbContext);
            if (result != null)
            {
                var ex = new ArgumentException(result.ErrorMessage);
                ex.TreatAsWarning();
                throw ex;
            }
        }

        #endregion

        #region VerifyEmailFromKey

        public Task<(VerifyEmailFromKeyResult Result, string verifiedEmail, CrmUser user, bool IsNewUser)> VerifyEmailFromKeyEx(string verificationKey)
            => VerifyEmailFromKeyEx(verificationKey, null, null);
        public async Task<(VerifyEmailFromKeyResult Result, string verifiedEmail, CrmUser user, bool IsNewUser)> VerifyEmailFromKeyEx(string verificationKey, string password, CrmUser user)
        {
            if (String.IsNullOrWhiteSpace(verificationKey))
            {
                var exception = new ArgumentException($"VerificationKey '{verificationKey}' is null");
                Telemetry.TrackTrace(exception.Message, severityLevel: SeverityLevel.Warning);
                return (VerifyEmailFromKeyResult.NotFound, null, user, false);
            }
            if (user == null)
            {
                var vKey = CrmConfig.Crypto.Hash(verificationKey);
                user = await DbContext.GetByVerificationKeyAsync(vKey);
                if (user == null)
                {
                    var exception = new ArgumentException($"VerificationKey '{verificationKey}' not found");
                    Telemetry.TrackTrace(exception.Message, severityLevel: SeverityLevel.Warning);
                    return (VerifyEmailFromKeyResult.NotFound, null, user, false);
                }
            }
            if (!IsVerificationPurposeValid(user, VerificationKeyPurpose.ChangeEmail))
            {
                if (user.VerificationPurpose != VerificationKeyPurpose.ChangeEmail)
                {
                    Telemetry.TrackTrace($"'{verificationKey}' verification purpose invalid: {user.VerificationPurpose}. {nameof(user)}: {user.Key}", severityLevel: SeverityLevel.Warning);
                }
                else if (IsVerificationKeyStale(user))
                {
                    Telemetry.TrackTrace($"'{verificationKey}' verification key stale. {nameof(user)}: {user.Key}", severityLevel: SeverityLevel.Warning);
                }
                return (VerifyEmailFromKeyResult.Expired, null, user, false);
            }

            if (!string.IsNullOrEmpty(password))
            {
                if (user.HasPassword() && !await VerifyPasswordAsync(user, password))
                {
                    Telemetry.TrackTrace($"Invalid {nameof(password)}", severityLevel: SeverityLevel.Warning);
                    return (VerifyEmailFromKeyResult.InvalidPassword, null, user, false);
                }
            }
            var verifiedEmail = user.VerificationStorage;
            var key = user.Key;
            var isNew = !user.IsAccountVerified;
            await VerifyEmailFromKeyPrivateAsync(user);
            user = await DbContext.GetByKeyAsync(key);
            return (VerifyEmailFromKeyResult.OK, verifiedEmail, user, isNew);
        }

        internal async Task VerifyEmailFromKeyPrivateAsync(CrmUser user)
        {
            var newEmail = user.VerificationStorage;

            await ValidateEmail(user, user.VerificationStorage);

            var linkedAccount = user.CrmLinkedEMailAccounts.First(i => string.Equals(i.Email, newEmail, StringComparison.InvariantCultureIgnoreCase));
            linkedAccount.IsVerified = true;
            user.Contact.IsVerified = true;
            user.Contact.IsLoginAllowed = true;

            var existingCrm4User = await DbContext.FetchFirstOrDefault<Contact>(new Query.FetchXml.GetCrm4Contact(newEmail).TransformText());
            if (existingCrm4User != null &&
                existingCrm4User.Key != user.Key)
            {
                await DbContext.SaveAndUpdateUserCacheAsync(user);
                await DbContext.MergeCrm4ContactAsync(existingCrm4User, user);
            }

            await new CampusContractManager(DbContext).AddLicensesFromEmailVerficationAsync(user, newEmail);
            new ProjectManager(DbContext).ConfirmInvitationAfterAccountCreation(user);

            ClearVerificationKey(user);
            linkedAccount.ClearVerifcationData();

            await DbContext.SaveAsync();
            await CrmUserCache.RemoveAsync(user);
        }

        #endregion

        #region ValidatePassword

        void ValidatePassword(CrmUser account, string value)
        {
            // null is allowed (e.g. for external providers)
            if (value == null)
            {
                return;
            }

            if (!account.IsNew() && VerifyHashedPassword(account, value))
            {
                var exception = new ValidationException(MembershipRebootConstants.NewPasswordMustBeDifferent);
                exception.TreatAsWarning();
                Telemetry.TrackException(exception);
            }
        }

        #endregion

        #region VerifyPasswordAsync

        async Task<bool> VerifyPasswordAsync(CrmUser account, string password)
        {
            if (String.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (!account.HasPassword())
            {
                return false;
            }

            try
            {
                if (CheckHasTooManyRecentPasswordFailures(account))
                {
                    return false;
                }

                if (VerifyHashedPassword(account, password))
                {
                    if (account.FailedLoginCount != 0)
                    {
                        account.FailedLoginCount = 0;
                    }
                    return true;
                }
                else
                {
                    RecordInvalidLoginAttempt(account);
                    return false;
                }
            }
            finally
            {
                await DbContext.SaveAsync(account);
            }
        }

        #endregion

        #region VerifyHashedPassword

        internal bool VerifyHashedPassword(CrmUser account, string password)
        {
            if (!account.HasPassword())
            {
                return false;
            }

            return CrmConfig.Crypto.VerifyHashedPassword(account.HashedPassword, password);
        }

        #endregion

        #region IsVerificationKeyValid

        bool IsVerificationKeyValid(CrmUser account, VerificationKeyPurpose purpose, string key)
        {
            if (!IsVerificationPurposeValid(account, purpose))
            {
                return false;
            }

            var result = CrmConfig.Crypto.VerifyHash(key, account.VerificationKey);
            if (!result)
            {
                return false;
            }
            return true;
        }

        #endregion

        public async Task AddLinkAccount(string userId, string issuer, string issuerAssignedId)
        {
            await InitializeAzureB2CManager();
            await _azureB2CManager.AddLinkAccount(userId, issuer, issuerAssignedId);
        }

        public async Task AddLinkEmail(string userId, string email)
        {
            await InitializeAzureB2CManager();
            await _azureB2CManager.AddLinkEmail(userId, email);
        }

        private async Task InitializeAzureB2CManager()
        {
            if (_azureB2CManager == null)
            {
                var graphApiClientId = await AzureRegionResolver.Instance.GetRegionalKeyVaultSecretAsync(KeyVaultSecrets.AzureB2CKeys.GraphApiClientId, string.Empty);
                var tenantId = await AzureRegionResolver.Instance.GetRegionalKeyVaultSecretAsync(KeyVaultSecrets.AzureB2CKeys.TenantId, string.Empty);
                var clientSecret = await AzureRegionResolver.Instance.GetRegionalKeyVaultSecretAsync(KeyVaultSecrets.AzureB2CKeys.GraphApiClientSecret, string.Empty);
                var confidentialClientApplication = Client.ConfidentialClientApplicationBuilder.Create(graphApiClientId)
                                                                                        .WithTenantId(tenantId)
                                                                                        .WithClientSecret(clientSecret)
                                                                                        .Build();
                var authProvider = new ClientCredentialProvider(confidentialClientApplication);
                var client = new GraphServiceClient(authProvider);
                _azureB2CManager = new AzureB2CManager(DbContext, client);
            }
        }

        #endregion
    }
}
