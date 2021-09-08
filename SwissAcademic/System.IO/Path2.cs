using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using SwissAcademic;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Resources;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CharUnicodeInfo = SwissAcademic.Globalization.CharUnicodeInfo;

namespace System.IO
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Provides utility methods for paths.
    /// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public static class Path2
    {
        #region Konstanten

        public const int MAX_PATH = 256;

        #endregion

        #region Felder

        public static readonly Regex DigitInParenthesesRegex = new Regex(@"\(\d*\)");

        #endregion

        #region AddExtension

        public static string AddExtension(string path, string extension)
        {
            //as opposed to ChangeExtension this method assumes, that a filename is extension-less, see e.g.:
            //Path. ChangeExtension("Aufstand des 17.06.1953", ".pdf") = "Aufstand des 17.06.pdf"      ... the year is thought to be an extension and exchanged for "pdf"
            //Path2.AddExtension   ("Aufstand des 17.06.1953", ".pdf") = "Aufstand des 17.06.1953.pdf" ... this is what is actually expected!

            var pathIsEmpty = string.IsNullOrEmpty(path);
            var extensionIsEmpty = string.IsNullOrEmpty(extension);

            if (pathIsEmpty && extensionIsEmpty) return string.Empty;
            if (pathIsEmpty) return extension;
            if (extensionIsEmpty) return path;


            if (extension.FirstChar() == CharUnicodeInfo.Dot)
            {
                return "{0}{1}".FormatString(path, extension);
            }
            else
            {
                return "{0}.{1}".FormatString(path, extension);
            }
        }

        #endregion

        #region CheckAccess

        /* From http://www.codeproject.com/KB/files/UserFileAccessRights.aspx
		 * This only checks the Windows file access rights, not the network share access rights!
		 * And I found some quite exotic situations where these methods returned wrong results:
		 * - Set a file on sas-hyperv-host to SAS-Administrators-Deny-Write
		 * - CheckAccess returns true for my account, however a StreamWriter can't write in the file
		 * (which I don't understand anyway...)
		 * Therefore, use with caution!
		 * */

        public static bool CheckAccess(this FileInfo fileInfo, Security.Principal.WindowsIdentity user, Security.AccessControl.FileSystemRights expectedRights)
        {
            Security.AccessControl.FileSecurity fs;
            try
            {
                fs = fileInfo.GetAccessControl();
            }
            catch (InvalidOperationException x) when (x.GetBaseException()?.HResult == -2146233079)
            {
                // SQL Server FileTable virtual directories throw this exception when queried for GetAccessControl() with read-only permissions. 
                // We assume that the permissions are read-only if the directory exists.

                return expectedRights == Security.AccessControl.FileSystemRights.Read;
            }

            var acl = fs.GetAccessRules(true, true, typeof(Security.Principal.SecurityIdentifier));

            var rules = new List<System.Security.AccessControl.AuthorizationRule>(acl.Count);
            foreach (Security.AccessControl.AuthorizationRule rule in acl)
            {
                if (user.User.Equals(rule.IdentityReference) || user.Groups.Contains(rule.IdentityReference)) rules.Add(rule);
            }

            Security.AccessControl.FileSystemRights denyRights = 0;
            Security.AccessControl.FileSystemRights allowRights = 0;

            // iterates on rules to compute denyRights and allowRights  
            foreach (Security.AccessControl.FileSystemAccessRule rule in rules)
            {
                if (rule.AccessControlType.Equals(Security.AccessControl.AccessControlType.Deny)) denyRights |= rule.FileSystemRights;
                else if (rule.AccessControlType.Equals(Security.AccessControl.AccessControlType.Allow)) allowRights |= rule.FileSystemRights;
            }

            allowRights = allowRights & ~denyRights;

            // are rights sufficient?  
            return (allowRights & expectedRights) == expectedRights;
        }

        public static bool CheckAccess(this DirectoryInfo directoryInfo, Security.Principal.WindowsIdentity user, Security.AccessControl.FileSystemRights expectedRights)
        {
            Security.AccessControl.DirectorySecurity ds;
            try
            {
                ds = directoryInfo.GetAccessControl();
            }
            catch (InvalidOperationException x) when (x.GetBaseException()?.HResult == -2146233079)
            {
                // SQL Server FileTable virtual directories throw this exception when queried for GetAccessControl() with read-only permissions. 
                // We assume that the permissions are read-only if the directory exists.

                return expectedRights == Security.AccessControl.FileSystemRights.Read;
            }

            var acl = ds.GetAccessRules(true, true, typeof(Security.Principal.SecurityIdentifier));
            var rules = new List<Security.AccessControl.AuthorizationRule>(acl.Count);
            foreach (Security.AccessControl.AuthorizationRule rule in acl)
            {
                if (user.User.Equals(rule.IdentityReference) || user.Groups.Contains(rule.IdentityReference)) rules.Add(rule);
            }

            Security.AccessControl.FileSystemRights denyRights = 0;
            Security.AccessControl.FileSystemRights allowRights = 0;

            // iterates on rules to compute denyRights and allowRights  
            foreach (Security.AccessControl.FileSystemAccessRule rule in rules)
            {
                if (rule.AccessControlType.Equals(Security.AccessControl.AccessControlType.Deny)) denyRights |= rule.FileSystemRights;
                else if (rule.AccessControlType.Equals(Security.AccessControl.AccessControlType.Allow)) allowRights |= rule.FileSystemRights;
            }

            allowRights = allowRights & ~denyRights;

            // are rights sufficient?  
            return (allowRights & expectedRights) == expectedRights;
        }

        #endregion

        #region CopyDirectory

        public static void CopyTo(this DirectoryInfo source, DirectoryInfo target, BackgroundWorker backgroundWorker = null)
        {
            if (!Directory.Exists(target.FullName)) Directory.CreateDirectory(target.FullName);

            var totalNumberOfFiles = backgroundWorker == null || !backgroundWorker.WorkerReportsProgress ? 0 : source.GetFilesSafe(string.Empty, SearchOption.AllDirectories).Length;
            int counter = 0;
            CopyToPrivate(source, target, totalNumberOfFiles, ref counter, backgroundWorker, CancellationToken.None, null);
        }

        public static void CopyTo(this DirectoryInfo source, DirectoryInfo target, IProgress<PercentageAndTextProgressInfo> progress, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(target.FullName)) Directory.CreateDirectory(target.FullName);

            var totalNumberOfFiles = progress == null ? 0 : source.GetFilesSafe(string.Empty, SearchOption.AllDirectories).Length;
            int counter = 0;

            CopyToPrivate(source, target, totalNumberOfFiles, ref counter, null, cancellationToken, progress);
        }

        static void CopyToPrivate(DirectoryInfo source, DirectoryInfo destination, int totalNumberOfFiles, ref int counter, BackgroundWorker backgroundWorker, CancellationToken cancellationToken, IProgress<PercentageAndTextProgressInfo> progress)
        {
            //JHP Hier muss Logging stattfinden! 
            //Es gibt immer wieder mal Anwender, die sich beklagen, dass Dateien nicht kopiert werden ("alle bis auf 5")
            //s. z.B. ST 104-17520134-0212
            //JHP Logging ergänzt wg. ST 144-17D38803-0232 (2012-09-04)

            var tooLongFilePaths = new List<string>();
            var tooLongFolderPaths = new List<string>();

            #region Iterate Files

            foreach (var fileInfo in source.GetFilesSafe())
            {
                var destinationPath = Path.Combine(destination.FullName, fileInfo.Name);

                try
                {
                    fileInfo.CopyTo(destinationPath, true);
                }


                catch (PathTooLongException exception)
                {
                    Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat, (nameof(TelemetryProperty.Description), "Destination path/file name too long"), (nameof(destinationPath), destinationPath));
                    tooLongFilePaths.Add(destinationPath);
                }

                catch (IOException exception)
                {
                    Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat, (nameof(TelemetryProperty.Description), "IOException when copying"), ("Source", fileInfo.FullName), ("Target", destinationPath));

                    exception.Data.Add("SourceFile", fileInfo.FullName);
                    exception.Data.Add("TargetFile", destinationPath);

                    var pathRoot = Path.GetPathRoot(destinationPath);
                    var driveInfo = new DriveInfo(pathRoot);
                    exception.Data.Add("DriveInfo", driveInfo);

                    throw;
                }

                if (totalNumberOfFiles != 0)
                {
                    if (backgroundWorker != null)
                    {
                        if (backgroundWorker.CancellationPending) return;
                    }
                    else if (cancellationToken != CancellationToken.None)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    counter++;

                    var progressPercentage = Convert.ToInt32(Convert.ToSingle(counter) / Convert.ToSingle(totalNumberOfFiles) * 100);

                    if (backgroundWorker != null)
                    {
                        backgroundWorker.ReportProgressSafe(progressPercentage, fileInfo.Name);
                    }
                    else if (progress != null)
                    {
                        progress.Report(new PercentageAndTextProgressInfo { Text = fileInfo.Name, Percentage = progressPercentage });
                    }
                }
            }

            #endregion Iterate Files

            #region Iterate Folders

            foreach (var sourceSubdirectory in source.GetDirectories())
            {
                var createDirectory = false;
                var destinationSubdirectory = new DirectoryInfo(Path.Combine(destination.FullName, sourceSubdirectory.Name));

                var tt = new SasTraceTelemetry
                {
                    Message = "Create Subdirectory (Path2 > CopyToPrivate)",
                    SeverityLevel = SeverityLevel.Verbose
                };
                tt.Properties.Add("Source directory length", sourceSubdirectory.FullName.Length.ToString());
                tt.Properties.Add("Source directory characters", sourceSubdirectory.FullName);
                tt.Properties.Add("Destination directory length", destinationSubdirectory.FullName.Length.ToString());
                tt.Properties.Add("Destination directory characters", destinationSubdirectory.FullName);

                Telemetry.TrackTrace(tt);

                var existingFileSystemItem = destination.EnumerateFileSystemInfos(sourceSubdirectory.Name).FirstOrDefault();

                if (existingFileSystemItem == null)
                {
                    createDirectory = true;
                }
                else
                {
                    if (existingFileSystemItem is FileInfo)
                    {
                        destinationSubdirectory = new DirectoryInfo(destination.GetUniqueFilePath(sourceSubdirectory.Name));
                        createDirectory = true;
                    }
                }

                if (createDirectory)
                {
                    try
                    {
                        destinationSubdirectory.Create();
                    }
                    catch (PathTooLongException exception)
                    {
                        Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat, (nameof(TelemetryProperty.Description), "Destination path/folder name too long"), ("DestinationPath", destinationSubdirectory.FullName));
                        tooLongFolderPaths.Add(destinationSubdirectory.FullName);
                    }
                }

                if (destinationSubdirectory.Exists)
                {
                    try
                    {
                        CopyToPrivate(sourceSubdirectory, destinationSubdirectory, totalNumberOfFiles, ref counter, backgroundWorker, cancellationToken, progress);
                    }

                    catch (PathTooLongException exception)
                    {
                        //JHP read out all critical folder and file paths from the error objects data dictionary and add them to the local lists
                        #region Add error information to e.Data

                        if (exception.Data != null)
                        {
                            foreach (DictionaryEntry entry in exception.Data)
                            {
                                if (entry.Key == null) continue;
                                var key = entry.Key.ToString();

                                if (key.Equals("TooLongFolderPaths", StringComparison.OrdinalIgnoreCase))
                                {
                                    var list = entry.Value as List<string>;
                                    if (list != null) tooLongFolderPaths.AddRange(list);
                                }
                                else if (key.Equals("TooLongFilePaths", StringComparison.OrdinalIgnoreCase))
                                {
                                    var list = entry.Value as List<string>;
                                    if (list != null) tooLongFilePaths.AddRange(list);
                                }
                            }
                        }

                        #endregion Add error information to e.Data
                    }
                }


                if (backgroundWorker != null)
                {
                    if (backgroundWorker.CancellationPending) return;
                }
                else if (cancellationToken != CancellationToken.None)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            #endregion Iterate Folders

            #region Throw PathTooLongException if necessary

            PathTooLongException pathTooLongException = null;

            if (tooLongFolderPaths != null && tooLongFolderPaths.Count > 0)
            {
                if (pathTooLongException == null) pathTooLongException = new PathTooLongException();
                pathTooLongException.Data.Add("TooLongFolderPaths", tooLongFolderPaths);
            }
            if (tooLongFilePaths != null && tooLongFilePaths.Count > 0)
            {
                if (pathTooLongException == null) pathTooLongException = new PathTooLongException();
                pathTooLongException.Data.Add("TooLongFilePaths", tooLongFilePaths);
            }

            if (pathTooLongException != null) throw pathTooLongException;

            #endregion Throw PathTooLongException if necessary
        }

        #endregion CopyDirectory

        #region CreateUniqueFile
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Creates a zero-byte temporary file on disk and returns the full path of that file. Ensures that 
        /// the proposed file name inside the specified directory is unique by adding " (2)", " (3)", and so forth.
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string CreateUniqueFile(this DirectoryInfo directory, string proposedFileName)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (string.IsNullOrEmpty(proposedFileName)) throw new ArgumentNullException(nameof(proposedFileName));

            var filePath = Path.Combine(directory.FullName, Path.GetFileName(proposedFileName));

            var nameWithoutExtension = Path.GetFileNameWithoutExtension(proposedFileName);
            var extension = Path.GetExtension(proposedFileName);

            while (filePath.Length > MAX_PATH - 10)       // -10 to account for parentheses which may be added in the next step
            {
                if (nameWithoutExtension.Length > 20) nameWithoutExtension = SwissAcademic.Resources.ResourceHelper.Shorten(nameWithoutExtension, 20).TrimEnd('.');
                else nameWithoutExtension = nameWithoutExtension.Substring(0, nameWithoutExtension.Length - 1);

                if (nameWithoutExtension.Length == 0) throw new PathTooLongException(proposedFileName);
                filePath = Path.Combine(directory.FullName, Path.ChangeExtension(nameWithoutExtension, extension));
            }

            try
            {
                new FileStream(filePath, FileMode.CreateNew).Dispose();
                return filePath;
            }
            catch (IOException)
            {
            }

            if (nameWithoutExtension.EndsWith(")"))
            {
                var matches = DigitInParenthesesRegex.Matches(nameWithoutExtension).Cast<Match>();
                if (matches.Any())
                {
                    var match = matches.Last();
                    if (match.Index + match.Length == nameWithoutExtension.Length) nameWithoutExtension = nameWithoutExtension.Substring(0, match.Index).Trim();
                    if (string.IsNullOrEmpty(nameWithoutExtension)) nameWithoutExtension = "File";
                }
            }

            var counter = 2;
            filePath = Path.Combine(directory.FullName, Path.ChangeExtension("{0} ({1})".FormatString(nameWithoutExtension, counter), extension));

            // Path.ChangeExtension changes "Asymmetric Rosenberg.Coleman Effect.pdf" to "Asymmetric Rosenberg". The counter is cut off, resulting
            // in an endless loop.
            nameWithoutExtension = nameWithoutExtension.Replace('.', ' ');

            var errorCounter = 0;
            while (true)
            {
                filePath = Path.Combine(directory.FullName, Path.ChangeExtension("{0} ({1})".FormatString(nameWithoutExtension, counter), extension));
                counter++;

                try
                {
                    new FileStream(filePath, FileMode.CreateNew).Dispose();
                    return filePath;
                }
                catch (IOException)
                {
                    errorCounter++;
                    if (errorCounter > 1000) throw;
                }
            }
        }

        #endregion


        #region GetDriveType

        public static DriveType GetDriveType(string path)
        {
            try
            {
                if (IsUncPath(path)) return System.IO.DriveType.Network;

                var driveInfo = new DriveInfo(Path.GetPathRoot(path));
                if (!driveInfo.IsReady) return DriveType.Network;
                return driveInfo.DriveType;
            }

            catch (Exception exception)
            {
                Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                return DriveType.Unknown;
            }
        }

        #endregion

        #region GetTempFileName

        public static string GetTempFileName(string extension = null, bool createZeroBytePlaceholder = true)
        {
            var fileName = extension == null ?
                Path.GetRandomFileName() :
                Path.ChangeExtension(Path.GetRandomFileName(), extension);

            var tempDirectory = new DirectoryInfo(Path.GetTempPath());

            return createZeroBytePlaceholder ?
                tempDirectory.CreateUniqueFile(fileName) :
                tempDirectory.GetUniqueFilePath(fileName);
        }

        #endregion

        #region EmptyDirectory

        public static void EmptyDirectory(this DirectoryInfo directory)
        {
            if (directory == null) return;
            if (!directory.Exists) return;

            foreach (var file in directory.GetFilesSafe()) file.Delete();
            foreach (var subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        #endregion EmptyDirectory

        #region GetDropBoxPath

        static string _dropBoxPath;

        public static string GetDropBoxPath()
        {
            return LazyInitializer.EnsureInitialized(ref _dropBoxPath, () =>
            {
                try
                {
                    var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                    var infoJsonPath = Path.Combine(appDataPath, @"Dropbox\info.json");
                    if (!File.Exists(infoJsonPath))
                    {
                        var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        infoJsonPath = Path.Combine(localAppDataPath, @"Dropbox\info.Json");
                    }

                    if (File.Exists(infoJsonPath))
                    {
                        var json = File.ReadAllText(infoJsonPath);
                        var jobject = JObject.Parse(json);

                        var personal = jobject.SelectToken("personal");
                        if (personal != null)
                        {
                            var path = personal.SelectToken("path");
                            if (path != null) return path.Value<string>();
                        }

                        return string.Empty;
                    }
                    else
                    {
                        var dbPath = Path.Combine(appDataPath, "Dropbox\\host.db");
                        if (!File.Exists(dbPath)) return string.Empty;

                        var lines = File.ReadAllLines(dbPath);
                        var dbBase64Text = Convert.FromBase64String(lines[1]);
                        var folderPath = Encoding.UTF8.GetString(dbBase64Text);

                        return folderPath ?? string.Empty;
                    }
                }
                catch (Exception exception)
                {
                    Telemetry.TrackException(exception, SeverityLevel.Warning, ExceptionFlow.Eat);
                    return string.Empty;
                }
            });
        }

        #endregion

        #region GetDropBoxBusinessPath

        static string _dropBoxBusinessPath;

        public static string GetDropBoxBusinessPath()
        {
            return LazyInitializer.EnsureInitialized(ref _dropBoxBusinessPath, () =>
            {
                try
                {
                    var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                    var infoJsonPath = Path.Combine(appDataPath, @"Dropbox\info.json");
                    if (!File.Exists(infoJsonPath))
                    {
                        var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        infoJsonPath = Path.Combine(localAppDataPath, @"Dropbox\info.Json");
                    }
                    if (File.Exists(infoJsonPath))
                    {
                        var json = File.ReadAllText(infoJsonPath);
                        var jobject = JObject.Parse(json);

                        var business = jobject.SelectToken("business");
                        if (business != null)
                        {
                            var path = business.SelectToken("path");
                            if (path != null) return path.Value<string>();
                        }
                    }
                }
                catch (Exception exception)
                {
                    Telemetry.TrackException(exception, SeverityLevel.Warning, ExceptionFlow.Eat);
                }

                return string.Empty;
            });
        }

        #endregion

        #region GetGoogleDrivePath

        static string _googleDrivePath;

        public static string GetGoogleDrivePath()
        {
            return LazyInitializer.EnsureInitialized(ref _googleDrivePath, () =>
            {
                //https://stackoverflow.com/questions/21383749/how-do-i-get-the-google-drive-location?rq=1#
                try
                {
                    var dbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Drive", "sync_config.db");

                    if (!File.Exists(dbFilePath))
                    {
                        dbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Drive\\user_default\\sync_config.db");
                        if (!File.Exists(dbFilePath))
                        {
                            return string.Empty;
                        }
                    }


                    string csGDrive = @"Data Source=" + dbFilePath + ";Version=3;New=False;Compress=True;";
                    using (var cnn = new SQLiteConnection(csGDrive))
                    {
                        cnn.Open();
                        using (var sqliteCmd = new SQLiteCommand(cnn))
                        {
                            sqliteCmd.CommandText = "select * from data where entry_key='local_sync_root_path'";
                            using (var reader = sqliteCmd.ExecuteReader())
                            {
                                reader.Read();

                                if (!reader.HasRows)
                                {
                                    var pers = Path.Combine(
                                       Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                       @"Google Drive");

                                    return Directory.Exists(pers) ? pers : string.Empty;
                                }
                                //String retrieved is in the format "\\?\<path>" that's why I have used Substring function to extract the path alone.
                                var drivePath = reader["data_value"].ToString().Substring(4);

                                return drivePath ?? string.Empty;
                            }
                        }
                    }

                }
                catch (Exception ignored)
                {
                    Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                    return string.Empty;
                }
            });
        }

        #endregion

        #region GetOneDrivePath

        static string _oneDrivePath;

        public static string GetOneDrivePath()
        {
            return LazyInitializer.EnsureInitialized(ref _oneDrivePath, () =>
            {
                try
                {
                    if (Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\SkyDrive") != null)
                    {
                        var path = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\SkyDrive", "UserFolder", string.Empty).ToString();
                        if (string.IsNullOrEmpty(path))
                        {
                            var skydriveDefaultFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "SkyDrive");
                            if (Directory.Exists(skydriveDefaultFolder)) return skydriveDefaultFolder;
                        }
                        if (!string.IsNullOrEmpty(path)) return path;
                    }

                    if (Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\OneDrive") != null) //Neuer Ordner in Win 10
                    {
                        var path = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\OneDrive", "UserFolder", string.Empty).ToString();
                        if (string.IsNullOrEmpty(path))
                        {
                            var skydriveDefaultFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OneDrive");
                            if (Directory.Exists(skydriveDefaultFolder)) return skydriveDefaultFolder;
                        }
                        return path ?? string.Empty;
                    }

                    if (Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\OneDrive\\Accounts\\Personal") != null) //Neuer Ordner in Win 10
                    {
                        var path = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\OneDrive\\Accounts\\Personal", "UserFolder", string.Empty).ToString();
                        if (string.IsNullOrEmpty(path))
                        {
                            var skydriveDefaultFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OneDrive");
                            if (Directory.Exists(skydriveDefaultFolder)) return skydriveDefaultFolder;
                        }
                        return path ?? string.Empty;
                    }
                }
                catch (Exception ignored)
                {
                    Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
                }

                return string.Empty;
            });
        }

        #endregion

        #region GetOneDriveBusinessPath

        static IEnumerable<string> _oneDriveBusinessPaths;
        public static IEnumerable<string> GetOneDriveBusinessPath()
        {
            return LazyInitializer.EnsureInitialized(ref _oneDriveBusinessPaths, () =>
            {
                var businessFolders = new List<string>();
                try
                {
                    var accountsKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\OneDrive\\Accounts");
                    if (accountsKey != null) //Neuer Ordner in Win 10
                    {
                        foreach (var account in accountsKey.GetSubKeyNames())
                        {
                            if (account.StartsWith("Business"))
                            {
                                var path = accountsKey.OpenSubKey(account).GetValue("UserFolder")?.ToString();
                                if (path != null) businessFolders.Add(path);
                            }
                        }
                    }
                }
                catch (Exception ignored)
                {
                    Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
                }
                return businessFolders;
            });
        }

        #endregion

        #region GetSyncFolderPath

        public static string GetSyncFolderPath(SyncFolderType syncFolderType)
        {
            switch (syncFolderType)
            {
                case SyncFolderType.DropBox:
                    return GetDropBoxPath();

                case SyncFolderType.DropBoxBusiness:
                    return GetDropBoxBusinessPath();

                case SyncFolderType.GoogleDrive:
                    return GetGoogleDrivePath();

                case SyncFolderType.OneDrive:
                    return GetOneDrivePath();

                case SyncFolderType.OneDriveBusiness:
                    return GetOneDriveBusinessPath()?.FirstOrDefault();

                default:
                    return string.Empty;
            }
        }

        #endregion

        #region GetTemporaryDirectory
        public static string GetTemporaryDirectory()
        {
            string tempDirectory = null;
            do
            {
                tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }
            while (Directory.Exists(tempDirectory));

            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
        #endregion

        #region GetFileNamesSafe
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Calls DirectoryInfo.GetFiles(), but ignores an exception thrown on Ubuntu when no files are
        /// found. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 22.10.2009. </remarks>
        ///
        /// <exception cref="ArgumentNullException">	
        /// Thrown when <paramref name="directoryInfo"/> is null. 
        /// </exception>
        ///
        /// <param name="directoryInfo">	The directory. </param>
        ///
        /// <returns>	A file list from the current directory. </returns>
        ///
        /// ### <param name="searchPattern">	The search pattern. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static FileInfo[] GetFilesSafe(this DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null) throw new ArgumentNullException("directoryInfo");
            return GetFilesSafe(directoryInfo, string.Empty);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Calls DirectoryInfo.GetFiles(searchPattern), but ignores an exception thrown on Ubuntu 
        /// when no files are found. </summary>
        ///
        /// <remarks>	Thomas.Schempp, 22.10.2009. </remarks>
        ///
        /// <param name="directoryInfo">	The directory. </param>
        /// <param name="searchPattern">	The search pattern. </param>
        ///
        /// <returns>	A file list from the current directory. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static FileInfo[] GetFilesSafe(this DirectoryInfo directoryInfo, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            try
            {
                if (string.IsNullOrEmpty(searchPattern)) searchPattern = "*.*";
                return directoryInfo.GetFiles(searchPattern, searchOption);
            }

            catch (Exception ignored)
            {
                //Ubuntu-Fehler wenn keine Dateien gefunden werden:
                //System.IO.IOException: Es sind keine weiteren Dateien vorhanden.

                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                return new FileInfo[0];
            }
        }

        #endregion

        #region GetFriendlyTypeNameFromExtension
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Gets the friendly file type from the Windows registry. </summary>
        ///
        /// <remarks>	Thomas Schempp, 25.01.2010. </remarks>
        ///
        /// <exception cref="ArgumentNullException">	
        /// Thrown when extension is null or empty. 
        /// </exception>
        ///
        /// <param name="extension">	The extension. </param>
        ///
        /// <returns>	The friendly file type name. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string GetFriendlyTypeNameFromExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException();

            var registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extension);
            if (registryKey == null) goto ReturnDefault;
            var value = registryKey.GetValue(null);
            if (value == null) goto ReturnDefault;

            registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(value.ToString());
            if (registryKey == null) goto ReturnDefault;
            value = registryKey.GetValue(null);
            if (value == null) goto ReturnDefault;
            return value.ToString();

        ReturnDefault:
            if (extension.StartsWith(".")) extension = extension.Substring(1);
            return string.Concat(extension.ToUpper(), " ", Strings.File);
        }

        #endregion

        #region GetExecutablesFriendlyName

#if !Web
        public static string GetExecutablesFriendlyName(string fileName)
        {
            uint pcchOut = 0;

            //sh: changed NativeMethods.AssocStr.Executable to NativeMethods.AssocStr.FriendlyAppName because it gave some weird signs
            NativeMethods.AssocQueryString(NativeMethods.AssocF.Verify, NativeMethods.AssocStr.FriendlyAppName, fileName, null, null, ref pcchOut);
            if (pcchOut == 0) return "Windows";

            var pszOut = new StringBuilder((int)pcchOut);
            NativeMethods.AssocQueryString(NativeMethods.AssocF.Verify, NativeMethods.AssocStr.FriendlyAppName, fileName, null, pszOut, ref pcchOut);
            if (pszOut.Length == 0 /*|| Path.GetExtension(pszOut.ToString()) != ".exe" */) return "Windows";

            NativeMethods.AssocQueryString(NativeMethods.AssocF.Verify, NativeMethods.AssocStr.FriendlyAppName, fileName, null, null, ref pcchOut);
            pszOut = new StringBuilder((int)pcchOut);
            NativeMethods.AssocQueryString(NativeMethods.AssocF.Verify, NativeMethods.AssocStr.FriendlyAppName, fileName, null, pszOut, ref pcchOut);

            if (pszOut.Length == 0) return "Windows";
            return pszOut.ToString();
        }

#endif

        #endregion

        #region GetFullPathFromPathWithVariables

#if !Web
        public static string GetFullPathFromPathWithVariables(string path)
        {
            // The logic has been transferred to Environment.ExpandEnvironmentVariables,
            // but we leave the method here for Api compatibility (AddOns etc.)
            return SwissAcademic.Environment.ExpandEnvironmentVariables(path);
        }
#endif

        #endregion

        #region GetUniqueFilePath
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Ensures that the proposed file name inside the specified directory is unique by adding " (2)",
        /// " (3)", and so forth. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
        ///
        /// <exception cref="ArgumentNullException">	
        /// Thrown when <paramref name="directory"/> is null or <paramref name="proposedFileName"/> is
        /// null or empty. 
        /// </exception>
        ///
        /// <param name="directory">		Pathname of the directory. </param>
        /// <param name="proposedFileName">	Name of the proposed file. May be a complete file path from
        /// 								which the file name is extracted. </param>
        ///
        /// <returns>	The complete file path with a file name that is unique in its directory. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string GetUniqueFilePath(this DirectoryInfo directory, string proposedFileName, FileInfo existingFileInfo = null)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (string.IsNullOrEmpty(proposedFileName)) throw new ArgumentNullException(nameof(proposedFileName));

            var proposedFullPath = Path.Combine(directory.FullName, Path.GetFileName(proposedFileName));
            var filePath = proposedFullPath;

            if (existingFileInfo != null && proposedFullPath.Equals(existingFileInfo.FullName, StringComparison.Ordinal))
            {
                return proposedFullPath;
            }

            var nameWithoutExtension = Path.GetFileNameWithoutExtension(proposedFileName);
            var extension = Path.GetExtension(proposedFileName);

            while (filePath.Length > MAX_PATH - 10)       // -10 to account for parentheses which may be added in the next step
            {
                if (nameWithoutExtension.Length > 20) nameWithoutExtension = ResourceHelper.Shorten(nameWithoutExtension, 20).TrimEnd('.');
                else nameWithoutExtension = nameWithoutExtension.Substring(0, nameWithoutExtension.Length - 1);

                if (nameWithoutExtension.Length == 0)
                {
                    var pathTooLongException = new PathTooLongException();
                    pathTooLongException.Data.Add("Proposed file name", proposedFileName);
                    pathTooLongException.Data.Add("Proposed full path", proposedFullPath);
                    pathTooLongException.Data.Add("Length", proposedFullPath.Length.ToString());
                    pathTooLongException.Data.Add("Message", "Proposed file name would have to be shortened to a zero length");

                    throw pathTooLongException;
                }

                filePath = Path.Combine(directory.FullName, Path2.AddExtension(nameWithoutExtension, extension));
            }

            if (!File.Exists(filePath) && !Directory.Exists(filePath))
            {
                return filePath;
            }

            var match = StringComparerEx.TrailingDigitInParenthesesRegex.Match(nameWithoutExtension);
            if (match.Success)
            {
                nameWithoutExtension = match.Groups["Name"].Value;
                if (string.IsNullOrWhiteSpace(nameWithoutExtension)) nameWithoutExtension = "File";
            }

            var counter = 2;
            filePath = Path.Combine(directory.FullName, AddExtension("{0} ({1})".FormatString(nameWithoutExtension, counter), extension));

            while ((File.Exists(filePath) || Directory.Exists(filePath)) &&
                (existingFileInfo == null || !filePath.Equals(existingFileInfo.FullName, StringComparison.Ordinal)))
            {
                counter++;
                filePath = Path.Combine(directory.FullName, AddExtension("{0} ({1})".FormatString(nameWithoutExtension, counter), extension));
            }

            return filePath;
        }

        #endregion

        #region GetSyncFolderType

        public static SyncFolderType GetSyncFolderType(this DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null) return SyncFolderType.None;

            try
            {
                var syncFolder = GetDropBoxPath();
                Uri uri;
                if (Uri.TryCreate(syncFolder, UriKind.RelativeOrAbsolute, out uri))
                {
                    if (!string.IsNullOrEmpty(syncFolder) &&
                        (string.Compare(directoryInfo.FullName, syncFolder, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                        IsBelowFolder(directoryInfo, syncFolder))) return SyncFolderType.DropBox;
                }

                syncFolder = GetDropBoxBusinessPath();
                if (Uri.TryCreate(syncFolder, UriKind.RelativeOrAbsolute, out uri))
                {
                    if (!string.IsNullOrEmpty(syncFolder) &&
                        (string.Compare(directoryInfo.FullName, syncFolder, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                        IsBelowFolder(directoryInfo, syncFolder))) return SyncFolderType.DropBoxBusiness;
                }

                syncFolder = GetOneDrivePath();
                if (Uri.TryCreate(syncFolder, UriKind.RelativeOrAbsolute, out uri))
                {
                    if (!string.IsNullOrEmpty(syncFolder) &&
                        (string.Compare(directoryInfo.FullName, syncFolder, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                        IsBelowFolder(directoryInfo, syncFolder))) return SyncFolderType.OneDrive;
                }

                syncFolder = GetGoogleDrivePath();
                if (Uri.TryCreate(syncFolder, UriKind.RelativeOrAbsolute, out uri))
                {
                    if (!string.IsNullOrEmpty(syncFolder) &&
                        (string.Compare(directoryInfo.FullName, syncFolder, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                        IsBelowFolder(directoryInfo, syncFolder))) return SyncFolderType.GoogleDrive;
                }
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat, (nameof(TelemetryProperty.Description), "DirectoryInfo.FullName to check for SyncFolderType"), (nameof(directoryInfo), directoryInfo.FullName));
            }
            return SyncFolderType.None;
        }

        #endregion

        #region IsBelowFolder

        public static bool IsBelowFolder(this FileInfo fileInfo, string folderPath)
        {
            if (fileInfo == null || string.IsNullOrEmpty(folderPath)) return false;
            if (!folderPath.EndsWith("\\")) folderPath += "\\";

            var folderUri = new Uri(folderPath);
            var fileInfoUri = new Uri(fileInfo.FullName);
            return folderUri.IsBaseOf(fileInfoUri);
        }

        #endregion

        #region IsBelowFolder

        public static bool IsBelowFolder(this DirectoryInfo directoryInfo, string folderPath)
        {
            if (directoryInfo == null || string.IsNullOrEmpty(folderPath)) return false;
            return IsBelowFolder(directoryInfo, new DirectoryInfo(folderPath));
        }

        public static bool IsBelowFolder(this DirectoryInfo directoryInfo, DirectoryInfo other)
        {
            if (directoryInfo == null || other == null) return false;

            var directoryInfoUri = new Uri(Path.Combine(directoryInfo.FullName, "dummy.tmp"));
            var otherUri = new Uri(Path.Combine(other.FullName, "dummy.tmp"));

            return otherUri != directoryInfoUri && otherUri.IsBaseOf(directoryInfoUri);
        }

        public static bool IsBelowOrInSameFolder(this DirectoryInfo directoryInfo, string folderPath)
        {
            if (directoryInfo == null || string.IsNullOrEmpty(folderPath)) return false;
            return IsBelowOrInSameFolder(directoryInfo, new DirectoryInfo(folderPath));
        }

        public static bool IsBelowOrInSameFolder(this DirectoryInfo directoryInfo, DirectoryInfo other)
        {
            if (directoryInfo == null || other == null) return false;

            var directoryInfoUri = new Uri(Path.Combine(directoryInfo.FullName, "dummy.tmp"));
            var otherUri = new Uri(Path.Combine(other.FullName, "dummy.tmp"));

            return otherUri.IsBaseOf(directoryInfoUri);
        }

        #endregion

        #region IsUncPath

        public static bool IsUncPath(string path)
        {
#if Web
            return false;
#else
            return NativeMethods.PathIsUNC(path);
#endif
        }

        #endregion

        #region ResolveUrlShortcut
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Resolves an url shortcut (*.url) in the Windows file system to its URL. </summary>
        ///
        /// <remarks>	Thomas Schempp, 26.01.2010. </remarks>
        ///
        /// <param name="filePath">	
        /// Full path of the file. If <paramref name="filePath"/> is null or empty, or if the file does
        /// not exist, the function returns null. 
        /// </param>
        ///
        /// <returns>	The URL. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string ResolveUrlShortcut(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            if (!File.Exists(filePath)) return null;

            if (!Path.GetExtension(filePath).Equals(".url", StringComparison.OrdinalIgnoreCase)) return null;

            using (var reader = new StreamReader(filePath))
            {
                string result = reader.ReadLine();

                while (!string.IsNullOrEmpty(result) &&
                    string.CompareOrdinal(result, "[InternetShortcut]") != 0)
                {
                    result = reader.ReadLine();
                };

                if (!string.IsNullOrEmpty(result)) result = reader.ReadLine();

                if (!string.IsNullOrEmpty(result))
                {
                    return result.Replace("URL=", "");
                }
            }

            return null;
        }

        #endregion

        #region TrimPath

        static Regex _driveRegex = new Regex("^[A-Za-z]:$", RegexOptions.CultureInvariant);

        /// <summary>
        /// Removes quotation marks at the beginning and end of the path. Removes quotation marks,
        /// spaces and backslashes at the end of the path. If the path consists of a drive letter only (e.g. C:),
        /// the trailing backslash will remain in order to support Path.Combine correctly.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string TrimPath(string path, bool trimLeadingBackslashes = false)
        {
            if (string.IsNullOrEmpty(path)) return path;

            path = path.TrimStart('"').TrimEnd('"', '\\', CharUnicodeInfo.Space, CharUnicodeInfo.NonBreakingSpace);

            if (trimLeadingBackslashes)
            {
                path = path.TrimStart('\\');
            }

            // Bug 18225: If the path consists of a drive letter (D:) only, Path.Combine does not append a backslash
            // Path.Combine("A:", "test.txt") > A:test.txt. This leads to an UriFormatException.
            // If the Path is not a drive letter, a backslash is added automatically
            // Path.Combine("A:\Folder", "test.txt") > A:\Folder\test.txt
            if (_driveRegex.IsMatch(path)) path = path + "\\";

            return path;
        }

        #endregion

        #region TryCreateFile in directory
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Checks permissions in a directory by temporary creating a random file and deleting it
        /// afterwords. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
        ///
        /// <exception cref="ArgumentNullException">	
        /// Thrown when <paramref name="directoryInfo"/> is null. 
        /// </exception>
        ///
        /// <param name="directoryInfo">		The directory where the temporary file is created. </param>
        /// <param name="throwExceptionOnFail">	true to throw the appropriate exception if the write
        /// 									operation failes, false to suppress the exception. </param>
        ///
        /// <returns>	true if the file can be written and deleted, otherwise false. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool TryCreateFile(this DirectoryInfo directoryInfo, bool throwExceptionOnFail)
        {
            if (directoryInfo == null) throw new ArgumentNullException("directoryInfo");
            Exception exception;
            return TryCreateFile(directoryInfo, out exception, throwExceptionOnFail);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Checks permissions in a directory by temporary creating a random file and deleting it
        /// afterwords. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
        ///
        /// <exception cref="ArgumentNullException">	
        /// Thrown when <paramref name="directoryInfo"/> is null. 
        /// </exception>
        ///
        /// <param name="directoryInfo">	The directory where the temporary file is created. </param>
        /// <param name="exception">		[out] The exception thrown by the write/delete operation or
        /// 								null, if the file can be created and deleted. </param>
        ///
        /// <returns>	true if the file can be written and deleted, otherwise false. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool TryCreateFile(this DirectoryInfo directoryInfo, out Exception exception)
        {
            if (directoryInfo == null) throw new ArgumentNullException("directoryInfo");
            return TryCreateFile(directoryInfo, out exception, false);
        }

        static bool TryCreateFile(DirectoryInfo directoryInfo, out Exception exception, bool throwExceptionOnFail)
        {
            var randomFileName = GetValidFileName(Guid.NewGuid().ToString());
            try
            {
                var fileName = Path.Combine(directoryInfo.FullName, randomFileName);

                using (File.Create(fileName)) { }
                File.Delete(fileName);

                Telemetry.TrackTrace($"Result of {nameof(TryCreateFile)}", SeverityLevel.Information, (nameof(TryCreateFile), true));

                exception = null;
                return true;
            }

            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                if (!throwExceptionOnFail)
                {
                    Telemetry.TrackException(unauthorizedAccessException, SeverityLevel.Warning, ExceptionFlow.Eat, (nameof(TelemetryProperty.Description), "TryWrite for directory returns false."), (nameof(directoryInfo), directoryInfo));
                }

                var newException = new UnauthorizedAccessException(unauthorizedAccessException.Message.Replace("\\" + randomFileName, string.Empty));
                if (throwExceptionOnFail) throw newException;

                exception = newException;
                return false;
            }

            catch (Exception generalException)
            {
                if (throwExceptionOnFail) throw;

                Telemetry.TrackException(generalException, SeverityLevel.Warning, ExceptionFlow.Eat, (nameof(TelemetryProperty.Description), "TryWrite for directory returns false."), (nameof(directoryInfo), directoryInfo));

                exception = generalException;
                return false;
            }
        }

        #endregion

        #region TryRead file
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Checks read permissions on a file. </summary>
        ///
        /// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
        ///
        /// <exception cref="ArgumentNullException">	
        /// Thrown when <paramref name="fileInfo"/> is null. 
        /// </exception>
        ///
        /// <param name="fileInfo">				The file to be read. </param>
        /// <param name="throwExceptionOnFail">	true to throw the appropriate exception if the read
        /// 									operation failes, false to suppress the exception. </param>
        ///
        /// <returns>	true if the file can be read, otherwise false. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool TryRead(this FileInfo fileInfo, bool throwExceptionOnFail)
        {
            if (fileInfo == null) throw new ArgumentNullException("fileInfo");
            Exception exception;
            return TryRead(fileInfo, out exception, throwExceptionOnFail);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Checks read permissions on a file. </summary>
        ///
        /// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
        ///
        /// <exception cref="ArgumentNullException">	
        /// Thrown when <paramref name="fileInfo"/> is null.
        /// </exception>
        ///
        /// <param name="fileInfo">		The file to be read. </param>
        /// <param name="exception">	[out] The exception thrown by the read operation or null if the
        /// 							file can be read. </param>
        ///
        /// <returns>	true if the file can be read, otherwise false. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool TryRead(this FileInfo fileInfo, out Exception exception)
        {
            if (fileInfo == null) throw new ArgumentNullException("fileInfo");
            return TryRead(fileInfo, out exception, false);
        }

        static bool TryRead(FileInfo fileInfo, out Exception exception, bool throwExceptionOnFail)
        {
            try
            {
                using (var fileStream = fileInfo.OpenRead())
                {
                    exception = null;
                    return true;
                }
            }

            catch (Exception ignored)
            {
                if (throwExceptionOnFail) throw;
                exception = ignored;
                return false;
            }
        }

        #endregion

        #region TryWrite file
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Checks write permissions on a file by creating the file and deleting it afterwords. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
        ///
        /// <exception cref="ArgumentNullException">	
        /// Thrown when <paramref name="fileInfo"/> is null. 
        /// </exception>
        ///
        /// <param name="fileInfo">				The file to be written. </param>
        /// <param name="throwExceptionOnFail">	true to throw the appropriate exception if the write
        /// 									operation failes, false to suppress the exception. </param>
        ///
        /// <returns>	true if the file can be written, otherwise false. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool TryWrite(this FileInfo fileInfo, bool throwExceptionOnFail)
        {
            if (fileInfo == null) throw new ArgumentNullException("fileInfo");

            Exception exception;
            return TryWrite(fileInfo, out exception, throwExceptionOnFail);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Checks write permissions on a file by creating the file and deleting it afterwords. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
        ///
        /// <exception cref="ArgumentNullException">	Thrown when <paramref name="fileInfo"/> is null. </exception>
        ///
        /// <param name="fileInfo">		The file to be written. </param>
        /// <param name="exception">	[out] The exception thrown by the write operation or null if the
        /// 							file can be written. </param>
        ///
        /// <returns>	true if the file can be written, otherwise false. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool TryWrite(this FileInfo fileInfo, out Exception exception)
        {
            if (fileInfo == null) throw new ArgumentNullException();
            return TryWrite(fileInfo, out exception, false);
        }

        static bool TryWrite(FileInfo fileInfo, out Exception exception, bool throwExceptionOnFail)
        {
            try
            {
                //BUG: FileMode.CreateNew , Wenn schon exist --> Fehler Neu:Create

                using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write)) { }
                fileInfo.Delete();

                exception = null;
                return true;
            }

            catch (Exception ignored)
            {
                if (throwExceptionOnFail) throw;
                exception = ignored;
                return false;
            }
        }

        #endregion

        #region GetValidFileName
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Replaces all invalid characters in the specified file name by spaces. </summary>
        ///
        /// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
        ///
        /// <exception cref="ArgumentNullException">	Thrown when <paramref name="fileName"/> is null
        /// 											or empty. </exception>
        /// <exception cref="ArgumentException">		Thrown when <paramref name="fileName"/> is a file
        /// 											path (i.e., contains the operating system's
        /// 											directory separator character. </exception>
        ///
        /// <param name="fileName">	Name of the file. </param>
        ///
        /// <returns>	The valid file name. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string GetValidFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");

            var replacements = new Tuple<Func<char, bool>, char>[]
            {
                // Replace dash by hyphen (problems with Windows function "Send to ZIP")
                new Tuple<Func<char, bool>, char>(c => CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.DashPunctuation, '-')
            };

            fileName = fileName.Clean(IllegalCharacters.NonStandardWhitespace, true, null, Path.GetInvalidFileNameChars().Concat(Path.DirectorySeparatorChar), replacements, null);

            if (string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(fileName).Trim()))
            {
                fileName = string.Concat("File", Path.GetExtension(fileName));
            }

            return fileName;
        }

        #endregion

