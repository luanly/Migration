using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trinet.Core.IO.Ntfs;

namespace System.IO
{
    public static class FileInfoUtility
    {
        #region Constants

        const string CitaviMetadataHiddenFilePrefix = "~Ctv";
        const string CitaviMetadataAdsName = "CitaviMetadata";

        #endregion

        #region ComputeFileHash

        public static string ComputeFileHash(this FileInfo fileInfo)
        {
            using (var file = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
#pragma warning disable CA5351, SCS0006 // Keine beschädigten kryptografischen Algorithmen verwenden
                var md5 = new MD5CryptoServiceProvider();
#pragma warning restore CA5351, SCS0006 // Keine beschädigten kryptografischen Algorithmen verwenden
                return Convert.ToBase64String(md5.ComputeHash(file));
            }
        }

        #endregion

        #region CopyOrMoveAsync

        static async Task CopyOrMoveAsync(FileInfo fileInfo, string destinationPath, bool move, bool overwrite, CancellationToken cancellationToken)
        {
            if (!overwrite && File.Exists(destinationPath))
            {
                throw new IOException("The file exists already.");
            }

            using (var sourceStream = fileInfo.OpenRead())
            {
                using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    await CopyOrMoveAsync(sourceStream, destinationStream, cancellationToken);
                }
            }

            if (move) fileInfo.Delete();
        }

        static Task CopyOrMoveAsync(Stream sourceStream, Stream destinationStream, CancellationToken cancellationToken) => sourceStream.CopyToAsync(destinationStream, 4096, cancellationToken);

        #endregion

        #region CopyToAsync

        public static Task CopyToAsync(this FileInfo fileInfo, string destinationPath) => CopyOrMoveAsync(fileInfo, destinationPath, false, false, CancellationToken.None);

        public static Task CopyToAsync(this FileInfo fileInfo, string destinationPath, CancellationToken cancellationToken) => CopyOrMoveAsync(fileInfo, destinationPath, false, false, cancellationToken);

        public static Task CopyToAsync(this FileInfo fileInfo, string destinationPath, bool overwrite, CancellationToken cancellationToken) => CopyOrMoveAsync(fileInfo, destinationPath, false, overwrite, cancellationToken);

        public static async Task CopyToAsync(this FileInfo fileInfo, Stream destinationStream, CancellationToken cancellationToken)
        {
            using (var sourceStream = fileInfo.OpenRead())
            {
                await CopyOrMoveAsync(sourceStream, destinationStream, cancellationToken);
            }
        }

        #endregion

        #region DeleteMetadata

        public static void DeleteMetadata(this FileInfo fileInfo)
        {
            try
            {
                if (new DriveInfo(Path.GetPathRoot(fileInfo.FullName)).DriveFormat.Equals("NTFS", StringComparison.Ordinal))
                {
                    if (fileInfo.AlternateDataStreamExists(CitaviMetadataAdsName))
                    {
                        fileInfo.DeleteAlternateDataStream(CitaviMetadataAdsName);
                    }
                }
                else
                {
                    var metaDataFileInfo = new FileInfo(Path.Combine(fileInfo.DirectoryName, CitaviMetadataHiddenFilePrefix + fileInfo.Name));
                    if (!metaDataFileInfo.Exists) return;

                    metaDataFileInfo.Delete();
                }
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                throw new IOException(Strings.MetadataError, exception);
            }
        }

        #endregion 

        #region GetFileInfo2

        public static FileInfo2 GetFileInfo2(this FileInfo fileInfo)
        {
            return new FileInfo2(fileInfo);
        }

        #endregion

        #region GetMetaData

        public static IDictionary<string, string> GetMetadata(this FileInfo fileInfo)
        {
            try
            {
                if (new DriveInfo(Path.GetPathRoot(fileInfo.FullName)).DriveFormat.Equals("NTFS", StringComparison.Ordinal))
                {
                    return GetAdsMetadata(fileInfo);
                }
                else
                {
                    return GetHiddenFileMetadata(fileInfo);
                }
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                throw new IOException(Strings.MetadataError, exception);
            }
        }

        #region GetAdsMetadata

