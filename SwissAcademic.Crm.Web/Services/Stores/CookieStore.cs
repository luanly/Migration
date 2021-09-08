using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class CookieStore
        :
        ITicketStore
    {
        #region Felder

        const string PartitionKey = AzureConstants.CookieStoreTable;
        static List<string> GetAsyncColumnsNames = new List<string> { nameof(CookieStoreItem.JsonCode), nameof(CookieStoreItem.Expiration) };

        #endregion

        #region Eigenschaften

        protected static TableStorageRepository Table { get; set; }

        #region SerializerSettings

        static Lazy<JsonSerializerSettings> _serializerSettings = new Lazy<JsonSerializerSettings>(() =>
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ClaimConverter());
            settings.Converters.Add(new ClaimsPrincipalConverter());
            settings.Converters.Add(new AuthenticationTicketConverter());
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
                await Table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error, property1: ("Entity", JsonConvert.SerializeObject(entity)));
            }
        }

        #endregion

        #region ConvertFromJson

        protected AuthenticationTicket ConvertFromJson(string json)
        {
            return JsonConvert.DeserializeObject<AuthenticationTicket>(json, SerializerSettings);
        }

        #endregion

        #region ConvertToJson

        protected string ConvertToJson(AuthenticationTicket value)
        {
            return JsonConvert.SerializeObject(value, SerializerSettings);
        }

        #endregion

        #region GetAllAsync

        public async Task<IEnumerable<AuthenticationTicket>> GetAllAsync(string subject)
        {
            try
            {
                var results = await GetAllTableEntitiesAsync(subject, nameof(CookieStoreItem.JsonCode));

                return results.Select(i => ConvertFromJson(i[nameof(CookieStoreItem.JsonCode)].StringValue))
                              .Cast<AuthenticationTicket>().ToList();
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
            }
            return null;
        }

        async Task<IEnumerable<DynamicTableEntity>> GetAllTableEntitiesAsync(string subject, params string[] columsNames)
        {
            var partitionFilter = TableQuery.GenerateFilterCondition(nameof(CookieStoreItem.PartitionKey), QueryComparisons.Equal, subject);
            var query = new TableQuery<DynamicTableEntity>().Where(partitionFilter);
            query.SelectColumns = columsNames;
            return await Table.ExecuteQueryAsync(query);
        }

        #endregion

        #region InitialzeAsync

        public static async Task InitialzeAsync()
        {
            Table = new TableStorageRepository(false);
            await Table.InitializeAsync(AzureConstants.CookieStoreTable, multiRegionSupport: true);
        }

        #endregion

        #region RemoveAsync

        public async Task RemoveAsync(string key)
        {
            try
            {
                var query = TableOperation.Retrieve<DynamicTableEntity>(PartitionKey, key);
                var result = await Table.ExecuteAsync(query);
                var token = result.Result as DynamicTableEntity;
                if (token != null)
                {
                    token.ETag = "*";
                    await Table.ExecuteAsync(TableOperation.Delete(token));
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
        public async Task RemoveAllAsync(string contactKey, params string[] expectCookies)
        {
            try
            {
                var results = await GetAllTableEntitiesAsync(contactKey, nameof(PersistedGrantStoreItem.RowKey));

                foreach (var r in results)
                {
                    if (expectCookies.Contains(r.RowKey))
                    {
                        continue;
                    }
                    r.ETag = "*";
                    try
                    {
                        await Table.ExecuteAsync(TableOperation.Delete(r));
                    }
                    catch (Exception exception)
                    {
                        if (exception.Message?.IndexOf("404") != -1)
                        {
                            continue;
                        }
                        else
                        {
                            Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
                        }
                    }

                    r.PartitionKey = PartitionKey;
                    try
                    {
                        await Table.ExecuteAsync(TableOperation.Delete(r));
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

        #endregion

        #region RenewAsync

        public async Task RenewAsync(string ticketKey, AuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                throw new NotSupportedException($"{nameof(AuthenticationTicket)} must not be null");
            }

            var contactKey = ticket.Principal.GetContactKey();

            if (string.IsNullOrEmpty(contactKey))
            {
                throw new NotSupportedException($"{nameof(contactKey)} must not be null");
            }
            if (string.IsNullOrEmpty(ticketKey))
            {
                throw new NotSupportedException($"{nameof(ticketKey)} must not be null");
            }

            var item = new CookieStoreItem
            {
                PartitionKey = PartitionKey,
                RowKey = ticketKey,
                SubjectId = contactKey,
                JsonCode = ConvertToJson(ticket),
                Expiration = ticket.Properties.ExpiresUtc,
                Version = 1
            };

            var item2 = new CookieStoreItem
            {
                PartitionKey = contactKey,
                RowKey = ticketKey,
                SubjectId = contactKey,
                Expiration = ticket.Properties.ExpiresUtc,
                JsonCode = "",
                Version = 1
            };

            await Task.WhenAll(AddAsync(item), AddAsync(item2));
        }

        #endregion

        #region RetrieveAsync

        public async Task<AuthenticationTicket> RetrieveAsync(string key)
         => await RetrieveAsync(key, null);
        public async Task<AuthenticationTicket> RetrieveAsync(string key, DataCenter? dataCenter = null)
        {
            try
            {
                var query = TableOperation.Retrieve<DynamicTableEntity>(PartitionKey, key, GetAsyncColumnsNames);
                var result = await Table.ExecuteAsync(query, dataCenter);
                if (result == null ||
                   result.Result == null)
                {
                    Telemetry.TrackDiagnostics($"Cookie not found: {key}");
                    return null;
                }
                var tokenResult = result.Result as DynamicTableEntity;
                var token = new
                {
                    Expiry = tokenResult[nameof(PersistedGrantStoreItem.Expiration)].DateTime,
                    JsonCode = tokenResult[nameof(PersistedGrantStoreItem.JsonCode)].StringValue
                };

                if (token == null || token.Expiry < DateTime.UtcNow)
                {
                    Telemetry.TrackDiagnostics($"Cookie is expired: {key}");
                    return null;
                }

                return ConvertFromJson(token.JsonCode);
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, flow: ExceptionFlow.Eat, severityLevel: SeverityLevel.Error);
            }
            return null;
        }

        #endregion

        #region StoreAsync

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var ticketKey = ticket.Properties.GetSessionId();

            await RenewAsync(ticketKey, ticket);

            return ticketKey;
        }

        #endregion

        #endregion

        #region Statische Instanzen

        public readonly static CookieStore Instance = new CookieStore();

        #endregion
    }
}
