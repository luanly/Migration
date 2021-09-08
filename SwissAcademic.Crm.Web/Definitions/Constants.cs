using System;
using System.Diagnostics.CodeAnalysis;

namespace SwissAcademic.Crm.Web
{
    #region AuthenticateResultConstants

    public static class AuthenticateResultConstants
    {
        public const string AccountIsClosed = "AccountIsClosed";
        public const string AccountLockoutFailedLoginAttempts = "AccountLockoutFailedLoginAttempts";
        public const string AccountIsNotAllowedToLogin = "AccountIsNotAllowedToLogin";
        public const string LoginNotAllowedLinkedEMailAccountIsNotVerified = "LoginNotAllowedLinkedEMailAccountIsNotVerified";
        public const string NewAccountFromExternalProviderButEmailAlreadyExists = "NewAccountFromExternalProviderButEmailAlreadyExists";
        public const string ContactUpdateFailedIsKeyContact = "ContactIsKeyContact";
    }

    #endregion

    #region AzureConstants

    public static class AzureConstants
    {
        public const string ConsentTable = "ConsentStore";
        public const string OrganizationSettingsBlobContainer = "organizationsettings";
        public const string PersistedGrantStoreTable2 = "PersistedGrantStore2";
        public const string CookieStoreTable = "CookieStore";
        public const string CrmCacheTable = "CrmCache";
        public const string CrmCacheUserImagesBlobContainer = "crmcache/userimages";
        public const string ShibollethCacheBlobContainer = "shibolleth";
        public const string DistributedCache = "DistributedCache";
        public const string ProjectEntryRegionCache = "ProjectEntryRegionCache";
        public const string TableStorageLockTable = "TableStorageLock";
    }

    #endregion

    #region ApiResourceNames

    public static class ApiResourceNames
    {
        public const string WebApi = "api";
    }

    #endregion

    #region CitaviScopes

    public static class CitaviScopes
    {
        public const string Project = "project";
        public const string WebApi = "webapi";
    }

    #endregion

    #region CitaviClaimTypes
    public static class CitaviClaimTypes
    {
        public const string AccessToken = "http://schemas.citavi.com/identity/claims/accesstoken";
        public const string ContactKey = "sub";
        public const string FirstDesktopLogin = "http://schemas.citavi.com/identity/claims/firstdesktoplogin";

        public const string Project = "project";
        public const string DataCenter = "datacenter";
        public const string DataCenterShortName = "datacentershortname";
    }

	#endregion

	#region ClientIds

	public static class ClientIds
    {
        public const string Web = "CitaviWebClient";
        public const string WebWordAddIn = "CitaviWebWordAddInClient";
        public const string GoogleChromePicker = "GoogleChromePickerClient";
        public const string EdgePicker = "MicrosoftEdgePickerClient";
        public const string FirefoxPicker = "FirefoxPickerClient";
        public const string GoogleDocsAddIn = "CitaviWebGoogleDocsAddInClient";
        public const string Crm = "CRMClient";
        public const string Desktop = "CitaviDesktopClient";
        public const string Mobile = "CitaviMobileClient";
        public const string Keylight = "Keylight";
        public const string SciFlow = "SciFlow";
        public const string UnitTest = "CitaviTestClient";
        public const string UseResponse = "UseResponseClient";
        public const string ZenDesk = "ZenDeskClient";
    }

    #endregion

    #region ClientSecrets

    public static class ClientSecrets
    {
        public const string KeylightClientSecret = "177A3D96-D4C1-40D5-87D7-46E9B637EDF3";
        public const string CitaviDesktopClientSecret = "4AA914A3-A575-4CDD-8674-2DCC00449620";
        public const string CitaviUnitTestClientSecret = "A932ADB2-3FF6-4F85-BB61-1BF428F2D2E6";
        public const string CitaviMobileClientSecret = "56644748-0762-4249-8169-E155E3DF193B";
    }

    #endregion

    #region CleverbridgeProductIds

