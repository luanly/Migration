using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace SwissAcademic
{
    public static partial class Environment
    {
        #region Eigenschaften

        #region BuildType

        static Lazy<BuildType> _buildType = new Lazy<BuildType>(() =>
        {
#if !Web
            var attribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyConfigurationAttribute>();
            return (BuildType)Enum.Parse(typeof(BuildType), attribute.Configuration);
#else
            return ConfigurationManager.AppSettings["BuildType"].ParseEnum<BuildType>();
#endif
        });

        public static BuildType Build
        {
            get { return _buildType.Value; }
        }

        #endregion

        #region CollapseEnvironmentVariables
#if !Web
        public static string CollapseEnvironmentVariables(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            string variable;

            if (Path.IsPathRooted(path))
            {
                variable = Path.GetPathRoot(Assembly.GetExecutingAssembly().Location);
                if (Path2.GetDriveType(variable) == DriveType.Removable && path.StartsWith(variable)) return string.Concat(@"%CurrentDrive%", Path.DirectorySeparatorChar, path.Substring(variable.Length));
            }

            variable = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
            if (string.IsNullOrEmpty(variable)) return path;
            if (path.Contains(variable)) return path.Replace(variable, "%DesktopDirectory%" + Path.DirectorySeparatorChar);

            variable = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            if (string.IsNullOrEmpty(variable)) return path;
            if (path.Contains(variable)) return path.Replace(variable, "%MyDocuments%" + Path.DirectorySeparatorChar);

            foreach (var e in Enum.GetValues(typeof(SyncFolderType)))
            {
                var value = (SyncFolderType)e;
                variable = Path2.GetSyncFolderPath(value);
                if (string.IsNullOrEmpty(variable)) continue;
                if (path.Contains(variable)) return path.Replace(variable, $"%{value.ToString()}%" + Path.DirectorySeparatorChar).Replace("\\\\", "\\");
            }

            return path;
        }

#endif
        #endregion

        #region DefaultFont

        static Font _defaultFont;

        public static Font DefaultFont
        {
            get { return LazyInitializer.EnsureInitialized(ref _defaultFont, () => GetDefaultFont()); }

            set
            {
                if (value == _defaultFont) return;

                if (value == SystemFonts.MessageBoxFont) _defaultFont = null;
                else _defaultFont = value;
            }
        }

        #endregion

        #region DefaultFontBold

        static Font _defaultFontBold;

        public static Font DefaultFontBold
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _defaultFontBold, () =>
                {
                    Font font;

                    try
                    {
                        font = new Font(DefaultFont, FontStyle.Bold);
                    }
                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                        font = DefaultFont;
                    }

                    return font;
                });
            }
        }

        #endregion

        #region DefaultFontItalic

        static Font _defaultFontItalic;

        public static Font DefaultFontItalic
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _defaultFontItalic, () =>
                {
                    Font font;

                    try
                    {
                        font = new Font(DefaultFont, FontStyle.Italic);
                    }
                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                        font = DefaultFont;
                    }

                    return font;
                });
            }
        }

        #endregion

        #region DefaultFontItalicStrikeout

        static Font _defaultFontItalicStrikeOut;

        public static Font DefaultFontItalicStrikeOut
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _defaultFontItalicStrikeOut, () =>
                {
                    Font font;

                    try
                    {
                        font = new Font(DefaultFont, FontStyle.Italic | FontStyle.Strikeout);
                    }
                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                        font = DefaultFont;
                    }

                    return font;
                });
            }
        }

        #endregion

        #region DefaultFontBoldItalic

        static Font _defaultFontBoldItalic;

        public static Font DefaultFontBoldItalic
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _defaultFontBoldItalic, () =>
                {
                    Font font;

                    try
                    {
                        font = new Font(DefaultFont, FontStyle.Bold | FontStyle.Italic);
                    }
                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                        font = DefaultFont;
                    }

                    return font;
                });
            }
        }

        #endregion

        #region DefaultFontBoldItalicStrikeOut

        static Font _defaultFontBoldItalicStrikeOut;

        public static Font DefaultFontBoldItalicStrikeOut
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _defaultFontBoldItalicStrikeOut, () =>
                {
                    Font font;

                    try
                    {
                        font = new Font(DefaultFont, FontStyle.Bold | FontStyle.Italic | FontStyle.Strikeout);
                    }
                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                        font = DefaultFont;
                    }

                    return font;
                });
            }
        }

        #endregion

        #region DefaultFontBoldStrikeOut

        static Font _defaultFontBoldStrikeOut;

        public static Font DefaultFontBoldStrikeOut
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _defaultFontBoldStrikeOut, () =>
                {
                    Font font;

                    try
                    {
                        font = new Font(DefaultFont, FontStyle.Bold | FontStyle.Strikeout);
                    }
                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                        font = DefaultFont;
                    }

                    return font;
                });
            }
        }

        #endregion

        #region DefaultFontForRtf

