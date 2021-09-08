using System.IO.Compression;
using System.Threading.Tasks;

namespace System.IO
{
    public static class StreamUtility
    {
        #region IsZipArchive

        //http://www.onicos.com/staff/iz/formats/zip.html
        const int ZIP_Header_Signature = 0x04034b50;

        public static bool IsZipArchive(this Stream stream)
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                var bytes = new byte[4];
                stream.Read(bytes, 0, 4);
                return (BitConverter.ToInt32(bytes, 0) == ZIP_Header_Signature);
            }
            catch
            {
                return false;
            }
            finally
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
        }

        #endregion

        #region ToByteArray

        public static byte[] ToByteArray(Action<Stream> toStreamAction)
        {
            using (var memoryStream = new MemoryStream())
            {
                toStreamAction(memoryStream);
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        #endregion

        #region UnZipToStreamAsync

        public static async Task UnZipToStreamAsync(this Stream self, Stream targetStream)
        {
            using (var zipStream = new GZipStream(self, CompressionMode.Decompress, true))
            {
                await zipStream.CopyToAsync(targetStream);
            }
            targetStream.Position = 0;
        }

        #endregion

        #region ZipToStreamAsync

        public static async Task ZipToStreamAsync(this Stream self, Stream targetStream, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            using (var zipStream = new GZipStream(targetStream, compressionLevel, true))
            {
                self.Position = 0;
                await self.CopyToAsync(zipStream);
            }
            targetStream.Position = 0;
        }

        #endregion

        #region UnZipToStreamAsync

        public static void UnZipToStream(this Stream self, Stream targetStream)
        {
            using (var zipStream = new GZipStream(self, CompressionMode.Decompress, true))
            {
                zipStream.CopyTo(targetStream);
            }
            targetStream.Position = 0;
        }

        #endregion

        #region ZipToStreamAsync

        public static void ZipToStream(this Stream self, Stream targetStream, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            using (var zipStream = new GZipStream(targetStream, compressionLevel, true))
            {
                self.Position = 0;
                self.CopyTo(zipStream);
            }
            targetStream.Position = 0;
        }

        #endregion
    }
}