        /// <summary>
        /// Reads a metadata dictionary from an alternate data stream. Only works with NTFS drives.
        /// </summary>
        /// <param name="fileInfo"></param>
        static IDictionary<string, string> GetAdsMetadata(FileInfo fileInfo)
        {
            if (!fileInfo.AlternateDataStreamExists(CitaviMetadataAdsName)) return null;

            var streamInfo = fileInfo.GetAlternateDataStream(CitaviMetadataAdsName, FileMode.Open);

            using (var stream = streamInfo.OpenRead())
            {
                using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                {
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        var serializer = new JsonSerializer();
                        var metadata = serializer.Deserialize<IDictionary<string, string>>(jsonTextReader);
                        var keys = new List<string>(metadata.Keys);
                        foreach (var key in keys)
                        {
                            try
                            {
                                metadata[key] = metadata[key].DecodeFrom64();
                            }
                            catch (FormatException ignored)
                            {
                                //This error is thrown, if string is not Base64 encoded: Invalid length for a Base-64 char array or string
                                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat, (nameof(TelemetryProperty.Description), "String cannot be decoded from Base64"), ("Metadata[Key]", metadata[key]));
                            }
                            catch (Exception exception)
                            {
                                Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                                throw new IOException(Strings.MetadataError, exception);
                            }
                        }

                        return metadata;
                    }
                }
            }
        }

        #endregion

        #region GetHiddenFileMetadata

        /// Reads a metadata dictionary from a hidden file starting with ~CtvFileName. Works also with non-NTFS drives.
        /// </summary>
        /// <remarks>~Ctv is used as a prefix because there could already be another file with a tilde in front, for
        /// example with office documents.</remarks>
        /// <param name="fileInfo"></param>
        static IDictionary<string, string> GetHiddenFileMetadata(FileInfo fileInfo)
        {
            var metaDataFileInfo = new FileInfo(Path.Combine(fileInfo.DirectoryName, CitaviMetadataHiddenFilePrefix + fileInfo.Name));
            if (!metaDataFileInfo.Exists) return null;

            using (var fileStream = File.OpenRead(metaDataFileInfo.FullName))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        var serializer = new JsonSerializer();
                        var metadata = serializer.Deserialize<IDictionary<string, string>>(jsonTextReader);
                        var keys = new List<string>(metadata.Keys);
                        foreach (var key in keys)
                        {
                            try
                            {
                                metadata[key] = metadata[key].DecodeFrom64();
                            }
                            catch (FormatException ignored)
                            {
                                //This error is thrown, if string is not Base64 encoded: Invalid length for a Base-64 char array or string
                                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat, (nameof(TelemetryProperty.Description), "String cannot be decoded from Base64"), ("Metadata[Key]", metadata[key]));
                            }
                            catch (Exception exception)
                            {
                                Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                                throw new IOException(Strings.MetadataError, exception);
                            }
                        }

                        return metadata;
                    }
                }
            }
        }

        #endregion

        #endregion

        #region GetNameWithoutExtension

        public static string GetNameWithoutExtension(this FileInfo fileInfo)
        {
            if (fileInfo == null) return null;
            return Path.GetFileNameWithoutExtension(fileInfo.FullName);
        }

        #endregion

        #region MoveToAsync

        public static Task MoveToAsync(this FileInfo fileInfo, string destinationPath)
        {
            return CopyOrMoveAsync(fileInfo, destinationPath, true, false, CancellationToken.None);
        }

        public static Task MoveToAsync(this FileInfo fileInfo, string destinationPath, CancellationToken cancellationToken)
        {
            return CopyOrMoveAsync(fileInfo, destinationPath, true, false, cancellationToken);
        }

        public static Task MoveToAsync(this FileInfo fileInfo, string destinationPath, bool overwrite, CancellationToken cancellationToken)
        {
            return CopyOrMoveAsync(fileInfo, destinationPath, true, overwrite, cancellationToken);
        }

        #endregion

        #region SetMetadata

        public static void SetMetadata(this FileInfo fileInfo, IDictionary<string, string> metadata)
        {
            try
            {
                if (new DriveInfo(Path.GetPathRoot(fileInfo.FullName)).DriveFormat.Equals("NTFS", StringComparison.Ordinal))
                {
                    SetAdsMetadata(fileInfo, metadata);
                }
                else
                {
                    SetHiddenFileMetadata(fileInfo, metadata);
                }
            }

            catch (Exception exception)
            {
                Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                throw new IOException(Strings.MetadataError, exception);
            }
        }

        #region SetAdsMetadata

        /// <summary>
        /// Writes a metadata dictionary into an alternate data stream. Only works with NTFS drives.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="metadata"></param>
        static void SetAdsMetadata(FileInfo fileInfo, IDictionary<string, string> metadata)
        {
            if (metadata == null || !metadata.Any())
            {
                if (fileInfo.AlternateDataStreamExists(CitaviMetadataAdsName))
                {
                    fileInfo.DeleteAlternateDataStream(CitaviMetadataAdsName);
                }

                return;
            }

            metadata = (from pair in metadata
                        select pair).ToDictionary(pair => pair.Key, pair => pair.Value.EncodeToBase64());

            var streamInfo = fileInfo.GetAlternateDataStream(CitaviMetadataAdsName, FileMode.Create);

            using (var stream = streamInfo.OpenWrite())
            {
                using (var textWriter = new StreamWriter(stream, Encoding.UTF8))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(textWriter, metadata);
                }
            }
        }

        #endregion

        #region SetHiddenFileMetadata

        /// <summary>
        /// Writes a metadata dictionary into a hidden file starting with ~CtvFileName. Works also with non-NTFS drives.
        /// </summary>
        /// <remarks>~Ctv is used as a prefix because there could already be another file with a tilde in front, for
        /// example with office documents.</remarks>
        /// <param name="fileInfo"></param>
        /// <param name="metadata"></param>

        static void SetHiddenFileMetadata(FileInfo fileInfo, IDictionary<string, string> metadata)
        {
            var metaDataFileInfo = new FileInfo(Path.Combine(fileInfo.DirectoryName, CitaviMetadataHiddenFilePrefix + fileInfo.Name));

            if (metadata == null || !metadata.Any())
            {
                if (metaDataFileInfo.Exists)
                {
                    metaDataFileInfo.Delete();
                }

                return;
            }

            metadata = (from pair in metadata
                        select pair).ToDictionary(pair => pair.Key, pair => pair.Value.EncodeToBase64());

            using (var fileStream = new FileStream(metaDataFileInfo.FullName, FileMode.Create))
            {
                using (var stream = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(stream, metadata);
                }
            }
        }

        #endregion

        #endregion
    }

    public class FileInfo2
    {
        public FileInfo2(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            if (fileInfo.Exists)
            {
                IsDriveAttached = true;
                LastWriteTimeString = fileInfo.LastWriteTimeUtc.ToString("s");
            }

            else
            {
                IsDriveAttached = fileInfo.Directory.Root.Exists;
                LastWriteTimeString = string.Empty;
            }
        }

        public bool IsDriveAttached { get; private set; }
        public string LastWriteTimeString { get; private set; }
    }
}
