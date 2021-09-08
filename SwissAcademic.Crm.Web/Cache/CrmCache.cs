using SwissAcademic.ApplicationInsights;
using SwissAcademic.Crm.Web.Query.FetchXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class CrmCache
    {
        #region Felder

        internal static Dictionary<string, CampusContract> _campusContracts = new Dictionary<string, CampusContract>();
        static List<IPRange> _ipRanges = new List<IPRange>();
        static List<EmailDomain> _emailDomains = new List<EmailDomain>();
        internal static volatile bool _refreshCampusContractsPending;

        #endregion

        #region Eigenschaften

        public static IEnumerable<CampusContract> CampusContracts => _campusContracts.Values;
        public static IEnumerable<EmailDomain> EmailDomains => _emailDomains;
        public static IEnumerable<IPRange> IPRanges => _ipRanges;
        public static Dictionary<int, CleverbridgeProduct> CleverbridgeProductsByProductId { get; private set; } = new Dictionary<int, CleverbridgeProduct>();
        public static Dictionary<string, CleverbridgeProduct> CleverbridgeProductsByKey { get; private set; } = new Dictionary<string, CleverbridgeProduct>();
        public static Dictionary<string, LicenseType> LicenseTypesByKey { get; private set; } = new Dictionary<string, LicenseType>();
        public static Dictionary<string, LicenseType> LicenseTypesByCode { get; private set; } = new Dictionary<string, LicenseType>();
        public static Dictionary<string, Pricing> PricingsByKey { get; private set; } = new Dictionary<string, Pricing>();
        public static Dictionary<string, Pricing> PricingsByCode { get; private set; } = new Dictionary<string, Pricing>();
        public static Dictionary<string, Product> Products { get; private set; } = new Dictionary<string, Product>();
        public static Dictionary<string, Product> ProductsByCode { get; private set; } = new Dictionary<string, Product>();

        public static CrmProjectEntryCache Projects { get; private set; } = new CrmProjectEntryCache();

        public static bool IsInitialized { get; private set; }

        static bool _isResetCampusContractsRunning;

        #endregion

        #region Methoden

        #region Initialize

        public static async Task Initialize(bool campusContracts = true, bool cleverbridgeProducts = true)
        {
            var productsTask = ResetProductCache(throwOnError: true);
            var pricingsTask = ResetPricingCache(throwOnError: true);
            var licenseTypesTask = ResetLicenseTypesCache(throwOnError: true);

            await Task.WhenAll(productsTask, pricingsTask, licenseTypesTask);

            if (cleverbridgeProducts)
            {
                await ResetCleverbridgeProductsCache(throwOnError: true);
            }

            if (campusContracts)
            {
                await ResetEmailDomainCache(throwOnError: true);
                await ResetIPRangesCache(throwOnError: true);
                await ResetCampusContractsCache(throwOnError: true);
            }

            IsInitialized = true;
        }

        #endregion

        #region InitializeEntityChangesCache

        static FetchXmlExpression CampusContractProductsChangesQuery;
        internal static async Task InitializeEntityChangesCache(CrmDbContext context)
        {
            await context.RetrieveEntityChanges<EmailDomain>();
            await context.RetrieveEntityChanges<IPRange>();
            await context.RetrieveEntityChanges<CampusContract>();

            var query = new CampusContractIntersecQuery(CrmRelationshipNames.CampusContractProduct).TransformText();
            CampusContractProductsChangesQuery = new FetchXmlExpression(CrmEntityNames.CampusContract, query);

            await CrmWebApi.RetrieveRelationshipChanges(CampusContractProductsChangesQuery);
        }

        #endregion

        #region RefreshCampusContractCache

        internal static async Task RefreshCampusContractCache(CrmDbContext context)
        {
            var pending_cc_changes = new List<string>();
            var emailDomain_changes = await context.RetrieveEntityChanges<EmailDomain>();
            if (emailDomain_changes.Any())
            {
                Telemetry.TrackDiagnostics($"RetrieveEntityChanges (EmailDomain): {emailDomain_changes.Count()}");
                await ResetEmailDomainCache(false);

                foreach (var change in emailDomain_changes)
                {
                    var domain = EmailDomains.FirstOrDefault(e => e.Key == change.Key);
                    if (domain == null)
                    {
                        //Domain wurde gelöscht
                        foreach (var cc in CampusContracts)
                        {
                            if (cc.EmailsDomainsResolved == null)
                            {
                                continue;
                            }

                            var ed = cc.EmailsDomainsResolved.FirstOrDefault(e => e.Key == change.Key);
                            if (ed == null)
                            {
                                continue;
                            }

                            cc.EmailsDomainsResolved.Remove(ed);
                        }
                    }
                    else
                    {
                        var cc = CampusContracts.FirstOrDefault(c => c.DataContractAccountKey == domain.DataContractAccountKey);
                        if (cc == null)
                        {
                            continue;
                        }

                        pending_cc_changes.Add(cc.Key);
                    }
                }
            }

            var ip_changes = await context.RetrieveEntityChanges<IPRange>();
            if (ip_changes.Any())
            {
                Telemetry.TrackDiagnostics($"RetrieveEntityChanges (IPRange): {ip_changes.Count()}");
                await ResetIPRangesCache(false);
                foreach (var change in ip_changes)
                {
                    var ipRange = IPRanges.FirstOrDefault(i => i.Key == change.Key);
                    if (ipRange == null)
                    {
                        //IPRange wurde gelöscht
                        foreach (var cc in CampusContracts)
                        {
                            if (cc.IPRangesResolved == null)
                            {
                                continue;
                            }

                            var ip = cc.IPRangesResolved.FirstOrDefault(e => e.Key == change.Key);
                            if (ip == null)
                            {
                                continue;
                            }

                            cc.IPRangesResolved.Remove(ip);
                        }
                    }
                    else
                    {
                        var cc = CampusContracts.FirstOrDefault(c => c.DataContractAccountKey == ipRange.DataContractAccountKey);
                        if (cc == null)
                        {
                            continue;
                        }

                        pending_cc_changes.Add(cc.Key);
                    }
                }
            }

            var cc_product_changes = await CrmWebApi.RetrieveRelationshipChanges(CampusContractProductsChangesQuery);
            if (cc_product_changes.Any())
            {
                Telemetry.TrackDiagnostics($"RetrieveRelationshipChanges (CampusContractProducts): {cc_product_changes.Count()}");
                foreach (var changes in cc_product_changes)
                {
                    var campusContract = CampusContracts.FirstOrDefault(cc => cc.Id == changes.Entity2Id);
                    if (campusContract == null)
                    {
                        //Kann nie passieren. Das ist ein CC welcher ein Produkt bekommt, wir haben den aber nicht im Cache
                        //-> Ignorieren
                        Telemetry.TrackDiagnostics($"CampusContract not found: {changes.Entity2Id}");
                        continue;
                    }
                    pending_cc_changes.Add(campusContract.Key);
                }
            }

            var cc_changes = await context.RetrieveEntityChanges<CampusContract>();
            pending_cc_changes.AddRange(cc_changes.Select(c => c.Key));

            if (pending_cc_changes.Any())
            {
                pending_cc_changes = pending_cc_changes.Distinct().ToList();
                Telemetry.TrackDiagnostics($"RetrieveEntityChanges (CampusContract): {pending_cc_changes.Count}");
                foreach (var ccKey in pending_cc_changes)
                {
                    await ResetCampusContractsCache(throwOnError: false, campusContractKey: ccKey);
                }
            }
        }

        #endregion

        #region Reset

        public static async Task ResetCampusContractsAsync(string campusContractKey = null)
        {
            var resetCampusContractsSetting = ConfigurationManager.AppSettings["ResetCampusContractsViaServiceBus"];
            var resetCampusContracts = true;

            if (string.IsNullOrEmpty(resetCampusContractsSetting))
            {
                Telemetry.TrackTrace("AppSetting \"ResetCampusContractsViaServiceBus\" is missing. Using default value \"true\"", SeverityLevel.Warning);
            }

            else if (!bool.TryParse(resetCampusContractsSetting, out resetCampusContracts))
            {
                Telemetry.TrackTrace($"AppSetting \"ResetCampusContractsViaServiceBus\" has an invalid value: \"{resetCampusContractsSetting}\". Using default value \"true\"", SeverityLevel.Error);
            }

            if (!resetCampusContracts)
            {
                Telemetry.TrackTrace($"ResetCampusContractsViaServiceBus setting is \"false\" -> skipping ResetCampusContracts", SeverityLevel.Warning);
                return;
            }

            using (Telemetry.StartOperation("ResetCampusContracts"))
            {
                await ResetEmailDomainCache(throwOnError: false);
                await ResetIPRangesCache(throwOnError: false);
                await ResetCampusContractsCache(throwOnError: false, campusContractKey: campusContractKey);
            }
        }

        internal static async Task ResetCampusContractsCache(bool throwOnError = false, string campusContractKey = null)
        {
            if (_isResetCampusContractsRunning)
            {
                return;
            }
            try
            {
                _isResetCampusContractsRunning = true;

                if (string.IsNullOrEmpty(campusContractKey))
                {
                    Telemetry.TrackDiagnostics($"ResetCampusContracts");

                    var cache = new Dictionary<string, CampusContract>();

                    using (var context = new CrmDbContext())
                    {
                        var queryXml = new Query.FetchXml.GetCampusContracts().TransformText();
                        IEnumerable<CitaviCrmEntity> entities;
                        try
                        {
                            entities = await context.Fetch(FetchXmlExpression.Create<CampusContract>(queryXml));
                        }
                        catch (Exception)
                        {
                            await Task.Delay(5000);
                            entities = await context.Fetch(FetchXmlExpression.Create<CampusContract>(queryXml));
                        }
                        var set = new CrmSet(entities);

                        foreach (var item in set.CampusContracts)
                        {
                            if (string.IsNullOrEmpty(item.Key))
                            {
                                Telemetry.TrackTrace("CampusContract ohne Key: " + item.Id, SeverityLevel.Warning);
                                continue;
                            }
                            if (string.IsNullOrEmpty(item.DataContractAccountKey))
                            {
                                Telemetry.TrackTrace("CampusContract ohne AccountKey: " + item.Key, SeverityLevel.Warning);
                                continue;
                            }
                            if (item.ProductsResolved.Count == 0)
                            {
                                var product = await item.LookupLegacyProduct();
                                if (product == null)
                                {
                                    Telemetry.TrackTrace("CampusContract ohne Product: " + item.Key, SeverityLevel.Warning);
                                    continue;
                                }
                                item.ProductsResolved.Add(product);
                            }

                            item.AccountResolved = set.Accounts.FirstOrDefault(i => i._key == item._dataContractAccountKey);
                            item.EmailsDomainsResolved = EmailDomains.Where(i => i._dataContractAccountKey == item._dataContractAccountKey).ToList();

                            item.IPRangesResolved.Clear();
                            item.IPRangesResolved.AddRange(IPRanges.Where(i => i._dataContractAccountKey == item._dataContractAccountKey).ToList());

                            cache.Add(item.Key, item);
                        }
                    }
                    _campusContracts = cache;
                }
                else
                {
                    Telemetry.TrackDiagnostics($"ResetCampusContractByKey: {campusContractKey}");
                    using (var context = new CrmDbContext())
                    {
                        var existing = new Dictionary<string, CampusContract>();
                        _campusContracts.ForEach((k) => existing.Add(k.Key, k.Value));

                        var queryXml = new Query.FetchXml.GetCampusContractByKey(campusContractKey).TransformText();
                        var entities = await context.Fetch(FetchXmlExpression.Create<CampusContract>(queryXml));
                        var set = new CrmSet(entities);
                        var item = set.CampusContracts?.FirstOrDefault();

                        if (item == null ||
                           item.ContractReceived != ContractReceivedType.Yes)
                        {
                            //Campusvertrag gekündigt oder deaktiviert
                            existing.Remove(campusContractKey);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(item.Key))
                            {
                                Telemetry.TrackTrace("CampusContract ohne Key: " + item.Id, SeverityLevel.Warning);
                                return;
                            }
                            if (string.IsNullOrEmpty(item.DataContractAccountKey))
                            {
                                if (!item.Key.StartsWith(CrmConstants.UnitTestCrmEntityKeyPrefix, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    Telemetry.TrackTrace("CampusContract ohne AccountKey: " + item.Key, SeverityLevel.Warning);
                                }
                                return;
                            }
                            if (item.ProductsResolved.Count == 0)
                            {
                                var product = await item.LookupLegacyProduct();
                                if (product == null)
                                {
                                    if (!item.Key.StartsWith(CrmConstants.UnitTestCrmEntityKeyPrefix, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        Telemetry.TrackTrace("CampusContract ohne Product: " + item.Key, SeverityLevel.Warning);
                                    }
                                    return;
                                }
                                item.ProductsResolved.Add(product);
                            }

                            item.AccountResolved = set.Accounts.Where(i => i.Key == item.DataContractAccountKey).FirstOrDefault();
                            item.EmailsDomainsResolved = EmailDomains.Where(i => i.DataContractAccountKey == item.DataContractAccountKey).ToList();

                            item.IPRangesResolved.Clear();
                            item.IPRangesResolved.AddRange(IPRanges.Where(i => i.DataContractAccountKey == item.DataContractAccountKey).ToList());

                            if (existing.ContainsKey(campusContractKey))
                            {
                                existing[campusContractKey] = item;
                            }
                            else
                            {
                                existing.Add(item.Key, item);
                            }
                        }
                        _campusContracts = existing;
                    }
                }
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, flow: ExceptionFlow.Eat);
                if (throwOnError)
                {
                    throw;
                }
            }
            finally
            {
                _isResetCampusContractsRunning = false;
            }
        }

        internal static async Task ResetIPRangesCache(bool throwOnError = false)
        {
            try
            {
                var cache = new List<IPRange>();
                using (var context = new CrmDbContext())
                {
                    var queryXml = new Query.FetchXml.GetIPRanges().TransformText();

                    foreach (var item in await context.Fetch<IPRange>(queryXml))
                    {
                        //Diese Prüfung muss hier sein, damit wird auch die _internal_ Variable abgefüllt
                        if (string.IsNullOrEmpty(item.DataContractAccountKey))
                        {
                            continue;
                        }

                        cache.Add(item);
                    }
                }
                _ipRanges = cache;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, flow: ExceptionFlow.Eat);
                if (throwOnError)
                {
                    throw;
                }
            }
        }

        internal static async Task ResetCleverbridgeProductsCache(bool throwOnError = false)
        {
            try
            {
                var cache = new Dictionary<int, CleverbridgeProduct>();
                var cache2 = new Dictionary<string, CleverbridgeProduct>();
                foreach (var item in await CleverbridgeProduct.GetAll())
                {
                    if (string.IsNullOrEmpty(item.Key))
                    {
                        continue;
                    }

                    cache.Add(item.ProductId, item);
                    cache2.Add(item.Key, item);
                }

                CleverbridgeProductsByProductId = cache;
                CleverbridgeProductsByKey = cache2;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, flow: ExceptionFlow.Eat);
                if (throwOnError)
                {
                    throw;
                }
            }
        }

        internal static async Task ResetEmailDomainCache(bool throwOnError = false)
        {
            try
            {
                var cache = new List<EmailDomain>();
                using (var context = new CrmDbContext())
                {
                    var queryXml = new Query.FetchXml.GetEmailDomains().TransformText();

                    foreach (var item in await context.Fetch<EmailDomain>(queryXml))
                    {
                        //Diese Prüfung muss hier sein, damit wird auch die _internal_ Variable abgefüllt
                        if (string.IsNullOrEmpty(item.DataContractAccountKey))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(item.DataContractAccountName))
                        {
                            continue;
                        }

                        cache.Add(item);
                    }
                }
                _emailDomains = cache;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, flow: ExceptionFlow.Eat);
                if (throwOnError)
                {
                    throw;
                }
            }
        }

        static async Task ResetLicenseTypesCache(bool throwOnError = false)
        {
            try
            {
                var cacheByKey = new Dictionary<string, LicenseType>();
                var cacheByCode = new Dictionary<string, LicenseType>();
                using (var context = new CrmDbContext())
                {
                    foreach (var item in await context.GetAll<LicenseType>())
                    {
                        if (string.IsNullOrEmpty(item.Key))
                        {
                            continue;
                        }

                        cacheByKey.Add(item.Key, item);
                        cacheByCode.Add(item.LicenseCode, item);
                    }
                }
                LicenseTypesByKey = cacheByKey;
                LicenseTypesByCode = cacheByCode;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, flow: ExceptionFlow.Eat);
                if (throwOnError)
                {
                    throw;
                }
            }
        }

        static async Task ResetPricingCache(bool throwOnError = false)
        {
            try
            {
                var cacheByKey = new Dictionary<string, Pricing>();
                var cacheByCode = new Dictionary<string, Pricing>();
                using (var context = new CrmDbContext())
                {
                    foreach (var item in await context.GetAll<Pricing>())
                    {
                        if (string.IsNullOrEmpty(item.Key))
                        {
                            continue;
                        }

                        cacheByKey.Add(item.Key, item);
                        cacheByCode.Add(item.PricingCode, item);
                    }
                }
                PricingsByKey = cacheByKey;
                PricingsByCode = cacheByCode;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, flow: ExceptionFlow.Eat);
                if (throwOnError)
                {
                    throw;
                }
            }
        }

        static async Task ResetProductCache(bool throwOnError = false)
        {
            try
            {
                var cache = new Dictionary<string, Product>();
                var cache2 = new Dictionary<string, Product>();
                using (var context = new CrmDbContext())
                {
                    foreach (var item in await context.GetAll<Product>())
                    {
                        if (string.IsNullOrEmpty(item.Key))
                        {
                            continue;
                        }

                        if (item.CitaviProductCode == "None")
                        {
                            continue;
                        }

                        if (item.CitaviMajorVersion == -1)
                        {
                            Telemetry.TrackTrace($"Was ist das für ein Produkt: {item.CitaviProductName} / {item.Key}");
                        }
                        cache.Add(item.Key, item);
                        cache2.Add(item.CitaviProductCode, item);

                    }
                }
                Products = cache;
                ProductsByCode = cache2;

            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, flow: ExceptionFlow.Eat);
                if (throwOnError)
                {
                    throw;
                }
            }
        }

        #endregion

        #region RemoveProjectAndProjectMembersFromCacheAsync

        public static async Task RemoveProjectAndProjectMembersFromCacheAsync(string projectKey)
        {
            await CrmCache.Projects.RemoveAsync(projectKey);
            using (var context = new CrmDbContext())
            {
                var projectEntry = await context.Get<ProjectEntry>(projectKey);
                var projectRoles = await projectEntry.ProjectRoles.Get();
                foreach (var projectRole in projectRoles)
                {
                    var contact = await projectRole.Contact.Get();
                    await CrmUserCache.RemoveAsync(contact.Key);
                }
            }
        }

        #endregion

        #region ProcessMessage

        public static async Task<bool> ProcessCacheMessageAsync(string label, IDictionary<string, string> properties)
        {
            try
            {
                switch (label)
                {
                    case MessageKey.ProjectRoleChanged:
                    case MessageKey.RemoveFromCrmUserCache:
                        {
                            if (!properties.ContainsKey(MessageKey.ContactKey))
                            {
                                Telemetry.TrackTrace($"CRM:ProcessCacheMessageAsync. ContactKey is missing. {label}", SeverityLevel.Warning);
                                return true;
                            }
                            await CrmUserCache.RemoveAsync(properties[MessageKey.ContactKey].ToString(CultureInfo.InvariantCulture));
                        }
                        return true;

                    case MessageKey.ProjectEntryChanged:
                        {
                            if (!properties.ContainsKey(MessageKey.ProjectKey))
                            {
                                Telemetry.TrackTrace($"CRM:ProcessCacheMessageAsync. ProjectKey is missing. {label}", SeverityLevel.Warning);
                                return true;
                            }
                            var projectKey = properties[MessageKey.ProjectKey].ToString(CultureInfo.InvariantCulture);
                            await RemoveProjectAndProjectMembersFromCacheAsync(projectKey);
                        }
                        return true;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, flow: ExceptionFlow.Eat, property1: (nameof(TelemetryProperty.Description), $"{label}:\r\n{properties.ToString("\r\n")}"));
                return false;
            }
        }


        #endregion

        #endregion
    }
}
