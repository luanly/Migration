using SwissAcademic;
using SwissAcademic.ApplicationInsights;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System
{
    // Icon grabbing code from http://www.codeproject.com/KB/files/fileicon.aspx

    #region UriFormatStyle

    public enum UriFormatStyle
    {
        /// <summary>
        /// Returns the standard Uri.ToString() result.
        /// </summary>
        Base,

        /// <summary>
        /// For a local file Uri, returns the LocalPath. For a remote Uri, returns the AbsoluteUri.
        /// </summary>
        Default,

        /// <summary>
        /// For a local file Uri, returns the file name. For a remote Uri, returns the host and the file name in the scheme "www.citavi.com ... document.pdf"
        /// </summary>
        FileName,

        /// <summary>
        /// For a local file Uri, returns "FileName (FilePath)". For a remote Uri, returns "Host (Url)".
        /// </summary>
        FileNameAndCompleteUriInParentheses,

        /// <summary>
        /// For a local file Uri, returns the FileName with the FilePath on a new line. For a remote Uri, returns the Host and the Url on a new line)".
        /// </summary>
        FileNameAndCompleteUriOnNewLine,

        /// <summary>
        /// Returns a full listing of all properties.
        /// </summary>
        Full
    }

    #endregion

    public static class UriUtility
    {
        #region Enums

        #region IconSize

        /// <summary>
        /// Options to specify the size of icons to return.
        /// </summary>
        public enum IconSize
        {
            /// <summary>
            /// Specify large icon - 32 pixels by 32 pixels.
            /// </summary>
            Large,
            /// <summary>
            /// Specify small icon - 16 pixels by 16 pixels.
            /// </summary>
            Small = 1
        }

        #endregion

        #endregion

        #region ExtensionAndIconPair class

#if !Web
        class ExtensionAndBitmapPair
        {
            // Key is "Small" + extension or "Large" + extension
            public string Key;
            public Bitmap Bitmap;
        }
        static List<ExtensionAndBitmapPair> _cache = new List<ExtensionAndBitmapPair>();
#endif

        #endregion

        #region Felder


        public static Regex ArxivRegex = new Regex("^(arxiv\\s*\\:?\\s*)?(?<ID>\\d\\d(01|02|03|04|05|06|07|08|09|10|11|12)\\.\\d{4,5}(v\\d{1,3})?)|(arxiv\\s*\\:?\\s*)(?<ID>(?<archive>\\w+){0,1}(?<subject>\\.\\w+){0,1}/\\d+(v\\d{1,3})?)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        public static readonly Regex DoiRegex = new Regex(@"(doi:?\s*|doi.org\/)?(?<DOI>(10\.\d+\/.+?)|((10\/|\b)\w{4,6}))(\s|$)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        public static Regex PmcIdRegex = new Regex(@"^pmc\d+(\s|$)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex _simpleWebAdressRegex = new Regex(@"^www\d*\.");
        public static Regex UrnRegex = new Regex(@"(?<Protocol>(?<=(^|\s+))urn):(?<URN>(?<NID>[a-z0-9][:a-z0-9-]{1,31}):(?<NSS>[a-z0-9%\(\)+,-.:=@;\$_!*']+))", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        public static Regex UrlRegex = new Regex(@"(^|(?<=;)|(?<=\s))((\w+://|(www[0-9]*)\.|urn:|doi:|mailto:).+?)((?=;?\s)|(?=;$)|(?=;\w+://)|(?=;mailto:)|(?=;doi:)|(?=;urn:)|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        #endregion

        #region Methoden

        #region AddReferrers

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Uri AddReferrers(this Uri uri)
        {
            if (uri == null || !uri.IsAbsoluteUri) return uri;

            if (uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
                uri.Host.EndsWith("worldcat.org", StringComparison.OrdinalIgnoreCase) &&
                !uri.PathAndQuery.EndsWith("citavi", StringComparison.OrdinalIgnoreCase))
            {
                var url = string.Concat(uri, "&ap=citavi");
                return new Uri(url);
            }

            return uri;
        }

        #endregion

        #region CreateUri

        public static Uri CreateUri(string url, bool mapRelativeAddressToAbsoluteHttpUri = false)
        {
            url = url.Clean(IllegalCharacters.Return | IllegalCharacters.Tab);

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                var match = _simpleWebAdressRegex.Match(url);
                if (match.Success)
                {
                    url = $"http://{url}";
                }
                else
                {
                    match = DoiRegex.Match(url);
                    if (match.Success && match.Index == 0)
                    {
                        return new Uri($"https://doi.org/{match.Groups["DOI"].Value}");
                    }

                    if (url.IsNumeric())
                    {
                        return new Uri($"http://www.ncbi.nlm.nih.gov/pubmed/{url}");
                    }

                    match = ArxivRegex.Match(url);
                    if (match.Success)
                    {
                        return new Uri($"https://arxiv.org/pdf/{match.Groups["ID"].Value}.pdf");
                    }

                    match = UrnRegex.Match(url);
                    if (match.Success)
                    {
                        return new Uri($"http://nbn-resolving.de/urn/resolver.pl?urn:{match.Groups["URN"].Value}");
                    }

                    match = PmcIdRegex.Match(url);
                    if (match.Success)
                    {
                        return new Uri($"https://www.ncbi.nlm.nih.gov/pmc/articles/{match.Value}");
                    }
                }
            }

#if !Web
            if (url.StartsWith("%"))
            {
                url = SwissAcademic.Environment.ExpandEnvironmentVariables(url);
            }
#endif
            Uri result;

            try
            {
                if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out result))
                {
                    if (result.IsAbsoluteUri)
                    {
                        if (result.IsFile)
                        {
                            if (url.IndexOfAny(Path.GetInvalidPathChars()) != -1) return null;

#if !Web
                            if (Path.GetExtension(result.GetLocalPathSafe()).Equals(".url", StringComparison.OrdinalIgnoreCase))
                            {
                                var urlFileContent = Path2.ResolveUrlShortcut(result.GetLocalPathSafe());
                                if (!string.IsNullOrEmpty(urlFileContent)) return CreateUri(urlFileContent, mapRelativeAddressToAbsoluteHttpUri);
                            }
#endif
                        }
                    }

                    else
                    {
                        if (url.IndexOfAny(Path.GetInvalidPathChars()) != -1) return null;

                        if (mapRelativeAddressToAbsoluteHttpUri)
                        {
                            Uri absoluteHttpUri;
                            if (Uri.TryCreate(string.Concat("http://", url), UriKind.Absolute, out absoluteHttpUri)) return absoluteHttpUri;
                        }
                    }

                    return result;
                }
            }

            catch (Exception exception)
            {
                Telemetry.TrackException(exception, SeverityLevel.Warning, ExceptionFlow.Eat);
                return null;
            }

            return null;
        }

        #endregion

        #region GetIcon
#if !Web
        public static Bitmap GetIcon(this Uri uri, IconSize size)
        {
            try
            {
                if (!uri.IsAbsoluteUri) throw new ArgumentException("Uri must be an absolute uri", "uri");
                if (!Enum.IsDefined(typeof(IconSize), size)) size = IconSize.Large;

                if (uri.IsFile) return GetIconFromFile(uri, size);
                else return GetIconFromUrl(uri, size);
            }

            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                return GetIconFromFileNative(".blank", size, false);
            }
        }
#endif

        #endregion

        #region GetIconFromFile
#if !Web
        static Bitmap GetIconFromFile(Uri fileUri, IconSize size)
        {
            try
            {
                if (fileUri.IsExistingDirectory())
                {
                    var key = fileUri.GetLocalPathSafe();
                    var pair = _cache.Find(item => string.CompareOrdinal(item.Key, key) == 0);
                    if (pair != null) return pair.Bitmap;

                    var bitmap = GetIconFromFolderNative(key, size);
                    _cache.Add(new ExtensionAndBitmapPair() { Key = key, Bitmap = bitmap });
                    return bitmap;
                }

                else
                {
                    var extension = Path.GetExtension(fileUri.AbsoluteUri).ToLowerInvariant();

                    string key;
                    ExtensionAndBitmapPair pair;
                    Bitmap bitmap;

                    if (string.IsNullOrEmpty(extension))
                    {
                        key = size.ToString() + "Blank";
                        pair = _cache.Find(item => string.CompareOrdinal(item.Key, key) == 0);
                        if (pair != null) return pair.Bitmap;

                        bitmap = GetIconFromFileNative(".blank", size, false);
                        _cache.Add(new ExtensionAndBitmapPair() { Key = key, Bitmap = bitmap });
                        return bitmap;
                    }

                    key = size.ToString() + extension;
                    pair = _cache.Find(item => string.CompareOrdinal(item.Key, key) == 0);
                    if (pair != null) return pair.Bitmap;

                    bitmap = GetIconFromFileNative(fileUri.GetLocalPathSafe(), size, false);
                    _cache.Add(new ExtensionAndBitmapPair() { Key = key, Bitmap = bitmap });
                    return bitmap;
                }
            }

            catch (Exception exception)
            {
                Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                return GetIconFromFileNative(".blank", size, false);
            }
        }
#endif
        #endregion

        #region GetIconFromFileNative
#if !Web
        static Bitmap GetIconFromFileNative(string filePath, IconSize size, bool linkOverlay)
        {
            var shfi = new NativeMethods.SHFILEINFO();
            var flags = NativeMethods.FileInfoFlags.SHGFI_ICON | NativeMethods.FileInfoFlags.SHGFI_USEFILEATTRIBUTES;

            if (linkOverlay) flags |= NativeMethods.FileInfoFlags.SHGFI_LINKOVERLAY;

            if (size == IconSize.Large) flags |= NativeMethods.FileInfoFlags.SHGFI_LARGEICON;
            else flags |= NativeMethods.FileInfoFlags.SHGFI_SMALLICON;

            NativeMethods.SHGetFileInfo(filePath, (uint)NativeMethods.FileAttribute.FILE_ATTRIBUTE_NORMAL, ref shfi, (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi), (uint)flags);

            var icon = Icon.FromHandle(shfi.hIcon);
            var bitmap = icon.ToBitmap();

            // Cleanup
            icon.Dispose();
            NativeMethods.DestroyIcon(shfi.hIcon);

            return bitmap;
        }

#endif

        #endregion

        #region GetIconFromFolderNative
#if !Web
        static Bitmap GetIconFromFolderNative(string folderPath, IconSize size)
        {
            var flags = NativeMethods.FileInfoFlags.SHGFI_ICON;

            if (size == IconSize.Large) flags |= NativeMethods.FileInfoFlags.SHGFI_LARGEICON;
            else flags |= NativeMethods.FileInfoFlags.SHGFI_SMALLICON;


            // Get the folder icon
            var shfi = new NativeMethods.SHFILEINFO();
            NativeMethods.SHGetFileInfo(folderPath, (uint)NativeMethods.FileAttribute.FILE_ATTRIBUTE_DIRECTORY, ref shfi, (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi), (uint)flags);

            var icon = Icon.FromHandle(shfi.hIcon);

            var bitmap = icon.ToBitmap();

            // Cleanup
            icon.Dispose();
            NativeMethods.DestroyIcon(shfi.hIcon);

            return bitmap;
        }
#endif
        #endregion

        #region GetIconFromUrl
#if !Web
        static Bitmap GetIconFromUrl(Uri remoteUri, IconSize size)
        {
            var extension = remoteUri.GetLocalPathSafe().Intersect(Path.GetInvalidPathChars()).Any() ?
                string.Empty :
                Path.GetExtension(remoteUri.GetLocalPathSafe()).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension))
            {
                return GetIconFromFileNative(".htm", size, false);
            }

            switch (extension)
            {
                case ".doc":
                case ".docx":
                case ".odt":
                case ".pdf":
                case ".ppt":
                case ".pptx":
                case ".xls":
                case ".xlsx":
                    return GetIconFromFileNative(extension, size, false);

                default:
                    return GetIconFromFileNative(".htm", size, false);
            }
        }

