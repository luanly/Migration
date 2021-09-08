using SwissAcademic.Azure.Swagger;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web
{
    #region ActiveLoginAccountType

    public enum ActiveLoginAccountType
    {
        IdentityProvider,
        EMail
    }

    #endregion

    #region AssignLicenseResult

    public enum AssignLicenseResult
    {
        NotAllowed,
        UserHasProductAlready,
        Success
    }

    #endregion

    #region AccountPartnerType

    public enum AccountPartnerType
    {
        Reseller = 1,
        KeyAccount = 2
    }

    #endregion

    #region CrmAccessRights

    [Flags]
    public enum CrmAccessRights
    {
        None = 0,
        ReadAccess = 1,
        WriteAccess = 2,
        AppendAccess = 4,
        AppendToAccess = 16,
        CreateAccess = 32,
        DeleteAccess = 65536,
        ShareAccess = 262144,
        AssignAccess = 524288
    }

    #endregion

    #region ClaimTypes

    public static class ClaimTypes
    {
        const string claimTypeNamespace = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims";

        const string anonymous = claimTypeNamespace + "/anonymous";
        const string dns = claimTypeNamespace + "/dns";
        const string email = claimTypeNamespace + "/emailaddress";
        const string hash = claimTypeNamespace + "/hash";
        const string name = claimTypeNamespace + "/name";
        const string rsa = claimTypeNamespace + "/rsa";
        const string sid = claimTypeNamespace + "/sid";
        const string denyOnlySid = claimTypeNamespace + "/denyonlysid";
        const string spn = claimTypeNamespace + "/spn";
        const string system = claimTypeNamespace + "/system";
        const string thumbprint = claimTypeNamespace + "/thumbprint";
        const string upn = claimTypeNamespace + "/upn";
        const string uri = claimTypeNamespace + "/uri";
        const string x500DistinguishedName = claimTypeNamespace + "/x500distinguishedname";

        const string givenname = claimTypeNamespace + "/givenname";
        const string surname = claimTypeNamespace + "/surname";
        const string streetaddress = claimTypeNamespace + "/streetaddress";
        const string locality = claimTypeNamespace + "/locality";
        const string stateorprovince = claimTypeNamespace + "/stateorprovince";
        const string postalcode = claimTypeNamespace + "/postalcode";
        const string country = claimTypeNamespace + "/country";
        const string homephone = claimTypeNamespace + "/homephone";
        const string otherphone = claimTypeNamespace + "/otherphone";
        const string mobilephone = claimTypeNamespace + "/mobilephone";
        const string dateofbirth = claimTypeNamespace + "/dateofbirth";
        const string gender = claimTypeNamespace + "/gender";
        const string ppid = claimTypeNamespace + "/privatepersonalidentifier";
        const string webpage = claimTypeNamespace + "/webpage";
        const string nameidentifier = claimTypeNamespace + "/nameidentifier";
        const string authentication = claimTypeNamespace + "/authentication";
        const string authorizationdecision = claimTypeNamespace + "/authorizationdecision";

        static public string Anonymous { get { return anonymous; } }
        static public string DenyOnlySid { get { return denyOnlySid; } }
        static public string Dns { get { return dns; } }
        static public string Email { get { return email; } }
        static public string Hash { get { return hash; } }
        static public string Name { get { return name; } }
        static public string Rsa { get { return rsa; } }
        static public string Sid { get { return sid; } }
        static public string Spn { get { return spn; } }
        static public string System { get { return system; } }
        static public string Thumbprint { get { return thumbprint; } }
        static public string Upn { get { return upn; } }
        static public string Uri { get { return uri; } }
        static public string X500DistinguishedName { get { return x500DistinguishedName; } }
        static public string NameIdentifier { get { return nameidentifier; } }
        static public string Authentication { get { return authentication; } }
        static public string AuthorizationDecision { get { return authorizationdecision; } }

        // used in info card 
        static public string GivenName { get { return givenname; } }
        static public string Surname { get { return surname; } }
        static public string StreetAddress { get { return streetaddress; } }
        static public string Locality { get { return locality; } }
        static public string StateOrProvince { get { return stateorprovince; } }
        static public string PostalCode { get { return postalcode; } }
        static public string Country { get { return country; } }
        static public string HomePhone { get { return homephone; } }
        static public string OtherPhone { get { return otherphone; } }
        static public string MobilePhone { get { return mobilephone; } }
        static public string DateOfBirth { get { return dateofbirth; } }
        static public string Gender { get { return gender; } }
        static public string PPID { get { return ppid; } }
        static public string Webpage { get { return webpage; } }
    }

	#endregion

	#region AsyncOperationStatus

	public enum AsyncOperationStatus
    {
        WaitingForResources = 0,
        Waiting = 10,
        InProgress = 20,
        Pausing = 21,
        Canceling = 22,
        Succeeded = 30,
        Failed = 31,
        Canceled = 32,
    }

    #endregion

    #region BreezeEntityState

    [Flags]
    public enum BreezeEntityState
    {
        Detached = 1,
        Unchanged = 2,
        Added = 4,
        Deleted = 8,
        Modified = 16,
    }

    #endregion

    #region BreezeAutoGeneratedKeyType

    public enum BreezeAutoGeneratedKeyType
    {
        None,
        Identity,
        KeyGenerator
    }

    #endregion

    #region CampusContractRenewalStateType

    public enum CampusContractRenewalStateType
    {
        /// <summary>
        /// Geplant
        /// </summary>
        Pending = 1,
        /// <summary>
        /// In Ausführung
        /// </summary>
        Executing = 2,
        /// <summary>
        /// Abgeschlossen
        /// </summary>
        Completed = 3,
        /// <summary>
        /// Fehlerhaft
        /// </summary>
        Failed = 4
    }

    #endregion

    #region CampusGroup

    public enum CampusGroup
    {
        Faculty = 1,
        Students = 2
    }

    #endregion

    #region CitaviLicenseType

    public enum CitaviLicenseType
    {
        Free = 0,
        Pro = 1,
        DBServer = 2
    }

    #endregion

    #region CitaviLicensePermission

    public enum CitaviLicensePermission
    {
        Default = 0,
        OnlineRequestCredentialExport = 1
    }

    #endregion

    #region ClientType

    public enum ClientType
	{
        Assistant,
        Desktop,
        Picker,
        Web,
	}

    #endregion

    #region LicenseSubscriptionStatus

    public enum LicenseSubscriptionStatus
    {
        Active = 1,
        Deactivated = 2
    }

    #endregion

    #region ConditionOperator
    public enum ConditionOperator
    {
        //
        // Zusammenfassung:
        //     The values are compared for equality. Value = 0.
        Equal = 0,
        //
        // Zusammenfassung:
        //     The two values are not equal. Value = 1.
        NotEqual = 1,
        //
        // Zusammenfassung:
        //     The value is greater than the compared value. Value = 2.
        GreaterThan = 2,
        //
        // Zusammenfassung:
        //     The value is less than the compared value. Value = 3.
        LessThan = 3,
        //
        // Zusammenfassung:
        //     The value is greater than or equal to the compared value. Value = 4.
        GreaterEqual = 4,
        //
        // Zusammenfassung:
        //     The value is less than or equal to the compared value. Value = 5.
        LessEqual = 5,
        //
        // Zusammenfassung:
        //     The character string is matched to the specified pattern. Value = 6.
        Like = 6,
        //
        // Zusammenfassung:
        //     The character string does not match the specified pattern. Value = 7.
        NotLike = 7,
        //
        // Zusammenfassung:
        //     TheThe value exists in a list of values. Value = 8.
        In = 8,
        //
        // Zusammenfassung:
        //     The given value is not matched to a value in a subquery or a list. Value = 9.
        NotIn = 9,
        //
        // Zusammenfassung:
        //     The value is between two values. Value = 10.
        Between = 10,
        //
        // Zusammenfassung:
        //     The value is not between two values. Value = 11.
        NotBetween = 11,
        //
        // Zusammenfassung:
        //     The value is null. Value = 12.
        Null = 12,
        //
        // Zusammenfassung:
        //     The value is not null. Value = 13.
        NotNull = 13,
        //
        // Zusammenfassung:
        //     The value equals yesterday’s date. Value = 14.
        Yesterday = 14,
        //
        // Zusammenfassung:
        //     The value equals today’s date. Value = 15.
        Today = 15,
        //
        // Zusammenfassung:
        //     The value equals tomorrow’s date. Value = 16.
        Tomorrow = 16,
        //
        // Zusammenfassung:
        //     The value is within the last seven days including today. Value = 17.
        Last7Days = 17,
        //
        // Zusammenfassung:
        //     The value is within the next seven days. Value = 18.
        Next7Days = 18,
        //
        // Zusammenfassung:
        //     The value is within the previous week including Sunday through Saturday. Value
        //     = 19.
        LastWeek = 19,
        //
        // Zusammenfassung:
        //     The value is within the current week. Value = 20.
        ThisWeek = 20,
        //
        // Zusammenfassung:
        //     The value is within the next week. Value = 21.
        NextWeek = 21,
        //
        // Zusammenfassung:
        //     The value is within the last month including first day of the last month and
        //     last day of the last month. Value = 22.
        LastMonth = 22,
        //
        // Zusammenfassung:
        //     The value is within the current month. Value = 23.
        ThisMonth = 23,
        //
        // Zusammenfassung:
        //     The value is within the next month. Value = 24.
        NextMonth = 24,
        //
        // Zusammenfassung:
        //     The value is on a specified date. Value = 25.
        On = 25,
        //
        // Zusammenfassung:
        //     The value is on or before a specified date. Value = 26.
        OnOrBefore = 26,
        //
        // Zusammenfassung:
        //     The value is on or after a specified date. Value = 27.
        OnOrAfter = 27,
        //
        // Zusammenfassung:
        //     The value is within the previous year. Value = 28.
        LastYear = 28,
        //
        // Zusammenfassung:
        //     The value is within the current year. Value = 29.
        ThisYear = 29,
        //
        // Zusammenfassung:
        //     The value is within the next year. Value = 30.
        NextYear = 30,
        //
        // Zusammenfassung:
        //     The value is within the last X hours. Value =31.
        LastXHours = 31,
        //
        // Zusammenfassung:
        //     The value is within the next X (specified value) hours. Value = 32.
        NextXHours = 32,
        //
        // Zusammenfassung:
        //     The value is within last X days. Value = 33.
        LastXDays = 33,
        //
        // Zusammenfassung:
        //     The value is within the next X (specified value) days. Value = 34.
        NextXDays = 34,
        //
        // Zusammenfassung:
        //     The value is within the last X (specified value) weeks. Value = 35.
        LastXWeeks = 35,
        //
        // Zusammenfassung:
        //     The value is within the next X weeks. Value = 36.
        NextXWeeks = 36,
        //
        // Zusammenfassung:
        //     The value is within the last X (specified value) months. Value = 37.
        LastXMonths = 37,
        //
        // Zusammenfassung:
        //     The value is within the next X (specified value) months. Value = 38.
        NextXMonths = 38,
        //
        // Zusammenfassung:
        //     The value is within the last X years. Value = 39.
        LastXYears = 39,
        //
        // Zusammenfassung:
        //     The value is within the next X years. Value = 40.
        NextXYears = 40,
        //
        // Zusammenfassung:
        //     The value is equal to the specified user ID. Value = 41.
        EqualUserId = 41,
        //
        // Zusammenfassung:
        //     The value is not equal to the specified user ID. Value = 42.
        NotEqualUserId = 42,
        //
        // Zusammenfassung:
        //     The value is equal to the specified business ID. Value = 43.
        EqualBusinessId = 43,
        //
        // Zusammenfassung:
        //     The value is not equal to the specified business ID. Value = 44.
        NotEqualBusinessId = 44,
        //
        // Zusammenfassung:
        //     No token name is specified &lt;?Comment AL: Bug fix 5/30/12 2012-05-30T11:03:00Z
        //     Id=&#39;2?&gt;internal&lt;?CommentEnd Id=&#39;2&#39; ?&gt;.
        ChildOf = 45,
        //
        // Zusammenfassung:
        //     The value is found in the specified bit-mask value. Value = 46.
        Mask = 46,
        //
        // Zusammenfassung:
        //     The value is not found in the specified bit-mask value. Value = 47.
        NotMask = 47,
        //
        // Zusammenfassung:
        //     For internal use only. Value = 48.
        MasksSelect = 48,
        //
        // Zusammenfassung:
        //     The string contains another string. Value = 49. You must use the Contains operator
        //     for only those attributes that are enabled for full-text indexing. Otherwise,
        //     you will receive a generic SQL error message while retrieving data. In a Microsoft
        //     Dynamics 365 default installation, only the attributes of the KBArticle (article)
        //     entity are enabled for full-text indexing.
        Contains = 49,
        //
        // Zusammenfassung:
        //     The string does not contain another string. Value = 50.
        DoesNotContain = 50,
        //
        // Zusammenfassung:
        //     The value is equal to the language for the user. Value = 51.
        EqualUserLanguage = 51,
        //
        // Zusammenfassung:
        //     For internal use only.
        NotOn = 52,
        //
        // Zusammenfassung:
        //     The value is older than the specified number of months. Value = 53.
        OlderThanXMonths = 53,
        //
        // Zusammenfassung:
        //     The string occurs at the beginning of another string. Value = 54.
        BeginsWith = 54,
        //
        // Zusammenfassung:
        //     The string does not begin with another string. Value = 55.
        DoesNotBeginWith = 55,
        //
        // Zusammenfassung:
        //     The string ends with another string. Value = 56.
        EndsWith = 56,
        //
        // Zusammenfassung:
        //     The string does not end with another string. Value = 57.
        DoesNotEndWith = 57,
        //
        // Zusammenfassung:
        //     The value is within the current fiscal year . Value = 58.
        ThisFiscalYear = 58,
        //
        // Zusammenfassung:
        //     The value is within the current fiscal period. Value = 59.
        ThisFiscalPeriod = 59,
        //
        // Zusammenfassung:
        //     The value is within the next fiscal year. Value = 60.
        NextFiscalYear = 60,
        //
        // Zusammenfassung:
        //     The value is within the next fiscal period. Value = 61.
        NextFiscalPeriod = 61,
        //
        // Zusammenfassung:
        //     The value is within the last fiscal year. Value = 62.
        LastFiscalYear = 62,
        //
        // Zusammenfassung:
        //     The value is within the last fiscal period. Value = 63.
        LastFiscalPeriod = 63,
        //
        // Zusammenfassung:
        //     The value is within the last X (specified value) fiscal periods. Value = 0x40.
        LastXFiscalYears = 64,
        //
        // Zusammenfassung:
        //     The value is within the last X (specified value) fiscal periods. Value = 65.
        LastXFiscalPeriods = 65,
        //
        // Zusammenfassung:
        //     The value is within the next X (specified value) fiscal years. Value = 66.
        NextXFiscalYears = 66,
        //
        // Zusammenfassung:
        //     The value is within the next X (specified value) fiscal period. Value = 67.
        NextXFiscalPeriods = 67,
        //
        // Zusammenfassung:
        //     The value is within the specified year. Value = 68.
        InFiscalYear = 68,
        //
        // Zusammenfassung:
        //     The value is within the specified fiscal period. Value = 69.
        InFiscalPeriod = 69,
        //
        // Zusammenfassung:
        //     The value is within the specified fiscal period and year. Value = 70.
        InFiscalPeriodAndYear = 70,
        //
        // Zusammenfassung:
        //     The value is within or before the specified fiscal period and year. Value = 71.
        InOrBeforeFiscalPeriodAndYear = 71,
        //
        // Zusammenfassung:
        //     The value is within or after the specified fiscal period and year. Value = 72.
        InOrAfterFiscalPeriodAndYear = 72,
        //
        // Zusammenfassung:
        //     The record is owned by teams that the user is a member of. Value = 73.
        EqualUserTeams = 73,
        //
        // Zusammenfassung:
        //     The record is owned by a user or teams that the user is a member of. Value =
        //     74.
        EqualUserOrUserTeams = 74,
        //
        // Zusammenfassung:
        //     Returns all child records below the referenced record in the hierarchy. Value
        //     = 76.
        Under = 75,
        //
        // Zusammenfassung:
        //     Returns all records not below the referenced record in the hierarchy. Value =
        //     77.
        NotUnder = 76,
        //
        // Zusammenfassung:
        //     Returns the referenced record and all child records below it in the hierarchy.
        //     Value = 79.
        UnderOrEqual = 77,
        //
        // Zusammenfassung:
        //     Returns all records in referenced record&#39;s hierarchical ancestry line. Value
        //     = 75.
        Above = 78,
        //
        // Zusammenfassung:
        //     Returns the referenced record and all records above it in the hierarchy. Value
        //     = 78.
        AboveOrEqual = 79,
        //
        // Zusammenfassung:
        //     When hierarchical security models are used, Equals current user or their reporting
        //     hierarchy. Value = 80.
        EqualUserOrUserHierarchy = 80,
        //
        // Zusammenfassung:
        //     When hierarchical security models are used, Equals current user and his teams
        //     or their reporting hierarchy and their teams. Value = 81.
        EqualUserOrUserHierarchyAndTeams = 81,
        //
        OlderThanXYears = 82,
        //
        OlderThanXWeeks = 83,
        //
        OlderThanXDays = 84,
        //
        OlderThanXHours = 85,
        //
        OlderThanXMinutes = 86,
        //
        ContainValues = 87,
        //
        DoesNotContainValues = 88
    }

    #endregion

    #region ContractType

    public enum ContractType
    {
        Hochschule = 1,
        Forschunseinrichtung = 2,
        LIS_Aktion = 3,
        Landeslizenz = 4
    }

    #endregion

    #region ContractReceivedType

    public enum ContractReceivedType
    {
        Yes = 1,
        No = 2,
        Terminated = 3
    }

    #endregion

    #region CampusBenefitEligibilityType

    public enum CampusBenefitEligibilityType
    {
        /// <summary>
        /// Der Kunde hatte nie eine Campuslizenz
        /// </summary>
        NotApplicable = 0,
        /// <summary>
        /// Der Kunde hat irgendwann mal eine Campuslizenz erhalten
        /// </summary>
        Eligible = 1,
        /// <summary>
        /// Der Kunde hat das Angebot eingelöst
        /// </summary>
        Redeemed = 2
    }

    #endregion

    #region CampusBenefitEligibilityType

    public enum CloudSpaceWarningSentType
    {
        /// <summary>
        /// Keine CloudSpace Warning Mail bis jetzt gesendet
        /// </summary>
        None = 0,
        /// <summary>
        /// 80 % CloudSpace überschritten Mail gesendet
        /// </summary>
        Warning80Percentage = 80,
        /// <summary>
        /// 90 % CloudSpace überschritten Mail gesendet
        /// </summary>
        Warning90Percentage = 90,
        /// <summary>
        /// 95 % CloudSpace überschritten Mail gesendet
        /// </summary>
        Warning95Percentage = 95,
        /// <summary>
        /// 100 % CloudSpace überschritten Mail gesendet
        /// </summary>
        Exceeded = 100,
    }

    #endregion

    #region CrmCacheRepositoryType

    public enum CrmCacheRepositoryType
    {
        InMemory,
        Redis,
        TableStorage
    }

    #endregion

    #region CrmEntityRelationshipType

    public enum CrmEntityRelationshipType
    {
        OneToMany,
        ManyToMany,
        ManyToOne
    }

    #endregion

    #region DeliveryState

    public enum DeliveryState
    {
        Scheduled = 1,
        InProgress = 2,
        Completed = 3,
        Failed = 4,
        Created = 5
    }

    #endregion

    #region EntityState

    //[SwaggerIgnore]
    [SwaggerType("CrmEntityState")]
    public enum EntityState
    {
        Unchanged = 0,
        Created = 1,
        Changed = 2
    }

    #endregion

    #region EmailTemplateType

    public enum EmailTemplateType
    {
        /// <summary>
        /// Um die Einrichtung Ihres Citavi-Accounts abzuschließen, müssen wir nur noch sicherstellen, dass diese E-Mail-Adresse Ihnen gehört.
        /// </summary>
        AccountCreatedVerifyEmail,
        /// <summary>
        /// Willkommen bei Citavi 6
        /// </summary>
        AccountCreatedViaProcessOrderMailBilling_Home,
        AccountCreatedViaProcessOrderMailBilling_Business,
        AccountCreatedViaProcessOrderMailBilling_DbServer,
        /// <summary>
        /// Ihnen wurden vom Administrator Lizenzdaten für Citavi zur Nutzung zugewiesen. Sie wurden in Ihrem Citavi-Account bereitgestellt.
        /// </summary>
        AccountCreatedViaProcessOrderMailLicensee,
        /// <summary>
        /// Vielen Dank für Ihre Bestellung! In der Anlage schicken wir Ihnen den Geschenkgutschein, den Sie als PDF-Datei weiterleiten oder ausdrucken und übergeben können.
        /// </summary>
        CitaviGiftCard,
        /// <summary>
        /// Bestätigen Sie Ihre E-Mail-Adresse
        /// </summary>
        ConfirmEmailAddressMail,
        ConfirmEmailAddressMailWithSignature,
        /// <summary>
        /// Bestätigen Sie Ihre E-Mail-Adresse für neue Campus-Lizenz
        /// </summary>
        ConfirmEmailAddressCampusContractLicenseMail,
        /// <summary>
        /// Der Lizenzadministrator hat Ihnen die Lizenzdaten für Citavi entzogen. Bitte Löschen Sie deshalb die Lizenzdaten in Citavi.
        /// </summary>
        LicenseWithdrawal,
        /// <summary>
        /// Um Ihr Passwort ändern zu können, müssen wir nur noch sicherstellen, dass diese E-Mail-Adresse Ihnen gehört.
        /// </summary>
        PasswordResetRequested,
        /// <summary>
        /// Vielen Dank, dass Sie sich für Citavi entschieden haben! Wir haben Ihre Lizenzdaten in Ihrem Account bereitgestellt. Melden Sie sich an, um sie abzufen.
        /// </summary>
        OrderProcessBilling_Home,
        OrderProcessBilling_Business,
        OrderProcessBilling_DbServer,

        OrderProcessBilling_Billomat_Home,
        OrderProcessBilling_Billomat_Business,
        OrderProcessBilling_Billomat_DbServer,

        /// <summary>
        /// Ihnen wurden vom Administrator Lizenzdaten für Citavi zur Nutzung zugewiesen. Sie wurden in Ihrem Citavi-Account bereitgestellt. Melden Sie sich an, um sie abzurufen.
        /// </summary>
        OrderProcessLicensee,
        /// <summary>
        /// Citavi for DBServer: Alle Concurrent-Lizenzen verbraucht Sie setzten Citavi for DBServer mit Concurrent-Lizenzen ein. Mit dieser E-Mail informieren wir Sie darüber, dass ein Nutzer erfolglos versucht hat, eine Lizenz abzurufen, weil zu dem Zeitpunkt bereits alle Lizenzen genutzt wurden. Sollten Sie diese Benachrichtigung regelmäßig erhalten, empfehlen wir den Kauf weiterer Lizenzen. Hinweise dazu, wie Sie die aktuellen Nutzer der Lizenzen ermitteln, finden Sie im Handbuch.
        /// </summary>
        ConcurrentLicenseExceeded,

        ConfirmProjectInvitation,
        ConfirmProjectInvitationNewUser,

        CloudSpaceExceeded,
        CloudSpaceWarning,

        ProjectRoleChanged,
        ProjectDeleted,

        AccountMerging,
        RecoverDeletedProject,
        /// <summary>
        /// Citavi Mobile: Sends a collection of references by e-mail
        /// </summary>
        SendFormattedReferences,

        BulkEmail,
        
        /// <summary>
        /// Ihre Hochschule oder Organisation hat die Campuslizenz verlängert. Die neuen Lizenzdaten stehen in Ihrem Citavi-Account bereit. Melden Sie sich an, um sie abzurufen.
        /// </summary>
        CampusContractExtension,
        /// <summary>
        /// Ihre Hochschule oder Organisation hat die Campuslizenz verlängert. Die neuen Lizenzdaten stehen in Ihrem Citavi-Account bereit. Melden Sie sich an, um sie abzurufen.
        /// </summary>
        CampusContractExtensionVerifyLicense,
        /// <summary>
        /// Ihre Hochschule oder Organisation hat die Campuslizenz verlängert. Wir haben Ihre Lizenzdaten in Ihrem Account bereitgestellt.
        /// </summary>
        CampusContractExtensionVerifyAccount,
        /// <summary>
        /// Ihre Hochschule oder Organisation hat die Campuslizenz verlängert. Wir haben Ihre Lizenzdaten in Ihrem Account bereitgestellt.
        /// </summary>
        CampusContractExtensionVerifyAccountWithMail,
        /// <summary>
        /// Ihre Hochschule oder Organisation hat die Campuslizenz verlängert. Wir haben Ihre Lizenzdaten in Ihrem Account bereitgestellt.
        /// </summary>
        CampusContractExtensionLoginShibboleth,
        /// <summary>
        /// Ihre Hochschule nutzt eine Campuslizenz unserer Software »Citavi – Wissen organisieren«. Damit Sie aus Citavi heraus eine Einstellungsdatei erstellen können, die u.a. die Zugangsdaten zu den lizenzpflichtigen Fachdatenbanken enthält, benötigen Sie eine Speziallizenz, die wir Ihnen in der Anlage senden.
        /// </summary>
        CampusContractExtensionLicensfileExportData,
        /// <summary>
        /// Ihre Hochschule nutzt eine Campuslizenz unserer Software »Citavi – Wissen organisieren«. Damit Sie Citavi auch auf öffentlichen Rechnern (z.B. in einem Schulungsraum) nutzen können, senden wir Ihnen beiliegend eine Lizenzdatei für  ##productname zu.
        /// </summary>
        CampusContractExtensionLicensfilePublicInstall,
        /// <summary>
        /// An Ihrer Hochschule bieten Sie eine Citavi-Campuslizenz an. Der Lizenzvertrag wurde jetzt verlängert. Alle Citavi-Nutzerinnen und -Nutzer erhalten eine E-Mail, in der sie über die nächsten Schritte informiert werden.
        /// </summary>
        CampusContractExtensionInfoMail,

        CampusNewsletterStatisticMail,

        UserRemovedFromProject,

        SubscriptionLicenseDeactivated,
        SubscriptionLicenseReadOnly
    }

    #endregion

    #region GenderCodeType

    public enum GenderCodeType
    {
        Male = 1,
        Female = 2,
        Unknown = 200000
    }

    #endregion

    #region LanguageType

    public enum LanguageType
    {
        German = 1031,
        English = 1033,
        French = 1036,
        Italian = 1040,
        Polish = 1045,
        Portuguese = 1046,
        Spanish = 1034
    }

    #endregion

    #region LinkedEmailAccountSource

    public enum LinkedEmailAccountSource
    {
        CampusContract,
        IdentityProvider,
        User,
    }

    #endregion

    #region LicenseFileType

    public enum LicenseFileType
    {
        Campus = 3,
        Firma = 2,
        Privat = 1
    }

    #endregion

    #region MergeAccountResult

    public enum MergeAccountResult
    {
        ErrorSameUser,
        VerificationKeyNotFound,
        VerificationKeyExpired,
        Success,
        UserNotFound,
    }

    #endregion

    #region OrderCategory

    public enum OrderCategory
    {
        Standard = 1,
        Neuversand = 2,
        Verlangerung = 3,
        Upgrade = 4,
        Lizenzubertragung = 5,
        Trial = 6
    }

    #endregion

    #region OrganizationSettingConsent

    public enum OrganizationSettingConsent
    {
        /// <summary>
        /// User muss selbständig in den Einstellungen-Dialog und kann die Einstellungen dort importieren
        /// </summary>
        Manual = 0,
        /// <summary>
        /// User kriegt Hinweis, dass er die Einstellungen jetzt importieren kann
        /// </summary>
        OptIn = 1,
        /// <summary>
        /// Einstellungen werden ungefragt importiert
        /// </summary>
        Force = 2
    }

    #endregion

    #region QueryOrderType

    public enum QueryOrderType
    {
        Ascending = 0,
        Descending = 1,
        None = 2
    }

    #endregion

    #region OrderProcessProductGroup

    public enum OrderProcessProductGroup
    {
        Home,
        Business,
        DbServer,
    }

    #endregion

    #region ProjectOnlineStatus

    public enum ProjectOnlineStatus
    {
        Online = 0,
        OfflineByManager = 1,
        OfflineForTechReasons = 2
    }

    #endregion

    #region PersonAffiliationType

    /// <summary>
    /// Specifies the person's affiliation within a particular security domain in broad categories.
    /// http://wiki.aaf.edu.au/tech-info/attributes/edupersonscopedaffiliation
    /// https://www.switch.ch/aai/support/documents/attributes/edupersonaffiliation/
    /// https://www.switch.ch/aai/support/documents/attributes/
    /// http://macedir.org/specs/internet2-mace-dir-eduperson-201305-draft-08.html
    /// </summary>
    [Flags]
    public enum PersonAffiliationType
    {
        None = 0,
        /// <summary>
        /// Relationship with the institution short of full member
        /// </summary>
        Affiliate = 1 << 0,
        /// <summary>
        /// Alumnus/alumna (graduate)
        /// </summary>
        Alum = 1 << 1,
        /// <summary>
        /// Employee other than staff, e.g. contractor
        /// </summary>
        Employee = 1 << 2,
        /// <summary>
        /// Academic or research staff
        /// </summary>
        Faculty = 1 << 3,
        /// <summary>
        /// A person physically present in the library
        /// </summary>
        LibraryWalkIn = 1 << 4,
        /// <summary>
        /// Comprises all the categories named above, plus other members with normal institutional privileges, such as honorary staff or visiting scholar
        /// </summary>
        Member = 1 << 5,
        /// <summary>
        /// All staff
        /// </summary>
        Staff = 1 << 6,
        /// <summary>
        /// Undergraduate or postgraduate student
        /// </summary>
        Student = 1 << 7,
        /// <summary>
        /// All
        /// </summary>
        All = ~(-1 << 8)
    }

    #endregion

    #region SubscriptionStatusType

    //https://docs.cleverbridge.com/public/all/using-the-platform/subscription-statuses-and-update-options.htm#Statuses
    [Serializable]
    public enum SubscriptionStatusType
    {
        /// <summary>
        /// The Active subscription status generally means that the customer has agreed to be billed per interval .
        /// </summary>
        [XmlEnum("ACT")]
        Active = 1,
        /// <summary>
        /// The Deactivated subscription status generally means the customer has canceled a subscription or a refund was processed.
        /// </summary>
        [XmlEnum("DEA")]
        Deactivated = 2,
        /// <summary>
        /// The Finished subscription status generally means that a subscription product was set up in which the checkbox End subscription after this many billing events was selected..
        /// </summary>
        [XmlEnum("FIN")]
        Finished = 3,
        /// <summary>
        /// The Grace subscription status means that cleverbridge didn't receive payment for a subscription but that the customer may still use the product
        /// </summary>
        [XmlEnum("GRA")]
        Grace = 4,
        /// <summary>
        /// The Hold subscription status generally means that cleverbridge didn't received payment for a subscription
        /// </summary>
        [XmlEnum("HLD")]
        Hold = 5,
        /// <summary>
        /// The New subscription status means that the customer signed up for a subscription (initial subscription purchase), but that cleverbridge hasn't received a payment yet.
        /// </summary>
        [XmlEnum("NEW")]
        New = 6,
        /// <summary>
        /// The client controls the entire subscription process. cleverbridge does not handled anything for the client.
        /// </summary>
        [XmlEnum("HBC")]
        HandledByClient = 7,
        /// <summary>
        /// ????
        /// </summary>
        [XmlEnum("OTH")]
        Other = 8,
    }

    #endregion

    #region SubscriptionItemStatusType

    [Serializable]
    public enum SubscriptionItemStatusType
    {
        /// <summary>
        /// Item is currently active.
        /// </summary>
        [XmlEnum("ACT")]
        Active = 1,

        /// <summary>
        /// Item was deactivated.
        /// </summary>
        [XmlEnum("DEA")]
        Deactivated = 2,

        /// <summary>
        /// All required rebilling events are complete for an item.
        /// </summary>
        [XmlEnum("DEA")]
        Finished = 3,

        /// <summary>
        /// Item was removed and cannot be reactivated. For example, the item may have been replaced with another item.
        /// </summary>
        [XmlEnum("REM")]
        Removed = 4,

        /// <summary>
        /// Item is on hold until the customer confirms that the subscription should stay active.
        /// </summary>
        [XmlEnum("WAT")]
        AwaitingReinstate = 5,

        /// <summary>
        /// Handled by Client
        /// </summary>
        [XmlEnum("HBC")]
        HandledByClient = 6,

        /// <summary>
        /// ???
        /// </summary>
        [XmlEnum("OTH")]
        Other = 7,
    }

    #endregion

    #region SubscriptionItemStatusType

    [Serializable]
    public enum SubscriptionRenewalType
    {
        /// <summary>
        /// With Automatic, the subscription automatically renews at the end of the billing interval using the billing information provided by the customer
        /// </summary>
        [XmlEnum("Automatic")]
        Automatic = 1,
        /// <summary>
        /// With Manual, the customer must initiate the renewal at the end of the billing interval.
        /// </summary>
        [XmlEnum("Manual")]
        Manual = 2,
    }

    #endregion

    #region SendVerificationKeyMailResult

    public enum SendVerificationKeyMailResult
    {
        AlreadyVerified,
        EmailAlreadyExists,
        EmailIsNotValid,
        EMailNotFound,
        LicenseNotFound,
        UserNotFound,
        OK
    }

    #endregion

    #region ShopCheckoutScope

    public enum ShopCheckoutScope
    {
        Checkout,
        Quote
    }

    #endregion

    #region ShopProductEdition

    public enum ShopProductEdition
    {
        Unknown = 0,
        Web = 1,
        Windows = 2,
        DbServer = 3,
        Cloudspace = 4,
        Upgrade = 5,
        Maintenance = 6
    }

    #endregion

    #region StateCode

    public enum StateCode
    {
        Active = 0,
        Inactive = 1
    }

    #endregion

    #region StatusCode

    public enum StatusCode
    {
        Active = 1,
        Inactive = 2
    }

    #endregion

    #region SendState

    public enum SendState
    {
        Offen = 1,
        Zugesandt = 2,
        Unterzeichnet = 3
    }

    #endregion

    #region UserLoadContexts
    [Flags]
    public enum UserLoadContexts
    {
        Licenses = 1 << 0,
        LinkedAccounts = 1 << 1,
        LinkedEmailAccounts = 1 << 2,
        VoucherBlocks = 1 << 3,
        ProjectRoles = 1 << 4,

        All = Licenses | LinkedAccounts | LinkedEmailAccounts | VoucherBlocks | ProjectRoles,
    }

    #endregion

    #region YesNoOptionSet

    public enum YesNoOptionSet
    {
        Yes = 1,
        No = 2,
    }

    #endregion

    #region VerifyEmailFromKeyResult

    public enum VerifyEmailFromKeyResult
    {
        NotFound,
        Expired,
        InvalidPassword,
        OK
    }

    #endregion

    #region VerifyEmailSignatureResult

    public enum VerifyEmailSignatureResult
    {
        Error,
        EmailPresentMerge,
        Success
    }

    #endregion

    #region VerificationKeyPurpose
    public enum VerificationKeyPurpose
    {
        ResetPassword = 2,
        ChangeEmail = 3,
        ChangeMobile = 4
    }

    #endregion

    #region VoucherStatus

    public enum VoucherStatus
    {
        Active = 1,
        Redeemed = 2,
        Inactive = 3
    }

    #endregion

    #region VoucherRedeemResultType

    public enum VoucherRedeemResultType
    {
        Success,
        TooManyAttempts,
        VoucherIsNotActive,
        VoucherNotFound,
        Error
    }

    #endregion

    #region WorkflowStateCode

    public enum WorkflowStateCode
    {
        Draft = 0,
        Activated = 1
    }

    #endregion

    #region WorkflowStatusCode

    public enum WorkflowStatusCode
    {
        Draft = 1,
        Activated = 2
    }

    #endregion

    #region WorkflowType

    public enum WorkflowType
    {
        Definition = 1,
        Activation = 2,
        Template = 3
    }

    #endregion

    //Uluru

    #region UluruUpgradeType

    public enum UluruUpgradeType
    {
        None = 0,
        OneVersion = 1,
        TwoVersion = 2,
        Crossgrade = 3,
        Benefit = 4
	}

    #endregion

    #region UluruLicenseType

    public enum UluruLicenseType
    {
        TRIAL = 0,
        PERPETUAL = 1,
        SUBSCRIPTION = 2,
        ELA_NAMED = 3,
        ELA_FTE = 4,
        PERPETUAL_Concurrent = 5,
        PERPETUAL_Seat = 6,
        SUBSCRIPTION_Concurrent = 7,
        SUBSCRIPTION_Seat = 8,
        AUTO_SUBSCRIPTION = 9
    }

    #endregion

    #region UluruContactType
    public enum UluruContactType
    {
        BillTo,
        SoldTo,
    }

    #endregion

    //Citavi Crm Entites

    #region AutoNumberSequencePropertyId

    public enum AutoNumberSequencePropertyId
    {
        EntityName,
        Format,
        Id,
        Key,
        LastNumber
    }

    #endregion

    #region AccountPropertyId

    public enum AccountPropertyId
    {
        Address1_City,
        CampusCities,
        CBPartnerId,
        CBPartnerTyp,
        CBUsername,
        CleverbrigeAffiliateId,
        DataCenter,
        ExclusiveCountries,
        Id,
        Key,
        Name,
        SendCampusStatistic,
        StatusCode,
        StateCode
    }

    #endregion

    #region BulkMailQueryPropertyId

    public enum BulkMailQueryPropertyId
    {
        Description,
        FetchXml,
        Key,
        Id,
        QueryName,
    }

    #endregion

    #region BulkMailTemplatePropertyId

    public enum BulkMailTemplatePropertyId
    {
        Description,
        Key,
        Id,
        IsCampusNewsletter,
        MailTextDE,
        MailTextEN,
        SubjectDE,
        SubjectEN,
        TemplateName
    }

    #endregion

    #region CitaviCampaignPropertyId

    public enum CitaviCampaignPropertyId
    {
        CampaignCode,
        CitaviCampagneName,
        Key,
        Id,
    }

    #endregion

    #region CampusContractPropertyId

    public enum CampusContractPropertyId
    {
        Key,
        CitaviTeamCosts,
        ContractDuration,
        ContractType,
        ContractNumber,
        ContractReceived,
        ContractSignDate,
        CurrentStudents,
        CitaviSpaceInGB,
        DelayC6,
        Id,
        InfoWebsite,
        FirstRequest,
        EternalEmail,
        LeaseTimeYears,
        MailsSendetAt,
        NewContractAvailable,
        NumberOfStudents,
        VoucherOrderUrl,
        RelevantRenewalInfos,
        RssFeedUrl,
        OrderUrl,
        OrganizationName,
        VerifyMaEmail,
        VerifyMaIP,
        VerifyMaVoucher,
        VerifyStEmail,
        VerifyStIP,
        VerifyStVoucher,
        ShibbolethEntityId,
        ShibbolethAlum,
        ShibbolethAffiliate,
        ShibbolethEmployee,
        ShibbolethFaculty,
        ShibbolethLibraryWalkIn,
        ShibbolethMember,
        ShibbolethStudent,
        ShibbolethStaff,
        ShibbolethPersonEntitlement,
        ShowOnWeb,
        StatusCode,
        StateCode,
        Sponsor,
        SourceText
    }

    #endregion

    #region CampusContractRenewalId

    public enum CampusContractRenewalPropertyId
    {
        AISessionId,
        BouncedMails,
        BlockedMails,
        ContractNr,
        EndDate,
        LicenceMigState,
        Id,
        MailsSentCount,
        MigratedLicensesCount,
        QueueItemId,
        QueueItemPopReceipt,
        RenewStatus,
        SendMailDateTime,
        SendMailState,
        StartDate,
        Summiglic
    }

    #endregion

    #region CampusContractStatisticPropertyId

    public enum CampusContractStatisticPropertyId
    {
        Bounces,
        BouncesTotal,
        CreatedOn,
        DailyTotal,
        Desktop_Lastlogin_Daily,
        Desktop_Lastlogin_Monthly,
        Id,
        Key,
        MaCount,
        MaVerifiedCount,
        StdCount,
        StdVerifiedCount,
        TotalCount,
        TotalHistoric,
        TotalVerifiedCount,
        Web_Lastlogin_Daily,
        Web_Lastlogin_Monthly,
        WordAssistant_Lastlogin_Daily,
        WordAssistant_Lastlogin_Monthly,
    }

    #endregion

    #region CleverbridgeProductPropertyId

    public enum CleverbridgeProductPropertyId
    {
        CitaviSpaceInGB,
        IndexForSorting,
        IsResellerProduct,
        Key,
        MaxQuantity,
        MonthsValid,
        Name,
        ProductId,
        ShowInShop
    }

    #endregion

    #region ContactPropertyId

    public enum ContactPropertyId
    {
        Address1_Line1,
        Address1_Line2,
        Address1_PostalCode,
        Address1_City,
        Address1_Country,
        Address1_Telephone1,
        Address1_Telephone2,
        Address1_Fax,
        Address1_StateOrProvince,
        AllowLargeImports,
        CampusBenefitEligibility,
        CloudSpaceWarningSent,
        ClosedDate,
        ContactCreatedByOrdermail,
        CreatedOn,
        DataCenter,
        EMailAddress1,
        FailedLoginCount,
        FailedPasswordResetCount,
        FailedRedeemVoucherCount,
        FirstName,
        Firm,
        FullName,
        GenderCode,
        HashedPassword,
        HasUserSettingsWE,
        Id,
        IsKeyContact,
        IsSasAdmin,
        Key,
        Language,
        LastName,
        LastLogin,
        LastLoginPicker,
        LastLoginCitaviWeb,
        LastLoginWordAssistant,
        LastFailedRedeemVoucher,
        IsVerified,
        IsLoginAllowed,
        LastFailedLogin,
        LastFailedPasswordReset,
        MergeAccountVerificationKey,
        MergeAccountVerificationKeySent,
        NickName,
        PasswordChanged,
        RequiresPasswordReset,
        ResellerAffiliateId,
        Salutation,
        SoftBounceCounter,
        StatusCode,
        StateCode,
        TitelDefinedByContact,
        ZuoraAccountId,
        AzureB2CId
    }

    #endregion

    #region DataOrderProcessingPropertyId

    public enum DataOrderProcessingPropertyId
    {
        ClientAndVersion,
        Id,
        SendState,
        Url,
        Version
    }

    #endregion

    #region DeliveryPropertyId

    public enum DeliveryPropertyId
    {
        BounceMailCount,
        DeliveryName,
        DeliveryState,
        Description,
        Key,
        Id,
        IsCampusNewsletter,
        ProcessEnd,
        ProcessStart,
        QueueItemId,
        QueueItemPopReceipt,
        ReceiverCount,
        SendDate
    }

    #endregion

    #region EmailDomainPropertyId

    public enum EmailDomainPropertyId
    {
        Key,
        EMail_Domain_Or_Campus_Name,
        OrganizationName,
        OrganizationNameForOrderMails,
        StatusCode,
        StateCode
    }

    #endregion

    #region ExternalAccountPropertyId

    public enum ExternalAccountPropertyId
    {
        ADAccess,
        GLAccess,
        Id,
        Name,
        SAAccess,
    }

    #endregion

    #region LinkedAccountPropertyId

    public enum LinkedAccountPropertyId
    {
        EduPersonEntitlement,
        EduPersonScopedAffiliation,
        IdentityProviderId,
        Key,
        LastLogin,
        Name,
        NameIdentifier,
        Id,
        StatusCode,
        StateCode
    }

    #endregion

    #region LinkedEmailAccountPropertyId

    public enum LinkedEmailAccountPropertyId
    {
        BounceStatus,
        CreatedOn,
        Email,
        IsVerified,
        Key,
        LastBounceReason,
        LastLogin,
        VerificationIPAddress,
        VerificationKey,
        VerificationKeySent,
        VerificationPurpose,
        VerificationStorage,
        VerificationVocherCode,
        VerificationCampusGroup,
        Id,
        StatusCode,
        StateCode
    }

    #endregion

    #region LinkedMobilePhonePropertyId

    public enum LinkedMobilePhonePropertyId
    {
        PhoneNumber,
        IsVerified,
        Key,
        LastLogin,
        VerificationKey,
        VerificationKeySent,
        VerificationPurpose,
        VerificationStorage,
        Id,
        StatusCode
    }

    #endregion

    #region LicensePropertyId

    public enum LicensePropertyId
    {
        AllowUnlimitedReaders,
        CampusGroup,
        CitaviKey,
        CitaviLicenseName,
        CitaviSpaceInMB,
        Crm4Id,
        ConcurrentReaderCount,
        ContractExtensionVoucherLicense,
        ExpiryDate,
        FieldsOfStudy,
        Free,
        Language,
        OrderMailSentOn,
        OrganizationName,
        Key,
        Id,
        InvoiceNumber,
        IsVerified,
        IsOrganizationSettingsAdmin,
        OrderCategory,
        OrderDate,
        ReadOnly,
        ServerAmount,
        ServerConcurrent,
        ServerId,
        StatusCode,
        StateCode,
        SubscriptionStatus,
        VerificationKey,
        VerificationStorage,
        VerificationKeySent
    }

    #endregion

    #region LicenseFilePropertyId

    public enum LicenseFilePropertyId
    {
        Ammount,
        ExportAccessFiles,
        Id,
        Key,
        LicenseFileType,
        ShortInfo
    }

    #endregion

    #region LicenseTypePropertyId

    public enum LicenseTypePropertyId
    {
        Key,
        LicenseCode,
        LicenseTypeName,
        StatusCode
    }

    #endregion

    #region IPRangePropertyId

    public enum IPRangePropertyId
    {
        End,
        Key,
        Id,
        IPEnd,
        IPStart,
        OrganizationName,
        Range,
        Start,
        StatusCode,
        StateCode
    }

    #endregion

    #region OrderProcessPropertyId

    public enum OrderProcessPropertyId
    {
        BillingAccountText,
        BillomatCustomerOrderNumber,
        BillomatInvoiceNumber,
        CbOrderPdf,
        CleverBridgeOrderNr,
        DeliveryAccountText,
        Key,
        LicenseAmmount,
        IsReseller,
        OrderDate,
        OrderProcessTrackingNr,
        PartnerID,
        PartnerName,
        StatusCode,
        StateCode,
        XmlRaw
    }

    #endregion

    #region OrganizationSettingPropertyId

    public enum OrganizationSettingPropertyId
    {
        BlobContainerUrl,
        BlobName,
        Consent,
        DataCenter,
        Description,
        Key,
        ModifiedOn,
        Name,
        IsPublic,
        UpdatedOn
    }

    #endregion

    #region ProjectEntryPropertyId

    public enum ProjectEntryPropertyId
    {
        CreatedOn,
        DataCenter,
        DataSource,
        DeletedOn,
        Id,
        InitialCatalog,
        InitialCatalogBeforeMigration,
        Key,
        MinClientVersion,
        Name,
        OnlineStatus,
        StatusCode,
        StateCode,
        ProjectLastModifiedOn,
        RecoveryKey
    }

    #endregion

    #region ProjectRolePropertyId

    public enum ProjectRolePropertyId
    {
        Confirmed,
        ConfirmationKey,
        ConfirmationKeySent,
        ConfirmationKeyStorage,
        ProjectRoleType,
        Key,
        Name,
        Id,
        StatusCode,
        StateCode
    }

    #endregion

    #region PricingPropertyId

    public enum PricingPropertyId
    {
        Key,
        PricingCode,
        PricingName,
        StatusCode
    }

    #endregion

    #region ProductPropertyId

    public enum ProductPropertyId
    {
        Key,
        CitaviProductCode,
        CitaviProductName,
        StatusCode
    }

    #endregion

    #region VoucherPropertyId

    public enum VoucherPropertyId
    {
        Key,
        Id,
        RedeemedOn,
        VoucherCode,
        VoucherCodeInt,
        VoucherCodePre,
        VoucherStatus,
        StatusCode,
        StateCode
    }

    #endregion

    #region VoucherBlockPropertyId

    public enum VoucherBlockPropertyId
    {
        BlockNumber,
        CampusContractVoucher,
        CampusUseVoucherBlockProduct,
        CitaviSpaceInGB,
        CbOrderNummer,
        Key,
        Id,
        NumberOfVouchers,
        OrganizationName,
        ShowInLicenseManagement,
        TempDeact,
        VoucherValidityInMonths,
        StatusCode,
        StateCode
    }

    #endregion

    #region WorkflowPropertyId

    public enum WorkflowPropertyId
    {
        Id,
        Name,
        PrimaryEntity,
        StateCode,
        StatusCode,
        Type
    }

    #endregion

    #region SubscriptionPropertyId

    public enum SubscriptionPropertyId
    {
        AllowReorder,
        CancellationUrl,
        CbSubscriptionId,
        ChangePaymentSubscriptionUrl,
        Id,
        Key,
        StatusCode,
        StateCode
    }

    #endregion

    #region SubscriptionItemPropertyId

    public enum SubscriptionItemPropertyId
    {
        CbSubscriptionItemId,
        DisplayName,
        Id,
        ItemStatus,
        IntervalLengthInDays,
        IntervalLengthInMonths,
        Key,
        LicenseAmount,
        NextBillingDate,
        RenewalType,
        StatusCode,
        StateCode
    }

    #endregion

    #region TeamPropertyId

    public enum TeamPropertyId
    {
        Id,
        Name,
    }

    #endregion

    #region TransactionCurrencyPropertyId

    public enum TransactionCurrencyPropertyId
    {
        Id,
        CurrencyName,
    }

    #endregion

    #region CampusRenewalStep

    public enum CampusRenewalStep
    {
        CloneLicenses,
        SendMail
    }

    #endregion
}
