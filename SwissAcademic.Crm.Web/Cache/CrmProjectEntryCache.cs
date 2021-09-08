using SwissAcademic.Azure;
using SwissAcademic.Crm.Web.Query.FetchXml;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class CrmProjectEntryCache
    {
        #region Konstruktor

        public CrmProjectEntryCache() :
            this(CrmConfig.CacheRepository)
        {

        }

        public CrmProjectEntryCache(TableStorageRepository repo)
        {
            Repo = repo;
        }

        #endregion

        #region Eigenschaften

        internal TableStorageRepository Repo { get; private set; }

        #endregion

        #region Methoden

        #region AddOrUpdateAsync
        internal Task AddOrUpdateAsync(ProjectEntry entity) => Repo.AddOrUpdateAsync(entity, entity?.Key);
        #endregion

        #region GetAsync

        public Task<ProjectEntry> GetAsync(string key, bool fromCacheOnly = false, DataCenter? dataCenter = null) => GetAsync(null, key, fromCacheOnly, dataCenter);

        internal async Task<ProjectEntry> GetAsync(CrmDbContext context, string key, bool fromCacheOnly = false, DataCenter? dataCenter = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var disposeContext = false;
            try
            {
                var entity = await Repo.GetAsync<ProjectEntry>(key, dataCenter);

                //02.09.2020: Alte Projekte im Cache ohne OwnerKey. Brauchen wir für CloudSpace
                if(entity != null && 
                   string.IsNullOrEmpty(entity.DataContractOwnerContactKey) &&
                   !fromCacheOnly)
				{
                    entity = null;
				}
                if (entity == null && !fromCacheOnly)
                {
                    if (context == null)
                    {
                        disposeContext = true;
#pragma warning disable CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                        context = new CrmDbContext();
#pragma warning restore CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                    }
                    entity = await context.Get<ProjectEntry>(key);
                    if (entity == null)
                    {
                        return null;
                    }

                    var fetchXml = new GetProjectOwner(key).TransformText();
                    var result = await context.Fetch<ProjectRole>(fetchXml);
                    if(result != null && result.Any())
					{
                        entity.DataContractOwnerContactKey = result.First().DataContractContactKey;
                    }
                    await AddOrUpdateAsync(entity);
                    await CrmProjectEntryRegionCache.AddOrUpdateAsync(entity);
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

        public Task RemoveAsync(ProjectEntry entity)
        {
            return RemoveAsync(entity?.Key);
        }
        public async Task RemoveAsync(string key)
        {
            await Repo.RemoveAsync(key);
            await CrmProjectEntryRegionCache.RemoveAsync(key);
        }

        #endregion

        #endregion
    }
}