#endif

        #endregion

        #region GetDirectoryInfo

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Gets a <see cref="DirectoryInfo"/> object for the specified URI. Returns null if the URI is
        /// null or not a File URI. This method checks if the URI points to an existing directory. If yes,
        /// this directory is returned. Otherwise, the parent directory of URI.LocalPath is returned. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 30.04.2010. </remarks>
        ///
        /// <param name="uri">	The URI for the FileInfo. </param>
        ///
        /// <returns>	The directory information. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static DirectoryInfo GetDirectoryInfo(this Uri uri)
        {
            if (uri == null) return null;
            if (!uri.IsAbsoluteUri) return null;
            if (!uri.IsFile) return null;

            if (uri.IsExistingDirectory()) return new DirectoryInfo(uri.GetLocalPathSafe());

            var fileInfo = uri.GetFileInfo();
            // FileInfo can be "C:\". In this case, fileInfo.Directory is null.
            if (fileInfo == null) return new DirectoryInfo(uri.GetLocalPathSafe());
            return fileInfo.Directory;
        }

        #endregion

        #region GetExecutablesFriendlyName

#if !Web
        public static string GetExecutablesFriendlyName(this Uri uri)
        {
            if (uri == null) return "Windows";
            if (uri.IsAbsoluteUri && !uri.IsFile) return Path2.GetExecutablesFriendlyName(".html");

            string extension;
            try
            {
                extension = Path.GetExtension(uri.ToString(UriFormatStyle.Default));
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                extension = string.Empty;
            }

            if (string.IsNullOrEmpty(extension)) return "Windows";
            return Path2.GetExecutablesFriendlyName(extension);
        }