    public static class CleverbridgeProductIds
    {
        public const int Citavi3ProAcademic = 72708;
        public const int Citavi3ProAcademicUniSaarland = 72712;
        public const int Citavi3ProBenefit = 72709;
        public const int Citavi3ProBusiness = 72706;
        public const int Citavi3ProHome = 72707;
        public const int Citavi3ProHomeGeschenkgutschein = 73722;
        public const int Citavi3ProHomeTwitter = 90699;
        public const int Citavi3ReaderAcademic = 77886;
        public const int Citavi3ReaderBusiness = 77895;
        public const int Citavi3TeamAcademic = 77887;
        public const int Citavi3TeamAcademicUniSaarland = 72713;
        public const int Citavi3TeamAcademicUpgradeCitavi2 = 78558;
        public const int Citavi3TeamAcademicUpgradeCitavi3Pro = 77888;
        public const int Citavi3TeamBusiness = 77896;
        public const int Citavi3TeamBusinessUpgradeCitavi2 = 78557;
        public const int Citavi3TeamBusinessUpgradeCitavi3Pro = 77897;
        public const int Citavi4ProAcademic = 93254;
        public const int Citavi4ProAcademicUpgradeCitavi3Pro = 93262;
        public const int Citavi4ProBenefit = 93287;
        public const int Citavi4ProBusiness = 93256;
        public const int Citavi4ProBusinessUpgradeCitavi3Pro = 93263;
        public const int Citavi4ProHome = 93252;
        public const int Citavi4ProHomeGeschenkgutschein = 150030;
        public const int Citavi4ProHomeUpgradeCitavi3Pro = 93261;
        public const int Citavi4ReaderAcademic = 93259;
        public const int Citavi4ReaderAcademicUpgradeCitavi3Reader = 93270;
        public const int Citavi4ReaderBusiness = 93260;
        public const int Citavi4ReaderBusinessUpgradeCitavi3Reader = 93271;
        public const int Citavi4TeamAcademic = 93257;
        public const int Citavi4TeamAcademicUpgradeCitavi3Pro = 93264;
        public const int Citavi4TeamAcademicUpgradeCitavi3Team = 93266;
        public const int Citavi4TeamAcademicUpgradeCitavi4Pro = 93265;
        public const int Citavi4TeamBusiness = 93258;
        public const int Citavi4TeamBusinessUpgradeCitavi3Pro = 93267;
        public const int Citavi4TeamBusinessUpgradeCitavi3Team = 93269;
        public const int Citavi4TeamBusinessUpgradeCitavi4Pro = 93268;
        public const int Citavi5forDBServerCONCURRENTAcademic = 150716;
        public const int Citavi5forDBServerCONCURRENTAcademicUpgradeCitavi3 = 150717;
        public const int Citavi5forDBServerCONCURRENTAcademicUpgradeCitavi4 = 150718;
        public const int Citavi5forDBServerCONCURRENTAcademicUpgradeCitavi5 = 152024;
        public const int Citavi5forDBServerCONCURRENTBusiness = 150710;
        public const int Citavi5forDBServerCONCURRENTBusinessUpgradeCitavi3 = 150713;
        public const int Citavi5forDBServerCONCURRENTBusinessUpgradeCitavi4 = 150714;
        public const int Citavi5forDBServerCONCURRENTBusinessUpgradeCitavi5 = 152022;
        public const int Citavi5forDBServerPERSEATAcademic = 150715;
        public const int Citavi5forDBServerPERSEATAcademicUpgradeCitavi3 = 150720;
        public const int Citavi5forDBServerPERSEATAcademicUpgradeCitavi4 = 150721;
        public const int Citavi5forDBServerPERSEATAcademicUpgradeCitavi5 = 152023;
        public const int Citavi5forDBServerPERSEATBusiness = 150709;
        public const int Citavi5forDBServerPERSEATBusinessUpgradeCitavi3 = 150711;
        public const int Citavi5forDBServerPERSEATBusinessUpgradeCitavi4 = 150712;
        public const int Citavi5forDBServerPERSEATBusinessUpgradeCitavi5 = 152021;
        public const int Citavi5forDBServerREADERAcademic = 173790;
        public const int Citavi5forDBServerREADERBusiness = 173786;
        public const int Citavi5forWindowsAcademic = 150615;
        public const int Citavi5forWindowsAcademicUpgradeCitavi3 = 150613;
        public const int Citavi5forWindowsAcademicUpgradeCitavi4 = 150614;
        public const int Citavi5forWindowsBusiness = 150612;
        public const int Citavi5forWindowsBusinessUpgradeCitavi3 = 150610;
        public const int Citavi5forWindowsBusinessUpgradeCitavi4 = 150611;
        public const int Citavi5forWindowsGiftcertificate = 152063;
        public const int Citavi5forWindowsHome = 150618;
        public const int Citavi5forWindowsHomeUpgradeCitavi3 = 150616;
        public const int Citavi5forWindowsHomeUpgradeCitavi4 = 150617;
        public const int Citavi5forWindowsHomeReseller = 160137;
        public const int Citavi5forWindowsSiteLicenseDiscount = 152062;
        public const int Citavi5forWindowsStudentReseller=160138;
        public const int Citavi6forDBServerCONCURRENTAcademic = 201565;
        public const int Citavi6forDBServerCONCURRENTAcademicUpgradeCitavi4 = 201569;
        public const int Citavi6forDBServerCONCURRENTAcademicUpgradeCitavi5 = 201564;
        public const int Citavi6forDBServerCONCURRENTBusiness = 201554;
        public const int Citavi6forDBServerCONCURRENTBusinessUpgradeCitavi4 = 201557;
        public const int Citavi6forDBServerCONCURRENTBusinessUpgradeCitavi5 = 201552;
        public const int Citavi6forDBServerPERSEATAcademic = 201563;
        public const int Citavi6forDBServerPERSEATAcademicUpgradeCitavi4 = 201572;
        public const int Citavi6forDBServerPERSEATAcademicUpgradeCitavi5 = 201570;
        public const int Citavi6forDBServerPERSEATBusiness = 201562;
        public const int Citavi6forDBServerPERSEATBusinessUpgradeCitavi4 = 201567;
        public const int Citavi6forDBServerPERSEATBusinessUpgradeCitavi5 = 201568;
        public const int Citavi6forDBServerREADERAcademic = 201559;
        public const int Citavi6forDBServerREADERBusiness = 201553;
        public const int Citavi6forWindowsAcademic = 201551;
        public const int Citavi6forWindowsAcademicUpgradeCitavi4 = 201571;
        public const int Citavi6forWindowsAcademicUpgradeCitavi5 = 201561;
        public const int Citavi6forWindowsBusiness = 201566;
        public const int Citavi6forWindowsBusinessUpgradeCitavi4 = 201558;
        public const int Citavi6forWindowsBusinessUpgradeCitavi5 = 201560;
        public const int Citavi6forWindowsGiftCertificate = 201574;
        public const int Citavi6forWindowsHome = 201556;
        public const int Citavi6forWindowsHomeUpgradeCitavi4 = 201555;
        public const int Citavi6forWindowsHomeUpgradeCitavi5 = 201550;
        public const int Citavi6forWindowsSiteLicenseDiscount = 201575;
        public const int CitaviforDBServerCONCURRENTAcademic12months = 222826;
        public const int CitaviforDBServerCONCURRENTBusiness12months = 222830;
        public const int CitaviforDBServerCONCURRENTMaintenanceAcademic12months = 222731;
        public const int CitaviforDBServerCONCURRENTMaintenanceBusiness12months = 222735;
        public const int CitaviforDBServerPERSEATAcademic12months = 222827;
        public const int CitaviforDBServerPERSEATBusiness12months = 222831;
        public const int CitaviforDBServerPERSEATMaintenanceAcademic12months = 220227;
        public const int CitaviforDBserverPERSEATMaintenanceBusiness12months = 222734;
        public const int CitaviforDBServerREADERAcademic12months = 222828;
        public const int CitaviforDBServerREADERBusiness12months = 222832;
        public const int CitaviforDBServerREADERMaintenanceBusiness12months = 222736;
        public const int CitaviforDBServerREADERMaintenanceAcademic12months = 222732;
        public const int CitaviforWindowsAcademic12months = 222825;
        public const int CitaviforWindowsBusiness12months = 222829;
        public const int CitaviforWindowsHome12months = 222833;
        public const int CitaviforWindowsMaintenanceAcademic12months = 222730;
        public const int CitaviforWindowsMaintenanceBusiness12months = 222733;
        public const int CitaviforWindowsMaintenanceHome12months = 222737;
        public const int CitaviSpace100GBAcademic12months = 222740;
        public const int CitaviSpace100GBBusiness12months = 222741;
        public const int CitaviSpace100GBHome12months = 222742;
        public const int CitaviSpace5GBAcademic12months = 222738;
        public const int CitaviSpace5GBBusiness12months = 222739;
        public const int CitaviSpace5GBHome12months = 220225;
        public const int CitaviWebAcademic12months = 222743;
        public const int CitaviWebBusiness12months = 222744;
        public const int CitaviWebHome12months = 222745;

    }

