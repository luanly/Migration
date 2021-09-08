using Aspose.Words.Drawing;
using Microsoft.Azure.Storage.Blob;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using SwissAcademic.KeyVaultUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class BlobStorageRepository
    {
        #region Fields

        Dictionary<DataCenter, CloudBlobContainer> MultiRegionBlobs = new Dictionary<DataCenter, CloudBlobContainer>();

        #endregion

        #region Konstruktor

        public BlobStorageRepository()
        {
            
        }

		#endregion

		#region Eigenschaften

		public CloudBlobClient Client { get; private set; }
        public CloudBlobContainer Container { get; private set; }

        string RootContainerName { get; set; }
        string SubContainerName { get; set; }

        public bool MultiRegionSupport { get; private set; }

        #endregion

        #region AddOrUpdate

        public async Task AddOrUpdateAsync(object item, string key)
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
                var bytes = item as byte[];
                if (bytes == null)
                {
                    bytes = Encoding.UTF8.GetBytes(CrmJsonConvert.SerializeObject(item));
                }

                var blobName = BuildBlobFileName(key);
                var blob = Container.GetBlockBlobReference(blobName);
                await blob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
                await UploadFromByteArrayMultiRegionAsync(blobName, bytes);
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
            }
        }

        #endregion

        #region BuildBlobFileName

        static string BuildBlobFileName(string name) => name.ToLowerInvariant();

        #endregion

        #region Clear

        [ExcludeFromCodeCoverage]
        public async Task Clear()
        {
            try
            {
                if (CrmConfig.IsUnittest)
                {
                    var c = Client.GetContainerReference(RootContainerName);
                    var blobs = c.ListBlobs(prefix: $"{SubContainerName}/{CrmConstants.UnitTestCrmEntityKeyPrefix}", useFlatBlobListing: true);
                    foreach (CloudBlockBlob blob in blobs)
                    {
                        await blob.DeleteAsync();
                        await DeleteMultiRegionAsync(blob.Name);
                    }
                }
                else
                {
                    throw new NotSupportedException("Clear is not supported in non-unittests environments");
                }
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
            }
        }

        #endregion

        #region DeleteMultiRegionAsync
        async Task DeleteMultiRegionAsync(string blobName)
        {
            if (!MultiRegionSupport)
            {
                return;
            }

            var tasks = new List<Task>();
            foreach (var regionBlobContainer in MultiRegionBlobs.Values)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var blob = regionBlobContainer.GetBlockBlobReference(blobName);
                        await blob.DeleteIfExistsAsync();
                    }
                    catch (Exception ex)
                    {
                        Telemetry.TrackException(ex, SeverityLevel.Warning, ExceptionFlow.Eat);
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }

        #endregion

        #region Get

        public async Task<T> GetAsync<T>(string key, DataCenter? dataCenter = null) where T : class
        {
            try
            {
                var container = Container;
                if (dataCenter != null && MultiRegionBlobs.ContainsKey(dataCenter.Value))
                {
                    container = MultiRegionBlobs[dataCenter.Value];
                }

                var blob = container.GetBlockBlobReference(BuildBlobFileName(key));
                if (!await blob.ExistsAsync())
                {
                    return null;
                }

                if (typeof(T) == typeof(byte[]))
                {
                    using (var stream = new MemoryStream())
                    {
                        using (var blobStream = await blob.OpenReadAsync())
                        {
                            var bytes = new byte[2048];
                            int bytesRead;
                            while ((bytesRead = await blobStream.ReadAsync(bytes, 0, bytes.Length)) > 0)
                            {
                                await stream.WriteAsync(bytes, 0, bytesRead);
                            }
                            return stream.ToArray() as T;
                        }
                    }
                }
                else
                {
                    using (var reader = new StreamReader(await blob.OpenReadAsync()))
                    {
                        var json = await reader.ReadToEndAsync();
                        return CrmJsonConvert.DeserializeObject<T>(json);
                    }
                }

            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, flow: ExceptionFlow.Eat);
            }
            return null;
        }

        #endregion

        #region GetSharedAccessSignature

        public async Task<SharedAccessSignatureResult> GetSharedAccessSignature(string key)
        {
            var blob = Container.GetBlockBlobReference(BuildBlobFileName(key));
            try
            {
                await blob.FetchAttributesAsync();

                if (blob.Properties.Length == 0)
                {
                    return new SharedAccessSignatureResult { Success = true };
                }
            }
            catch
            {
                return new SharedAccessSignatureResult();
            }

            var policy = new SharedAccessBlobPolicy();
            policy.SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24);
            policy.Permissions = SharedAccessBlobPermissions.Read;
            return new SharedAccessSignatureResult
            {
                SharedAccessSignatureUrl = string.Concat(blob.Uri, blob.GetSharedAccessSignature(policy)),
                Success = true
            };
        }

		#endregion

		#region Initialize

		public async Task InitializeAsync(string container, bool multiRegionSupport = false)
        {
            try
            {
                MultiRegionSupport = multiRegionSupport;
                Client = AzureHelper.BlobStorageAccount;
                Container = Client.GetContainerReference(container);
                if (container.Contains("/"))
                {
                    RootContainerName = container.Substring(0, container.IndexOf("/"));
                    SubContainerName = container.Substring(container.IndexOf("/") + 1);
                }
                else
                {
                    RootContainerName = container;
                }

                var rootContainerReference = Client.GetContainerReference(RootContainerName);
                await rootContainerReference.CreateIfNotExistsAsync();

                if (MultiRegionSupport)
                {
                    var regionalUrls = await AzureRegionResolver.Instance.GetRegionalKeyVaultSecretAsync(KeyVaultSecrets.StorageAccounts.CitaviWeb);
                    foreach (var regionalUrl in regionalUrls)
                    {
                        var cloudStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(regionalUrl.Value);
                        var azureRegionClient = cloudStorageAccount.CreateCloudBlobClient();
                        var azureRegionContainer = azureRegionClient.GetContainerReference(container);
                        if(string.Equals(Container.Uri.AbsoluteUri, azureRegionContainer.Uri.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase))
						{
                            continue;
						}
                        MultiRegionBlobs[regionalUrl.Key] = azureRegionContainer;

                        var azureRegionRootContainerReference = azureRegionClient.GetContainerReference(RootContainerName);
                        await azureRegionRootContainerReference.CreateIfNotExistsAsync();
                    }
                }
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
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
                var blobName = BuildBlobFileName(key);
                var blob = Container.GetBlockBlobReference(blobName);
                await blob.DeleteIfExistsAsync();
                await DeleteMultiRegionAsync(blobName);
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
            }
        }

        #endregion

        #region UploadFromByteArrayMultiRegionAsync
        async Task UploadFromByteArrayMultiRegionAsync(string blobName, byte[] bytes)
        {
            if (!MultiRegionSupport)
            {
                return;
            }

            var tasks = new List<Task>();
            foreach (var regionBlobContainer in MultiRegionBlobs.Values)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var blob = regionBlobContainer.GetBlockBlobReference(blobName);
                        await blob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
                    }
                    catch (Exception ex)
                    {
                        Telemetry.TrackException(ex, SeverityLevel.Warning, ExceptionFlow.Eat);
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }

        #endregion
    }
}
