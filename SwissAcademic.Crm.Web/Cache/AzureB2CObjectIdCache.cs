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
    public static class AzureB2CObjectIdCache
    {
        #region Eigenschaften

        internal static TableStorageRepository Repo { get; private set; }

        #endregion

        #region Methoden

        #region AddOrUpdateAsync
        internal static Task AddOrUpdateAsync(string azureB2CObjectId, string citaviKey) => Repo.AddOrUpdateAsync(citaviKey, azureB2CObjectId);
        #endregion

        #region InitializeAsync

        public async static Task InitializeAsync()
        {
            Repo = new TableStorageRepository(false);
            await Repo.InitializeAsync("AzureB2CObjectIdCache", multiRegionSupport: false, withInMemoryCache: true);
        }

        #endregion

        #region GetAsync

        public static Task<string> GetAsync(string key) => GetAsync(null, key);

        internal static async Task<string> GetAsync(CrmDbContext context, string key)
        {
            var disposeContext = false;
            try
            {
                var citaviKey = await Repo.GetAsync<string>(key);

                if (string.IsNullOrEmpty(citaviKey))
                {
                    if (context == null)
                    {
                        disposeContext = true;
#pragma warning disable CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                        context = new CrmDbContext();
#pragma warning restore CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                    }
                    var entity = await context.Get<Contact>(ContactPropertyId.AzureB2CId, key, ContactPropertyId.Key);
                    if (entity == null)
                    {
                        return null;
                    }
                    citaviKey = entity.Key;
                    await AddOrUpdateAsync(key, citaviKey);
                }

                return citaviKey;
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

        public static Task RemoveAsync(string key) => Repo.RemoveAsync(key);

        #endregion

        #endregion
    }
}
