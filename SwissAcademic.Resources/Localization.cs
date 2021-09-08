using System.Globalization;

namespace SwissAcademic.Resources
{
    public static class Localization
    {
        #region Methoden

        #region GetControlText

        public static string GetControlText(string typeName, string controlName)
        {
            return GetControlText(string.Concat(typeName, "_", controlName));
        }

        public static string GetControlText(string typeName, string toolName, CultureInfo culture = null)
        {
            return GetControlText(string.Concat(typeName, "_", toolName), culture);
        }

        public static string GetControlText(string name)
        {
            var controlString = ControlTexts.ResourceManager.GetString(name);
            return controlString;
        }

        public static string GetControlText(string name, CultureInfo culture = null)
        {
            var controlString = culture == null ?
                ControlTexts.ResourceManager.GetString(name) :
                ControlTexts.ResourceManager.GetString(name, culture);

            return controlString;
        }

        #endregion

        #region GetToolCaption

        public static string GetToolCaption(string typeName, string toolName)
        {
            return GetToolCaption(string.Concat(typeName, "_", toolName));
        }

        public static string GetToolCaption(string typeName, string toolName, CultureInfo culture = null)
        {
            return GetToolCaption(string.Concat(typeName, "_", toolName), culture);
        }

        public static string GetToolCaption(string name)
        {
            return Tools.ResourceManager.GetString(name);
        }

        public static string GetToolCaption(string name, CultureInfo culture = null)
        {
            if (culture == null) return Tools.ResourceManager.GetString(name);
            return Tools.ResourceManager.GetString(name, culture);
        }

        #endregion

        #endregion
    }
}

