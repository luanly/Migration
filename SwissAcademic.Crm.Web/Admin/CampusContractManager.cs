using Aspose.Words.Drawing;
using Microsoft.AspNetCore.Authentication;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class CampusContractManager
    {
        #region Konstruktor

        public CampusContractManager(CrmDbContext dbContext)
        {
            DbContext = dbContext;
            LicenseManager = new LicenseManager(DbContext);
            VoucherManager = new VoucherManager(DbContext);
        }

        #endregion

        #region Eigenschaften

        CrmDbContext DbContext { get; set; }
        LicenseManager LicenseManager { get; set; }
        VoucherManager VoucherManager { get; set; }

        #endregion

        #region Methoden

        #region AddLicenses

        public IEnumerable<CitaviLicense> AddLicenses(CrmUser user, CampusContract campusContract, string organizationName, CampusGroup? campusGroup = null)
        {
            var added = new List<CitaviLicense>();

            if (campusContract == null)
            {
                return added;
            }

            if (campusContract.NewContractAvailable)
            {
                return added;
            }

            if (string.IsNullOrEmpty(organizationName))
            {
                organizationName = campusContract.OrganizationName;
            }
            if (string.IsNullOrEmpty(organizationName))
            {
                Telemetry.TrackTrace($"CampusContract AddLicense: OrganizationName is null. ContractNumber: {campusContract.ContractNumber}", SeverityLevel.Warning);
                return added;
            }

            foreach (var product in campusContract.ProductsResolved)
            {
                if (user.Licenses.Any(lic => lic.DataContractCampusContractKey == campusContract.Key &&
                                             lic.DataContractProductKey == product.Key))
                {
                    continue;
                }

                if (!product.IsCampusContractProduct)
                {
                    //Mail von 22.02.2019 HHS:
                    //SQLProdukte werden nicht automatisch vergeben
                    //Nur via CRM UI oder via CC - Verlängerung
                    //Account macht hier nichts
                    continue;
                }

                var license = LicenseManager.CreateLicenseWithCampusContract(user.Contact, campusContract, product, organizationName);
                user.AddEndUserLicense(license);
                user.AddOwnerLicense(license);
                if (user.Contact.CampusBenefitEligibility == CampusBenefitEligibilityType.NotApplicable ||
                    !user.Contact.CampusBenefitEligibility.HasValue)
                {
                    //Der Kunde hatte nie eine Campuslizenz -> Neu Anspruch auf 50% Benefit Product
                    user.Contact.CampusBenefitEligibility = CampusBenefitEligibilityType.Eligible;
                }
                license.IsVerified = true;
                if (campusGroup != null &&
                    campusGroup.HasValue)
                {
                    license.CampusGroup = campusGroup;
                }
                added.Add(license);
            }

            return added;
        }

        #endregion

        #region AddOrUpdateLicensesFromShibbolethClaims

        internal async Task AddOrUpdateLicensesFromShibbolethClaimsAsync(CrmUser user, IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                return;
            }


            var providerId = claims.GetClaimSave(SAML2ClaimTypes.ShibbolethIssuer);
            if (providerId == null)
            {
                return;
            }

            var resetCloudSpace = false;

            var contracts = CrmCache.CampusContracts.Where(i => string.Equals(i.ShibbolethEntityId, providerId.Value, StringComparison.InvariantCultureIgnoreCase)).ToList();
            var deactiveLicenses = false;
            foreach (var campusContract in contracts.Where(cc => !cc.NewContractAvailable))
            {
                var affiliations = SAML2ClaimTypes.ParseAffiliationClaims(claims);
                var campusGroup = affiliations.Any(i => i == PersonAffiliationType.Student) ? CampusGroup.Students : CampusGroup.Faculty;
                if (Environment.Build == BuildType.Alpha)
                {
                    if (campusContract.ShibbolethPersonAffiliation == PersonAffiliationType.All &&
                        !affiliations.Any())
                    {
                        //Der Campusvertrag unterstützt alle Affiliations
                        //Der ShibbolethProvider liefert aber keine Affiliations
                        //Wir fügen eine Affiliation hinzu, sodass der Kunde eine Lizenz bekommt.
                        affiliations = new List<PersonAffiliationType> { PersonAffiliationType.Student };
                    }
                }

                void addShibbolethLicense()
                {
                    deactiveLicenses = true;
                    foreach (var lic in AddLicenses(user, campusContract, campusContract.OrganizationName, campusGroup))
                    {
                        lic.IsVerified = true;
                        lic.DataContractEndUserIsVerified = true;
						if (lic.CitaviSpaceInMB.HasValue &&
                            lic.CitaviSpaceInMB.Value > 0)
						{
                            resetCloudSpace = true;
                        }
                    }
                };

                var shibOk = false;
                foreach (var affiliation in affiliations)
                {
                    if (affiliation == PersonAffiliationType.None)
                    {
                        continue;
                    }

                    if (!campusContract.ShibbolethPersonAffiliation.HasFlag(affiliation))
                    {
                        continue;
                    }

                    addShibbolethLicense();
                    shibOk = true;
                    break;
                }

                if (!string.IsNullOrEmpty(campusContract.ShibbolethPersonEntitlement))
                {
                    var personEntitlements = claims.Where(c => c.Type == SAML2ClaimTypes.PersonEntitlement).ToList();
                    foreach (var personEntitlement in personEntitlements)
                    {
                        if (string.Equals(personEntitlement.Value, campusContract.ShibbolethPersonEntitlement, StringComparison.InvariantCultureIgnoreCase))
                        {
                            addShibbolethLicense();
                            shibOk = true;
                            break;
                        }
                    }
                }



                if (shibOk)
                {

                    foreach (var existing in user.Licenses.GetCampusContractLicenses(campusContract).ToList())
                    {
                        //Kunde hat die Lizenz schon, wir prüfen ob es sich um eine "nicht verifizierte Lizenz" handelt
                        if (!existing.IsVerified)
                        {
                            existing.IsVerified = true;
                            existing.DataContractEndUserIsVerified = true;
                        }
                        if (existing.CitaviSpaceInMB.HasValue && 
                            existing.CitaviSpaceInMB.Value > 0)
                        {
                            resetCloudSpace = true;
                        }
                    }
                }
            }

            if (deactiveLicenses)
            {
                //Alter Vertrag, lizenzen deaktivieren, wenn wir vom neuen Vertrag Lizenzen hinzufügen konnten -> #2659
                foreach (var campusContract in contracts.Where(cc => cc.NewContractAvailable))
                {
                    var deactived = DeactiveLicenses(user, campusContract);
                    if (deactived.Any(d => d.CitaviSpaceInMB.HasValue && d.CitaviSpaceInMB.Value > 0))
                    {
                        resetCloudSpace = true;
                    }
                }
            }

            var la = user.CrmLinkedAccounts.FirstOrDefault(l => l.IdentityProviderId == providerId.Value);
            la?.UpdateShibbolethProperties(claims);

			if (resetCloudSpace)
			{
                await CitaviSpaceCache.RemoveAsync(user.Key);
            }
        }

        #endregion

        #region AddLicensesFromEmailVerfication

        internal async Task<bool> AddLicensesFromEmailVerficationAsync(CrmUser user, string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            email = email.RemoveNonStandardWhitespace();

            var linkedEMailAccount = user.GetLinkedEmailAccount(email);
            if (linkedEMailAccount == null)
            {
                Telemetry.TrackTrace("No linked email account found", property1:(nameof(email), email));
                return false;
            }

            if (!linkedEMailAccount.IsVerified.Value)
            {
                Telemetry.TrackTrace("linked email account is not verified", property1:(nameof(email), email));
                return false;
            }

            var campusContracts = new Dictionary<CampusContract, string>();
            var resetCitaviSpaceCache = false;

            #region IP

            if (!string.IsNullOrEmpty(linkedEMailAccount.VerificationIPAddress))
            {
                var ipCampusContracts = (from contract in CrmCache.CampusContracts
                                         where (contract.VerifyMaIP || contract.VerifyStIP)
                                         let ipRange = contract.IPRangesResolved.FirstOrDefault(i => i.IsInRange(linkedEMailAccount.VerificationIPAddress))
                                         where ipRange != null
                                         select new
                                         {
                                             CampusContract = contract,
                                             IPRange = ipRange
                                         }).ToList();

                foreach (var cc in ipCampusContracts)
                {
                    var campusContract = cc.CampusContract;
                    if (campusContracts.ContainsKey(campusContract))
                    {
                        continue;
                    }

                    if (campusContract.NewContractAvailable)
                    {
                        //Entfernen von alten Campus-Lizenz (für den neuen wird die Lizenz hinzugefügt)
                        var deactivated = DeactiveLicenses(user, campusContract);
                        if (deactivated.Any(d => d.CitaviSpaceInMB.HasValue && d.CitaviSpaceInMB.Value > 0))
                        {
                            resetCitaviSpaceCache = true;
                        }
                        continue;
                    }

                    var ok = true;
                    if (linkedEMailAccount.VerificationCampusGroup.HasValue)
                    {
                        ok = (campusContract.VerifyMaIP && linkedEMailAccount.VerificationCampusGroup == CampusGroup.Faculty) ||
                             (campusContract.VerifyStIP && linkedEMailAccount.VerificationCampusGroup == CampusGroup.Students);
                    }
                    if (!ok)
                    {
                        Telemetry.TrackTrace($"AddLicensesFromIPVerfication: IP ok, but {linkedEMailAccount.VerificationCampusGroup} not found.");
                    }
                    else
                    {
                        campusContracts.Add(campusContract, cc.IPRange.OrganizationName);
                    }
                }
            }

            #endregion

            #region Voucher

            if (!string.IsNullOrEmpty(linkedEMailAccount.VerificationVocherCode))
            {
                var result = await VoucherManager.RedeemAsync(user, linkedEMailAccount.VerificationVocherCode);
                if (result.Status == VoucherRedeemResultType.Success)
                {
                    var lic = user.Licenses.FirstOrDefault(i => i.Key == result.LicenseKey);
                    if (lic != null &&
                        linkedEMailAccount.VerificationCampusGroup != null)
                    {
                        lic.CampusGroup = linkedEMailAccount.VerificationCampusGroup;
                        await DbContext.SaveAndUpdateUserCacheAsync(user);
                    }
                }
                else
                {
                    Telemetry.TrackTrace($"AddLicensesFromEmailVerfication: {linkedEMailAccount.VerificationVocherCode} not ok: {result.Status}.");
                }
            }

            #endregion

            #region Email

            var emailCampusContracts = (from contract in CrmCache.CampusContracts
                                        where (contract.VerifyMaEmail || contract.VerifyStEmail) &&
                                              //string.IsNullOrEmpty(contract.ShibbolethEntityId) && //4.10.2016, Neu wird im CRM auf "Nein" gestellt
                                              contract.EmailsDomainsResolved != null
                                        let emailDomain = contract.EmailsDomainsResolved.FirstOrDefault(i => i.IsMatch(email))
                                        where emailDomain != null
                                        select new
                                        {
                                            CampusContract = contract,
                                            EmailDomain = emailDomain
                                        }).ToList();


            if (emailCampusContracts.Any())
            {
                foreach (var cc in emailCampusContracts)
                {
                    var campusContract = cc.CampusContract;
                    if (campusContract.NewContractAvailable)
                    {
                        //Entfernen von alten Campus-Lizenzen (für den neuen wird die Lizenz hinzugefügt)
                        var deactivated = DeactiveLicenses(user, campusContract);
                        if (deactivated.Any(d => d.CitaviSpaceInMB.HasValue && d.CitaviSpaceInMB.Value > 0))
                        {
                            resetCitaviSpaceCache = true;
                        }
                        continue;
                    }

                    var ok = true;
                    if (linkedEMailAccount.VerificationCampusGroup.HasValue)
                    {
                        ok = (campusContract.VerifyMaEmail && linkedEMailAccount.VerificationCampusGroup == CampusGroup.Faculty) ||
                             (campusContract.VerifyStEmail && linkedEMailAccount.VerificationCampusGroup == CampusGroup.Students);
                    }
                    if (!ok)
                    {
                        Telemetry.TrackTrace($"AddLicensesFromEmailVerfication: Email ok, but {linkedEMailAccount.VerificationCampusGroup} not found.");
                    }
                    else
                    {
                        if (campusContracts.Any(i => i.Key.Key == campusContract.Key))
                        {
                            Telemetry.TrackTrace($"AddLicensesFromEmailVerfication: Email ok. campusContract already added");
                        }
                        else
                        {
                            campusContracts.Add(campusContract, cc.EmailDomain.OrganizationName);
                        }
                    }
                }

                foreach (var lic in user.Licenses)
                {
                    if (lic.IsVerified)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(lic.DataContractCampusContractKey))
                    {
                        continue;
                    }

                    var campusContract = CrmCache.CampusContracts.Where(i => i.Key == lic.DataContractCampusContractKey).FirstOrDefault();
                    if (campusContract == null)
                    {
                        continue;
                    }

                    var ok = true;
                    if (linkedEMailAccount.VerificationCampusGroup.HasValue)
                    {
                        ok = (campusContract.VerifyMaEmail && linkedEMailAccount.VerificationCampusGroup == CampusGroup.Faculty) ||
                             (campusContract.VerifyStEmail && linkedEMailAccount.VerificationCampusGroup == CampusGroup.Students);
                    }
                    if (!ok)
                    {
                        continue;
                    }

                    if (campusContract.EmailsDomainsResolved == null)
                    {
                        continue;
                    }

                    if (campusContract.EmailsDomainsResolved.Any(i => i.IsMatch(email)))
                    {
                        //Lizenz welche "vor" der Verifizierung bereits beim Kontakt war. -> Via CRM, Campusvertragsverlängerung
                        //Wir verifizieren die Lizenz
                        lic.IsVerified = true;
                    }
                }
            }

            #endregion

            foreach (var campusContractItem in campusContracts)
            {
                AddLicenses(user, campusContractItem.Key, campusContractItem.Value, linkedEMailAccount.VerificationCampusGroup);
            }

			if (resetCitaviSpaceCache)
			{
                await CitaviSpaceCache.RemoveAsync(user.Key);
			}

            return true;
        }

        #endregion

        #region AddLicensesFromIPVerification

        public async Task<IEnumerable<CitaviLicense>> AddLicensesFromIPVerification(CrmUser user, string ipAddress)
        {
            var resetCitaviSpaceCache = false;
            var added = new List<CitaviLicense>();

            var ipCampusContracts = from contract in CrmCache.CampusContracts
                                    where contract.IsIPContract
                                    let ipRange = contract.IPRangesResolved.FirstOrDefault(i => i.IsInRange(ipAddress))
                                    where ipRange != null
                                    select new
                                    {
                                        CampusContract = contract,
                                        IPRange = ipRange
                                    };

            //Alle IP-Campusverträge vom User
            foreach (var cc in ipCampusContracts.ToList())
            {
                var campusContract = cc.CampusContract;
                if (campusContract.NewContractAvailable)
                {
                    //Entfernen von alten Campus-Lizenz (für den neuen wird die Lizenz hinzugefügt)
                    var deactivated = DeactiveLicenses(user, campusContract);
                    if (deactivated.Any(d => d.CitaviSpaceInMB.HasValue && d.CitaviSpaceInMB.Value > 0))
                    {
                        resetCitaviSpaceCache = true;
                    }
                }
                else
                {
                    //Campuslizenz verfügbar, alle Lizenzen erstellen

                    foreach (var lic in AddLicenses(user, campusContract, cc.IPRange.OrganizationName))
                    {
                        if (lic != null)
                        {
                            lic.DataContractEndUserIsVerified = true;
                            if(lic.CitaviSpaceInMB.HasValue && lic.CitaviSpaceInMB.Value > 0)
							{
                                resetCitaviSpaceCache = true;
							}
                            added.Add(lic);
                        }
                    }
                    //Alle Lizenzen von diesem Campusvertrag verifizieren
                    foreach (var license in user.Licenses)
                    {
                        if (license.DataContractCampusContractKey != campusContract.Key)
                        {
                            continue;
                        }

                        if (!license.IsVerified)
                        {
                            license.IsVerified = true;
                            license.DataContractEndUserIsVerified = true;
                        }

                        if (string.IsNullOrEmpty(license.OrganizationName))
                        {
                            license.OrganizationName = string.IsNullOrEmpty(cc.IPRange.OrganizationName) ? cc.CampusContract.OrganizationName : cc.IPRange.OrganizationName;
                        }
                    }
                }
            }

			if (resetCitaviSpaceCache)
			{
                await CitaviSpaceCache.RemoveAsync(user.Key);
			}

            return added;
        }

        #endregion

        #region DeactiveLicenses

        public IEnumerable<CitaviLicense> DeactiveLicenses(CrmUser user, CampusContract campusContract)
        {
            var deactivated = new List<CitaviLicense>();

            foreach (var license in user.Licenses.GetCampusContractLicenses(campusContract).ToList())
            {
                user.RemoveAndDeactivteLicense(license);
                deactivated.Add(license);
            }

            return deactivated;
        }

        #endregion

        #region GetCampusLicenseVerificationInfo

        public async Task<CampusLicenseVerificationInfo> GetCampusLicenseVerificationInfo(CrmUser user, string licenseKey)
        {
            var license = user.Licenses.FirstOrDefault(i => i.Key == licenseKey);
            if (license == null)
            {
                Telemetry.TrackTrace($"License is null: {licenseKey}", severityLevel: SeverityLevel.Warning);
                return null;
            }

            var campusContract = CrmCache.CampusContracts.FirstOrDefault(i => i.Key == license.DataContractCampusContractKey);
            if (campusContract == null)
            {
                await CrmCache.ResetCampusContractsCache(throwOnError: false, campusContractKey: license.DataContractCampusContractKey);
                campusContract = CrmCache.CampusContracts.FirstOrDefault(i => i.Key == license.DataContractCampusContractKey);
                if(campusContract == null)
                {
                    throw new NotSupportedException($"CampusContract is null. LicenseKey: {licenseKey}. CC: {license.DataContractCampusContractKey}");
                }
            }

            return GetCampusLicenseVerificationInfo(user, campusContract);
        }

        internal CampusLicenseVerificationInfo GetCampusLicenseVerificationInfo(CrmUser user, CampusContract campusContract)
        {
            var result = new CampusLicenseVerificationInfo();

            result.HighSchool = campusContract.DataContractAccountName;

            if (!string.IsNullOrEmpty(campusContract.ShibbolethEntityId))
            {
                result.ShibbolethEntityId = campusContract.ShibbolethEntityId;
                result.ShibbolethVerification = true;
                return result;
            }

            result.IPVerification = campusContract.VerifyMaIP || campusContract.VerifyStIP;
            result.VoucherVerification = campusContract.VerifyMaVoucher || campusContract.VerifyStVoucher;

            if (campusContract.VerifyMaEmail || campusContract.VerifyStEmail)
            {
                var email = from emailAccount in user.CrmLinkedEMailAccounts
                            where emailAccount.IsVerified.GetValueOrDefault() &&
                                  campusContract.EmailsDomainsResolved.Any(i => i.IsMatch(emailAccount.Email))
                            select emailAccount;

                result.EmailVerification = email.Any();
                if (result.EmailVerification)
                {
                    result.EmailVerificationEmailAddress = email.First().Email;
                }
                else
                {
                    result.EmailVerificationEmailDomains.AddRange(campusContract.EmailsDomainsResolved.Select(d => d.Email_Domain_Or_Campus_Name));
                }
            }
            else
            {
                result.EmailVerification = false;
            }

            return result;
        }

        #endregion

        #region HasShibbolethClaimsButNotRequiredAffiliations

        public static bool HasShibbolethClaimsButNotRequiredAffiliations(AuthenticateResult result)
		{
            try
            {
                if (result?.Principal?.Claims == null)
                {
                    return false;
                }

				if (!result.Succeeded)
				{
                    return false;
				}

                var claims = result.Principal.Claims;
                var shibbolethIssuer = claims.GetClaimSave(SAML2ClaimTypes.ShibbolethIssuer);

                if (shibbolethIssuer == null)
                {
                    //No Shib-Login
                    return false;
                }

                var contracts = CrmCache.CampusContracts.Where(i => string.Equals(i.ShibbolethEntityId, shibbolethIssuer.Value, StringComparison.InvariantCultureIgnoreCase)).ToList();
                if (!contracts.Any())
                {
                    return false;
                }

                foreach (var campusContract in contracts.Where(cc => !cc.NewContractAvailable))
                {
                    var affiliations = SAML2ClaimTypes.ParseAffiliationClaims(claims);
                    var campusGroup = affiliations.Any(i => i == PersonAffiliationType.Student) ? CampusGroup.Students : CampusGroup.Faculty;
                    if (Environment.Build == BuildType.Alpha)
                    {
                        if (campusContract.ShibbolethPersonAffiliation == PersonAffiliationType.All &&
                            !affiliations.Any())
                        {
                            affiliations = new List<PersonAffiliationType> { PersonAffiliationType.Student };
                        }
                    }

                    var shibOk = false;
                    foreach (var affiliation in affiliations)
                    {
                        if (affiliation == PersonAffiliationType.None)
                        {
                            continue;
                        }

                        if (!campusContract.ShibbolethPersonAffiliation.HasFlag(affiliation))
                        {
                            continue;
                        }
                        shibOk = true;
                        break;
                    }

                    if (!string.IsNullOrEmpty(campusContract.ShibbolethPersonEntitlement))
                    {
                        var personEntitlements = claims.Where(c => c.Type == SAML2ClaimTypes.PersonEntitlement).ToList();
                        foreach (var personEntitlement in personEntitlements)
                        {
                            if (string.Equals(personEntitlement.Value, campusContract.ShibbolethPersonEntitlement, StringComparison.InvariantCultureIgnoreCase))
                            {
                                shibOk = true;
                                break;
                            }
                        }
                    }

                    if (shibOk)
                    {
                        return false;
                    }
                }

                //User hat Shib-CC-EntityId aber nicht die nötigen Rechte

                return true;
            }
            catch(Exception ex)
			{
                Telemetry.TrackException(ex, SeverityLevel.Warning, ExceptionFlow.Eat);
			}
            return false;
        }

        #endregion

        #region UpadateLicensesAfterLogin

        public async Task UpadateLicensesAfterLoginAsync(CrmUser user, string ipAddress, bool saveChanges = true)
        {
#if DEBUG
            if (!CrmConfig.IsUnittest)
            {
                ipAddress = "84.253.11.96";
            }
#endif
            if (user.Key.StartsWith(CrmConstants.LoadTestCrmEntityKeyPrefix))
            {
                return;
            }

            #region Email Campus Licenses

            var emailCampusContracts = (from emailAccount in user.CrmLinkedEMailAccounts
                                        where emailAccount.IsVerified.GetValueOrDefault()
                                        from contract in CrmCache.CampusContracts
                                        where contract.IsEmailContract

                                        let emailDomain = contract.EmailsDomainsResolved.FirstOrDefault(i => i.IsMatch(emailAccount.Email))
                                        where emailDomain != null
                                        select new
                                        {
                                            EmailDomain = emailDomain,
                                            CampusContract = contract,
                                        }).ToList();

            foreach (var item in emailCampusContracts.ToList())
            {
                var campusContract = item.CampusContract;

                //4.10.2016, Neu wird im CRM auf "Nein" gestellt. Wir prüfen beim Hinzufügen der Lizenzen ob der Kunde schon eine hat.
                //Er bekommt also keine Lizenzen doppelt
                //if (!string.IsNullOrEmpty(campusContract.ShibbolethEntityId)) continue;

                foreach (var product in campusContract.ProductsResolved)
                {
                    if (!user.Licenses.HasCampusContractLicense(campusContract, product))
                    {
                        //User hat noch keine Campus-Lizenz mit diesem Produkt
                        //Verfahren wurde geändert oder Campusvertrag hat ein neues Produkt
                        //Wenn er eine andere bestätigte Campuslizenz hat, verifizieren wir die neue Lizenz

                        var verifedLicenseExists = user.Licenses.GetCampusContractLicenses(campusContract).Any(lic => lic.IsVerified);

                        foreach (var lic in AddLicenses(user, campusContract, item.EmailDomain.OrganizationName))
                        {
                            lic.IsVerified = verifedLicenseExists;
                        }
                    }
                }
            }

            #endregion

            #region IP Campus Licenses

            var addedIPLicenses = await AddLicensesFromIPVerification(user, ipAddress);

            #endregion

            var resetCloudSpaceCache = addedIPLicenses.Any(lic => lic.CitaviSpaceInMB.HasValue && lic.CitaviSpaceInMB.Value > 0);

            try
            {
                foreach (var license in user.Licenses.ToList())
                {
                    if (string.IsNullOrEmpty(license.DataContractCampusContractKey))
                    {
                        continue;
                    }

                    var campusContract = CrmCache.CampusContracts.FirstOrDefault(cc => cc.Key == license.DataContractCampusContractKey);
                    if (campusContract == null)
                    {
                        continue;
                    }

                    if (!string.Equals(campusContract.RSSFeedUrl, license.DataContractRssFeedUrl, StringComparison.InvariantCultureIgnoreCase))
                    {
                        license.DataContractRssFeedUrl = campusContract.RSSFeedUrl;
                    }

                    foreach (var product in campusContract.ProductsResolved)
                    {
                        if (license.DataContractProductKey == product.Key)
                        {
                            continue;
                        }

                        if (!campusContract.IsVoucherContract && !campusContract.IsShibbolethContract)
                        {
                            continue;
                        }

                        if (user.Licenses.HasCampusContractLicense(campusContract, product))
                        {
                            continue;
                        }

                        var hasVerifiedLicence = user.Licenses.GetCampusContractLicenses(campusContract).Any(lic => lic.IsVerified);
                        var existingLicenceWithExpryDate = user.Licenses.GetCampusContractLicenses(campusContract).FirstOrDefault(lic => lic.ExpiryDate.HasValue);

                        foreach (var added in AddLicenses(user, campusContract, campusContract.OrganizationName))
                        {
                            if (campusContract.IsVoucherContract &&
                                existingLicenceWithExpryDate != null)
                            {
                                //User hat bereites eine Vocher-Lizenz von diese Uni.
                                //Diese hat eine Ablaufdatum. Wir müssen diese übernehmen.
                                added.ExpiryDate = existingLicenceWithExpryDate.ExpiryDate;
                            }
                            added.IsVerified = hasVerifiedLicence;
                            if(added.CitaviSpaceInMB.HasValue &&
                               added.CitaviSpaceInMB.Value > 0 &&
                               added.IsVerified)
							{
                                resetCloudSpaceCache = true;
							}
                        }
                    }
                }
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
            }

            foreach (var incompleteLicense in user.Licenses.Where(i => i.HasMissingValues(user.Key)).ToList())
            {
                LicenseManager.UpdateLicenseKey(user, incompleteLicense);
            }

            foreach (var license in user.Licenses.ToList())
            {
                if (string.IsNullOrEmpty(license.DataContractCampusContractKey))
                {
                    continue;
                }
                var campusContract = CrmCache.CampusContracts.FirstOrDefault(cc => cc.Key == license.DataContractCampusContractKey);
                if (campusContract == null)
                {
                    continue;
                }
                if (!campusContract.CitaviSpaceInGB.HasValue)
                {
                    continue;
                }
                if (campusContract.NewContractAvailable)
                {
                    continue;
                }
                if (!license.CitaviSpaceInMB.HasValue)
                {
                    license.CitaviSpaceInMB = campusContract.CitaviSpaceInGB * 1024;
                    resetCloudSpaceCache = true;
                }
            }

            if (saveChanges)
            {
                await DbContext.SaveAndUpdateUserCacheAsync(user);
            }

            await new LicenseManager(DbContext).AddLegacyFreeLicense(user);

            if (resetCloudSpaceCache)
            {
                await CitaviSpaceCache.RemoveAsync(user.Key);
            }
        }

        #endregion

        #region SetLicenseVerificationKeyAndSendMail

        public async Task<SendVerificationKeyMailResult> SetLicenseVerificationKeyAndSendMailAsync(CrmUser user, string licenseKey)
        {
            var license = user.Licenses.FirstOrDefault(i => i.Key == licenseKey);
            if (license == null)
            {
                return SendVerificationKeyMailResult.LicenseNotFound;
            }

            var available = from emailAccount in user.CrmLinkedEMailAccounts
                            where emailAccount.IsVerified.GetValueOrDefault()
                            from contract in CrmCache.CampusContracts
                            where license.DataContractCampusContractKey == contract.Key &&
                                   (contract.VerifyMaEmail || contract.VerifyStEmail) &&
                                   contract.EmailsDomainsResolved.Any(i => i.IsMatch(emailAccount.Email))
                            select new
                            {
                                emailAccount = emailAccount,
                                contract = contract,
                            };

            if (!available.Any())
            {
                return SendVerificationKeyMailResult.AlreadyVerified;
            }

            var email = available.First().emailAccount.Email;
            await SetLicenseVerificationKeyAndSendMail(user, email, license);
            await DbContext.SaveAndUpdateUserCacheAsync(user);
            return SendVerificationKeyMailResult.OK;
        }

        public async Task SetLicenseVerificationKeyAndSendMail(CrmUser user, string email, CitaviLicense license)
        {
            license.VerificationStorage = email;
            license.VerificationKeySent = DateTime.UtcNow;
            license.VerificationKey = Security.PasswordGenerator.WebKey.Generate();
            await EmailService.SendConfirmEmailAddressCampusContractLicenseMail(user, email, license.VerificationKey);
        }

        #endregion

        #region VerifyCampusLicenseVerificationKey

        public async Task<(VerifyEmailFromKeyResult, string)> VerifyCampusLicenseVerificationKeyAsync(CrmUser user, string verificationKey)
        {
            var lic = user.Licenses.FirstOrDefault(i => i.VerificationKey == verificationKey);
            if (lic == null || lic.IsVerified)
            {
                return (VerifyEmailFromKeyResult.NotFound, "");
            }

            if (lic.VerificationKeySent + TimeSpan.FromDays(14) < DateTime.UtcNow)
            {
                return (VerifyEmailFromKeyResult.Expired, "");
            }

            if (!string.Equals(lic.VerificationKey, verificationKey, StringComparison.InvariantCultureIgnoreCase))
            {
                return (VerifyEmailFromKeyResult.InvalidPassword, "");
            }

            var email = lic.VerificationStorage;

            var newCampusContract = CrmCache.CampusContracts.FirstOrDefault(i => i.Key == lic.DataContractCampusContractKey);
            if (newCampusContract == null)
            {
                Telemetry.TrackTrace($"VerifyCampusLicenseVerificationKey: CampusContract not found: {lic.DataContractCampusContractKey}");
                return (VerifyEmailFromKeyResult.NotFound, "");
            }

            //Alle Campusverträge von dieser Uni, welche einen neuen Campusvertrag haben
            var expiredCampusContracts = CrmCache.CampusContracts.Where(i => i.NewContractAvailable).Where(i => i.DataContractAccountKey == newCampusContract.DataContractAccountKey);

            //Alle Lizenzen welche durch neue ersetzt werden
            foreach (var expiredCampusContract in expiredCampusContracts)
            {
                DeactiveLicenses(user, expiredCampusContract);
            }

            var resetCloudSpaceCache = false;
            //Alle Lizenzen von diesem Campusvertrag verifizieren
            foreach (var ccLicense in user.Licenses.ToList())
            {
                if (ccLicense.DataContractCampusContractKey != lic.DataContractCampusContractKey)
                {
                    continue;
                }

                if (ccLicense.IsVerified)
                {
                    continue;
                }

                ccLicense.VerificationKey = null;
                ccLicense.VerificationKeySent = null;
                ccLicense.VerificationStorage = null;

                if (!string.IsNullOrEmpty(newCampusContract.OrganizationName))
                {
                    ccLicense.OrganizationName = newCampusContract.OrganizationName;
                }

				if (ccLicense.IsCitaviSpace)
				{
                    resetCloudSpaceCache = true;
                }
                ccLicense.IsVerified = true;
                LicenseManager.UpdateLicenseKey(user, ccLicense);
            }

            await DbContext.SaveAndUpdateUserCacheAsync(user);

			if (resetCloudSpaceCache)
			{
                await CitaviSpaceCache.RefreshAsync(user, DbContext);
			}

            return (VerifyEmailFromKeyResult.OK, email);
        }

        #endregion

        #endregion
    }
}