#if !Web
        static Font _defaultFontForRtf;
        public static event EventHandler DefaultFontForRtfChanged;
#endif

        public static Font DefaultFontForRtf
        {
            get
            {
#if Web
                return DefaultFontForWeb;
#else
                if (_defaultFontForRtf == null) return GetDefaultFont();
                return _defaultFontForRtf;
#endif
            }
#if !Web
            set
            {



                if (value == _defaultFontForRtf) return;
                _defaultFontForRtf = value;
                DefaultFontForRtfChanged?.Invoke(null, EventArgs.Empty);
            }
#endif
        }

        #endregion

        #region DefaultFontForWeb

        static Font _defaultFontForWeb;

        public static Font DefaultFontForWeb
        {
            get { return LazyInitializer.EnsureInitialized(ref _defaultFontForWeb, () => new Font("Segoe UI Semilight", 15F)); }
        }

        #endregion

        #region DefaultFontStrikeOut

        static Font _defaultFontStrikeOut;

        public static Font DefaultFontStrikeOut
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _defaultFontStrikeOut, () =>
                {
                    Font font;

                    try
                    {
                        font = new Font(DefaultFont, FontStyle.Strikeout);
                    }
                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                        font = DefaultFont;
                    }

                    return font;
                });
            }
        }

        #endregion

        #region DefaultFontUnderlined

        static Font _defaultFontUnderlined;

        public static Font DefaultFontUnderlined
        {
            get
            {
                if (_defaultFontUnderlined == null)
                {
                    try
                    {
                        _defaultFontUnderlined = new Font(DefaultFont, FontStyle.Underline);
                    }
                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                    }
                }

                return _defaultFontUnderlined;
            }
        }

        #endregion

        #region DefaultSubtitleFont

        static Font _defaultSubtitleFont;

        public static Font DefaultSubtitleFont
        {
            get
            {
                if (_defaultSubtitleFont == null)
                {
                    try
                    {
                        _defaultSubtitleFont = new Font(DefaultFont.FontFamily, Convert.ToSingle(Math.Round((DefaultFont.Size * 7 / 6), 1)));
                    }
                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                    }
                }

                return _defaultSubtitleFont;
            }
        }

        #endregion

        #region DefaultSubtitleFontUnderlined

#if !Web
        static Font _defaultSubtitleFontUnderlined;

        public static Font DefaultSubtitleFontUnderlined
        {
            get
            {
                if (_defaultSubtitleFontUnderlined == null)
                {
                    try
                    {
                        _defaultSubtitleFontUnderlined = new Font(DefaultSubtitleFont, FontStyle.Underline);
                    }
                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                    }
                }

                return _defaultSubtitleFontUnderlined;
            }
        }


#endif
        #endregion

        #region DefaultTitleFont

#if !Web
        static Font _defaultTitleFont;

        public static Font DefaultTitleFont
        {
            get
            {
                if (_defaultTitleFont == null)
                {
                    try
                    {
                        _defaultTitleFont = new Font(DefaultFont.FontFamily, Convert.ToSingle(Math.Round((DefaultFont.Size * 4 / 3), 1)));
                    }
                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                    }
                }

                return _defaultTitleFont;
            }
        }

#endif
        #endregion

        #region DesignTime