    #endregion

    #region CrmAttributeNames

    internal static class CrmAttributeNames
    {
        public const string ModifiedOn = "modifiedon";
        public const string Key = "new_key";
        public const string LastModifiedBy = "lastmodifiedby";
        public const string LastModifiedOn = "lastmodifiedon";
        public const string StatusCode = "statuscode";
        public const string EntityState = "entitystate";
    }

    #endregion

    #region CrmConstants

    [ExcludeFromCodeCoverage]
    public static class CrmConstants
    {
        public const string CitaviBetaLicenseSuffix = "-beta";
        public static TimeSpan CleanUpDeletedProjectsAfterDefault { get; } = TimeSpan.FromDays(14);
        internal const string CustomAttributePrefix = "new_";
        internal const string LookupSuffix = "_lookup";
        public const string UnitTestCrmEntityKeyPrefix = "unittest";
        public const string LoadTestCrmEntityKeyPrefix = "lotest";
        public const string PersistentLoadTestCrmEntityKeyPrefix = "ptest";
        public const int MaxLookupContactInfosCount = 20;
    }

    #endregion

    #region CrmQueueConstants

    [ExcludeFromCodeCoverage]
    public static class CrmQueueConstants
    {
        public const string PendingChanges = "CrmPendingChanges";
    }

    #endregion

    #region CookieNames

    public static class CookieNames
    {
        public const string RegistrationInfo = "__Secure-citavi.registration";
        public const string TestingInProduction = "x-ms-routing-name";
    }

    #endregion

    #region CrmAnnotationPropertyNames

    //internal class CrmAnnotationPropertyNames
    //{
    //    public const string AnnotationId = "annotationid";
    //    public const string NoteText = "notetext";
    //    public const string ObjectId = "objectid";
    //    public const string ObjectTypeCode = "objecttypecode";
    //    public const string Subject = "subject";
    //}

    #endregion

    #region CrmDataTypeConstants

    internal static class CrmDataTypeConstants
    {
        public static readonly DateTime MinDate = new DateTime(1900, 1, 1);
    }

    #endregion

    #region CrmEntityNames

    public static class CrmEntityNames
    {
        public const string Account = "account";
        public const string ActivityMimeAttachment = "activitymimeattachment";
        public const string ActivityParty = "activityparty";
        public const string Annotation = "annotation";
        public const string AutoNumberSequence = "new_autonumbersequence";
        public const string BulkMailQuery = "new_bulkmailquery";
        public const string BulkMailTemplate = "new_bulkmailtemplate";
        public const string CleverbridgeProduct = "new_cleverbridgeproduct";
        public const string Contact = "contact";
        public const string CitaviCampaign = "new_citavicampaign";
        public const string Delivery = "new_delivery";
        public const string DataOrderProcessing = "new_dataorderprocessing";
        public const string EMailDomain = "new_emaildomainorcampusname";
        public const string Email = "email";
        public const string ExternalAccount = "new_externeraccount";
        public const string License = "new_citavilicense";
        public const string LicenseType = "new_licensetyp";
        public const string LicenseFile = "new_licensfile";
        public const string LinkedAccount = "new_linkedaccount";
        public const string LinkedEmailAccount = "new_linkedemailaccount";
        public const string LinkedMobilePhonee = "new_linkedmobilephone";
        public const string IPRange = "new_iprange";
        public const string Queue = "queue";
        public const string Pricing = "new_pricing";
        public const string Project = "new_project";
        public const string ProjectRole = "new_projectrole";
        public const string Product = "new_citaviproduct";
        public const string Task = "task";
        public const string VoucherBlock = "new_voucherblock";
        public const string Voucher = "new_voucher";
        public const string OrderProcess = "new_orderprocess";
        public const string CampusContract = "new_campuscontract";
        public const string CampusContractRenewal = "new_ccoldnewcontract";
        public const string CampusContractStatistic = "new_contractstats";
        public const string OrganizationSetting = "new_organizationsetting";
        public const string Workflow = "workflow";
        public const string SystemUser = "systemuser";
        public const string Subscription = "new_subscription";
        public const string SubscriptionItem = "new_subscriptionitem";
        public const string Team = "team";
        public const string TransactionCurrency = "transactioncurrency";
    }

    #endregion

    #region CrmEmailHelpManualUrls

    public static class CrmEmailHelpManualUrls
    {
        public const string LicenseFileHelpUrl = "https://www.citavi.com/licensefile";
        public const string LicenseFileHelpUrlC6 = "https://www.citavi.com/licensefile6";
        public const string ExportSettingsFileUrl = "https://www.citavi.com/exportsettingsfile";
        public const string CampusContractExtensionInfoMailContact = "https://www.citavi.com/sitelicense";
        public const string CampusLicenseHelpGetOldKey = " https://www.citavi.com/keys-for-previous-versions";
        public const string ConcurrentLicenseExceeded = "https://www.citavi.com/cdbserver";

    }

    #endregion

    #region CrmEmailTemplatePlaceholderConstants

