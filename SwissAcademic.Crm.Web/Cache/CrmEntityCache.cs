using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class CrmEntityCache<T>
       where T : CitaviCrmEntity

    {
        #region Konstruktor

        public CrmEntityCache() :
            this(CrmConfig.CacheRepository)
        {

        }

        public CrmEntityCache(TableStorageRepository repo)
        {
            Repo = repo;
        }

        #endregion

        #region Eigenschaften

        internal TableStorageRepository Repo { get; private set; }

        #endregion

        #region Methoden

        #region AddOrUpdateAsync
        internal Task AddOrUpdateAsync(T entity) => Repo.AddOrUpdateAsync(entity, entity?.Key);
        #endregion

        #region GetAsync

        public Task<T> GetAsync(string key, bool fromCacheOnly = false) => GetAsync(null, key, fromCacheOnly);

        internal async Task<T> GetAsync(CrmDbContext context, string key, bool fromCacheOnly = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var disposeContext = false;
            try
            {
                T entity = await Repo.GetAsync<T>(key);

                if (entity == null && !fromCacheOnly)
                {
                    if (context == null)
                    {
                        disposeContext = true;
#pragma warning disable CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                        context = new CrmDbContext();
#pragma warning restore CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                    }
                    entity = await context.Get<T>(key);
                    if (entity == null)
                    {
                        return null;
                    }

                    await AddOrUpdateAsync(entity);
                }
                if (!disposeContext)
                {
                    context?.Attach(entity);
                }
                return entity;
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

        public Task RemoveAsync(T entity) => Repo.RemoveAsync(entity?.Key);
        public Task RemoveAsync(string key) => Repo.RemoveAsync(key);

        #endregion

        #endregion
    }
}