#if !Web
        static bool _designTime = true;
        static bool _designTimeChecked;

        public static bool DesignTime
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                if (!_designTimeChecked)
                {
#if DEBUG
                    /* Update 5.4.2019: Da wir in C6 MAPI ausgebaut haben, ist der unten stehende Kommentar
                     * nicht mehr relevant.
                     * _designTime im Release immer auf false zu setzen, verhindert das Laden von AddOn-Formularen
                     * im Entwurfsmodus. Deshalb führen wir die Prüfung auch für Release wieder ein.
                     * 
                     * Very Voodoo, aber reproduzierbar: Diese Zeile verhindert den 
                    * Logon der E-Mail-Methode ins MAPI-System. Wenn ein anderer 
                    * Standard-Mail-Client als Outlook konfiguriert ist, schlägt 
                    * der Logon mit SimpleMapi fehl! Danach kommt immer der 
                    * Outlook-Login-Screen, auch wenn Outlook nicht default ist.
                    * Deshalb die DesignTime-Abfrage im Release immer auf false;
                    * */

                    _designTime = (System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv");
#else
					_designTime = (System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv");
					//_designTime = false;
#endif
                    _designTimeChecked = true;
                }

                return _designTime;
            }
        }
#endif
        #endregion

        #region DisplayVersionMajor

        public static int DisplayVersionMajor => SwissAcademic.Environment.InformationalVersion.Major;

        #endregion

        #region InformationalVersion

        static Version _informationalVersion;
        static bool _informationalVersionChecked;

        public static Version InformationalVersion
        {
            get
            {
                if (!_informationalVersionChecked)
                {
                    _informationalVersion = new Version(((System.Reflection.AssemblyInformationalVersionAttribute)System.Reflection.Assembly.GetAssembly(typeof(SwissAcademic.Environment)).GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)[0]).InformationalVersion);
                    _informationalVersionChecked = true;
                }

                return _informationalVersion;
            }
        }

        #endregion

        #region IsAeroActive

#if !Web
        public static bool IsAeroActive
        {
            get
            {
                try
                {
                    if (System.Environment.OSVersion.Version.Major < 6) return false;

                    int enabled = 0;
                    NativeMethods.DwmIsCompositionEnabled(ref enabled);
                    return enabled == 1;
                }

                catch (Exception ignored)
                {
                    Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                    return false;
                }
            }
        }

#endif

        #endregion

        #region IsRunningInParallels

#if !Web
        public static bool IsRunningInParallels
        {
            get
            {
                var key = Registry.CurrentUser.OpenSubKey(@"Software\Parallels");
                return key != null;
            }
        }

#endif

        #endregion

        #region IsWindows8OrHigher

#if !Web
        static bool? _isWin8;

        public static bool IsWindows8OrHigher
        {
            get
            {
                if (_isWin8 == null)
                {
                    _isWin8 = System.Environment.OSVersion.Version >= new Version(6, 2);
                }
                return _isWin8.Value;
            }
        }

#endif

        #endregion

        #region MailClient

#if !Web
        public static bool HasDefaultMailClient
        {
            get
            {
                try
                {
                    return Registry.ClassesRoot.OpenSubKey("\\mailto\\shell\\open\\command").GetValue(string.Empty).ToString().IndexOf("rundll32.exe") != -1;
                }
                catch (Exception exception)
                {
                    Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                    return false;
                }
            }
        }

#endif

        #endregion

        #region NullDate

        public static readonly DateTime NullDate = new DateTime(4501, 1, 1);

        #endregion

        #region Services

        public static ServiceProvider ServiceProvider { get; private set; }

        #endregion

        #region SolutionConfigurationIsDebug

        //Wird von c-web benötigt (Razor/cshtml)
        public static bool SolutionConfigurationIsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        #endregion

        #region TwipsPerPixelX

#if !Web
        static int _twipsPerPixelX = -1;

        public static int TwipsPerPixelX
        {
            get
            {
                if (_twipsPerPixelX == -1)
                {
                    _twipsPerPixelX = 15;
                    IntPtr dc = IntPtr.Zero;

                    try
                    {
                        dc = NativeMethods.GetDC(IntPtr.Zero);

                        if (dc != IntPtr.Zero)
                        {
                            _twipsPerPixelX = 1440 / NativeMethods.GetDeviceCaps(dc, 0x58);
                        }
                    }

                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                    }

                    finally
                    {
                        NativeMethods.ReleaseDC(IntPtr.Zero, dc);
                    }
                }

                return _twipsPerPixelX;
            }
        }