    public static class CrmEmailTemplatePlaceholderConstants
    {
        public const string Description = "##description";
        public const string Inviter = "##inviter";
        public const string ProjectRole = "##projectrole";
        public const string ProjectName = "##projectname";
        public const string Salutation = "##salutation";
        public const string Email = "##email";
        public const string Url = "id_link";
        public const string VerificationCode = "##code";
        public const string Text = "##text";

        public const string CustomerOrderNumber = "##customerordernumber";
        public const string InvoiceNumber = "##invoicenumber";
        /// <summary>
        /// Text aus dem CampusContract.TextSource (HTML formatiert)
        /// </summary>
        public const string AddOn = "##addOn";
        public const string Date = "##date";
        public const string HelpUrl = "##helpurl";
        public const string Organization = "##organization";
        public const string Productname = "##productname";
        public const string HelpGetOldVersion = "##helpgetoldversion";
        public const string NewLine = "##newline";
        public const string RenewalEmail = "##renewalEmail";
        public const string RenewalIP = "##renewalIP";
        public const string RenewalVoucher = "##renewalVoucher";
        public const string AccountName = "##accountName";
        public const string News = "##news";
        public const string Statistics = "##statistics";
        public const string StatisticsDate = "##statisticsDate";
        public const string StatisticsHistoric = "##statisticsHistoric";
        public const string StatisticsTotalCount = "##statisticsTotalCount";
        public const string StatisticsVerifiedCount = "##statisticsVerifiedCount";
        public const string StatisticsStudentsCount = "##statisticsStudentsCount";
        public const string StatisticsStudentsVerifiedCount = "##statisticsStudentsVerifiedCount";
        public const string StatisticsEmployeeCount = "##statisticsEmployeeCount";
        public const string StatisticsEmployeeVerifiedCount = "##statisticsEmployeeVerifiedCount";
        public const string StatisticsContractFirstDate = "##statisticsContractFirstDate";
        public const string StatisticsContractCurrentEndDate = "##statisticsContractCurrentEndDate";
        public const string StatisticsLastLoginDesktopDaily = "##statisticsLastLoginDesktopDaily";
        public const string StatisticsLastLoginDesktopMonthly = "##statisticsLastLoginDesktopMonthly";
        public const string StatisticsLastLoginWebDaily = "##statisticsLastLoginWebDaily";
        public const string StatisticsLastLoginWebMonthly = "##statisticsLastLoginWebMonthly";
        public const string StatisticsLastLoginAssistantDaily = "##statisticsLastLoginAssistantDaily";
        public const string StatisticsLastLoginAssistantMonthly = "##statisticsLastLoginAssistantMonthly";
    }

    #endregion

    #region CrmEmailHelpManualUrlsEx

    public static class CrmEmailHelpManualUrlsEx
    {
        public const string LicenseFileHelpUrlC5 = "https://www.citavi.com/licensefile";
        public const string LicenseFileHelpUrlC6 = "https://www.citavi.com/licensefile6";
        public const string ExportSettingsFileUrl = "https://www.citavi.com/exportsettingsfile";
        public const string CampusContractExtensionInfoMailContact = "https://www.citavi.com/sitelicense";
        public const string CampusLicenseHelpGetOldKey = " https://www.citavi.com/keys-for-previous-versions";
    }

    #endregion

    #region CrmRelationshipNames

    internal static class CrmRelationshipNames
    {
        public const string AccountLicenseType = "organization_new_licensetyp";
        public const string AccountVoucherBlock = "new_account_new_voucherblock";
        public const string AccountCampusContract = "new_account_new_campuscontract";
        public const string AccountEMailDomain = "new_account_new_emaildomainorcampusname";
        public const string AccountLicense = "account_new_citavilicense";//Gibt es nicht, brauchen wir aber für die Abfragen (Alias)
        public const string AccountIPRange = "new_account_new_iprange";
        public const string AccountNewsletterContact = "new_account_contact_newsletter";
        public const string AccountLicenseFile = "new_account_new_licensfile";

        public const string AccountDataOrderProcessing = "new_account_new_dataorderprocessing";

        public const string CampusContractProduct = "new_campuscontract_n_citaviproduct";
        public const string CampusContractLicenseFile = "new_campuscontract_new_licensfile";

        public const string CampusContractRenewalCampusContract_New = "new_newcontract";
        public const string CampusContractRenewalCampusContract_Old = "new_oldcontract";

        public const string CampusContractCampusContractStatistic = "new_new_campuscontract_new_contractstats";

        public const string ContactProject = "new_contact_new_project";
        public const string ContactProjectRole = "new_contact_new_projectrole";
        public const string ContactLinkedAccount = "new_contact_new_linkedaccount";
        public const string ContactLinkedEMailAccount = "new_contact_new_linkedemailaccount";
        public const string ContactLinkedMobilePhone = "new_contact_new_linkedmobilephone";

        public const string ContactOwnerLicense = "new_contact_new_citavilicense";
        public const string ContactEndUserLicense = "new_contact_new_citavilicense_enduser";
        public const string ContactVoucherBlock = "new_contact_new_voucherblock";
        public const string ContactVoucher = "new_contact_new_voucher";
        public const string ContactOrderProcess = "new_contact_new_orderprocess_bc";
        public const string ContactOrderProcessDC = "new_contact_new_orderprocess_dc";
        public const string ContactOrderProcessLC = "new_contact_new_orderprocess_lc";
        public const string ContactUpgradeTicket = "new_contact_updateticket";
        public const string ContactCampusContract = "new_contact_new_campuscontract";

        public const string CleverbridgeProductLicenseType = "new_licensetyp_new_cleverbridgeproduct";
        public const string CleverbridgeProductPricing = "new_pricing_new_cleverbridgeproduct";
        public const string CleverbridgeProductCitaviProduct = "new_citaviproduct_new_cleverbridgeproduct";
        public const string CleverbridgeProductAllowedUpgrades = "new_cleverbridgeproduct_upgrades";
        public const string CleverbridgeProductBaseProduct = "new_cbproduct_new_cbproduct";

