using IdentityModel.Client;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class PersistedGrantStore
        :
        IPersistedGrantStore
    {
        #region Felder

        static List<string> GetAsyncColumnsNames = new List<string> { nameof(PersistedGrantStoreItem.JsonCode), nameof(PersistedGrantStoreItem.Expiration), nameof(PersistedGrantStoreItem.Version) };
        const string PartitionKey = AzureConstants.PersistedGrantStoreTable2;

        #endregion

        #region Konstruktor

        public PersistedGrantStore()
        {
            
        }

        #endregion

        #region Eigenschaften

        internal static TableStorageRepository Repo { get; set; }

		#region SerializerSettings

		static Lazy<JsonSerializerSettings> _serializerSettings = new Lazy<JsonSerializerSettings>(() =>
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ClaimConverter());
            settings.Converters.Add(new ClaimsPrincipalConverter());
            return settings;
        });

        public static JsonSerializerSettings SerializerSettings
        {
            get { return _serializerSettings.Value; }
        }

        #endregion

        #endregion

        #region Methoden

        #region AddAsync

        protected async Task AddAsync(TableEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            try
            {
                await Repo.ExecuteAsync(TableOperation.InsertOrReplace(entity));
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error, property1: ("Entity", JsonConvert.SerializeObject(entity)));
            }
        }

        #endregion

        #region ConvertFromJson

        protected PersistedGrant ConvertFromJson(string json)
        {
            return JsonConvert.DeserializeObject<PersistedGrant>(json, SerializerSettings);
        }

        #endregion

        #region ConvertToJson

        protected string ConvertToJson(PersistedGrant value)
        {
            return JsonConvert.SerializeObject(value, SerializerSettings);
        }

        #endregion

        #region GetAsync

        public async Task<PersistedGrant> GetAsync(string key)
        => await GetAsync(key, null);
        public async Task<PersistedGrant> GetAsync(string key, DataCenter? dataCenter = null)
        {
            try
            {
                var query = TableOperation.Retrieve<DynamicTableEntity>(PartitionKey, WebUtility.UrlEncode(key), GetAsyncColumnsNames);
                var result = await Repo.ExecuteAsync(query);
                if (result == null ||
                    result.Result == null)
                {
                    return null;
                }
                var tokenResult = result.Result as DynamicTableEntity;
                var token = new
                {
                    Expiry = tokenResult[nameof(PersistedGrantStoreItem.Expiration)].DateTime,
                    JsonCode = tokenResult[nameof(PersistedGrantStoreItem.JsonCode)].StringValue,
                    Version = tokenResult[nameof(PersistedGrantStoreItem.Version)].Int32Value,
                };

                if (token == null || token.Expiry < DateTime.UtcNow)
                {
                    return null;
                }

                var grant = ConvertFromJson(token.JsonCode);

                try
                {
                    if (token.Version == 1)
                    {
                        var item2 = new PersistedGrantStoreItem
                        {
                            PartitionKey = grant.SubjectId,
                            RowKey = WebUtility.UrlEncode(key),
                            SubjectId = grant.SubjectId,
                            ClientId = grant.ClientId,
                            Expiration = grant.Expiration,
                            JsonCode = "",
                            Version = 2
                        };
                        await AddAsync(item2);
                    }
                }
                catch (Exception ex)
                {
                    Telemetry.TrackException(ex, SeverityLevel.Warning, ExceptionFlow.Eat);
                }
                return grant;

            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
            }
            return null;
        }

        #endregion

        #region GetAllAsync

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subject)
        {
            try
            {
                var results = await GetAllTableEntitiesAsync(subject, nameof(PersistedGrantStoreItem.JsonCode));

                return results.Select(i => ConvertFromJson(i[nameof(PersistedGrantStoreItem.JsonCode)].StringValue))
                              .Cast<PersistedGrant>().ToList();
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
            }
            return null;
        }

        async Task<IEnumerable<DynamicTableEntity>> GetAllTableEntitiesAsync(string subject, params string[] columsNames)
        {
            var partitionFilter = TableQuery.GenerateFilterCondition(nameof(PersistedGrantStoreItem.PartitionKey), QueryComparisons.Equal, subject);
            var query = new TableQuery<DynamicTableEntity>().Where(partitionFilter);
            query.SelectColumns = columsNames;
            return await Repo.ExecuteQueryAsync(query);
        }

        public Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter) => GetAllAsync(filter.SubjectId);

        #endregion

        #region InitialzeAsync

        public static async Task InitialzeAsync()
        {
            Repo = new TableStorageRepository(false);
            await Repo.InitializeAsync(AzureConstants.PersistedGrantStoreTable2, multiRegionSupport: true);
        }

        #endregion

        #region RemoveAsync

        public async Task RemoveAsync(string key)
        {
            try
            {
                var query = TableOperation.Retrieve<DynamicTableEntity>(PartitionKey, WebUtility.UrlEncode(key));
                var result = await Repo.ExecuteAsync(query);
                var token = result.Result as DynamicTableEntity;
                if (token != null)
                {
                    token.ETag = "*";
                    await Repo.ExecuteAsync(TableOperation.Delete(token));
                }
            }
            catch (Exception exception)
            {
                if (exception.Message?.IndexOf("404") == -1)
                {
                    Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
                }
            }
        }

        #endregion

        #region RemoveAllAsync

        public async Task RemoveAllAsync2(string contactKey, params string[] expectAccessTokens)
        {
            try
            {
                for (var i = 0; i < expectAccessTokens.Length; i++)
                {
                    expectAccessTokens[i] = WebUtility.UrlEncode(expectAccessTokens[i]);
                }

                var results = await GetAllTableEntitiesAsync(contactKey, nameof(PersistedGrantStoreItem.RowKey));

                var op = new TableBatchOperation();

                foreach (var r in results)
                {
                    if (expectAccessTokens.Contains(r.RowKey))
                    {
                        continue;
                    }
                    r.ETag = "*";
                    try
                    {
                        await Repo.ExecuteAsync(TableOperation.Delete(r));
                    }
                    catch (Exception exception)
                    {
                        if (exception.Message?.IndexOf("404") != -1)
                        {
                            Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Warning);
                        }
                        else
                        {
                            Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
                        }
                    }

                    r.PartitionKey = PartitionKey;
                    try
                    {
                        await Repo.ExecuteAsync(TableOperation.Delete(r));
                    }
                    catch (Exception exception)
                    {
                        if (exception.Message?.IndexOf("404") != -1 || exception.Message?.IndexOf("Not Found") != -1)
                        {
                            Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Verbose);
                        }
                        else
                        {
                            Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception.Message?.IndexOf("404") != -1 || exception.Message?.IndexOf("Not Found") != -1)
                {
                    Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Verbose);
                }
                else
                {
                    Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
                }
            }
        }

        public async Task RemoveAllAsync(string subject, string client)
        {
            try
            {
                var results = await GetAllTableEntitiesAsync(subject, nameof(PersistedGrantStoreItem.RowKey), nameof(PersistedGrantStoreItem.ClientId));
                var tokens = results.Select(i => new
                {
                    ClientId = i[nameof(PersistedGrantStoreItem.ClientId)].StringValue,
                    RowKey = i.RowKey,
                    PartitionKey = i.PartitionKey
                });

                foreach (var token in tokens)
                {
                    if (token.ClientId != client)
                    {
                        continue;
                    }

                    var op = TableOperation.Delete(new DynamicTableEntity
                    {
                        RowKey = token.RowKey,
                        PartitionKey = token.PartitionKey,
                        ETag = "*"
                    });
                    try
                    {
                        await Repo.ExecuteAsync(op);
                    }
                    catch (Exception exception)
                    {
                        if (exception.Message?.IndexOf("404") != -1)
                        {
                            Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Warning);
                        }
                        else
                        {
                            Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
                        }
                    }

                    op = TableOperation.Delete(new DynamicTableEntity
                    {
                        RowKey = token.RowKey,
                        PartitionKey = PartitionKey,
                        ETag = "*"
                    });
                    try
                    {
                        await Repo.ExecuteAsync(op);
                    }
                    catch (Exception exception)
                    {
                        if (exception.Message?.IndexOf("404") != -1 || exception.Message?.IndexOf("Not Found") != -1)
                        {
                            Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Warning);
                        }
                        else
                        {
                            Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception.Message?.IndexOf("404") != -1 || exception.Message?.IndexOf("Not Found") != -1)
                {
                    Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Warning);
                }
                else
                {
                    Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
                }
            }
        }

        public Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            return RemoveAllAsync(subjectId, clientId);
        }

        public Task RemoveAllAsync(PersistedGrantFilter filter) => RemoveAllAsync(filter.SubjectId, filter.ClientId);

        #endregion

        #region StoreAsync

        public async Task StoreAsync(PersistedGrant grant)
        {
            if (grant.ClientId == ClientIds.UnitTest &&
                string.IsNullOrEmpty(grant.SubjectId))
            {
                return;
            }
            var key = WebUtility.UrlEncode(grant.Key);
            var item = new PersistedGrantStoreItem
            {
                PartitionKey = PartitionKey,
                RowKey = key,
                SubjectId = grant.SubjectId,
                ClientId = grant.ClientId,
                JsonCode = ConvertToJson(grant),
                Expiration = grant.Expiration,
                Version = 2
            };

            //Neu mit Version 2: Ansonsten ist der Logout extrem langsam
            var item2 = new PersistedGrantStoreItem
            {
                PartitionKey = grant.SubjectId,
                RowKey = key,
                SubjectId = grant.SubjectId,
                ClientId = grant.ClientId,
                Expiration = grant.Expiration,
                JsonCode = "",
                Version = 2
            };

            await Task.WhenAll(AddAsync(item), AddAsync(item2));
        }

        #endregion

        #region UpdateAsync

        protected async Task UpdateAsync(DynamicTableEntity entity)
        {
            await Repo.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        #endregion

        #endregion

        #region Statische Instanzen

        public readonly static PersistedGrantStore Instance = new PersistedGrantStore();

        #endregion
    }
}
