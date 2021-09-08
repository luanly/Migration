using System.IO;

namespace System.Drawing
{
    public static class ImageUtility
    {


        public static bool TryResizeImage(this byte[] originalImageData, double startScale, double maxScale, long targetSizeInBytes, out byte[] resultingImageData)
        {
            var scale = startScale;
            byte[] data = originalImageData;
            resultingImageData = null;

            while (data.Length > targetSizeInBytes && scale >= maxScale)
            {
                data = ResizeImage(originalImageData, scale);
                scale -= 0.1;
            }

            if (data.Length > targetSizeInBytes)
                return false;
            else
            {
                resultingImageData = data;
                return true;
            }
        }

        public static byte[] ResizeImage(this byte[] originalImageData, double scale)
        {
            using (var memoryStream = new MemoryStream(originalImageData))
            {
                var originalImage = Image.FromStream(memoryStream);

                using (var outputStream = new MemoryStream())
                {
                    ResizeImage(originalImage, scale).Save(outputStream, originalImage.RawFormat);
                    return outputStream.ToArray();
                }
            }
        }

        public static Image ResizeImage(this Image originalImage, double scale)
        {
            Bitmap bmp = new Bitmap((int)Math.Ceiling(originalImage.Width * scale), (int)Math.Ceiling(originalImage.Height * scale));

            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(originalImage, 0, 0, bmp.Width, bmp.Height);
            }
            return bmp;
        }
    }
}
