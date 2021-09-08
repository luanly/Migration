using System.Collections.Generic;
using System.Linq;

namespace System.IO
{
    public static class DirectoryInfoExtensions
    {
        public static IEnumerable<FileInfo> EnumerateFilesWithoutHiddenAndSystem(this DirectoryInfo directoryInfo, string searchPattern, SearchOption searchOption)
        {
            foreach (var file in from file in directoryInfo.EnumerateFiles(searchPattern, searchOption)
                                 where
                                    (file.Attributes & FileAttributes.Hidden) == 0 &&
                                    (file.Attributes & FileAttributes.System) == 0
                                 select file)
            {
                yield return file;
            }
        }


    }
}
