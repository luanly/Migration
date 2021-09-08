using SwissAcademic.ApplicationInsights;
using System;
using System.Text.RegularExpressions;

namespace SwissAcademic.Crm.Web
{
    public static class CrmAttributeValidator
    {
        static readonly Regex RemoveWhitespace = new Regex(@"\s+");

        public static bool Validate(string attributeName, ReadOnlySpan<char> value, bool throwException = true)
        {
            if (attributeName == CrmAttributeNames.Key)
            {
                value = value.Trim();
                foreach (var c in value)
                {
                    if (char.IsWhiteSpace(c))
                    {
                        if (throwException)
                        {
                            Telemetry.TrackException(new CrmAttributeValidationException(attributeName, value.ToString(), $"{CrmAttributeNames.Key} must not contains whitespaces"), property2: (attributeName, value.ToString()));
                        }
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
