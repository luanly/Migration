using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using System;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class CrmUserCache
    {
        #region Konstruktor

        static CrmUserCache()
        {

        }

        #endregion

        #region Eigenschaften

        internal static TableStorageRepository Repo => CrmConfig.CacheRepository;

        #endregion

        #region Methoden

        #region AddOrUpdateAsync

        internal static Task AddOrUpdateAsync(CrmUser user) => Repo.AddOrUpdateAsync(user, user.Key);

        #endregion

        #region GetAsync
        public static Task<CrmUser> GetAsync(string key, bool fromCacheOnly = false, DataCenter? dataCenter = null) => GetAsync(null, key, fromCacheOnly, true, dataCenter);
        public static async Task<CrmUser> GetAsync(CrmDbContext context, string identifier, bool fromCacheOnly = false, bool updateCacheIfMissing = true, DataCenter? dataCenter = null)
        {
            var disposeContext = false;
            CrmUser user = null;
            try
            {
                if (context == null)
                {
                    disposeContext = true;
#pragma warning disable CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                    context = new CrmDbContext();
#pragma warning restore CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                }
                return await context.GetUserFromCrm(ContactPropertyId.Key, identifier);
            }
            finally
            {
                if (disposeContext)
                {
                    context.Dispose();
                }
            }
        }

        public static async Task<CitaviCrmEntity> GetCrmEntityAsync(CrmDbContext context, string identifier, bool fromCacheOnly = false, bool updateCacheIfMissing = true, DataCenter? dataCenter = null)
        {
            var disposeContext = false;
            CrmUser user = null;
            try
            {
                if (context == null)
                {
                    disposeContext = true;
#pragma warning disable CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                    context = new CrmDbContext();
#pragma warning restore CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                }
                return await context.GetCitaviCrmEntityFromCrm(ContactPropertyId.Key, identifier);
            }
            finally
            {
                if (disposeContext)
                {
                    context.Dispose();
                }
            }
        }

        #endregion

        #region RefreshAsync

        public static async Task RefreshAsync(string contactKey)
        {
            try
            {
                using (var context = new CrmDbContext())
                {
                    await RemoveAsync(contactKey);
                    var user = await context.GetUserFromCrm(ContactPropertyId.Key, contactKey);
                    if (user != null)
                    {
                        await AddOrUpdateAsync(user);
                    }
                }
            }
            catch(Exception ex)
            {
                // Telemetry.TrackException(ex, SeverityLevel.Warning, ExceptionFlow.Eat);
            }
        }

        #endregion

        #region RemoveAsync

        public static Task RemoveAsync(CrmUser user) => Repo.RemoveAsync(user.Key);

        public static Task RemoveAsync(string key) => Repo.RemoveAsync(key);

        public static async Task RemoveAsync(string[] keys)
        {
            foreach (var key in keys)
            {
                await Repo.RemoveAsync(key);
            }
        }

        #endregion

        #endregion
    }
}
