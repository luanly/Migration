using System.Collections.Generic;
using System.Security.AccessControl;

namespace System.IO
{
    public static class File2
    {
        #region IsOpen

        public static bool IsOpen(string filePath)
        {
            if (File.Exists(filePath) == false) throw new FileNotFoundException("{0} is not found".FormatString(filePath));

            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }

        #endregion

        #region EnsureFileIsWritable

        public static void EnsureFileIsWritable(string filePath)
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
            }
        }

        #endregion

        #region EnsureFileIsWritable2

        public static void EnsureFileIsWritable2(string filePath)
        {
            var fileInfo2 = new FileInfo(filePath);

            if (fileInfo2.Attributes.HasFlag(FileAttributes.ReadOnly)) throw new Exception(SwissAcademic.Resources.Strings.PdfExport_ReadOnlyFile.FormatString(filePath));

            EnsureFileIsWritable(filePath);
        }

        #endregion

        #region TryDelete

        public static bool TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}
