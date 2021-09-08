using Microsoft.Azure.Cosmos.Table;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class CrmConfig
    {
        #region Felder

        public readonly static bool RequireAccountVerification;
        public readonly static int AccountLockoutFailedLoginAttempts;
        public readonly static TimeSpan AccountLockoutDuration;
        public readonly static int PasswordHashingIterationCount;
        public readonly static int PasswordResetFrequency;
        public readonly static TimeSpan VerificationKeyLifetime;
        public readonly static TimeSpan VerificationKeyLockoutDuration;
        public readonly static DefaultCrypto Crypto;

		#endregion

		#region Konstruktor

		static CrmConfig()
		{
            //https://brockallen.com/2014/02/09/how-membershipreboot-stores-passwords-properly/
            PasswordHashingIterationCount = 10215;
            RequireAccountVerification = true;
            AccountLockoutFailedLoginAttempts = 10;
            AccountLockoutDuration = TimeSpan.FromMinutes(5);
            VerificationKeyLifetime = CrmMembershipRebootConstants.VerificationKeyLifetime;
            VerificationKeyLockoutDuration = TimeSpan.FromMinutes(1);
            Crypto = new DefaultCrypto();
        }

        #endregion

        #region Eigenschaften

        #region AzureStorageResolver

        public static IAzureStorageResolver AzureStorageResolver { get; private set; }

        #endregion

        #region AblyCallback

        public static Func<string, IDictionary<string, string>, Task> AblyCallback { get; private set; }

        #endregion

        #region BetaLicenseProductCode

        static string _betaLicenseProductCode;
        public static string BetaLicenseProductCode
        {
            get
            {
                if (!string.IsNullOrEmpty(_betaLicenseProductCode))
                {
                    return _betaLicenseProductCode;
                }

                _betaLicenseProductCode = ConfigurationManager.AppSettings["BetaLicenseProductCode"];

                return _betaLicenseProductCode;
            }
            set
            {
                _betaLicenseProductCode = value;
            }
        }
        static Product _betaLicenseProduct;
        public static Product BetaLicenseProduct
        {
            get
            {
                if (string.IsNullOrEmpty(BetaLicenseProductCode))
                {
                    return null;
                }

                if (_betaLicenseProduct != null)
                {
                    return _betaLicenseProduct;
                }

                _betaLicenseProduct = CrmCache.Products.Where(product => string.Equals(product.Value.CitaviProductCode, BetaLicenseProductCode, StringComparison.InvariantCultureIgnoreCase))
                                                             .Select(keyValue => keyValue.Value).FirstOrDefault();
                return _betaLicenseProduct;
            }
            set
            {
                _betaLicenseProduct = value;
            }
        }


        #endregion

        #region CacheRepository

        static TableStorageRepository _cacheRepository;

        public static TableStorageRepository CacheRepository
        {
            get
            {
                if (_cacheRepository == null)
                {
                    _cacheRepository = new TableStorageRepository(isComplexObject: true);
                }
                return _cacheRepository;
            }
            set
            {
                _cacheRepository = value;
            }
        }

        #endregion

        #region CleanUpDeletedProjectsAfterMinutes

        static TimeSpan _cleanUpDeletedProjectsTimeSpan = TimeSpan.MinValue;
        public static TimeSpan CleanUpDeletedProjectsAfter
        {
            get
            {
                if (_cleanUpDeletedProjectsTimeSpan == TimeSpan.MinValue)
                {
                    int minutes;
                    if (!int.TryParse(ConfigurationManager.AppSettings["CleanUpDeletedProjectsAfterMinutes"], out minutes))
                    {
                        _cleanUpDeletedProjectsTimeSpan = CrmConstants.CleanUpDeletedProjectsAfterDefault;
                    }
                    else
                    {
                        _cleanUpDeletedProjectsTimeSpan = TimeSpan.FromMinutes(minutes);
                    }
                }
                return _cleanUpDeletedProjectsTimeSpan;
            }
            set
            {
                _cleanUpDeletedProjectsTimeSpan = value;
            }
        }

        #endregion

        #region CurrentLicenseMajorVersion

        public const int CurrentLicenseMajorVersion = 6;

        #endregion

        #region calculateCitaviSpace

        internal static Func<string, bool, Task> CalculateCitaviSpace;

        #endregion

        #region IsAlphaOrDev

        public static bool IsAlphaOrDev { get; private set; }

        #endregion

        #region IsUnittest

        public static bool IsUnittest { get; private set; }

        #endregion

        #region MaxLicenseMajorVersion

        static int _maxLicenseMajorVersion = -1;
        /// <summary>
        /// Shop: Alle Produkte kleiner dieser Version werden nicht angezeigt
        /// User: Alle Lizenzen kleiner dieser Version werden nicht verwendet (Ausnahme Beta Lizenz wenn gesetzt)
        /// </summary>
        public static int MaxLicenseMajorVersion
        {
            get
            {
                if (_maxLicenseMajorVersion != -1)
                {
                    return _maxLicenseMajorVersion;
                }

                if (!int.TryParse(ConfigurationManager.AppSettings["MaxLicenseMajorVersion"], out _maxLicenseMajorVersion))
                {
                    _maxLicenseMajorVersion = CurrentLicenseMajorVersion;
                }

                return _maxLicenseMajorVersion;
            }
            set
            {
                _maxLicenseMajorVersion = value;
            }
        }



        #endregion

        #region LegacyFreeLicensePeriodEndDate

        /// <summary>
        /// After this date, users will no longer receive a free license
        /// </summary>
        public static DateTime LegacyFreeLicensePeriodEndDate { get; private set; }

        #endregion

        #region BetaLicensePeriodEndDate

        /// <summary>
        /// After this date, beta users need a web license
        /// </summary>
        public static DateTime BetaLicensePeriodEndDate { get; private set; }

        #endregion

        #region DesktopClientWithLegacyFreeLicenseSupportMinVersion

        /// <summary>
        /// DesktopClients with this version number or higher get the free license via api calls
        /// Older clients cannot work with the new legacy free license
        /// </summary>
        public static Version DesktopClientWithLegacyFreeLicenseSupportMinVersion { get; private set; }

        #endregion

        #region IsShopWebAppSubscriptionAvailable

        static bool? _isShopWebAppSubscriptionAvailable;
        /// <summary>
        /// Wenn True werden im Shop auch die Produkte Citavi Web u. Cloudspace angezeigt
        /// Kann mit Release von C-Web gelöscht werden
        /// Test: Auf Alpha-Dev true, auf Alpha false
        /// </summary>
        public static bool IsShopWebAppSubscriptionAvailable
        {
            get
            {
                if (!_isShopWebAppSubscriptionAvailable.HasValue)
                {
                    _isShopWebAppSubscriptionAvailable = string.Equals(ConfigurationManager.AppSettings["IsShopWebAppSubscriptionAvailable"], bool.TrueString, StringComparison.InvariantCultureIgnoreCase);
                }

                return _isShopWebAppSubscriptionAvailable.Value;
            }
            set
            {
                _isShopWebAppSubscriptionAvailable = value;
            }
        }

        #endregion

        #region SubscriptionReadOnlyPeriodInDays

        public static int SubscriptionReadOnlyPeriodInDays { get; private set; }

        #endregion

        #endregion

        #region Methods

        public static async Task Initialize(CrmConfigSet set, IAzureStorageResolver azureStorageResolver, Func<string, bool, Task> calculateCitaviSpace)
        {
            using (var op = Telemetry.StartOperation("CrmConfig Initialize"))
            {
                CalculateCitaviSpace = calculateCitaviSpace;
                AzureStorageResolver = azureStorageResolver;
                IsAlphaOrDev = ConfigurationManager.AppSettings["SlotName"] == null || ConfigurationManager.AppSettings["SlotName"].Contains("Alpha") || ConfigurationManager.AppSettings["SlotName"].Contains("Dev");

                IsUnittest = StringComparer.OrdinalIgnoreCase.Equals(ConfigurationManager.AppSettings["HostingEnvironment"], "UnitTest");

                SubscriptionReadOnlyPeriodInDays = 60;

                if (IsUnittest)
                {
                    LegacyFreeLicensePeriodEndDate = new DateTime(2021, 3, 19, 0, 0, 0, DateTimeKind.Utc);
                }
                else if(Environment.Build == BuildType.Alpha)
                {
                    LegacyFreeLicensePeriodEndDate = new DateTime(2021, 5, 1, 0, 0, 0, DateTimeKind.Utc);
                    SubscriptionReadOnlyPeriodInDays = 2;
                }
                else
                {
                    LegacyFreeLicensePeriodEndDate = new DateTime(2021, 5, 29, 0, 0, 0, DateTimeKind.Utc);
                }

                BetaLicensePeriodEndDate = new DateTime(2021, 10, 9, 0, 0, 0, DateTimeKind.Utc);

                DesktopClientWithLegacyFreeLicenseSupportMinVersion = new Version(6, 9, 1, 0);

                _cacheRepository = null;

                EntityNameResolver.Initialize();
                await EmailClient.InitializeAsync(set);

                AzureHelper.InitializeStorageAccount(azureStorageResolver);

                if (set.Queues)
                {
                    AblyCallback = set.AblyCallback;
                    await AzureHelper.ConfigureQueuesAsync(AblyCallback);
                    await SaveChangesQueue.InitalizeAsync();
                }

                #region CrmWebApi

                if (!string.IsNullOrEmpty(set.CrmWebApiClientId))
                {
                    await CrmWebApi.Connect(set.CrmWebApiClientId, set.CrmWebApiServiceAccounts, set.CrmConnectionUrl);
                }
                else
                {
                    await CrmWebApi.Connect();
                }

                #endregion

                #region Cache

                if (set.CrmCache)
                {
                    await CrmCache.Initialize(set.CrmCacheCampusContracts);
                }
                if (set.CrmUserCache)
                {
                    await CacheRepository.InitializeAsync(AzureConstants.CrmCacheTable, multiRegionSupport: true);
                    await CrmUserImageCache.Repo.InitializeAsync(AzureConstants.CrmCacheUserImagesBlobContainer, multiRegionSupport: true);
                    await CitaviSpaceCache.InitializeAsync(set);
                    await CrmProjectEntryRegionCache.InitializeAsync();
                    await AzureB2CObjectIdCache.InitializeAsync();
                }
                if (set.IdentityServerCache)
                {
                    await TableStorageCleanup.InitializeAsync();
                    await PersistedGrantStore.InitialzeAsync();
                    await DistributedCacheStore.InitializeAsync();
                    await CookieStore.InitialzeAsync();
                }

                #endregion

                await TableStorageLock.InitializeAsync(azureStorageResolver.GetCloudTableWestEurope());
                await ShibbolethMetadataLoader.Initialize();
            }
        }

        #endregion
    }

    public class CrmConfigSet
    {
        public IEmailClient EmailClient { get; set; }

        public string CrmWebApiClientId { get; set; }
        public string[] CrmWebApiServiceAccounts { get; set; }
        public string CrmConnectionUrl { get; set; }
        public Microsoft.Azure.Storage.CloudStorageAccount BlobStorageAccount { get; set; }
        public Microsoft.Azure.Cosmos.Table.CloudStorageAccount TableStorageAccount { get; set; }
        /// <summary>
        /// Cache für Products, Pricings, LicenseTypes
        /// </summary>
        public bool CrmCache { get; set; }
        /// <summary>
        /// Cache für CleverbridgeProducts
        /// </summary>
        public bool CrmCacheCleverbridgeProducts { get; set; }
        /// <summary>
        /// Cache für Campusverträge inkl. Refresh bei Änderungen im CRM
        /// </summary>
        public bool CrmCacheCampusContracts { get; set; }
        /// <summary>
        /// Cache für Users, Projects, ect. CrmUserCache-Instanz|CrmCache.Projects
        /// </summary>
        public bool CrmUserCache { get; set; }

        /// <summary>
        /// SaveChangesQueue und weitere Queues
        /// </summary>
        public bool Queues { get; set; }

        public bool IdentityServerCache { get; set; }

        public bool TableStorgeWithInMemoryCache { get; set; }

        public Func<string, IDictionary<string, string>, Task> AblyCallback { get; set; }

        public static CrmConfigSet Default(Func<string, IDictionary<string, string>, Task> ablyCallback)
        {
            var set = new CrmConfigSet();
            set.CrmCache = true;
            set.CrmCacheCampusContracts = true;
            set.IdentityServerCache = true;
            set.AblyCallback = ablyCallback ?? ((s1, s2) => Task.CompletedTask);
            set.CrmUserCache = true;
            set.CrmCacheCleverbridgeProducts = true;
            set.EmailClient = new SendGridEmailClient();

            if(ConfigurationManager.AppSettings["MailClient"] == "TipiMail")
            {
                set.EmailClient = new TipiMailClient();
            }

            set.TableStorgeWithInMemoryCache = false;
            set.Queues = true;
            return set;
        }

        /// <summary>
        /// Ohne: Ably, CampusContracts, CleverbridgeProcucts
        /// </summary>
        /// <returns></returns>
        public static CrmConfigSet CrmFunctions()
        {
            var set = Default(null);
            set.CrmCacheCleverbridgeProducts = false;
            set.CrmCacheCampusContracts = false;
            return set;
        }
        /// <summary>
        /// Ohne: Ably, CampusContracts, CleverbridgeProcucts
        /// </summary>
        /// <returns></returns>
        public static CrmConfigSet CrmLongRunningFunctions()
        {
            var set = Default(null);
            set.CrmCacheCleverbridgeProducts = false;
            set.CrmCacheCampusContracts = false;
            return set;
        }

        /// <summary>
        /// Ohne: Ably, CampusContracts, CleverbridgeProcucts
        /// </summary>
        /// <returns></returns>
        public static CrmConfigSet CloudSearch()
        {
            var set = Default(null);
            set.CrmCacheCleverbridgeProducts = false;
            set.CrmCacheCampusContracts = false;
            return set;
        }

        /// <summary>
        /// Ohne: Ably, CampusContracts, CleverbridgeProcucts
        /// </summary>
        /// <returns></returns>
        public static CrmConfigSet WebJobs()
        {
            var set = Default(null);
            set.CrmCacheCleverbridgeProducts = false;
            set.CrmCacheCampusContracts = false;
            return set;
        }

        /// <summary>
        /// Ohne: SaveChangesQueue, Ably, Email
        /// </summary>
        /// <returns></returns>
        public static CrmConfigSet LoadTests()
        {
            var set = Default(null);
            set.CrmCacheCleverbridgeProducts = false;
            set.EmailClient = new UnitTestEmailClient();
            set.Queues = false;
            return set;
        }


        public static CrmConfigSet UnitTests(Func<string, IDictionary<string, string>, Task> ablyCallback = null)
        {
            var set = Default(ablyCallback);
            set.EmailClient = new UnitTestEmailClient();
            return set;
        }

        public static CrmConfigSet MigrationTool(string crmWebApiClientId, string[] crmWebApiServiceAccounts, string crmConnectionUrl, Microsoft.Azure.Cosmos.Table.CloudStorageAccount tableStorageAccount, Microsoft.Azure.Storage.CloudStorageAccount blobStorageAccount)
        {
            var set = new CrmConfigSet();
            set.CrmWebApiClientId = crmWebApiClientId;
            set.CrmWebApiServiceAccounts = crmWebApiServiceAccounts;
            set.CrmConnectionUrl = crmConnectionUrl;
            set.TableStorageAccount = tableStorageAccount;
            set.BlobStorageAccount = blobStorageAccount;
            set.CrmUserCache = true;
            set.Queues = false;
            return set;
        }
    }
}