#endif

        #endregion

        #region TwipsPerPixelY

#if !Web
        static int _twipsPerPixelY = -1;

        public static int TwipsPerPixelY
        {
            get
            {
                if (_twipsPerPixelY == -1)
                {
                    _twipsPerPixelY = 15;

                    IntPtr dc = IntPtr.Zero;

                    try
                    {
                        dc = NativeMethods.GetDC(IntPtr.Zero);

                        if (dc != IntPtr.Zero)
                        {
                            _twipsPerPixelY = 1440 / NativeMethods.GetDeviceCaps(dc, 90);
                        }
                    }

                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                    }

                    finally
                    {
                        NativeMethods.ReleaseDC(IntPtr.Zero, dc);
                    }
                }

                return _twipsPerPixelY;
            }
        }

#endif

        #endregion

        #region WindowsFormsSynchronizationContext

#if !Web
        public static System.Threading.SynchronizationContext WindowsFormsSynchronizationContext { get; set; }
#endif

        #endregion

        #endregion

        #region Methoden

        #region CreateFont

#if !Web
        public static Font CreateFont(string family, float emSize)
        {
            return CreateFont(family, emSize, GraphicsUnit.Point);
        }
        public static Font CreateFont(string family, float emSize, GraphicsUnit unit)
        {
            FontStyle style = FontStyle.Regular;

            foreach (FontFamily fontFamily in FontFamily.Families)
            {
                if (fontFamily.Name == family)
                {
                    if (!fontFamily.IsStyleAvailable(style))
                    {
                        Telemetry.TrackTrace("Font style is not available", SeverityLevel.Warning, (nameof(TelemetryProperty.Description), "Font does not support style 'Regular'"), ("Font", fontFamily.Name));
                        style = FontStyle.Bold;
                    }
                    else
                    {
                        break;
                    }
                    if (!fontFamily.IsStyleAvailable(style))
                    {
                        Telemetry.TrackTrace("Font style is not available", SeverityLevel.Warning, (nameof(TelemetryProperty.Description), "Font does not support style 'Bold'"), ("Font", fontFamily.Name));
                        style = FontStyle.Italic;
                    }
                    else
                    {
                        break;
                    }
                    if (!fontFamily.IsStyleAvailable(style))
                    {
                        Telemetry.TrackTrace("Font style is not available", SeverityLevel.Warning, (nameof(TelemetryProperty.Description), "Font does not support style 'Italic'"), ("Font", fontFamily.Name));
                        style = FontStyle.Strikeout;
                    }
                    else
                    {
                        break;
                    }
                    if (!fontFamily.IsStyleAvailable(style))
                    {
                        Telemetry.TrackTrace("Font style is not available", SeverityLevel.Warning, (nameof(TelemetryProperty.Description), "Font does not support style 'Strikeout'"), ("Font", fontFamily.Name));
                        style = FontStyle.Underline;
                    }
                    else
                    {
                        break;
                    }
                    if (!fontFamily.IsStyleAvailable(style))
                    {
                        Telemetry.TrackTrace("Font style is not available", SeverityLevel.Warning, (nameof(TelemetryProperty.Description), "Font does not support style 'Underline'"), ("Font", fontFamily.Name));
                        return DefaultFont;
                    }
                    break;
                }
            }
            return new Font(family, emSize, style, unit);
        }

#endif
        #endregion

        #region ExpandEnvironmentVariables
