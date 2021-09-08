using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Authorization;
using SwissAcademic.Azure;
using SwissAcademic.Azure.Storage;
using SwissAcademic.Globalization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [DataContract]
    public class CrmUser
        :
        IHasCacheETag
    {
        #region Felder

        internal IHasVerificationData _verificationData;

        #endregion

        #region Konstruktor

        [JsonConstructor]
        internal CrmUser()
        {

        }

        public CrmUser(Contact contact)
        {
            Contact = contact;
        }

        #endregion

        #region Eigenschaften

        #region AccountClosed

        public DateTime? AccountClosed
        {
            get
            {
                return Contact.ClosedDate;
            }
            set
            {
                if (value == null)
                {
                    Contact.ClosedDate = CrmDataTypeConstants.MinDate;
                }
                else
                {
                    Contact.ClosedDate = value.Value;
                }
            }
        }

        #endregion

        #region AllowDeletion

        [DataMember]
        public bool AllowDeletion
        {
            get
            {
                if (Contact.IsKeyContact)
                {
                    return false;
                }

                if (VoucherBlocks.Count > 0)
                {
                    return false;
                }

                if (Licenses.Count == 1 &&
                    ((Licenses[0].DataContractOwnerContactKey == Key &&
                      Licenses[0].DataContractEndUserContactKey == Key) ||
                      Licenses[0].DataContractOwnerContactKey == Key &&
                      string.IsNullOrEmpty(Licenses[0].DataContractEndUserContactKey)))
                {
                    //#19715
                    //Falls man nur eine For Windows-Lizenz besitzt, sollte man ein normaler Nutzer sein und das eigene Konto löschen können
                    return true;
                }

                if (Licenses.Any(i => (Pricing.IsBusinessOrAcacemicPricing(i.DataContractPricingKey) &&
                                       i.DataContractOwnerContactKey == Key &&
                                       !i.IsBetaLicense &&
                                       i.OrderCategory != OrderCategory.Trial  &&
                                       !i.Free)))
                {
                    //Mehrere Kauf-Lizenzen, User ist Owner
                    return false;
                }

                return true;
            }
        }

        #endregion

        #region AllowLargeImports

        [DataMember]
        public bool AllowLargeImports
        {
            get
            {
                return Contact.AllowLargeImports;
            }
        }

        #endregion

        #region CacheETag

        [CacheDataMember]
        public string CacheETag { get; set; }

        #endregion

        #region Created

        public DateTime Created
        {
            get
            {
                return Contact.CreatedOn;
            }
            set
            {
                Contact.CreatedOn = value;
            }
        }

        #endregion

        #region DataCenter

        public DataCenter DataCenter
        {
            get => Contact.DataCenter;
            set => Contact.DataCenter = value;
        }

        #endregion

        #region Email

        public string Email
        {
            get
            {
                return Contact.EMailAddress1;
            }
            set
            {
                Contact.EMailAddress1 = value;

                if (Contact.EntityState == EntityState.Created &&
                   CrmLinkedEMailAccounts != null &&
                   !CrmLinkedEMailAccounts.Any(i => i.Email.Equals(value, StringComparison.InvariantCultureIgnoreCase)))
                {
                    AddLinkedEMailAccount(value);
                }
            }
        }

        #endregion

        #region FailedLoginCount

        public int FailedLoginCount
        {
            get
            {
                return Contact.FailedLoginCount.GetValueOrDefault();
            }
            set
            {
                Contact.FailedLoginCount = value;
            }
        }

        #endregion

        #region FailedPasswordResetCount

        public int FailedPasswordResetCount
        {
            get
            {
                return Contact.FailedPasswordResetCount.GetValueOrDefault();
            }
            set
            {
                Contact.FailedPasswordResetCount = value;
            }
        }

        #endregion

        #region FirstName

        public string FirstName
        {
            get
            {
                return Contact.FirstName;
            }
            set
            {
                Contact.FirstName = value;
            }
        }

        #endregion

        #region HashedPassword

        public string HashedPassword
        {
            get
            {
                return Contact.HashedPassword;
            }
            set
            {
                Contact.HashedPassword = value;
            }
        }

        #endregion

        #region HasUserSettingsWE

        public bool HasUserSettingsWE
        {
            get
            {
                return Contact.HasUserSettingsWE;
            }
            set
            {
                Contact.HasUserSettingsWE = value;
            }
        }

        #endregion

        #region Id

        public Guid Id
        {
            get
            {
                return Contact.Id;
            }
            set
            {
                if (Contact.Id != Guid.Empty)
                {
                    var exception = new NotSupportedException($"{nameof(Contact.Id)} is not Guid.Empty ({Contact.Id})");
                    Telemetry.TrackException(exception);
                }
                Contact.Id = value;
            }
        }

        #endregion

        #region IsAccountVerified

        /// <summary>
        /// Use "Contact.IsVerified" (IsAccountVerified only for crm - lib)
        /// </summary>
        public bool IsAccountVerified
        {
            get
            {
                return Contact.IsVerified.GetValueOrDefault() && (_verificationData == null || _verificationData.IsVerified.Value);
            }
            set
            {
                Contact.IsVerified = value;
                if (_verificationData != null)
                {
                    _verificationData.IsVerified = value;
                }
            }
        }

        #endregion

        #region IsAccountClosed

        public bool IsAccountClosed
        {
            get
            {
                return Contact.ClosedDate > CrmDataTypeConstants.MinDate;
            }
            set
            {
                if (value)
                {
                    Contact.ClosedDate = DateTime.Now;
                }
                else
                {
                    Contact.ClosedDate = CrmDataTypeConstants.MinDate;
                }
            }
        }
        #endregion

		#region IsLoginAllowed

		public bool IsLoginAllowed
        {
            get
            {
                return Contact.IsLoginAllowed.GetValueOrDefault();
            }
            set
            {
                Contact.IsLoginAllowed = value;
            }
        }

        #endregion

        #region IsSasAdmin

        public bool IsSasAdmin
        {
            get { return Contact.IsSasAdmin; }
        }

        #endregion

        #region IsTestUser

        [IgnoreDataMember]
        public bool IsTestUser => Email?.EndsWith(".UnitTest.noemail@citavi.com") == true;

        #endregion

        #region Key

        public string Key
        {
            get
            {
                return Contact.Key;
            }
        }

        #endregion

        #region LastFailedPasswordReset

        public DateTime? LastFailedPasswordReset
        {
            get
            {
                return Contact.LastFailedPasswordReset;
            }
            set
            {
                Contact.LastFailedPasswordReset = value.GetValueOrDefault();
            }
        }

        #endregion

        #region LastName

        public string LastName
        {
            get
            {
                return Contact.LastName;
            }
            set
            {
                Contact.LastName = value;
            }
        }

        #endregion

        #region LastUpdated

        public DateTime LastUpdated
        {
            get
            {
                return Contact.LastModifiedOn.GetValueOrDefault();
            }
            set
            {
                Contact.LastModifiedOn = value;
            }
        }

        #endregion

        #region LastFailedLogin

        public DateTime? LastFailedLogin
        {
            get
            {
                return Contact.LastFailedLogin;
            }
            set
            {
                Contact.LastFailedLogin = value.GetValueOrDefault();
            }
        }

        #endregion

        #region NickName

        public string NickName
        {
            get
            {
                return Contact.NickName;
            }
            set
            {
                Contact.NickName = value;
            }
        }

        #endregion

        #region PasswordChanged

        public DateTime? PasswordChanged
        {
            get
            {
                return Contact.PasswordChanged;
            }
            set
            {
                Contact.PasswordChanged = value.GetValueOrDefault();
            }
        }

        #endregion

        #region RequiresPasswordReset

        public bool RequiresPasswordReset
        {
            get
            {
                return Contact.RequiresPasswordReset.GetValueOrDefault();
            }
            set
            {
                Contact.RequiresPasswordReset = value;
            }
        }

        #endregion

        #region Username

        public string Username
        {
            get
            {
                return Contact.FullName;
            }
        }

        #endregion

        #region VerificationKey

        public string VerificationKey
        {
            get
            {
                if (_verificationData == null)
                {
                    return null;
                }

                return _verificationData.VerificationKey;
            }
            set
            {
                _verificationData.VerificationKey = value;
            }
        }

        #endregion

        #region VerificationKeySent

        public DateTime? VerificationKeySent
        {
            get
            {
                if (_verificationData == null)
                {
                    return null;
                }

                return _verificationData.VerificationKeySent;
            }
            set
            {
                _verificationData.VerificationKeySent = value;
            }
        }

        #endregion

        #region VerificationPurpose

        public VerificationKeyPurpose? VerificationPurpose
        {
            get
            {
                if (_verificationData == null)
                {
                    return null;
                }

                return _verificationData.VerificationPurpose;
            }
            set
            {
                _verificationData.VerificationPurpose = value;
            }
        }

        #endregion

        #region VerificationStorage

        public string VerificationStorage
        {
            get
            {
                if (_verificationData == null)
                {
                    return null;
                }

                return _verificationData.VerificationStorage;
            }
            set
            {
                _verificationData.VerificationStorage = value;
            }
        }

        #endregion

        #endregion

        #region DataMember Entities

        [DataMember]
        public Contact Contact { get; set; }
        [DataMember]
        public List<LinkedAccount> CrmLinkedAccounts { get; set; } = new List<LinkedAccount>();
        [DataMember]
        public List<LinkedEmailAccount> CrmLinkedEMailAccounts { get; set; } = new List<LinkedEmailAccount>();
        [DataMember]
        public List<CitaviLicense> Licenses { get; set; } = new List<CitaviLicense>();
        [DataMember]
        public List<ProjectRole> ProjectRoles { get; set; } = new List<ProjectRole>();
        [DataMember]
        public List<VoucherBlock> VoucherBlocks { get; set; } = new List<VoucherBlock>();

        #endregion

        #region Methoden

        #region AddLicense

        public void AddEndUserLicense(CitaviLicense license)
        {
            if (!Licenses.Any(i => i.Key == license.Key))
            {
                Licenses.Add(license);
            }
            license.EndUser.Set(Contact);
        }

        public void AddOwnerLicense(CitaviLicense license)
        {
            if (!Licenses.Any(i => i.Key == license.Key))
            {
                Licenses.Add(license);
            }
            license.Owner.Set(Contact);
        }

        #endregion

        #region CreateUserSettingsIfNotExistsAsync

        public async Task<bool> CreateUserSettingsIfNotExistsAsync(CrmDbContext context, bool saveChanges = true)
        {
            if (Contact.HasUserSettingsWE)
            {
                return false;
            }

            try
            {
                var table = CrmConfig.AzureStorageResolver.GetUserSettingsCloudTable(this);
                await table.CreateIfNotExistsWithCheckAsync();
                Contact.HasUserSettingsWE = true;
                if (saveChanges)
                {
                    await context.SaveAndUpdateUserCacheAsync(this);
                }
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
            }
            return true;
        }

        #endregion

        #region Deactivate

        /// <summary>
        /// Contact.StatusCode = CrmEntityStatusCode.Inactive.
        /// Es wird kein Save ausgelöst
        /// </summary>
        internal void Deactivate()
        {
            Contact.StatusCode = StatusCode.Inactive;
        }

        #endregion

        #region DeleteUserSettingsAsync

        public async Task DeleteUserSettingsAsync()
        {
            try
            {
                var table = CrmConfig.AzureStorageResolver.GetUserSettingsCloudTable(this);
                if (!await table.ExistsAsync())
                {
                    return;
                }
                await table.DeleteAsync();
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
            }
        }

		#endregion

		#region GetLegacyFreeLicense

        internal CitaviLicense GetLegacyFreeLicense()
		{
            return Licenses.FirstOrDefault(lic => lic.ProductResolved == Product.C6Windows && lic.Free);
		}

        #endregion

        #region HasMissingValues

        public bool HasMissingValues(bool skipEmailCheck = false)
        {
            if (skipEmailCheck)
            {
                return false;
            }

            return string.IsNullOrEmpty(LastName) ||
                   string.IsNullOrEmpty(FirstName) ||
                   string.IsNullOrEmpty(Email);
        }

		#endregion

		#region HasVerifiedEmail

		public bool HasVerifiedEmail()
        {
            return CrmLinkedEMailAccounts.Any(i => i.IsVerified.GetValueOrDefault());
        }

        #endregion

        #region HasLicense

        public bool HasReadLicense(string clientId)
        {
            if (!CrmConfig.IsShopWebAppSubscriptionAvailable)
            {
                return true;
            }

            switch (clientId)
            {
                case ClientIds.Desktop:
                    return HasCitaviWindowsReadLicense();

                case ClientIds.EdgePicker:
                case ClientIds.GoogleChromePicker:
                case ClientIds.FirefoxPicker:
                case ClientIds.WebWordAddIn:
                case ClientIds.GoogleDocsAddIn:
                    return HasCitaviWindowsReadLicense() || HasCitaviWebReadLicense();
            }

            return HasCitaviWebReadLicense();
        }

        public bool HasWriteLicense(string clientId)
		{
			if (!CrmConfig.IsShopWebAppSubscriptionAvailable)
			{
                return true;
			}

			switch (clientId)
			{
                case ClientIds.Desktop:
                    return HasCitaviWindowsWriteLicense();

                case ClientIds.EdgePicker:
                case ClientIds.GoogleChromePicker:
                case ClientIds.FirefoxPicker:
                case ClientIds.WebWordAddIn:
                case ClientIds.GoogleDocsAddIn:
                    return HasCitaviWindowsWriteLicense() || HasCitaviWebWriteLicense();
            }

            return HasCitaviWebWriteLicense();
        }

        public bool HasCitaviWindowsReadLicense()
        {
            return Licenses.Any(lic => lic.DataContractEndUserContactKey == Key &&
                                       (lic.CitaviMajorVersion >= 6 || lic.ProductResolved.CitaviProductCode == ProductCodes.CitaviWebAndWin));
        }

        public bool HasCitaviWindowsWriteLicense()
        {
            return Licenses.Any(lic => lic.DataContractEndUserContactKey == Key &&
                                       (lic.CitaviMajorVersion >= 6 || lic.ProductResolved.CitaviProductCode == ProductCodes.CitaviWebAndWin) &&
                                       !lic.ReadOnly);
        }

        public bool HasCitaviWebReadLicense()
		{
            if (Environment.Build == BuildType.Beta && DateTime.UtcNow < CrmConfig.BetaLicensePeriodEndDate)
            {
                return true;
            }
            return Licenses.Any(lic => lic.IsVerified &&
                                       lic.DataContractEndUserContactKey == Key &&
                                       (lic.ProductResolved.CitaviProductCode == ProductCodes.CitaviWeb ||
                                        lic.ProductResolved.CitaviProductCode == ProductCodes.CitaviWebAndWin));
		}

        public bool HasCitaviWebWriteLicense()
        {
            if (Environment.Build == BuildType.Beta && DateTime.UtcNow < CrmConfig.BetaLicensePeriodEndDate)
            {
                return true;
            }
            return Licenses.Any(lic => lic.IsVerified &&
                                       lic.DataContractEndUserContactKey == Key &&
                                       (lic.ProductResolved.CitaviProductCode == ProductCodes.CitaviWeb ||
                                        lic.ProductResolved.CitaviProductCode == ProductCodes.CitaviWebAndWin) &&
                                       !lic.ReadOnly);
        }

		#endregion

		#region GetImage

		public Task<byte[]> GetImageAsync()
        {
            return CrmUserImageCache.GetAsync(Key);
        }

		#endregion

		#region HasLicense

		public bool HasLicense(Product product)
        {
            return Licenses.Any(p => p.DataContractProductKey == product.Key);
        }

        #endregion

        #region HasPassword

        public bool HasPassword()
        {
            return !String.IsNullOrWhiteSpace(HashedPassword);
        }

        #endregion

        #region IsNew

        public bool IsNew()
        {
            return Contact.LastLogin == null && Contact.LastLoginCitaviWeb != null;
        }

        #endregion

        #region GetLinkedEmailAccount

        public LinkedEmailAccount GetLinkedEmailAccount(string email)
		{
            var linkedEmailAccount = CrmLinkedEMailAccounts.FirstOrDefault(la => la.IsEqual(email));
            if(linkedEmailAccount == null)
			{
                //Bsp: LinkedEmailAccount not found, but user is not null: estefanós.xyz @xyz.de, / estefanos.xyz@xyz.de')
                email = email.RemoveAccents();
                linkedEmailAccount = CrmLinkedEMailAccounts.FirstOrDefault(la => la.IsEqual(email));
            }
            return linkedEmailAccount;
		}

        #endregion

        #region GetOrganizationSettings

        public async Task<IEnumerable<OrganizationSetting>> GetOrganizationSettings(CrmDbContext context)
        {
            var licenseQuery = new Query.FetchXml.GetUserOrganizationSettings(Key).TransformText();
            var set = new CrmSet(await context.Fetch(FetchXmlExpression.Create<CitaviLicense>(licenseQuery), true));
            return set.OrganizationSettings;
        }

        #endregion

        #region GetPrincipal

        public IPrincipal Principal => GetPrincipal();
        ClaimsPrincipal Identity;
        ClaimsPrincipal GetPrincipal()
        {
            if (Identity != null)
            {
                return Identity;
            }

            var claims = new List<Claim>() { new Claim(CitaviClaimTypes.ContactKey, Key) };
            if (Contact != null && !string.IsNullOrEmpty(Contact.FullName))
            {
                claims.Add(new Claim(ClaimTypes.Name, Contact.FullName));
                claims.Add(new Claim(nameof(Contact.HasUserSettingsWE), Contact.HasUserSettingsWE.ToString()));
                claims.Add(new Claim(CitaviClaimTypes.DataCenter, Contact.DataCenter.ToString()));
                claims.Add(new Claim(CitaviClaimTypes.DataCenterShortName, AzureRegionResolver.Instance.GetShortName(Contact.DataCenter)));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "CitaviContactIdentity");
            Identity = new ClaimsPrincipal(claimsIdentity);
            return Identity;
        }

        #endregion

        #region LinkedAccounts

        internal void AddLinkedAccount(LinkedAccount item)
        {
            if (CrmLinkedAccounts.Any(i => string.Equals(i.IdentityProviderId, item.IdentityProviderId, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            if (string.IsNullOrEmpty(item.Name))
            {
                item.Name = CrmIdentityProviders.GetDisplayName(item.IdentityProviderId);
            }

            Contact.LinkedAccounts.Add(item);
            CrmLinkedAccounts.Add(item);
        }

        public LinkedEmailAccount AddLinkedEMailAccount(string email, bool isVerified = false)
        {
            email = email.RemoveNonStandardWhitespace();
            if (CrmLinkedEMailAccounts.Any(i => string.Equals(i.Email, email, StringComparison.InvariantCultureIgnoreCase)))
            {
                return null;
            }

            var context = Contact._context;
            var domain = EmailDomain.Parse(email);
            var linkedAccount = context.Create<LinkedEmailAccount>();
            linkedAccount.EmailDomain = domain;
            linkedAccount.Email = email;
            linkedAccount.IsVerified = isVerified;
            if (!isVerified)
            {
                linkedAccount.VerificationPurpose = VerificationKeyPurpose.ChangeEmail;
                linkedAccount.VerificationStorage = email;
            }

            Contact.LinkedEMailAccounts.Add(linkedAccount);
            CrmLinkedEMailAccounts.Add(linkedAccount);

            if (string.IsNullOrEmpty(Email))
            {
                Contact.EmailDomain = domain;
                Email = linkedAccount.Email;
            }
            if (CrmLinkedEMailAccounts.Count == 1)
            {
                Contact.EmailDomain = domain;
            }

            return linkedAccount;
        }

        internal bool CanRemoveLinkedEmailAddress(string email)
        {
            var existing = CrmLinkedEMailAccounts.First(i => string.Equals(email, i.Email, StringComparison.InvariantCultureIgnoreCase));
            if (existing.IsVerified.GetValueOrDefault())
            {
                var existingOther = CrmLinkedEMailAccounts.Any(i => !string.Equals(email, i.Email, StringComparison.InvariantCultureIgnoreCase) && i.IsVerified.GetValueOrDefault());
                if (!existingOther)
                {
                    return false;
                }
            }

            if (CrmLinkedEMailAccounts.Count == 1 && CrmLinkedAccounts.Count == 0)
            {
                return false;
            }

            return true;
        }

        internal void RemoveLinkedAccount(LinkedAccount item)
        {
            var linkedAccount = CrmLinkedAccounts.First(i => i.Key == item.Key);
            Contact.LinkedAccounts.Remove(linkedAccount);
            CrmLinkedAccounts.Remove(linkedAccount);
        }

        /// <summary>
        /// Die LAs werden nur aus der Collection geschmissen.
        /// Die müssen ev. zusätzlich:
        /// a.) gelöscht werden im CRM
        /// b.) die Relation aufgehoben werden
        /// </summary>
        internal void RemoveLinkedEMailAccount(LinkedEmailAccount linked)
        {
            CrmLinkedEMailAccounts.Remove(linked);
            if (linked == _verificationData)
            {
                _verificationData = null;
            }
        }

        #endregion

        #region Load

        internal async Task Load(CrmDbContext context, bool attachToContext, UserLoadContexts loadContext = UserLoadContexts.All)
        {
            var queries = new List<FetchXmlExpression>();
            if (loadContext.HasFlag(UserLoadContexts.Licenses))
            {
                queries.Add(FetchXmlExpression.Create<CitaviLicense>(new Query.FetchXml.GetUserLicenses(Id).TransformText()));
            }
            if (loadContext.HasFlag(UserLoadContexts.LinkedAccounts))
            {
                queries.Add(FetchXmlExpression.Create<Contact>(new Query.FetchXml.GetUserLinkedAccounts(Id).TransformText()));
            }
            if (loadContext.HasFlag(UserLoadContexts.LinkedEmailAccounts))
            {
                queries.Add(FetchXmlExpression.Create<Contact>(new Query.FetchXml.GetUserLinkedEmailAccounts(Id).TransformText()));
            }
            if (loadContext.HasFlag(UserLoadContexts.VoucherBlocks))
            {
                queries.Add(FetchXmlExpression.Create<VoucherBlock>(new Query.FetchXml.GetUserVoucherBlocks(Id).TransformText()));
            }
            if (loadContext.HasFlag(UserLoadContexts.ProjectRoles))
            {
                queries.Add(FetchXmlExpression.Create<Contact>(new Query.FetchXml.GetUserProjectRoles(Id).TransformText()));
            }
            var result = await context.FetchMultiple(queries.ToArray());
            var set = new CrmSet(result);

            #region Licenses

            if (loadContext.HasFlag(UserLoadContexts.Licenses))
            {
                Licenses = set.Licenses == null ? new List<CitaviLicense>() :
                                                 set.Licenses.Where(i => i.StatusCode == StatusCode.Active).OrderBy(l => l.OrderDate).ToList();

                foreach (var license in Licenses)
                {
                    if (!license.IsCampusLicense)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(license.DataContractCampusContractKey))
                    {
                        //Das sind "CreatePreviousVersionLizenzen" von Campusverträgen. Diese haben keinen Campusvertrag.
                        var lic = Licenses.Where(i => i.OrganizationName == license.OrganizationName).FirstOrDefault();
                        if (lic != null)
                        {
                            lic.DataContractCampusContractInfoWebsite = lic.DataContractCampusContractInfoWebsite;
                            license.DataContractCampusContractKey = lic.DataContractCampusContractKey;
                        }
                        continue;
                    }
                }

                if (!string.IsNullOrEmpty(CrmConfig.BetaLicenseProductCode) &&
                    !CrmConfig.IsUnittest &&
                    !Licenses.Any(license => license.CitaviMajorVersion == CrmConfig.BetaLicenseProduct?.CitaviMajorVersion))
                {
                    var betaLicense = CitaviLicense.GetBetaLicense(Contact);
                    if (betaLicense != null)
                    {
                        Licenses.Add(betaLicense);
                    }
                }
                if (attachToContext)
                {
                    context.Attach(Licenses);
                }
            }

            #endregion

            #region LinkedAccount

            if (loadContext.HasFlag(UserLoadContexts.LinkedAccounts))
            {
                CrmLinkedAccounts = set.CrmLinkedAccounts == null ? new List<LinkedAccount>() : set.CrmLinkedAccounts.ToList();
                if (attachToContext)
                {
                    context.Attach(CrmLinkedAccounts);
                }
            }

            #endregion

            #region LinkedEmailAccount

            if (loadContext.HasFlag(UserLoadContexts.LinkedEmailAccounts))
            {
                CrmLinkedEMailAccounts = set.CrmLinkedEMailAccounts == null ? new List<LinkedEmailAccount>() : set.CrmLinkedEMailAccounts.ToList();
                CrmLinkedEMailAccounts.Sort((a, b) => a.CreatedOn.CompareTo(b.CreatedOn));
                if (attachToContext)
                {
                    context.Attach(CrmLinkedEMailAccounts);
                }
            }

            #endregion

            #region ProjectRoles

            if (loadContext.HasFlag(UserLoadContexts.ProjectRoles))
            {
                ProjectRoles = set.ProjectRoles == null ? new List<ProjectRole>() : set.ProjectRoles.ToList();
                ProjectRoles.RemoveAll(projectRole => projectRole.DataContractProjectDeletedOn != null);
                if (attachToContext)
                {
                    context.Attach(ProjectRoles);
                }
            }

            #endregion

            #region VoucherBlocks

            if (loadContext.HasFlag(UserLoadContexts.VoucherBlocks))
            {
                VoucherBlocks = set.VoucherBlocks == null ? new List<VoucherBlock>() : set.VoucherBlocks.Where(i => i.StatusCode == StatusCode.Active).OrderBy(l => l.BlockNumber).GroupBy(c => c.Key, (key, c) => c.FirstOrDefault()).ToList();
                VoucherBlocks.RemoveAll(i => i.CampusContractVoucher && string.IsNullOrEmpty(i.DataContractCampusContractKey));

                if (attachToContext)
                {
                    context.Attach(VoucherBlocks);
                }
            }

            #endregion
        }

        #endregion

        #region UpdateLastLoginInfoOnLinkedEntities

        internal void UpdateLastLoginInfoOnLinkedEntities(ActiveLoginAccountType loginType, string name)
        {
            var lastLogin = DateTime.UtcNow;

            name = name.RemoveNonStandardWhitespace();

            switch (loginType)
            {
                case ActiveLoginAccountType.EMail:
                    {
                        var linkedEMailAccount = (from l in CrmLinkedEMailAccounts
                                                  where l.Email.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                                                  select l).FirstOrDefault();
                        if (linkedEMailAccount == null)
                        {
                            return;
                        }

                        linkedEMailAccount.LastLogin = lastLogin;
                    }
                    break;

                case ActiveLoginAccountType.IdentityProvider:
                    {
                        var linkedAccount = (from l in CrmLinkedAccounts
                                             where string.Equals(l.IdentityProviderId, name, StringComparison.InvariantCultureIgnoreCase)
                                             select l).FirstOrDefault();
                        if (linkedAccount == null)
                        {
                            return;
                        }

                        linkedAccount.LastLogin = lastLogin;
                    }
                    break;
            }
        }

        #endregion

        #region ProjectRole

        internal void RemoveProjectRole(ProjectRole existing)
        {
            var toRemove = ProjectRoles.FirstOrDefault(i => i.Id == existing.Id);
            ProjectRoles.Remove(toRemove);
            Contact.ProjectRoles.Remove(toRemove);
        }

        #endregion

        #region SetBounces

        public void SetBounces(string email, string bounceReason)
        {
            var linkedEMailAccount = GetLinkedEmailAccount(email);
            if (linkedEMailAccount == null)
            {
                Telemetry.TrackTrace($"ResetBounces LinkedEmailAccount not found: {email} | {Key}");
                return;
            }
            linkedEMailAccount.BounceStatus = 1;
            linkedEMailAccount.LastBounceReason = bounceReason;
            Contact.SoftBounceCounter = 1;
        }

        #endregion

        #region ResetBounces

        public async Task ResetBouncesIfExists(string email, CrmDbContext context)
		{
            if (await EmailClient.DeleteBounces(email))
            {
                var linkedEMailAccount = GetLinkedEmailAccount(email);
                if (linkedEMailAccount == null)
                {
                    Telemetry.TrackDiagnostics($"ResetBounces LinkedEmailAccount not found: {email} | {Key}");
                    return;
                }

                linkedEMailAccount.BounceStatus = 0;
                linkedEMailAccount.LastBounceReason = string.Empty;
                Contact.SoftBounceCounter = 0;

                await context.SaveAsync();
            }
        }

		#endregion

		#region RemoveAndDeactivteLicense

		public void RemoveAndDeactivteLicense(CitaviLicense license)
        {
            license.Deactivate();
            Licenses.Remove(license);
        }

        #endregion

        #region SetLastLogin

        public bool SetLastLogin(string clientId, DateTime? dateTime = null) => Contact.SetLastLogin(clientId, dateTime);

		#endregion

		#region SetVerificationData

		internal void SetVerificationData(IHasVerificationData verificationData)
        {
            _verificationData = verificationData;
        }

        #endregion

        #region ToString

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (Contact != null)
            {
                return Contact.FullName;
            }

            return base.ToString();
        }

        #endregion

        #endregion

        #region Operators

        public static implicit operator ClaimsPrincipal(CrmUser crmUser)
         => crmUser.GetPrincipal();

        #endregion
    }
}
