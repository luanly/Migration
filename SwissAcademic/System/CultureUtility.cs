using System.Globalization;

namespace System
{
    public static class CultureUtility
    {
        #region Extension Methods

        public static bool HasImeInput(this CultureInfo cultureInfo)
        {
            return cultureInfo.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase) ||
                cultureInfo.TwoLetterISOLanguageName.Equals("ja", StringComparison.OrdinalIgnoreCase) ||
                cultureInfo.TwoLetterISOLanguageName.Equals("ko", StringComparison.OrdinalIgnoreCase);
        }

        #endregion Extension Methods

        #region Static Methods

        public static bool TryGetCultureInfo(string cultureCode, string DefaultCultureCode, out CultureInfo culture)
        {
            try
            {
                culture = CultureInfo.GetCultureInfo(cultureCode);
                return true;
            }
            catch
            {
                if (DefaultCultureCode == null)
                    culture = CultureInfo.CurrentCulture;
                else
                {
                    try
                    {
                        culture = CultureInfo.GetCultureInfo(DefaultCultureCode);
                    }
                    catch (CultureNotFoundException)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }
                }
            }
            return false;
        }

        #endregion Static Methods
    }
}