#if !Web
        public static string ExpandEnvironmentVariables(string value)
        {
            return ExpandEnvironmentVariablesWithCheck(value).ExpandedPath;
        }

        static readonly Regex _startsWithEnvironmentVariableReges = new Regex("%[^<>:\" /\\\\|\\?\\*]+% ", RegexOptions.CultureInvariant);

        /// <summary>
        /// Expands the environment variables of <paramref name="path"/> and checks whether the 
        /// expanded path contains unresolved variables.
        /// </summary>
        public static (string ExpandedPath, bool Success) ExpandEnvironmentVariablesWithCheck(string path)
        {
            if (string.IsNullOrEmpty(path)) return (path, true);
            path = Path2.TrimPath(path);

            foreach (var pair in GetInstalledSyncFolders())
            {
                Replace($"%{pair.Key}%", pair.Value?.ToString() ?? string.Empty);
            }

            if (Path2.GetDriveType(Assembly.GetExecutingAssembly().Location) == DriveType.Removable && path.Contains("%CurrentDrive%"))
            {
                Replace("%CurrentDrive%", Path.GetPathRoot(Assembly.GetExecutingAssembly().Location));
            }

            Replace("%DesktopDirectory%", System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory));
            Replace("%MyDocuments%", System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
            Replace("%Personal%", System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));

            path = System.Environment.ExpandEnvironmentVariables(path);

            foreach (DictionaryEntry entry in System.Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User))
            {
                Replace($"%{entry.Key}%", entry.Value?.ToString() ?? string.Empty);
            }

            var success = !_startsWithEnvironmentVariableReges.IsMatch(path);

            if (success)
            {
                try
                {
                    success = Path.GetFullPath(path).Equals(path, StringComparison.OrdinalIgnoreCase);
                }
                catch(Exception)
                {
                    success = false;
                }
            }
            return (path, success);

            void Replace(string variable, string variableValue)
            {
                // don't trim leading backslashes in case of UNC drives
                variableValue = Path2.TrimPath(variableValue, !variableValue.StartsWith(@"\\"));

                var index = path.IndexOf(variable, StringComparison.OrdinalIgnoreCase);
                while (index != -1)
                {
                    path = Path.Combine(Path2.TrimPath(path.Substring(0, index)), variableValue, Path2.TrimPath(path.Substring(index + variable.Length), true));
                    index = path.IndexOf(variable, index + variableValue.Length, StringComparison.OrdinalIgnoreCase);
                }
            }
        }


#endif
        #endregion

        #region GetDefaultFont

        static Font GetDefaultFont()
        {
            return SystemFonts.MessageBoxFont;
        }

        #endregion

        #region GetInstalledSyncFolders
#if !Web
        public static IEnumerable<KeyValuePair<SyncFolderType, string>> GetInstalledSyncFolders()
        {
            var path = Path2.GetDropBoxPath();
            if (!string.IsNullOrEmpty(path)) yield return new KeyValuePair<SyncFolderType, string>(SyncFolderType.DropBox, path);

            path = Path2.GetDropBoxBusinessPath();
            if (!string.IsNullOrEmpty(path)) yield return new KeyValuePair<SyncFolderType, string>(SyncFolderType.DropBoxBusiness, path);

            path = Path2.GetGoogleDrivePath();
            if (!string.IsNullOrEmpty(path)) yield return new KeyValuePair<SyncFolderType, string>(SyncFolderType.GoogleDrive, path);

            path = Path2.GetOneDrivePath();
            if (!string.IsNullOrEmpty(path)) yield return new KeyValuePair<SyncFolderType, string>(SyncFolderType.OneDrive, path);

            foreach (var oneDriveBusinessPath in Path2.GetOneDriveBusinessPath())
            {
                yield return new KeyValuePair<SyncFolderType, string>(SyncFolderType.OneDriveBusiness, oneDriveBusinessPath);
            }
        }
#endif

        #endregion

        #region IsFontFamilyInstalled

#if !Web
        public static bool IsFontFamilyInstalled(string familyName)
        {
            try
            {
                if (string.IsNullOrEmpty(familyName)) throw new ArgumentNullException("familyName");

                using (var installedFonts = new InstalledFontCollection())
                {
                    var families = installedFonts.Families;
                    foreach (var family in families)
                    {
                        if (family.Name.Equals(familyName, StringComparison.Ordinal)) return true;
                    }

                    return false;
                }
            }

            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                return false;
            }
        }

#endif

        #endregion

        #region SendEvent

        public static void SendEvent(SendOrPostCallback d)
        {
#if !Web
            if (WindowsFormsSynchronizationContext == null) d(null);
            else WindowsFormsSynchronizationContext.Send(d, null);
#else
            d(null);
#endif
        }

        #endregion

        #region RegisterServices

        public static void RegisterServices(ServiceCollection services)
        {
            if (ServiceProvider != null)
            {
                throw new NotSupportedException("ServiceProvider must be null");
            }
            ServiceProvider = services.BuildServiceProvider();
        }

        #endregion

        #endregion
    }
}
