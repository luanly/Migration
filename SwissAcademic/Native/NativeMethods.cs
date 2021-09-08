#if !Web
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Text;

namespace SwissAcademic
{
#pragma warning disable CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden
    public static class NativeMethods
    {
        public const int MAX_PATH = 256;
        internal static int NETWORK_ALIVE_LAN = 0x00000001;

        #region FileAttribute

        internal enum FileAttribute
            :
            uint
        {
            FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
            FILE_ATTRIBUTE_NORMAL = 0x00000080
        }

        #endregion

        #region FileInfoFlags

        [Flags]
        internal enum FileInfoFlags
            :
            uint
        {
            /// <summary> 
            /// Retrieve the handle to the icon that represents the file and the index  
            /// of the icon within the system image list. The handle is copied to the  
            /// hIcon member of the structure specified by psfi, and the index is copied  
            /// to the iIcon member. 
            /// </summary> 
            SHGFI_ICON = 0x000000100,     // get icon

            /// <summary>
            /// Retrieve the display name for the file. The name is copied to the szDisplayName member of the structure specified in psfi. The returned display name uses the long file name, if there is one, rather than the 8.3 form of the file name.
            /// </summary>
            SHGFI_DISPLAYNAME = 0x000000200,

            /// <summary>
            /// Retrieve the string that describes the file's type. The string is copied to the szTypeName member of the structure specified in psfi.
            /// </summary>
            SHGFI_TYPENAME = 0x000000400,

            /// <summary>
            /// Retrieve the item attributes. The attributes are copied to the dwAttributes member of the structure specified in the psfi parameter. These are the same attributes that are obtained from IShellFolder::GetAttributesOf.
            /// </summary>
            SHGFI_ATTRIBUTES = 0x000000800,

            /// <summary>
            /// Retrieve the name of the file that contains the icon representing the file specified by pszPath, as returned by the IExtractIcon::GetIconLocation method of the file's icon handler. Also retrieve the icon index within that file. The name of the file containing the icon is copied to the szDisplayName member of the structure specified by psfi. The icon's index is copied to that structure's iIcon member.
            /// </summary>
            SHGFI_ICONLOCATION = 0x000001000,     // get icon location

            /// <summary>
            /// Retrieve the type of the executable file if pszPath identifies an executable file. The information is packed into the return value. This flag cannot be specified with any other flags.
            /// </summary>
            SHGFI_EXETYPE = 0x000002000,

            /// <summary>
            /// Retrieve the index of a system image list icon. If successful, the index is copied to the iIcon member of psfi. The return value is a handle to the system image list. Only those images whose indices are successfully copied to iIcon are valid. Attempting to access other images in the system image list will result in undefined behavior.
            /// </summary>
            SHGFI_SYSICONINDEX = 0x000004000,

            /// <summary>
            /// Modify SHGFI_ICON, causing the function to add the link overlay to the file's icon. The SHGFI_ICON flag must also be set.
            /// </summary>
            SHGFI_LINKOVERLAY = 0x000008000,

            /// <summary>
            /// Modify SHGFI_ICON, causing the function to blend the file's icon with the system highlight color. The SHGFI_ICON flag must also be set.
            /// </summary>
            SHGFI_SELECTED = 0x000010000,

            /// <summary>
            /// Modify SHGFI_ATTRIBUTES to indicate that the dwAttributes member of the SHFILEINFO structure at psfi contains the specific attributes that are desired. These attributes are passed to IShellFolder::GetAttributesOf. If this flag is not specified, 0xFFFFFFFF is passed to IShellFolder::GetAttributesOf, requesting all attributes. This flag cannot be specified with the SHGFI_ICON flag.
            /// </summary>
            SHGFI_ATTR_SPECIFIED = 0x000020000,

            /// <summary>
            /// Modify SHGFI_ICON, causing the function to retrieve the file's large icon. The SHGFI_ICON flag must also be set.
            /// </summary>
            SHGFI_LARGEICON = 0x000000000,

            /// <summary>
            /// Modify SHGFI_ICON, causing the function to retrieve the file's small icon. Also used to modify SHGFI_SYSICONINDEX, causing the function to return the handle to the system image list that contains small icon images. The SHGFI_ICON and/or SHGFI_SYSICONINDEX flag must also be set.
            /// </summary>
            SHGFI_SMALLICON = 0x000000001,

            /// <summary>
            /// Modify SHGFI_ICON, causing the function to retrieve the file's open icon. Also used to modify SHGFI_SYSICONINDEX, causing the function to return the handle to the system image list that contains the file's small open icon. A container object displays an open icon to indicate that the container is open. The SHGFI_ICON and/or SHGFI_SYSICONINDEX flag must also be set.
            /// </summary>
            SHGFI_OPENICON = 0x000000002,

            /// <summary>
            /// Modify SHGFI_ICON, causing the function to retrieve a Shell-sized icon. If this flag is not specified the function sizes the icon according to the system metric values. The SHGFI_ICON flag must also be set.
            /// </summary>
            SHGFI_SHELLICONSIZE = 0x000000004,     // get shell size icon

            /// <summary>
            /// Indicate that pszPath is the address of an ITEMIDLIST structure rather than a path name.
            /// </summary>
            SHGFI_PIDL = 0x000000008,

            /// <summary>
            /// Indicates that the function should not attempt to access the file specified by pszPath. Rather, it should act as if the file specified by pszPath exists with the file attributes passed in dwFileAttributes. This flag cannot be combined with the SHGFI_ATTRIBUTES, SHGFI_EXETYPE, or SHGFI_PIDL flags.
            /// </summary>
            SHGFI_USEFILEATTRIBUTES = 0x000000010,

            /// <summary>
            /// Version 5.0. Apply the appropriate overlays to the file's icon. The SHGFI_ICON flag must also be set.
            /// </summary>
            SHGFI_ADDOVERLAYS = 0x000000020,

            /// <summary>
            /// Version 5.0. Return the index of the overlay icon. The value of the overlay index is returned in the upper eight bits of the iIcon member of the structure specified by psfi. This flag requires that the SHGFI_ICON be set as well.
            /// </summary>
            SHGFI_OVERLAYINDEX = 0x000000040
        }

