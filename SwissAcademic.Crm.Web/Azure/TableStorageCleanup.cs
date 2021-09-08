using Microsoft.Azure.Cosmos.Table;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using SwissAcademic.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class TableStorageCleanup
    {
        #region Felder

        static bool _isExecuting;
        static TableStorageRepository _persistedGrantStoreTable = new TableStorageRepository(false);
        static TableStorageRepository _distributedCacheTable = new TableStorageRepository(false);
        static TableStorageRepository _cookieStoreTable = new TableStorageRepository(false);
        static TableStorageRepository _signalrConnectionCacheTable = new TableStorageRepository(false);

        #endregion

        #region Konstruktor

        static TableStorageCleanup()
        {

        }

        #endregion

        #region Methoden

        public static async Task<int> CleanUpFailedDeletedTableStorageLocks(int maxLockLifeTimeInSeconds = 60)
        {
            if (_isExecuting)
            {
                return -1;
            }

            try
            {
                _isExecuting = true;

                var partitionFilter = TableQuery.GenerateFilterConditionForDate(nameof(DynamicTableEntity.Timestamp), QueryComparisons.LessThan, DateTimeOffset.UtcNow.AddSeconds(-maxLockLifeTimeInSeconds));
                var query = new TableQuery<DynamicTableEntity>().Where(partitionFilter);
                query.TakeCount = 1000;
                query.SelectColumns = new string[] { "PartitionKey", "RowKey" };
                var results = await TableStorageLock.Repo.ExecuteQueryAsync(query);

                if (!results.Any())
                {
                    return 0;
                }

                using (var op = Telemetry.StartOperation($"{nameof(CleanUpFailedDeletedTableStorageLocks)}"))
                {
                    foreach (var partitionResult in results.GroupBy(i => i.PartitionKey))
                    {
                        var batch = new TableBatchOperation();

                        foreach (var result in partitionResult)
                        {
                            batch.Delete(result);
                            if (batch.Count == 75)
                            {
                                await TableStorageLock.Repo.ExecuteBatchAsync(batch);
                                batch.Clear();
                            }
                        }
                        if (batch.Any())
                        {
                            await TableStorageLock.Repo.ExecuteBatchAsync(batch);
                            batch.Clear();
                        }
                    }
                }

                return results.Count();
            }
            catch (Microsoft.Azure.Cosmos.Table.StorageException exception)
            {
                if (exception.RequestInformation?.HttpStatusCode != (int)HttpStatusCode.NotFound &&
                    !exception.Message.Contains(" in the batch returned an unexpected response code"))
                {
                    Telemetry.TrackException(exception, severityLevel: SeverityLevel.Warning, flow: ExceptionFlow.Eat);
                }
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, severityLevel: SeverityLevel.Warning, flow: ExceptionFlow.Eat);
            }
            finally
            {
                _isExecuting = false;
            }
            return -1;
        }

        public static async Task<int> CleanUpDistributedCacheItems()
        {
            if (_isExecuting)
            {
                return -1;
            }

            try
            {
                _isExecuting = true;

                var partitionFilter = TableQuery.GenerateFilterConditionForDate("AbsolutExperiation", QueryComparisons.LessThan, DateTimeOffset.UtcNow);
                var query = new TableQuery<DynamicTableEntity>().Where(partitionFilter);
                query.TakeCount = 1000;
                query.SelectColumns = new string[] { "PartitionKey", "RowKey" };
                var results = await _distributedCacheTable.ExecuteQueryAsync(query);

                if (!results.Any())
                {
                    return 0;
                }

                using (var op = Telemetry.StartOperation($"{nameof(CleanUpDistributedCacheItems)}"))
                {
                    foreach (var partitionResult in results.GroupBy(i => i.PartitionKey))
                    {
                        var batch = new TableBatchOperation();

                        foreach (var result in partitionResult)
                        {
                            batch.Delete(result);
                            if (batch.Count == 75)
                            {
                                await _distributedCacheTable.ExecuteBatchAsync(batch);
                                batch.Clear();
                            }
                        }
                        if (batch.Any())
                        {
                            await _distributedCacheTable.ExecuteBatchAsync(batch);
                            batch.Clear();
                        }
                    }
                }

                return results.Count();
            }
            catch (Microsoft.Azure.Cosmos.Table.StorageException exception)
            {
                if (exception.RequestInformation?.HttpStatusCode != (int)HttpStatusCode.NotFound &&
                    !exception.Message.Contains(" in the batch returned an unexpected response code"))
                {
                    Telemetry.TrackException(exception, severityLevel: SeverityLevel.Warning, flow: ExceptionFlow.Eat);
                }
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, severityLevel: SeverityLevel.Warning, flow: ExceptionFlow.Eat);
            }
            finally
            {
                _isExecuting = false;
            }
            return -1;
        }

        public static async Task<int> CleanUpPersistedGrantTokens()
        {
            if (_isExecuting)
            {
                return -1;
            }

            try
            {
                _isExecuting = true;
                var partitionFilter = TableQuery.GenerateFilterConditionForDate("Expiration", QueryComparisons.LessThan, DateTimeOffset.UtcNow);
                var query = new TableQuery<DynamicTableEntity>().Where(partitionFilter);
                query.TakeCount = 1000;
                query.SelectColumns = new string[] { "PartitionKey", "RowKey" };
                var results = await _persistedGrantStoreTable.ExecuteQueryAsync(query);

                if (!results.Any())
                {
                    return 0;
                }

                using (var operation = Telemetry.StartOperation($"{nameof(CleanUpPersistedGrantTokens)}"))
                {
                    foreach (var partitionResult in results.GroupBy(i => i.PartitionKey))
                    {
                        var batch = new TableBatchOperation();

                        foreach (var result in partitionResult)
                        {
                            batch.Delete(result);
                            if (batch.Count == 100)
                            {
                                await _persistedGrantStoreTable.ExecuteBatchAsync(batch);
                                batch.Clear();
                            }
                        }
                        if (batch.Any())
                        {
                            await _persistedGrantStoreTable.ExecuteBatchAsync(batch);
                            batch.Clear();
                        }
                    }
                }
                return results.Count();
            }
            catch (Microsoft.Azure.Cosmos.Table.StorageException exception)
            {
                if (exception.RequestInformation?.HttpStatusCode != (int)HttpStatusCode.NotFound &&
                    !exception.Message.Contains(" in the batch returned an unexpected response code"))
                {
                    Telemetry.TrackException(exception, severityLevel: SeverityLevel.Warning, flow: ExceptionFlow.Eat);
                }
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, severityLevel: SeverityLevel.Warning, flow: ExceptionFlow.Eat);
            }
            finally
            {
                _isExecuting = false;
            }
            return -1;
        }

        public static async Task<int> CleanUpCookieTokens()
        {
            if (_isExecuting)
            {
                return -1;
            }

            try
            {
                _isExecuting = true;
                var partitionFilter = TableQuery.GenerateFilterConditionForDate(nameof(CookieStoreItem.Expiration), QueryComparisons.LessThan, DateTimeOffset.UtcNow);
                var query = new TableQuery<DynamicTableEntity>().Where(partitionFilter);
                query.TakeCount = 1000;
                query.SelectColumns = new string[] { "PartitionKey", "RowKey" };
                var results = await _cookieStoreTable.ExecuteQueryAsync(query);

                if (!results.Any())
                {
                    return 0;
                }

                using (var op = Telemetry.StartOperation($"{nameof(CleanUpCookieTokens)})"))
                {
                    foreach (var partitionResult in results.GroupBy(i => i.PartitionKey))
                    {
                        var batch = new TableBatchOperation();

                        foreach (var result in partitionResult)
                        {
                            batch.Delete(result);
                            if (batch.Count == 100)
                            {
                                await _cookieStoreTable.ExecuteBatchAsync(batch);
                                batch.Clear();
                            }
                        }
                        if (batch.Any())
                        {
                            await _cookieStoreTable.ExecuteBatchAsync(batch);
                            batch.Clear();
                        }
                    }
                }
                return results.Count();
            }
            catch (Microsoft.Azure.Cosmos.Table.StorageException exception)
            {
                if (exception.RequestInformation?.HttpStatusCode != (int)HttpStatusCode.NotFound &&
                    !exception.Message.Contains(" in the batch returned an unexpected response code"))
                {
                    Telemetry.TrackException(exception, severityLevel: SeverityLevel.Warning, flow: ExceptionFlow.Eat);
                }
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, severityLevel: SeverityLevel.Warning, flow: ExceptionFlow.Eat);
            }
            finally
            {
                _isExecuting = false;
            }
            return -1;
        }

        public static async Task InitializeAsync()
		{
            await _persistedGrantStoreTable.InitializeAsync(AzureConstants.PersistedGrantStoreTable2);
            await _distributedCacheTable.InitializeAsync(AzureConstants.DistributedCache);
            await _signalrConnectionCacheTable.InitializeAsync(TableStoreConstants.SignalRConnectionCacheTableName);
            await _cookieStoreTable.InitializeAsync(AzureConstants.CookieStoreTable);
        }

        #endregion
    }
}
