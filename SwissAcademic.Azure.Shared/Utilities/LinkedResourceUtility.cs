using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SwissAcademic.Azure.Shared
{
	public static class LinkedResourceUtility
    {
        #region FileNameFromContentDisposition

        public static Regex FileNameRegex = new Regex(@"filename\s*=\s*(?<FileName>.*)", RegexOptions.IgnoreCase);
        public static string FileNameFromContentDisposition(string contentDisposition)
        {
            if (string.IsNullOrEmpty(contentDisposition)) return null;
            var match = FileNameRegex.Match(contentDisposition);
            return match.Success ? WebUtility.UrlDecode(match.Groups["FileName"].Value) : null;
        }

        #endregion
    }
}