        #endregion

        #region SHFILEINFO

        [StructLayout(LayoutKind.Sequential)]
        internal struct SHFILEINFO
        {
            internal const int NAMESIZE = 80;
            public IntPtr hIcon;
            public int iIcon;
            public FileAttribute dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NAMESIZE)]
            public string szTypeName;
        };

        #endregion

        #region Enums

        #region AssocF

        [Flags]
        internal enum AssocF
        {
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200
        }

        #endregion

        #region AssocStr

        internal enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic
        }

        #endregion

        #endregion

        #region AssocQueryString

        [DllImport("Shlwapi.dll", SetLastError = true)]

        internal static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);


        #endregion

        #region DwmIsCompositionEnabled

        [DllImport("dwmapi.dll")]
        internal static extern int DwmIsCompositionEnabled(ref int enabled);

        #endregion

        #region GetDC

        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hWnd);

        #endregion

        #region GetDeviceCaps

        [DllImport("gdi32.dll")]
        internal static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        #endregion

        #region WNetGetUniversalName

        [DllImport("mpr.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        internal static extern int WNetGetUniversalName(
            string lpLocalPath,
            [MarshalAs(UnmanagedType.U4)] int dwInfoLevel,
            IntPtr lpBuffer,
            [MarshalAs(UnmanagedType.U4)] ref int lpBufferSize);

        #endregion

        #region DestroyIcon

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        internal static extern int DestroyIcon(IntPtr hIcon);   //unsafe  

        #endregion

        #region SHGetFileInfo

        [DllImport("Shell32")]
        internal static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        #endregion

        #region PathIsUNC

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        internal static extern bool PathIsUNC([MarshalAsAttribute(UnmanagedType.LPWStr), In] string pszPath);

        #endregion

        #region ReleaseDC

        [DllImport("user32.dll")]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        #endregion

        #region IsNetworkAlive

        [DllImport("sensapi.dll")]
        internal extern static bool IsNetworkAlive(ref int flags);

        #endregion

        #region InternetGetConnectedStateEx

        [DllImport("wininet.dll")]
        internal static extern System.Int32 InternetGetConnectedStateEx(out System.Int32 lpdwFlags, StringBuilder lpszConnectionName, System.Int32 dwNameLen, System.Int32 dwReserved);

        #endregion

        #region IconInfo

        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        #endregion

        #region GetIconInfo

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        #endregion

        #region CreateIconIndirect

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        #endregion

        #region LogonUser

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        #endregion

        #region CloseHandle

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr handle);

        #endregion
    }

#pragma warning restore CA5392 // DefaultDllImportSearchPaths-Attribut für P/Invokes verwenden
}
#endif