#if !Web


        #region GetExecutable

        public static string GetExecutable(string fileName)
        {
            uint pcchOut = 0;

            NativeMethods.AssocQueryString(NativeMethods.AssocF.Verify, NativeMethods.AssocStr.Executable, fileName, null, null, ref pcchOut);
            if (pcchOut == 0) return "Windows";

            var pszOut = new StringBuilder((int)pcchOut);
            NativeMethods.AssocQueryString(NativeMethods.AssocF.Verify, NativeMethods.AssocStr.Executable, fileName, null, pszOut, ref pcchOut);
            if (pszOut.Length == 0 /*|| Path.GetExtension(pszOut.ToString()) != ".exe" */) return string.Empty;

            NativeMethods.AssocQueryString(NativeMethods.AssocF.Verify, NativeMethods.AssocStr.Executable, fileName, null, null, ref pcchOut);
            pszOut = new StringBuilder((int)pcchOut);
            NativeMethods.AssocQueryString(NativeMethods.AssocF.Verify, NativeMethods.AssocStr.Executable, fileName, null, pszOut, ref pcchOut);

            if (pszOut.Length == 0) return string.Empty;

            return pszOut.ToString();
        }

        #endregion

        #region GetPathWithVariablesFromFullPath

        public static string GetPathWithVariablesFromFullPath(string path)
        {
            // The logic has been transferred to Environment.CollapseEnvironmentVariables,
            // but we leave the method here for Api compatibility (AddOns etc.)
            return SwissAcademic.Environment.CollapseEnvironmentVariables(path);
        }
        #endregion

        #region GetUncPath

        // http://www.pinvoke.net/default.aspx/advapi32/WNetGetUniversalName.html

        /// <summary>
        /// Returns the UNC path from a local network share path
        /// </summary>
        /// <param name="localNetworkSharePath"></param>
        /// <returns></returns>
        public static string GetUncPath(string localNetworkSharePath)
        {
            if (IsUncPath(localNetworkSharePath)) return localNetworkSharePath;

            var pathRoot = Path.GetPathRoot(localNetworkSharePath);
            var driveInfo = new DriveInfo(pathRoot);

            switch (driveInfo.DriveType)
            {
        #region Network

                case DriveType.Network:
                    {
                        const int UNIVERSAL_NAME_INFO_LEVEL = 0x00000001;
                        //const int REMOTE_NAME_INFO_LEVEL = 0x00000002;

                        const int ERROR_MORE_DATA = 234;
                        const int NOERROR = 0;

                        string result = null;

                        // The pointer in memory to the structure.
                        var buffer = IntPtr.Zero;

                        // Wrap in a try/catch block for cleanup.
                        try
                        {
                            // First, call WNetGetUniversalName to get the size.
                            int size = 0;

                            // Make the call.
                            // Pass IntPtr.Size because the API doesn't like null, even though
                            // size is zero.  We know that IntPtr.Size will be
                            // aligned correctly.
                            var apiResult = NativeMethods.WNetGetUniversalName(localNetworkSharePath, UNIVERSAL_NAME_INFO_LEVEL, (IntPtr)IntPtr.Size, ref size);

                            // If the return value is not ERROR_MORE_DATA, then
                            // raise an exception.
                            if (apiResult != ERROR_MORE_DATA) throw new Win32Exception(apiResult);

                            // Allocate the memory.
                            buffer = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(size);

                            // Now make the call.
                            apiResult = NativeMethods.WNetGetUniversalName(localNetworkSharePath, UNIVERSAL_NAME_INFO_LEVEL, buffer, ref size);

                            // If it didn't succeed, then throw.
                            if (apiResult != NOERROR) throw new Win32Exception(apiResult);

                            // Now get the string.  It's all in the same buffer, but
                            // the pointer is first, so offset the pointer by IntPtr.Size
                            // and pass to PtrToStringAnsi.
                            result = System.Runtime.InteropServices.Marshal.PtrToStringAuto(new IntPtr(buffer.ToInt64() + IntPtr.Size), size);
                            result = result.Substring(0, result.IndexOf('\0'));
                        }

                        finally
                        {
                            // Release the buffer.
                            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(buffer);
                        }

                        return result;
                    }

        #endregion

        #region Default

                default:
                    return localNetworkSharePath;

        #endregion
            }
        }

        #endregion

        #region IsSyncFolder

        public static bool IsSyncFolder(string path, out SyncFolderType syncFolderType)
        {
            syncFolderType = SyncFolderType.None;

            if (string.IsNullOrEmpty(path)) return false;

            try
            {
                path = SwissAcademic.Environment.ExpandEnvironmentVariables(path);
                var directory = new DirectoryInfo(path);
                foreach (var installedSyncFolder in SwissAcademic.Environment.GetInstalledSyncFolders())
                {
                    if (directory.IsBelowOrInSameFolder(installedSyncFolder.Value))
                    {
                        syncFolderType = installedSyncFolder.Key;
                        return true;
                    }
                }
            }
            catch (Exception x)
            {
                Telemetry.TrackException(x, SeverityLevel.Information, ExceptionFlow.Eat);
            }
            return false;
        }

        #endregion
#endif
    }
}