#endif

        #endregion

        #region GetFileInfo

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Gets a <see cref="FileInfo"/> object for the specified URI. Returns null if the URI is null
        /// or not a File URI. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 30.04.2010. </remarks>
        ///
        /// <param name="uri">	The URI for the FileInfo. </param>
        ///
        /// <returns>	The FileInfo object. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static FileInfo GetFileInfo(this Uri uri)
        {
            if (uri == null) return null;
            if (!uri.IsAbsoluteUri) return null;
            if (!uri.IsFile) return null;

            return new FileInfo(uri.GetLocalPathSafe());
        }

        #endregion

        #region GetLocalPathSafe

        static Regex localPathSafeRegex = new Regex(@"(%[0-9A-Fa-f]{2}){1,2}");

        public static string GetLocalPathSafe(this Uri uri)
        {
            try
            {
                //see ST 3BF-1DAF8B19-047D: 
                //" " is %20 (3 charachters wide) but "ä" is %C3%A (6 characters wide) - this was not considered by the previous version of this extension method
                if (uri == null) return null;
                if (!uri.IsAbsoluteUri) return uri.OriginalString;
                if (!uri.IsFile) return uri.LocalPath;

                var localPath = uri.LocalPath;

                foreach (Match match in localPathSafeRegex.Matches(uri.OriginalString))
                {
                    if (localPath[match.Index] != '%')
                    {
                        localPath = localPath.Substring(0, match.Index) + uri.OriginalString.Substring(match.Index, match.Length) + localPath.Substring(match.Index + 1);
                    }
                }

                return localPath;
            }

            catch (Exception exception)
            {
                Telemetry.TrackException(exception, SeverityLevel.Warning, ExceptionFlow.Eat);
                return uri.LocalPath;
            }
        }

        #endregion

        #region IsExistingDirectory

        public static bool IsExistingDirectory(this Uri uri)
        {
            if (uri == null) return false;
            if (!uri.IsAbsoluteUri) return false;
            if (!uri.IsFile) return false;

            return Directory.Exists(uri.GetLocalPathSafe());
        }

        #endregion

        #region IsExistingFile

        public static bool IsExistingFile(this Uri uri)
        {
            if (uri == null) return false;
            if (!uri.IsAbsoluteUri) return false;
            if (!uri.IsFile) return false;

            return File.Exists(uri.GetLocalPathSafe());
        }

        #endregion

        #region RemoveReferrers

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Uri RemoveReferrers(this Uri uri)
        {
            if (uri == null || !uri.IsAbsoluteUri) return uri;

            var worldCatReferrer = "ap=citavi";
            if (uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
                uri.Host.Equals("www.worldcat.org", StringComparison.OrdinalIgnoreCase) &&
                uri.Query.EndsWith(worldCatReferrer, StringComparison.OrdinalIgnoreCase))
            {
                var url = uri.AbsoluteUri;
                return new Uri(url.Substring(0, url.Length - (worldCatReferrer.Length + 1)));
            }

            return uri;
        }

        #endregion

        #region Unescape

        public static Uri Unescape(this Uri uri) => Unescape(uri, UriKind.RelativeOrAbsolute);

        public static Uri Unescape(this Uri uri, UriKind uriKind)
        {
            var uriString = UnescapeDataString(uri.ToString());
            return new Uri(uriString, uriKind);
        }

        #endregion

        #region UnescapeDataString

        public static string UnescapeDataString(string stringToUnescape) => Uri.UnescapeDataString(stringToUnescape).Replace(":", "%3A");

        #endregion

        #region ToString

        public static string ToString(this Uri uri, UriFormatStyle style)
        {
            //JHP Prof. K. hat hier einge Fehler "Illegales Zeichen im Pfad", aber es ist unklar, welche URI dies verursacht.
            //Die Log Funktionen geben dies nicht her. Es ist auch unklar, welche Methode dies Extension bemühte, als der Fehler auftrat.

            if (uri == null) return string.Empty;
            if (!uri.IsAbsoluteUri) return uri.ToString();

            //BUG 12526: Die Vorschau parste "optionale Trennstriche" in URLs nicht: %E2%80%8B
            var absoluteUri = uri.AbsoluteUri.Replace("%E2%80%8B", "");

            switch (style)
            {
                #region Default

                case UriFormatStyle.Default:
                    {
                        if (uri.IsFile) return uri.GetLocalPathSafe();
                        return absoluteUri;
                    }

                #endregion

                #region FileName

                case UriFormatStyle.FileName:
                    {
                        if (uri.IsFile)
                        {
                            try
                            {
                                var fileName = Path.GetFileName(uri.GetLocalPathSafe());

                                if (string.IsNullOrEmpty(fileName)) return uri.GetLocalPathSafe();
                                return fileName;
                            }
                            catch (ArgumentException)
                            {
                                return uri.GetLocalPathSafe();
                            }
                        }

                        else
                        {
                            // Yes, "uri.GetLocalPathSafe()" even it's a remote address
                            // uri.AbsolutePath converts escaped characters, uri.GetLocalPathSafe() 
                            // does not.

                            if (uri.AbsolutePath == "/") return uri.Host;

                            try
                            {
                                if (uri.GetLocalPathSafe().Intersect(Path.GetInvalidPathChars()).Any())
                                {
                                    return uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
                                }
                                var fileName = Path.GetFileName(uri.GetLocalPathSafe());
                                if (fileName.Length < uri.GetLocalPathSafe().Length - 10) fileName = uri.Host + " ... " + fileName;
                                else fileName = uri.Host + uri.GetLocalPathSafe();

                                if (uri.Query.Length != 0) return fileName + "?...";
                                return fileName;
                            }

                            catch (Exception ignored)
                            {
                                // BUG 3908
                                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat, (nameof(TelemetryProperty.Description), "Error in Uri.ToString()"), (nameof(uri), uri.ToString(UriFormatStyle.Full)));
                                return uri.Host;
                            }
                        }
                    }

                #endregion //FileName

                #region FileNameAndCompleteUriInParentheses

                case UriFormatStyle.FileNameAndCompleteUriInParentheses:
                    {
                        if (uri.IsFile)
                        {
                            var fileName = Path.GetFileName(uri.GetLocalPathSafe());
                            if (string.IsNullOrEmpty(fileName)) return uri.GetLocalPathSafe();
                            return string.Format("{0}   ({1})", fileName, uri.GetLocalPathSafe());
                        }

                        else
                        {
                            if (uri.AbsolutePath == "/") return uri.Host;

                            try
                            {
                                var fileName = Path.GetFileName(uri.GetLocalPathSafe());
                                if (string.IsNullOrEmpty(fileName)) return uri.ToString();
                                return string.Format("{0}   ({1})", fileName, uri.ToString());
                            }
                            catch (Exception ignored)
                            {
                                // BUG 3908
                                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat, (nameof(TelemetryProperty.Description), "Error in Uri.ToString()"), (nameof(uri), uri.ToString(UriFormatStyle.Full)));
                                return string.Format("{0}   ({1})", uri.Host, absoluteUri);
                            }
                        }
                    }

                #endregion //FileNameAndCompleteUriInParentheses

                #region FileNameAndCompleteUriOnNewLine

                case UriFormatStyle.FileNameAndCompleteUriOnNewLine:
                    {
                        if (uri.IsFile)
                        {
                            var fileName = Path.GetFileName(uri.GetLocalPathSafe());
                            if (string.IsNullOrEmpty(fileName)) return uri.GetLocalPathSafe();
                            return string.Format("{0}\r\n{1}", fileName, uri.GetLocalPathSafe());
                        }

                        else
                        {
                            try
                            {
                                if (uri.AbsolutePath.Equals("/", StringComparison.Ordinal)) return uri.Host;
                                var fileName = Path.GetFileName(uri.GetLocalPathSafe());
                                if (string.IsNullOrEmpty(fileName)) return uri.AbsoluteUri;
                                return string.Format("{0}\r\n{1}", fileName, absoluteUri);
                            }

                            catch (Exception ignored)
                            {
                                // BUG 3908
                                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat, (nameof(TelemetryProperty.Description), "Error in Uri.ToString()"), (nameof(uri), uri.ToString(UriFormatStyle.Full)));
                                return string.Format("{0}\r\n{1}", uri.Host, absoluteUri);
                            }
                        }
                    }

                #endregion //FileNameAndCompleteUIriOnNewLine

                #region Full

                case UriFormatStyle.Full:
                    {
                        //JHP added only for documentation/logging/debugging/learning purposes
                        try
                        {
                            var stringBuilder = new System.Text.StringBuilder();

                            //AbsolutePath
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("AbsolutePath: {0}", uri.AbsolutePath).AppendLine();
                            else stringBuilder.AppendLine("AbsolutePath: not applicable for relative uri");

                            //AbsoluteUri
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("AbsoluteUri: {0}", absoluteUri).AppendLine();
                            else stringBuilder.AppendLine("AbsoluteUri: not applicable for relative uri");

                            //Authority
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("Authority: {0}", uri.Authority).AppendLine();
                            else stringBuilder.AppendLine("Authority: not applicable for relative uri");

                            //DnsSafeHost
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("DnsSafeHost: {0}", uri.DnsSafeHost).AppendLine();
                            else stringBuilder.AppendLine("DnsSafeHost: not applicable for relative uri");

                            //Fragment
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("Fragment: {0}", uri.Fragment).AppendLine();
                            else stringBuilder.AppendLine("Fragment: not applicable for relative uri");

                            //Host
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("Host: {0}", uri.Host).AppendLine();
                            else stringBuilder.AppendLine("Host: not applicable for relative uri");

                            //HostNameType
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("HostNameType: UriHostNameType.{0}", uri.HostNameType).AppendLine();
                            else stringBuilder.AppendLine("HostNameType: not applicable for relative uri");

                            //IsAbsoluteUri
                            stringBuilder.AppendFormat("IsAbsoluteUri: {0}", uri.IsAbsoluteUri).AppendLine();

                            //IsDefaultPort
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("IsDefaultPort: {0}", uri.IsDefaultPort).AppendLine();
                            else stringBuilder.AppendLine("IsDefaultPort: not applicable for relative uri");

                            //IsFile
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("IsFile: {0}", uri.IsFile).AppendLine();
                            else stringBuilder.AppendLine("IsFile: not applicable for relative uri");

                            //IsLoopback
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("IsLoopback: {0}", uri.IsLoopback).AppendLine();
                            else stringBuilder.AppendLine("IsLoopback: not applicable for relative uri");

                            //IsUnc
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("IsUnc: {0}", uri.IsUnc).AppendLine();
                            else stringBuilder.AppendLine("IsUnc: not applicable for relative uri");

                            //LocalPath
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("LocalPath: {0}", uri.GetLocalPathSafe()).AppendLine();
                            else stringBuilder.AppendLine("LocalPath: not applicable for relative uri");

                            //OriginalString
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("OriginalString: {0}", uri.OriginalString).AppendLine();
                            else stringBuilder.AppendLine("OriginalString: not applicable for relative uri");

                            //PathAndQuery
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("PathAndQuery: {0}", uri.PathAndQuery).AppendLine();
                            else stringBuilder.AppendLine("PathAndQuery: not applicable for relative uri");

                            //Port
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("Port: {0}", uri.Port).AppendLine();
                            else stringBuilder.AppendLine("Port: not applicable for relative uri");

                            //Query
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("Query: {0}", uri.Query).AppendLine();
                            else stringBuilder.AppendLine("Query: not applicable for relative uri");

                            //Scheme
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("Scheme: {0}", uri.Scheme).AppendLine();
                            else stringBuilder.AppendLine("Scheme: not applicable for relative uri");

                            //Segments
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("Segments: {0}", uri.Segments).AppendLine();
                            else stringBuilder.AppendLine("Segments: not applicable for relative uri");

                            //UserEscaped
                            stringBuilder.AppendFormat("UserEscaped: {0}", uri.UserEscaped).AppendLine();

                            //UserInfo
                            if (uri.IsAbsoluteUri) stringBuilder.AppendFormat("UserInfo: {0}", uri.UserInfo).AppendLine();
                            else stringBuilder.AppendLine("UserInfo: not applicable for relative uri");

                            return stringBuilder.ToString();
                        }

                        catch (Exception ignored)
                        {
                            return string.Format("Error in Uri.ToString(UriFormatStyle.Full): \n\r{0}", ignored.Message);
                        }

                    }

                #endregion //Full

                default:
                    return uri.ToString();
            }
        }

        #endregion

        #endregion
    }

    public static class UrlBuilder
    {
        public static string Combine(params string[] paths)
        {
            if (paths == null || !paths.Any()) return string.Empty;

            var stringBuilder = new StringBuilder();

            for (int i = 0; i < paths.Length; i++)
            {
                if (string.IsNullOrEmpty(paths[i])) continue;
                if (i != 0) stringBuilder.Append("/");
                stringBuilder.Append(paths[i].Trim(' ', '/'));
            }

            return stringBuilder.ToString();
        }
    }
}
