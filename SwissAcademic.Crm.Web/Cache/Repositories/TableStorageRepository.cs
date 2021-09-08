using Microsoft.Azure.Cosmos.Table;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using SwissAcademic.KeyVaultUtils;
using SwissAcademic.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class TableStorageRepository
    {
        #region Fields

        Dictionary<DataCenter, CloudTable> MultiRegionTables = new Dictionary<DataCenter, CloudTable>();

        #endregion

        #region Konstruktor

        /// <summary>
        /// IsComplexObject verwenden, wenn Object nicht "primitiv" ist
        /// Bei einfachen Type (int, long, string, etc) false setzen
        /// hasETag only used for InMemory-Cache
        /// </summary>
        /// <param name="isComplexObject"></param>
        public TableStorageRepository(bool isComplexObject = true, bool hasETag = false)
        {
            IsComplexObject = isComplexObject;
            HasETag = hasETag;
        }

        #endregion

        #region Eigenschaften

        public CloudTable Table { get; private set; }
        public InMemoryCacheRepository InMemoryCache { get; private set; }
		public bool IsComplexObject { get; }
        public bool HasETag { get; set; }
        public bool MultiRegionSupport { get; private set; }

        #endregion

        #region Methoden

        #region AddOrUpdate

        public async Task AddOrUpdateAsync<T>(T item, string key)
        {
            if (item == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            try
            {
                TableOperation operation;
                if (IsComplexObject)
                {
                    var json = CrmJsonConvert.SerializeObject(item);
                    var cacheEntity = CreateComplexCacheEntity(json, key);
                    operation = TableOperation.InsertOrReplace(cacheEntity);
                    if (InMemoryCache != null && HasETag)
                    {
                        await InMemoryCache.AddOrUpdateAsync(item, key);
                    }
                }
				else
				{
                    var cacheEntity = CreateSimpleTableEntity(item, key);
                    operation = TableOperation.InsertOrReplace(cacheEntity);
                    if (InMemoryCache != null)
                    {
                        await InMemoryCache.AddOrUpdateAsync(item, key);
                    }
                }

                await Table.ExecuteAsync(operation);
                await ExecuteMultiRegionOperationAsync(operation);
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat);
            }
        }

        #endregion

        #region AddMultiRegionCloudTable

        internal void AddMultiRegionCloudTable(DataCenter dataCenter, CloudTable cloudTable)
		{
            MultiRegionTables[dataCenter] = cloudTable;
            MultiRegionSupport = true;
        }

        #endregion

        #region BuildTableQuery

        TableOperation BuildTableQuery(string key, bool includeColumns = true)
        {
            key = WebUtility.UrlEncode(key);
            if (IsComplexObject)
            {
                if (includeColumns)
                {
                    return TableOperation.Retrieve<ComplexTableStorageCacheEntity>(BuildPartitionKey(key), key, ComplexTableStorageCacheEntity.ColumnNames);
                }
                else
                {
                    return TableOperation.Retrieve<ComplexTableStorageCacheEntity>(BuildPartitionKey(key), key);
                }
            }
			else
			{
                if (includeColumns)
                {
                    return TableOperation.Retrieve<SimpleTableStorageCacheEntity>(BuildPartitionKey(key), key, SimpleTableStorageCacheEntity.ColumnNames);
                }
                else
                {
                    return TableOperation.Retrieve<SimpleTableStorageCacheEntity>(BuildPartitionKey(key), key);
                }
            }
        }

        #endregion

        #region BuildPartitionKey

        public static string BuildPartitionKey(string key)
        {
            var pk = key.Substring(0, 2);
            if (CrmConfig.IsUnittest)
            {
                pk = CrmConstants.UnitTestCrmEntityKeyPrefix;
            }
            return pk;
        }

        #endregion

        #region CreateCacheEntity

        public ComplexTableStorageCacheEntity CreateComplexCacheEntity(string json, string key)
        {
            var entity = new ComplexTableStorageCacheEntity
            {
                PartitionKey = BuildPartitionKey(key),
                RowKey = key,
                ETag = "*",
            };

            entity.SetData(json);
            return entity;
        }

        public SimpleTableStorageCacheEntity CreateSimpleTableEntity(object obj, string key)
        {
            var entity = new SimpleTableStorageCacheEntity
            {
                PartitionKey = BuildPartitionKey(key),
                RowKey = key,
                ETag = "*",
            };

            entity.SetData(obj);

            return entity;
        }

        #endregion

        #region Clear

        public void Clear()
        {
            try
            {
                if (CrmConfig.IsUnittest)
                {
                    var query = new TableQuery<TableEntity>();

                    query = query.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, CrmConstants.UnitTestCrmEntityKeyPrefix));


                    TableContinuationToken continuationToken = null;
                    do
                    {
                        var entities = Table.ExecuteQuerySegmented(query, continuationToken);
                        continuationToken = entities.ContinuationToken;
                        Parallel.ForEach(entities, entity =>
                        {
                            try
                            {
                                Table.Execute(TableOperation.Delete(entity));
                                foreach (var cloudTable in MultiRegionTables.Values)
                                {
                                    try
                                    {
                                        cloudTable.Execute(TableOperation.Delete(entity));
                                    }
                                    catch (Exception ex) { }
                                }
                            }
                            catch { }
                        });
                    }
                    while (continuationToken != null);

                    if (InMemoryCache != null)
                    {
                        InMemoryCache.Clear();
                    }
                }
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception);
            }
        }

        #endregion

        #region Execute
        public TableResult Execute(TableOperation operation)
        {
            var result = Table.Execute(operation);
            if (operation.OperationType != TableOperationType.Retrieve)
            {
                ExecuteMultiRegionOperation(operation);
            }
            return result;
        }

        #endregion

        #region ExecuteAsync
        public async Task<TableResult> ExecuteAsync(TableOperation operation, DataCenter? dataCenter = null, bool throwOnException = false)
		{
            var table = Table;
            if (dataCenter != null && MultiRegionTables.ContainsKey(dataCenter.Value))
            {
                table = MultiRegionTables[dataCenter.Value];
            }
            TableResult result = null;
            try
            {
                result = await table.ExecuteAsync(operation);
                if (operation.OperationType != TableOperationType.Retrieve)
                {
                    await ExecuteMultiRegionOperationAsync(operation);
                }
            }
			catch
			{
                if(operation.OperationType == TableOperationType.Delete)
				{
                    await ExecuteMultiRegionOperationAsync(operation);
                }
				if (throwOnException)
				{
                    throw;
				}
			}
            return result;
        }


        #endregion

        #region ExecuteBatchAsync

        public async Task<TableBatchResult> ExecuteBatchAsync(TableBatchOperation batch)
        {
            var result = await Table.ExecuteBatchAsync(batch);
            await ExecuteMultiRegionBatchOperationAsync(batch);
            return result;
        }

        #endregion

        #region ExecuteQueryAsync
        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(TableQuery<T> query, TableContinuationToken token = null)
            where T : ITableEntity, new()
        {
            return await Table.ExecuteQueryAsync(query, token);
        }

        #endregion

        #region ExecuteMultiRegionOperationAsync

        void ExecuteMultiRegionOperation(TableOperation tableOperation)
        {
            if (!MultiRegionSupport)
            {
                return;
            }
            foreach (var cloudTable in MultiRegionTables.Values)
            {
                cloudTable.Execute(tableOperation);
            }
        }
        async Task ExecuteMultiRegionOperationAsync(TableOperation tableOperation)
		{
			if (!MultiRegionSupport)
			{
                return;
			}
            var tasks = new List<Task<TableResult>>();
            foreach(var cloudTable in MultiRegionTables.Values)
			{
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        return await cloudTable.ExecuteAsync(tableOperation);
                    }
                    catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404) { }
                    catch (Exception ex)
					{
                        Telemetry.TrackException(ex, SeverityLevel.Warning, ExceptionFlow.Eat);
					}
                    return null;
                }));
			}
            await Task.WhenAll(tasks);
		}
        async Task ExecuteMultiRegionBatchOperationAsync(TableBatchOperation batch)
        {
            if (!MultiRegionSupport)
            {
                return;
            }
            var tasks = new List<Task<TableBatchResult>>();
            foreach (var cloudTable in MultiRegionTables.Values)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        return await cloudTable.ExecuteBatchAsync(batch);
                    }
                    catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404) { }
                    catch (Exception ex)
                    {
                        Telemetry.TrackException(ex, SeverityLevel.Warning, ExceptionFlow.Eat);
                    }
                    return null;
                }));

            }
            await Task.WhenAll(tasks);
        }

        #endregion

        #region GetAsync

        public async Task<T> GetAsync<T>(string key, DataCenter? dataCenter = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default;
            }

            try
            {
                if (dataCenter == null &&
                    InMemoryCache != null)
                {
                    var fromCache = await InMemoryCache.GetAsync<T>(key);
                    if (fromCache != null)
                    {
                        if (HasETag)
                        {
                            var etag = await GetETagAsync(key);
                            if (!string.IsNullOrEmpty(etag) &&
                                ((IHasCacheETag)fromCache).CacheETag == etag)
                            {
                                return fromCache;
                            }
                        }
						else
						{
                            return fromCache;
                        }
                    }
                }

                var table = Table;
                if(dataCenter != null && MultiRegionTables.ContainsKey(dataCenter.Value))
				{
                    table = MultiRegionTables[dataCenter.Value];
                }

                var tableResult = await table.ExecuteAsync(BuildTableQuery(key));
                if (tableResult == null)
                {
                    return default;
                }

                if(IsComplexObject)
				{
                    var entry = tableResult.Result as ComplexTableStorageCacheEntity;
                    if (entry == null)
                    {
                        return default;
                    }
                    var item = CrmJsonConvert.DeserializeObject<T>(entry.GetData());

                    if (InMemoryCache != null && HasETag)
                    {
                        ((IHasCacheETag)item).CacheETag = tableResult.Etag;
                        await InMemoryCache.AddOrUpdateAsync(item, key);
                    }

                    return item;
                }
				else
				{
                    var entry = tableResult.Result as SimpleTableStorageCacheEntity;
                    if (entry == null)
                    {
                        return default;
                    }

                    var item = entry.GetData<T>();
                    if (InMemoryCache != null)
                    {
                        await InMemoryCache.AddOrUpdateAsync(item, key);
                    }
                    return item;
                }
                
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat);
            }
            return default;
        }

        #endregion

        #region GetETagAsync

        public async Task<string> GetETagAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            try
            {
                var tableResult = await Table.ExecuteAsync(BuildTableQuery(key, includeColumns: false));
                if (tableResult == null)
                {
                    return null;
                }

                return tableResult.Etag;
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat);
            }
            return null;
        }

        #endregion

        #region InitializeAsync

        public async Task InitializeAsync(string cacheName, bool multiRegionSupport = false, bool withInMemoryCache = false, CloudTableClient client = null)
        {
            MultiRegionSupport = multiRegionSupport;
            if (client == null)
            {
                client = AzureHelper.TableStorageAccount;
            }
            Table = client.GetTableReference(cacheName);
            if (withInMemoryCache)
            {
                InMemoryCache = new InMemoryCacheRepository();
            }
            await Table.CreateIfNotExistsAsync();

			if (MultiRegionSupport)
			{
                var regionalUrls = await AzureRegionResolver.Instance.GetRegionalKeyVaultSecretAsync(KeyVaultSecrets.StorageAccounts.CitaviWeb);
                foreach (var regionalUrl in regionalUrls)
                {
                    var cloudStorageAccount = Microsoft.Azure.Cosmos.Table.CloudStorageAccount.Parse(regionalUrl.Value);
                    var azureRegionTable = cloudStorageAccount.CreateCloudTableClient().GetTableReference(cacheName);
                    if(azureRegionTable.Uri == Table.Uri)
					{
                        continue;
					}
                    MultiRegionTables[regionalUrl.Key] = azureRegionTable;
                    await azureRegionTable.CreateIfNotExistsAsync();
                }
            }
        }

        #endregion

        #region Remove

        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            try
            {
                var entity = new TableEntity(BuildPartitionKey(key), key);
                entity.ETag = "*";
                var operation = TableOperation.Delete(entity);
                try
                {
                    await Table.ExecuteAsync(operation);
                }
                catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404) { }

                await ExecuteMultiRegionOperationAsync(operation);
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat);
            }

            if (InMemoryCache != null)
            {
                await InMemoryCache.RemoveAsync(key);
            }
        }

        #endregion

        #endregion
    }
}
