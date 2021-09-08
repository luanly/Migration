using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Microsoft.Win32
{
    public static class RegistryUtility
    {
        public enum RootRegistryNode
        {
            LocalMachine,
            CurrentUser
        }

        public static void RegisterURLProtocol(string protocolName, string applicationPath, string description)
        {
            var rootKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\", true);
            var protocolKey = rootKey.CreateSubKey(protocolName);

            protocolKey.SetValue(null, description);
            protocolKey.SetValue("URL Protocol", string.Empty);

            var defaultIconKey = protocolKey.CreateSubKey("DefaultIcon");
            defaultIconKey.SetValue(null, "\"{0}\", 1".FormatString(applicationPath));

            var shellKey = protocolKey.CreateSubKey("shell");
            var openKey = shellKey.CreateSubKey("open");
            var commandKey = openKey.CreateSubKey("command");
            commandKey.SetValue(null, "\"{0}\" %1".FormatString(applicationPath));
        }

        #region GetFromRegistryOrDefault
        public static T GetFromRegistryOrDefault<T>(string key, [CallerMemberName] string valueName = null, T defaultValue = default(T), RootRegistryNode rootRegistryNode = RootRegistryNode.LocalMachine)
        {
            using (var regKey = rootRegistryNode.OpenSubRegistryNode(key))
            {
                if (regKey == null) return defaultValue;

                try
                {
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    var val = regKey.GetValue(valueName, null);
                    if (val == null) return defaultValue;
                    if (typeof(T) == typeof(int))
                    {
                        return (T)Convert.ChangeType(val, TypeCode.Int32);
                    }
                    if (typeof(T) == typeof(bool))
                    {
                        return (T)Convert.ChangeType(val, TypeCode.Boolean);
                    }
                    if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
                    {
                        return (T)Convert.ChangeType(val, TypeCode.DateTime, CultureInfo.InvariantCulture);
                    }

                    return (T)converter.ConvertFrom(val);
                }
                catch
                {
                    return defaultValue;
                }
            }
        }
        #endregion

        #region RegistryKeyExists

        public static bool RegistryKeyExists(string key, RootRegistryNode rootRegistryNode = RootRegistryNode.LocalMachine)
        {
            using (var regKey = rootRegistryNode.OpenSubRegistryNode(key))
            {
                if (regKey == null) return false;
            }
            return true;
        }

        #endregion

        public static RegistryKey OpenSubRegistryNode(this RootRegistryNode rootRegistryNode, string node)
        {
            switch (rootRegistryNode)
            {
                case RootRegistryNode.LocalMachine:
                    return Registry.LocalMachine.OpenSubKey(node);
                case RootRegistryNode.CurrentUser:
                    return Registry.CurrentUser.OpenSubKey(node);
                default:
                    throw new NotImplementedException(rootRegistryNode.ToString());
            }
        }
    }
}
