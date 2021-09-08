using System.IO;
using System.IO.Compression;

namespace System
{
    public static class ByteArrayUtility
    {

        public static byte[] GZipCompress(this byte[] uncompressedData)
        {
            if (uncompressedData == null) return null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    gzipStream.Write(uncompressedData, 0, uncompressedData.Length);
                }

                return memoryStream.ToArray();
            }
        }


        public static byte[] GZipDecompress(this byte[] compressedData)
        {
            if (compressedData == null) return null;

            byte[] decompressedData = null;

            using (MemoryStream outputStream = new MemoryStream())
            {
                using (MemoryStream inputStream = new MemoryStream(compressedData))
                {
                    using (GZipStream zip = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        zip.CopyTo(outputStream);
                    }
                }
                decompressedData = outputStream.ToArray();
            }

            return decompressedData;
        }
    }
}
