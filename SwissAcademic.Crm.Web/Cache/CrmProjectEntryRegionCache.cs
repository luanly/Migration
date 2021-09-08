using Microsoft.Azure.Cosmos.Table;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using SwissAcademic.Crm.Web.Query.FetchXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class CrmProjectEntryRegionCache
    {
        #region Eigenschaften

        internal static TableStorageRepository Repo { get; private set; }

        #endregion

        #region Methoden

        #region AddOrUpdateAsync
        internal static Task AddOrUpdateAsync(ProjectEntry entity) => Repo.AddOrUpdateAsync((int)entity.DataCenter, entity?.Key);
        #endregion

        #region InitializeAsync

        public async static Task InitializeAsync()
        {
            Repo = new TableStorageRepository(false);
            await Repo.InitializeAsync(AzureConstants.ProjectEntryRegionCache, multiRegionSupport: true, withInMemoryCache: true);
        }

        #endregion

        #region GetAsync

        public static Task<DataCenter?> GetAsync(string key, bool fromCacheOnly = false, DataCenter? cacheDataCenter = null) => GetAsync(null, key, fromCacheOnly, cacheDataCenter);

        internal static async Task<DataCenter?> GetAsync(CrmDbContext context, string key, bool fromCacheOnly = false, DataCenter? cacheDataCenter = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return DataCenter.WestEurope;
            }

            var disposeContext = false;
            try
            {
                var projectDataCenter = await Repo.GetAsync<DataCenter?>(key, cacheDataCenter);

                if (projectDataCenter == null && !fromCacheOnly)
                {
                    if (context == null)
                    {
                        disposeContext = true;
#pragma warning disable CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                        context = new CrmDbContext();
#pragma warning restore CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                    }
                    var entity = await context.Get<ProjectEntry>(key);
                    if (entity == null)
                    {
                        return DataCenter.WestEurope;
                    }
                    await AddOrUpdateAsync(entity);
                    projectDataCenter = entity.DataCenter;
                }

                return projectDataCenter;
            }
            finally
            {
                if (disposeContext && context != null)
                {
                    context.Dispose();
                }
            }
        }

        #endregion

		#region RemoveAsync

		public static Task RemoveAsync(ProjectEntry entity) => Repo.RemoveAsync(entity?.Key);
        public static Task RemoveAsync(string key) => Repo.RemoveAsync(key);

        #endregion

        #endregion
    }
}
