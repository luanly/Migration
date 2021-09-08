using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure.Storage;
using SwissAcademic.KeyVaultUtils;
using SwissAcademic.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    internal static class AzureHelper
    {
        #region Fields

        static string[] _queueNames = new string[] {
            MessageKey.SendEmail,
            MessageKey.SendCrmEmail,
            MessageKey.UpdateCitaviSpace,
            CrmQueueConstants.PendingChanges
        };

      
        public static SasKeyVaultClient KeyVaultClient { get; private set; }

        #endregion

        #region Constructors

        static AzureHelper()
        {
#if DEBUG
            AzureServiceTokenProvider = new AzureServiceTokenProvider();
#else
            AzureServiceTokenProvider = new AzureServiceTokenProvider($"RunAs=App;AppId={ConfigurationManager.AppSettings[$"CitaviWeb Managed Identity"]}");
#endif
            KeyVaultClient = new SasKeyVaultClient(ConfigurationManager.AppSettings["KeyVaultBaseUrl"], AzureServiceTokenProvider);
        }

        #endregion

        #region Properties

        internal static AzureServiceTokenProvider AzureServiceTokenProvider { get; private set; }

        internal static CloudBlobClient BlobStorageAccount { get; set; }

        internal static CloudTableClient TableStorageAccount { get; set; }

        static CloudQueueClient QueueClient { get; set; }

        public static Dictionary<string, CloudQueue> Queues { get; private set; } = new Dictionary<string, CloudQueue>();

        public static Func<string, IDictionary<string, string>, Task> Ably { get; set; }

        #endregion

        #region Methods

        #region AddQueueMessage

        public static async Task AddQueueMessageAsync(CrmUser user, Dictionary<string, string> properties, string label, params string[] keysAndValues)
        {
            if (CrmConfig.IsUnittest)
            {
                await Ably.Invoke($"QueueMessage_{label}", properties);
                return;
            }

            var message = CreateMessage(user, properties, label, keysAndValues);
            var queueName = label.GetFullQueueNameFromBaseName();
            var queue = await GetQueueAsync(queueName);
            await queue.AddMessageAsync(message);
        }

        #endregion

        #region CreateMessage

        static CloudQueueMessage CreateMessage(CrmUser user, Dictionary<string, string> properties, string label, params string[] keysAndValues)
        {
            if (!properties.ContainsKey(MessageKey.ContactKey))
            {
                var contactKey = user?.Key;
                if (!string.IsNullOrEmpty(contactKey))
                {
                    properties.Add(MessageKey.ContactKey, contactKey);
                }
            }

            properties.Add(MessageKey.Label, label);

            if (keysAndValues != null && keysAndValues.Any())
            {
                var keysAndValuesProperties = CollectionUtility.ToDictionary(keysAndValues);
                foreach (var pair in keysAndValuesProperties)
                {
                    properties.Add(pair.Key, pair.Value);
                }
            }

            ((IWebTelemetryClient)Telemetry.Client).AddTelemetryContext(properties);

            return new CloudQueueMessage(JsonConvert.SerializeObject(properties));
        }

        #endregion

        #region ConfigureQueuesAsync
        public static async Task ConfigureQueuesAsync(Func<string, IDictionary<string, string>, Task> ably)
        {
            Ably = ably;
            foreach (var webJobQueueName in _queueNames)
            {
                var queueName = webJobQueueName.GetFullQueueNameFromBaseName();
                if (Queues.ContainsKey(queueName))
                {
                    continue;
                }

                var queue = await GetQueueAsync(queueName);
                Queues[queueName] = queue;
            }
        }

        #endregion

        #region GetQueue        

        async static Task<CloudQueue> GetQueueAsync(string queueName)
        {
            if (Queues.ContainsKey(queueName))
            {
                return Queues[queueName];
            }
            var queue = QueueClient.GetQueueReference(queueName.ToLowerInvariant());
            await queue.CreateIfNotExistsWithCheckAsync();
            return queue;
        }

        #endregion

        #region InitializeStorageAccount

        internal static void InitializeStorageAccount(IAzureStorageResolver azureStorageResolver)
        {
            BlobStorageAccount = azureStorageResolver.GetCloudBlobClient();
            TableStorageAccount = azureStorageResolver.GetCloudTable();
            QueueClient = azureStorageResolver.GetCloudQueueClient();
        }

        #endregion

        #endregion
    }
}
