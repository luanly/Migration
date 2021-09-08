using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwissAcademic.Azure.Storage
{
    public class AzureBlockBlobUploader
    {
        #region Constants

        const int BlockSize = 256 * 1024;

        #endregion

        #region Fields

        Progress<int> _progressHandler;
        CloudBlockBlob _blob;

        #endregion

        #region Constructors

        public AzureBlockBlobUploader(CloudBlockBlob blob, Progress<int> progress = null)
        {
            _blob = blob;
            _progressHandler = progress;
        }

        #endregion

        #region Methods

        #region ExecuteAsync

        public async Task ExecuteAsync(string fileName, CancellationToken cancellationToken)
        {
            var fileInfo = new FileInfo(fileName);

            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            var bytesToUpload = fileInfo.Length;

            _blob.StreamWriteSizeInBytes = BlockSize;

            if (bytesToUpload < BlockSize)
            {
                await _blob.UploadFromFileAsync(fileInfo.FullName, cancellationToken);
                _progressHandler?.ReportSafe(100);
            }
            else
            {
                var blockIds = new List<string>();
                var index = 1;
                long startPosition = 0;
                long bytesUploaded = 0;

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var bytesToRead = Math.Min(BlockSize, bytesToUpload);
                    var blobContents = new byte[bytesToRead];

                    using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open))
                    {
                        fileStream.Position = startPosition;
                        await fileStream.ReadAsync(blobContents, 0, (int)bytesToRead);
                    }

                    var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(index.ToString("d6")));
                    blockIds.Add(blockId);

                    await _blob.PutBlockAsync(blockId, new MemoryStream(blobContents), null, cancellationToken);

                    bytesUploaded += bytesToRead;
                    bytesToUpload -= bytesToRead;
                    startPosition += bytesToRead;
                    index++;

                    _progressHandler?.ReportSafe(bytesUploaded, fileInfo.Length);
                }
                while (bytesToUpload > 0);

                await _blob.PutBlockListAsync(blockIds, cancellationToken);
            }
        }

        public async Task ExecuteAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));


            var bytesToUpload = stream.Length;

            _blob.StreamWriteSizeInBytes = BlockSize;

            if (bytesToUpload < BlockSize)
            {
                await _blob.UploadFromStreamAsync(stream, cancellationToken);
                _progressHandler?.ReportSafe(100);
            }
            else
            {
                var blockIds = new List<string>();
                var index = 1;
                long startPosition = 0;
                long bytesUploaded = 0;

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var bytesToRead = Math.Min(BlockSize, bytesToUpload);
                    var blobContents = new byte[bytesToRead];

                    stream.Position = startPosition;
                    await stream.ReadAsync(blobContents, 0, (int)bytesToRead);

                    var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(index.ToString("d6")));
                    blockIds.Add(blockId);

                    await _blob.PutBlockAsync(blockId, new MemoryStream(blobContents), null, cancellationToken);

                    bytesUploaded += bytesToRead;
                    bytesToUpload -= bytesToRead;
                    startPosition += bytesToRead;
                    index++;

                    _progressHandler?.ReportSafe(bytesUploaded, stream.Length);
                }
                while (bytesToUpload > 0);

                await _blob.PutBlockListAsync(blockIds, cancellationToken);
            }
        }

        #endregion

        #endregion
    }
}
