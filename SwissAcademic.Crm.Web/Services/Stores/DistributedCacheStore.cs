using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Azure.Cosmos.Table;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using SwissAcademic.Security;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace SwissAcademic.Crm.Web
{
    public class DistributedCacheStore
        :
        IDistributedCache
    {
        #region Felder


        #endregion

        #region Konstruktor

        public DistributedCacheStore()
        {
            
        }

        #endregion

        #region Eigenschaften

        protected static TableStorageRepository Repo { get; set; }

        #endregion

        #region Methoden

        #region BuildQuery

        TableOperation BuildGetQuery(string key)
        {
            key = WebUtility.UrlEncode(key);

            return TableOperation.Retrieve<DistributedCachedItem>(BuildPartitionKey(key), key, DistributedCachedItem.ColumnNames);
        }

        #endregion

        #region BuildPartitionKey

        public static string BuildPartitionKey(string key)
        {
            var pk = key.Substring(0, 2);
            if (CrmConfig.IsUnittest)
            {
                pk = PasswordGenerator.Default.Prefix + pk;
            }
            return pk;
        }

        #endregion

        #region Get

        public byte[] Get(string key)
        {
            try
            {
                var tableResult = Repo.Execute(BuildGetQuery(key));
                if(tableResult == null)
                {
                    return null;
                }

                var item = tableResult.Result as DistributedCachedItem;
                if (item == null)
                {
                    return null;
                }
                if (item.IsExpired)
                {
                    item.ETag = "*";
                    Repo.Execute(TableOperation.Delete(item));
                    return null;
                }
                return item.Data;
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404) { }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
            }
            return null;
        }

		public async Task<byte[]> GetAsync(string key, CancellationToken token)
		 => await GetAsync(key, token, null);
		public async Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken), DataCenter? dataCenter = null)
        {
            try
            {
                var tableResult = await Repo.ExecuteAsync(BuildGetQuery(key));
                if (tableResult == null)
                {
                    return null;
                }
                var item = tableResult.Result as DistributedCachedItem;
                if (item == null)
                {
                    return null;
                }
                if (item.IsExpired)
                {
                    item.ETag = "*";
                    await Repo.ExecuteAsync(TableOperation.Delete(item));
                    return null;
                }
                return item.Data;
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404) { }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
            }
            return null;
        }

        #endregion

        #region InitializeAsync

        public static async Task InitializeAsync()
        {
            Repo = new TableStorageRepository(false);
            await Repo.InitializeAsync(AzureConstants.DistributedCache, multiRegionSupport: true);
        }

        #endregion

        #region Set
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            try
            {
                key = WebUtility.UrlEncode(key);
                var item = new DistributedCachedItem(options, value);
                item.PartitionKey = BuildPartitionKey(key);
                item.RowKey = key;
                Repo.Execute(TableOperation.InsertOrReplace(item));
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat);
            }
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            try
            {
                key = WebUtility.UrlEncode(key);
                var item = new DistributedCachedItem(options, value);
                item.PartitionKey = BuildPartitionKey(key);
                item.RowKey = key;
                await Repo.ExecuteAsync(TableOperation.InsertOrReplace(item));
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat);
            }
        }

        public async Task SetAsync(string key, string value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            try
            {
                key = WebUtility.UrlEncode(key);
                var item = new DistributedCachedItem(options, Encoding.UTF8.GetBytes(value));
                item.PartitionKey = BuildPartitionKey(key);
                item.RowKey = key;
                await Repo.ExecuteAsync(TableOperation.InsertOrReplace(item));
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat);
            }
        }

        #endregion

        #region Refresh

        public void Refresh(string key)
        {
            Telemetry.TrackTrace("DistributedCacheStore: Refresh is not supported", SeverityLevel.Warning);
        }
        public Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            Telemetry.TrackTrace("DistributedCacheStore: Refresh is not supported", SeverityLevel.Warning);
            return Task.CompletedTask;
        }

        #endregion

        #region Remove

        public void Remove(string key)
        {
            try
            {
                key = WebUtility.UrlEncode(key);
                var dt = new DynamicTableEntity(BuildPartitionKey(key), key);
                dt.ETag = "*";
                Repo.Execute(TableOperation.Delete(dt));
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, severityLevel: SeverityLevel.Warning, flow: ExceptionFlow.Eat);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            try
            {
                key = WebUtility.UrlEncode(key);
                var dt = new DynamicTableEntity(BuildPartitionKey(key), key);
                dt.ETag = "*";
                await Repo.ExecuteAsync(TableOperation.Delete(dt));
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, severityLevel: SeverityLevel.Warning, flow: ExceptionFlow.Eat);
            }
        }


        #endregion

        #endregion

        #region Statische Instanzen

        #region Instance

        public readonly static DistributedCacheStore Instance = new DistributedCacheStore();

        #endregion

        #endregion
    }
}