        public const string DeliveryBulkMailQuery = "new_bulkmailquery_new_delivery";
        public const string DeliveryBulkMailTemplate = "new_bulkmailtemplate_new_delivery";
        public const string DeliveryAccount = "new_delivery_account";
        public const string DeliveryContact = "new_delivery_contact";

        public const string LicenseLicenseType = "new_licensetyp_new_citavilicense";
        public const string LicenseTypeVoucherBlock = "new_licensetyp_new_voucherblock";
        public const string LicenseProduct = "new_citaviproduct_new_citavilicense";
        public const string LicenseVoucher = "new_voucher_new_citavilicense";
        public const string LicenseOrderProcess = "new_orderprocessnew_citavilicense";
        public const string LicenseCampusContract = "new_campuscontract_new_citavilicense";
        public const string LicenseCitaviCampaign = "new_citavicampaign_new_citavilicense";
        public const string LicensePricing = "new_pricing_new_citavilicense";
        public const string LicenseLicenseFile = "new_citavilicense_new_licensfile";

        public const string LicenseFileContacts = "new_licensfile_contact";

        public const string OrganizationSettingsCampusContract = "new_campuscontract_new_organizationsetting";

        public const string ProjectProjectRole = "new_project_new_projectrole";

        public const string PricingVoucherBlock = "new_pricing_new_voucherblock";
        public const string PricingUpgradeTicket = "new_pricing_new_updateticket";

        public const string VoucherVoucherBlock = "new_voucherblock_new_voucher";

        public const string VoucherBlockProduct = "new_citaviproduct_new_voucherblock";
        public const string VoucherBlockLicenseType = "new_licensetyp_new_voucherblock";
        public const string VoucherBlockLicenseFile = "new_voucherblock_new_licensfile";

        public const string VoucherLicense = "new_citavilicense_new_voucher";

        public const string ResellerOrderProcess = "new_reseller_new_orderprocess";

        public const string SubscriptionSubscriptionItem = "new_subscription_new_subscriptionitem";
        public const string SubscriptionItemLicenses = "new_subscriptionitem_new_citavilicense";
        public const string SubscriptionItemCleverbridgeProduct = "new_cleverbridgeproduct_new_subscriptionitem";
        public const string SubscriptionContact = "new_contact_new_subscription";
        public const string SubscriptionOrderProcesses = "new_subscription_new_orderprocesses";

        public const string OrderProcessVoucherBlocks = "new_orderprocess_new_voucherblock";

    }

    #endregion

    #region CrmRelationshipLookupNames

    public static class CrmRelationshipLookupNames
    {
        public const string LicenseProduct = "new_citaviproductid";
        public const string ContactLinkedAccount = "new_contact_new_linkedaccount_lookup";
        public const string ContactLinkedEmailAccount = "new_contact_new_linkedemail_lookup";
        public const string ContactProjectRole = "new_contact_new_projectrole_lookup";
        public const string ProjectProjectRole = "new_project_new_projectrole_lookup";
        public const string CleverbridgeUpgrade_To = "cleverbridgeproduct_upgrades.new_cleverbridgeproduct";
        public const string LicenseEndUser = "new_enduserid";
        public const string LicenseOwner = "new_contactid";
    }

    #endregion

    #region CrmODataConstants

    public static class CrmODataConstants
    {
        public const string ContentId = "Content-ID";
    }

    #endregion

    #region MailChimpListIds

    internal static class MailChimpListIds
    {
        internal const string NewsEnglish = "cbfdbffe40";
        internal const string NewsGerman = "27a3da2971";
    }

    #endregion

    #region CrmMembershipRebootConstants

    public static class CrmMembershipRebootConstants
    {
        public readonly static TimeSpan VerificationKeyLifetime = TimeSpan.FromHours(72);
        public readonly static TimeSpan MergeVerificationKeyLifetime = TimeSpan.FromMinutes(20);
    }

    #endregion

    #region EmailAdressesConstants

    public static class EmailAdressesConstants
    {
        public readonly static string NewsletterArchiveEmail = "campusnewsletterarchive@citavi.com";
    }

    #endregion

    #region FetchXmlConstants

    public static class FetchXmlConstants
    {
        public const string CriteriaPrefix = "criteria_";
        public const string ContactLinkedAccountAliasName = "contact_linkedaccount.new_linkedaccount"; //Anpassen wie "unten"
        public const string ContactLinkedEmailAliasName = "contact_linkedemailaccount.new_linkedemailaccount"; //Anpassen wie "unten"

        public readonly static string CampusContractStatisticCampusContractAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.CampusContractCampusContractStatistic) + "." + CrmEntityNames.CampusContractStatistic;

