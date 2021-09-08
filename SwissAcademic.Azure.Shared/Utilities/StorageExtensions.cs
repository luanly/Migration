using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SwissAcademic.Azure.Storage
{
    public static class StorageExtensions
    {
        //TODO ASYNC Torben
        public static bool CreateIfNotExistsWithCheck(this CloudTable cloudTable)
        {
            // Avoid HttpStatusCode 409 in ApplicationInsights when calling cloudTable.CreateIfNotExists() immediately.
            if (cloudTable.Exists()) return false;
            return cloudTable.CreateIfNotExists();
        }
        //TODO ASYNC Torben
        public static bool CreateIfNotExistsWithCheck(this CloudQueue cloudQueue)
        {
            // Avoid HttpStatusCode 409 in ApplicationInsights when calling cloudTable.CreateIfNotExists() immediately.
            if (cloudQueue.Exists()) return false;
            return cloudQueue.CreateIfNotExists();
        }


        public static async Task<bool> CreateIfNotExistsWithCheckAsync(this CloudTable cloudTable)
        {
            // Avoid HttpStatusCode 409 in ApplicationInsights when calling cloudTable.CreateIfNotExists() immediately.
            if (await cloudTable.ExistsAsync()) return false;
            return await cloudTable.CreateIfNotExistsAsync();
        }

        public static async Task<bool> CreateIfNotExistsWithCheckAsync(this CloudQueue cloudQueue)
        {
            // Avoid HttpStatusCode 409 in ApplicationInsights when calling cloudTable.CreateIfNotExists() immediately.
            if (await cloudQueue.ExistsAsync()) return false;
            return await cloudQueue.CreateIfNotExistsAsync();
        }

        public static async Task<bool> CreateIfNotExistsWithCheckAsync(this CloudBlobContainer blobContainer)
        {
            // Avoid HttpStatusCode 409 in ApplicationInsights when calling cloudTable.CreateIfNotExists() immediately.
            if (await blobContainer.ExistsAsync()) return false;
            return await blobContainer.CreateIfNotExistsAsync();
        }

        public static async Task<bool> CreateIfNotExistsWithCheckAsync(this CloudBlobContainer blobContainer, CancellationToken cancellationToken)
        {
            // Avoid HttpStatusCode 409 in ApplicationInsights when calling cloudTable.CreateIfNotExists() immediately.
            if (await blobContainer.ExistsAsync(cancellationToken)) return false;
            return await blobContainer.CreateIfNotExistsAsync(cancellationToken);
        }

#if Web
        public static string GetFullQueueNameFromBaseName(this string queueName)
        {
            var queueNameResolved = ConfigurationManager.AppSettings["WebJobQueue_" + queueName.ToLowerInvariant()];

            if (string.IsNullOrEmpty(queueNameResolved))
            {
#if DEBUG
                queueNameResolved = $"{queueName}-debug-{System.Environment.MachineName}".ToLowerInvariant();
#else
                var environmentName = ConfigurationManager.AppSettings["EnvironmentName"] ?? "AppSetting-EnvironmentName-Missing";
                var slotName = ConfigurationManager.AppSettings["SlotName"] ?? "AppSetting-SlotName-Missing";

                queueNameResolved = $"{queueName}-{environmentName}-{slotName}".ToLowerInvariant();
#endif
            }

            return queueNameResolved;
        }

        public static string GetFullFunctionQueueNameFromBaseName(this string queueName)
        {
            var queueNameResolved = ConfigurationManager.AppSettings["QueueName_" + queueName.ToLowerInvariant()];
            if (string.IsNullOrEmpty(queueNameResolved))
            {
#if DEBUG
                queueNameResolved = queueName.ToLowerInvariant() + "-localdev";
#else
                var suffix = ConfigurationManager.AppSettings["FunctionQueueSuffix"]?.ToString();

                if (suffix == null) suffix = string.Empty;
                else suffix = "-" + suffix.ToLowerInvariant();

                queueNameResolved = queueName + suffix;
#endif
            }
            return queueNameResolved;
        }
#endif
    }
}
