using System;
using System.Globalization;
using System.Text;

namespace SwissAcademic.Azure.Storage
{
    public abstract class AzureStorageAccess
    {
        #region Konstruktoren

        public AzureStorageAccess(Uri uri, string sharedAccessSignature)
        {
            Uri = uri;
            SharedAccessSignature = sharedAccessSignature;
        }

        #endregion

        #region Eigenschaften

        #region Uri

        public Uri Uri { get; private set; }

        #endregion

        #region SharedAccessSignature

        public string SharedAccessSignature { get; private set; }

        #endregion

        #endregion

        #region Methods

        private static string NormalizeString(string value)
        {
            if (value.Length > 1024) value = value.Substring(0, 1024);
            string normalizedFormD = value.Normalize(NormalizationForm.FormD);
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < normalizedFormD.Length; i++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(normalizedFormD[i]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(normalizedFormD[i]);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        #endregion
    }
}