        public readonly static string ContactLicenseOwnerAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.ContactOwnerLicense) + "." + CrmEntityNames.Contact;
        
        public readonly static string ContactLicenseEndUserAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.ContactEndUserLicense) + "." + CrmEntityNames.Contact;
        public readonly static string ContactLicenseEndUserAliasName2 = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.ContactEndUserLicense) + "." + CrmEntityNames.Contact + ".2";

        public readonly static string ContactProjectRoleAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.ContactProjectRole) + "." + CrmEntityNames.ProjectRole;

        public readonly static string LicenseTypeLicenseAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.LicenseLicenseType) + "." + CrmEntityNames.LicenseType;
        
        public readonly static string LicenseCampusContractAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.LicenseCampusContract) + "." + CrmEntityNames.CampusContract;

        public readonly static string LicenseSubscriptionItemAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.SubscriptionItemLicenses) + "." + CrmEntityNames.SubscriptionItem;

        public readonly static string LicenseOrderProcessAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.LicenseOrderProcess) + "." + CrmEntityNames.OrderProcess;
        public readonly static string LicenseAccountAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.AccountLicense) + "." + CrmEntityNames.Account;

        public readonly static string LicenseEndUserAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.ContactEndUserLicense) + "." + CrmEntityNames.License;

        public readonly static string ProductLicenseAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.LicenseProduct) + "." + CrmEntityNames.Product;
        public readonly static string ProductLicenseAliasName2 = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.LicenseProduct) + "." + CrmEntityNames.Product + ".2";

        public readonly static string PricingLicenseAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.LicensePricing) + "." + CrmEntityNames.Pricing;
        public readonly static string ProjectProjectRoleAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.ProjectProjectRole) + "." + CrmEntityNames.Project;

        public readonly static string ContactVoucherBlockAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.ContactVoucherBlock) + "." + CrmEntityNames.Contact;

        public readonly static string PricingVoucherBlockAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.PricingVoucherBlock) + "." + CrmEntityNames.Pricing;
        public readonly static string LicenseTypeVoucherBlockAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.LicenseTypeVoucherBlock) + "." + CrmEntityNames.LicenseType;

        public readonly static string CampusContractOrganizationSettingAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.OrganizationSettingsCampusContract) + "." + CrmEntityNames.CampusContract;
        public readonly static string LicenseOrganizationSettingAliasName = EntityNameResolver.GetEntityAliasePrefix($"{CrmEntityNames.License}_{CrmEntityNames.OrganizationSetting}") + "." + CrmEntityNames.OrganizationSetting;


        public readonly static string SubscriptionSubscriptionItemAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.SubscriptionSubscriptionItem) + "." + CrmEntityNames.Subscription;
        public readonly static string SubscriptionItemCleverbridgeProductAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.SubscriptionItemCleverbridgeProduct) + "." + CrmEntityNames.CleverbridgeProduct;
        //public static string SubscriptionOrderProcessAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.SubscriptionOrderProcess) + "." + CrmEntityNames.OrderProcess;
        public readonly static string SubscriptionOwnerAliasName = EntityNameResolver.GetEntityAliasePrefix(CrmRelationshipNames.SubscriptionContact) + "." + CrmEntityNames.Contact;
    }

    #endregion

    #region HttpClientNames

    public static class HttpClientNames
    {
        public const string Cleverbridge = "cleverbridge";
    }

	#endregion

	#region HttpHeaderNames

    public static class HttpHeaderNames
	{
        public const string OriginalHostHeaderName = "x-original-host";
        public const string OriginalIPAddress = "x-forwarded-for";
    }

    #endregion

    #region IdentityProviderNames

    public static class IdentityProviderNames
    {
        public const string Google = "Google";
        public const string Facebook = "Facebook";
        public const string Yahoo = "Yahoo";
        public const string Microsoft = "Microsoft";
        public const string Shibboleth = "Shibboleth";
    }

    #endregion

    #region MembershipRebootConstants
    public static class MembershipRebootConstants
    {
        public const string AccountAlreadyVerified = "AccountAlreadyVerified";
        public const string AccountClosed = "AccountClosed";
        public const string AccountCreateFailNoEmailFromIdp = "AccountCreateFailNoEmailFromIdp";
        public const string AccountNotConfiguredWithCertificates = "AccountNotConfiguredWithCertificates";
        public const string AccountNotConfiguredWithMobilePhone = "AccountNotConfiguredWithMobilePhone";
        public const string AccountNotConfiguredWithSecretQuestion = "AccountNotConfiguredWithSecretQuestion";
        public const string AccountNotApproved = "AccountNotApproved";
        public const string AccountNotVerified = "AccountNotVerified";
        public const string AccountPasswordResetRequiresSecretQuestion = "AccountPasswordResetRequiresSecretQuestion";
        public const string AddClientCertForTwoFactor = "AddClientCertForTwoFactor";
        public const string CantRemoveLastLinkedAccountIfNoPassword = "CantRemoveLastLinkedAccountIfNoPassword";
        public const string CertificateAlreadyInUse = "CertificateAlreadyInUse";
        public const string CodeRequired = "CodeRequired";
        public const string EmailAlreadyInUse = "EmailAlreadyInUse";
        public const string EmailRequired = "EmailRequired";
        public const string FailedLoginAttemptsExceeded = "FailedLoginAttemptsExceeded";
        public const string InvalidCertificate = "InvalidCertificate";
        public const string InvalidEmail = "InvalidEmail";
        public const string InvalidKey = "InvalidKey";
        public const string InvalidName = "InvalidName";
        public const string InvalidNewPassword = "InvalidNewPassword";
        public const string InvalidOldPassword = "InvalidOldPassword";
        public const string InvalidPassword = "InvalidPassword";
        public const string InvalidPhone = "InvalidPhone";
        public const string InvalidQuestionOrAnswer = "InvalidQuestionOrAnswer";
        public const string InvalidTenant = "InvalidTenant";
        public const string InvalidUsername = "InvalidUsername";
        public const string InvalidCredentials = "InvalidCredentials";
        public const string LoginFailEmailAlreadyAssociated = "LoginFailEmailAlreadyAssociated";
        public const string LoginNotAllowed = "LoginNotAllowed";
        public const string MobilePhoneAlreadyInUse = "MobilePhoneAlreadyInUse";
        public const string MobilePhoneMustBeDifferent = "MobilePhoneMustBeDifferent";
        public const string MobilePhoneRequired = "MobilePhoneRequired";
        public const string NameAlreadyInUse = "NameAlreadyInUse";
        public const string NameRequired = "NameRequired";
        public const string NewPasswordMustBeDifferent = "NewPasswordMustBeDifferent";
        public const string ParentGroupAlreadyChild = "ParentGroupAlreadyChild";
        public const string ParentGroupSameAsChild = "ParentGroupSameAsChild";
        public const string PasswordComplexityRules = "PasswordComplexityRules";
        public const string PasswordLength = "PasswordLength";
        public const string PasswordRequired = "PasswordRequired";
        public const string PasswordResetErrorNoEmail = "PasswordResetErrorNoEmail";
        public const string RegisterMobileForTwoFactor = "RegisterMobileForTwoFactor";
        public const string ReopenErrorNoEmail = "ReopenErrorNoEmail";
        public const string RejectAlreadyApproved = "RejectAlreadyApproved";
        public const string SecretAnswerRequired = "SecretAnswerRequired";
        public const string SecretQuestionAlreadyInUse = "SecretQuestionAlreadyInUse";
        public const string SecretQuestionRequired = "SecretQuestionRequired";
        public const string TenantRequired = "TenantRequired";
        public const string UsernameAlreadyInUse = "UsernameAlreadyInUse";
        public const string UsernameCannotContainAtSign = "UsernameCannotContainAtSign";
        public const string UsernameOnlyContainsValidCharacters = "UsernameOnlyContainsValidCharacters";
        public const string UsernameCannotRepeatSpecialCharacters = "UsernameCannotRepeatSpecialCharacters";
        public const string UsernameRequired = "UsernameRequired";
        public const string UsernameCanOnlyStartOrEndWithLetterOrDigit = "UsernameCanOnlyStartOrEndWithLetterOrDigit";
        public const string LinkedAccountAlreadyInUse = "LinkedAccountAlreadyInUse";
    }

    #endregion

    #region ChallangeConstants

    public static class ChallangeConstants
    {
        public const string AddLinkedAccount = "addlinkedaccount";
        public const string Action = "citaviaction";
        public const string KentorIdp = "idp";
        public const string Scheme = "scheme";
        public const string ReturnUrl = "returnUrl";
        public const string RemoteIpAddress = "remoteipaddress";
        public const string ShibbolethReturnUrl = "ShibbolethReturnUrl";
        public const string ShibbolethScheme = "Saml2";
        public const string TestingInProduction = "tip";
    }

    #endregion

    #region ShopConstants

    public static class ShopConstants
    {
        public const string BackupCDProductId = "58833";
        public const string BaseQuantity = "baseQuantity";
        public const string CheckoutScope = "checkoutScope";
        public const string Culture = "culture";
        public const string Upgradekey = "upgradeKey";
        public const string ProductKey = "productKey";
        public const string Quantity = "quantity";
        public const string Country = "country";
        public readonly static Uri GenerateUserSessionUrl = new Uri("https://rest.cleverbridge.com/urlgenerator/");
        public readonly static Uri GetProductPriceUrl = new Uri("https://rest.cleverbridge.com/product/");
        public readonly static Uri GraphQLUrl = new Uri("https://graph.cleverbridge.com/graphql");
        public const string CleverbridgeCheckoutUrl = "https://secure.citavi.com/635/?";

        public const string Design_Private = "design2016-b2c";
        public const string Design_Institutionen = "design2016-b2b";
        public const string Design_Institutionen_NoFax = "design2016-b2b_po"; //#3911
    }

    #endregion

    #region ProductCodes

    internal static class ProductCodes
    {
        #region C2Pro

        /// <summary>
        /// Citavi 2 Pro
        /// </summary>
        public const string C2Pro = "c2pro";

        #endregion

        #region C3Pro

        /// <summary>
        /// Citavi 3 Pro
        /// </summary>
        public const string C3Pro = "c3pro";

        #endregion

        #region Citavi 3 Upgrade Pro Team

        /// <summary>
        /// Citavi 3 Upgrade Pro Team
        /// </summary>
        public const string C3UpgradeProTeam = "c3tuc3";

        #endregion

        #region C3Reader

        /// <summary>
        /// Citavi 3 Reader
        /// </summary>
        public const string C3Reader = "c3read";

        #endregion

        #region C3Team

        /// <summary>
        /// Citavi 3 Team
        /// </summary>
        public const string C3Team = "c3team";

        #endregion

        #region C4Pro

        /// <summary>
        /// Citavi 4 Pro
        /// </summary>
        public const string C4Pro = "c4pro";

        #endregion

        #region C4Reader

        /// <summary>
        /// Citavi 4 Reader
        /// </summary>
        public const string C4Reader = "c4read";

        #endregion

        #region Citavi 4 Team

        /// <summary>
        /// Citavi 4 Team
        /// </summary>
        public const string C4Team = "c4team";

        #endregion

        #region Citavi 4 Team (Upgrade Citavi 3 Pro)

        /// <summary>
        /// Citavi 4 Team (Upgrade Citavi 3 Pro)
        /// </summary>
        public const string C4Up3T4 = "c4up3t4";

        #endregion

        #region Citavi 4 Team (Upgrade Citavi 4 Pro)

        /// <summary>
        /// Citavi 4 Team (Upgrade Citavi 4 Pro)
        /// </summary>
        public const string C4Up4T4 = "c4up4t4";

        #endregion

        #region Citavi 4 Pro (Upgrade Citavi 3 Pro)

        /// <summary>
        /// Citavi 4 Pro (Upgrade Citavi 3 Pro)
        /// </summary>
        public const string C4Up3P4 = "c4up3p4";

        #endregion

        #region Citavi 4 Reader (Upgrade Citavi 3 Reader)

        /// <summary>
        /// Citavi 4 Reader (Upgrade Citavi 3 Reader)
        /// </summary>
        public const string C4Ur3R4 = "c4ur3r4";

        #endregion

        #region Citavi 4 Team (Upgrade Citavi 3 Team)

        /// <summary>
        /// Citavi 4 Team (Upgrade Citavi 3 Team)
        /// </summary>
        public const string C4Ut3T4 = "c4ut3t4";

        #endregion

        #region Citavi 5 for DBServer PER SEAT

        /// <summary>
        /// Citavi 5 for DBServer PER SEAT
        /// </summary>
        public const string C5DBServerPerSeat = "c5dbs";

        #endregion

        #region Citavi 5 for DBServer CONCURRENT

        /// <summary>
        /// Citavi 5 for DBServer CONCURRENT
        /// </summary>
        public const string C5DBServerConcurrent = "c5dbc";

        #endregion

        #region Citavi 5 for Windows


        public const string C5Windows = "c5";

        #endregion

        #region Citavi 5 for DBServer CONCURRENT - Business (Upgrade Citavi 5)

        /// <summary>
        /// Citavi 5 for DBServer CONCURRENT - Business (Upgrade Citavi 5)
        /// </summary>
        public const string C5dbcUpC5 = "c5dbcuc5";

        #endregion

        #region Citavi 5 for DBServer SEAT - Business (Upgrade Citavi 5)

        /// <summary>
        /// Citavi 5 for DBServer SEAT - Business (Upgrade Citavi 5)
        /// </summary>
        public const string C5dbsUpC5 = "c5dbsuc5";

        #endregion

        #region Citavi 5 for DBServer CONCURRENT - Business (Upgrade Citavi 4)

        /// <summary>
        /// Citavi 5 for DBServer CONCURRENT - Business (Upgrade Citavi 4)
        /// </summary>
        public const string C5dbcUpC4 = "c5dbcuc4";

        #endregion

        #region Citavi 5 for DBServer CONCURRENT - Business (Upgrade Citavi 3)

        /// <summary>
        /// Citavi 5 for DBServer CONCURRENT - Business (Upgrade Citavi 3)
        /// </summary>
        public const string C5dbcUpC3 = "c5dbcuc3";

        #endregion

        #region Citavi 5 for DBServer SEAT - Business (Upgrade Citavi 4)

        /// <summary>
        /// Citavi 5 for DBServer SEAT - Business (Upgrade Citavi 4)
        /// </summary>
        public const string C5dbsUpC4 = "c5dbsuc4";

        #endregion

        #region Citavi 5 for DBServer SEAT - Business (Upgrade Citavi 3)

        /// <summary>
        /// Citavi 5 for DBServer SEAT - Business (Upgrade Citavi 3)
        /// </summary>
        public const string C5dbsUpC3 = "c5dbsuc3";

        #endregion

        #region Citavi 5 for Windows (Upgrade Citavi 4)

        /// <summary>
        /// Citavi 5 for Windows (Upgrade Citavi 4)
        /// </summary>
        public const string C5UpC4 = "c5upc4";

        #endregion

        #region Citavi 5 for Windows (Upgrade Citavi 3)

        /// <summary>
        /// Citavi 5 for Windows (Upgrade Citavi 3)
        /// </summary>
        public const string C5UpC3 = "c5upc3";

        #endregion

        #region Citavi 5 for DBServer READER

        /// <summary>
        /// Citavi 5 for DBServer READER 100
        /// </summary>
        public const string C5DBServerREADER = "c5dbr1";

        #endregion

        #region Citavi 6 for DBServer CONCURRENT

        /// <summary>
        /// Citavi 6 for DBServer CONCURRENT
        /// </summary>
        public const string C6DBServerCONCURRENT = "c6dbc";

        #endregion

        #region Citavi 6 for DBServer CONCURRENT (Upgrade Citavi 4)

        /// <summary>
        /// Citavi 6 for DBServer CONCURRENT (Upgrade Citavi 4)
        /// </summary>
        public const string C6DBServerCONCURRENTUpgradeC4 = "c6dbcuc4";

        #endregion

        #region Citavi 6 for DBServer CONCURRENT (Upgrade Citavi 5)

        /// <summary>
        /// Citavi 6 for DBServer CONCURRENT (Upgrade Citavi 5)
        /// </summary>
        public const string C6DBServerCONCURRENTUpgradeC5 = "c6dbcuc5";

        #endregion

        #region Citavi 6 for DBServer PER SEAT

        /// <summary>
        /// Citavi 6 for DBServer PER SEAT
        /// </summary>
        public const string C6DBServerPerSeat = "c6dbs";

        #endregion

        #region Citavi 6 for DBServer PER SEAT (Upgrade Citavi 4)

        /// <summary>
        /// Citavi 6 for DBServer PER SEAT (Upgrade Citavi 4)
        /// </summary>
        public const string C6DBServerPerSeatUpgradeC4 = "c6dbsuc4";

        #endregion

        #region Citavi 6 for DBServer PER SEAT (Upgrade Citavi 5)

        /// <summary>
        /// Citavi 6 for DBServer PER SEAT (Upgrade Citavi 5)
        /// </summary>
        public const string C6DBServerPerSeatUpgradeC5 = "c6dbsuc5";

        #endregion

        #region Citavi 6 for DBServer READER

        /// <summary>
        /// Citavi 6 for DBServer READER
        /// </summary>
        public const string C6DBServerReader = "c6dbr";

        #endregion

        #region Citavi 6 for Windows

        /// <summary>
        /// Citavi 6 for Windows
        /// </summary>
        public const string C6Windows = "c6";

        #endregion

        #region Citavi 6 for Windows (Upgrade Citavi 4)

        /// <summary>
        /// Citavi 6 for Windows (Upgrade Citavi 4)
        /// </summary>
        public const string C6WindowsUpgradeC4 = "c6uc4";

        #endregion

        #region Citavi 6 for Windows (Upgrade Citavi 5)

        /// <summary>
        /// Citavi 6 for Windows (Upgrade Citavi 5)
        /// </summary>
        public const string C6WindowsUpgradeC5 = "c6uc5";

        #endregion

        #region Citavi for Windows

        /// <summary>
        /// Citavi for Windows (Subscription) and Citavi Web
        /// </summary>
        public const string CitaviWebAndWin = "cwin";

        #endregion

        #region Citavi Web

        /// <summary>
        /// Citavi Web
        /// </summary>
        public const string CitaviWeb = "cweb";

        #endregion

        #region CitaviSpace

        /// <summary>
        /// Citavi Cloudspace
        /// </summary>
        public const string CitaviSpace = "ccs";

        #endregion

        #region Citavi for DBServer READER Subscription

        /// <summary>
        /// Citavi for DBServer READER Subscription
        /// </summary>
        public const string CitaviDBServerREADERSubscription = "cdbr";

        #endregion

        #region Citavi for DBServer PER SEAT Subscription

        /// <summary>
        /// Citavi for DBServer READER Miete
        /// </summary>
        public const string CitaviDBServerPERSEATSubscription = "cdbs";

        #endregion

        #region Citavi for DBServer CONCURRENT Subscription

        /// <summary>
        /// Citavi for DBServer CONCURRENT Subscription
        /// </summary>
        public const string CitaviDBServerCONCURRENTSubscription = "cdbc";

        #endregion


    }

    #endregion
}
