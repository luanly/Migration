using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SwissAcademic.Azure
{
    public static class TableClientUtility
    {
        #region ExecuteQueryAsync

        public static Task<IEnumerable<DynamicTableEntity>> ExecuteQueryAsync(this CloudTable table, TableQuery<DynamicTableEntity> query, TableContinuationToken token = null) => ExecuteQueryAsync<DynamicTableEntity>(table, query, token);
        public static async Task<IEnumerable<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query, TableContinuationToken token = null)
            where T : ITableEntity, new()

        {
            var segment = await table.ExecuteQuerySegmentedAsync(query, token);

            var result = segment.Results.ToList();

            if (segment.ContinuationToken != null)
            {
                if(query.TakeCount != null &&
                   result.Count >= query.TakeCount)
                {
                    return result;
                }
                result.AddRange(await ExecuteQueryAsync(table, query, segment.ContinuationToken));
            }

            return result;
        }

        #endregion
    }

    public static class BlobExtensions
    {
        /// <summary>
        /// Use this extension method to set blob metadata because it will take care for base64 encoding required to write umlauts etc.
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="metadata"></param>
        public static void SetMetadata(this ICloudBlob blob, IDictionary<string, string> metadata)
        {
            foreach (var pair in metadata)
            {
                blob.Metadata[pair.Key] = pair.Value.EncodeToBase64();
            }

            blob.SetMetadata();
        }

        /// <summary>
        /// Use this extension method to set blob metadata because it will take care for base64 encoding required to write umlauts etc.
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="metadata"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task SetMetadataAsync(this ICloudBlob blob, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            foreach (var pair in metadata)
            {
                blob.Metadata[pair.Key] = pair.Value.EncodeToBase64();
            }

            return blob.SetMetadataAsync(cancellationToken);
        }
    }
}
