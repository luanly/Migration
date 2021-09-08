using System.Globalization;

namespace System
{
    public static class DateTimeUtility
    {
        #region Extension Methods

        #region TrimToSecond

        public static DateTime TrimToSecond(this DateTime value)
        {
            // Zeiteinheiten unterhalb einer Sekunde abschneiden, falls Zeit automatisch generiert wurde.
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
        }

        #endregion TrimToSecond

        #region TrimToDay

        public static DateTime TrimToDay(this DateTime value)
        {
            // Zeiteinheiten komplett abschneiden
            return new DateTime(value.Year, value.Month, value.Day);
        }

        #endregion TrimToDay

        #region SetDateValue

        public static void SetDateValue(object value, Action<DateTime?> setter, DateTime? nullValue = null)
        {
            if (value == null || value == DBNull.Value)
            {
                setter(nullValue);
            }
            else if (value is string)
            {
                setter(DateTime.Parse(value.ToString()));
            }
            else
            {
                setter((DateTime)value);
            }
        }

        public static void SetDateValue(object value, Action<DateTime> setter, DateTime nullValue)
        {
            if (value == null || value == DBNull.Value)
            {
                setter(nullValue);
            }
            else if (value is string)
            {
                setter(DateTime.Parse(value.ToString()));
            }
            else
            {
                setter((DateTime)value);
            }
        }

        #endregion

        #region WillUpdate

        public static bool WillUpdate(object value, Func<DateTime?> getter, DateTime? nullValue = null)
        {
            DateTime? dateTime;

            if (value == null || value == DBNull.Value)
            {
                dateTime = nullValue;
            }
            else if (value is string)
            {
                if (DateTime.TryParse(value.ToString(), out var dt))
                {
                    dateTime = dt;
                }
                else
                {
                    dateTime = nullValue;
                }
            }
            else
            {
                dateTime = (DateTime)value;
            }

            return getter() != dateTime;
        }

        public static bool WillUpdate(object value, Func<DateTime> getter, DateTime nullValue)
        {
            DateTime dateTime;

            if (value == null || value == DBNull.Value)
            {
                dateTime = nullValue;
            }
            else if (value is string)
            {
                if(!DateTime.TryParse(value.ToString(), out dateTime))
                {
                    dateTime = nullValue;
                }
            }
            else
            {
                dateTime = (DateTime)value;
            }

            return getter() != dateTime;
        }

        #endregion

        #endregion Extension Methods

        #region ParseVariable

        public static DateTime ParseVariable(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");

            var variable = (DateTimeVariable)Enum.Parse(typeof(DateTimeVariable), value.Substring(1), true);

            switch (variable)
            {
                case DateTimeVariable.Today:
                    return DateTime.Today;

                case DateTimeVariable.Yesterday:
                    return DateTime.Today.AddDays(-1);

                case DateTimeVariable.FirstOfThisWeek:
                    {
                        var dayOfWeek = CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(DateTime.Today) - 1;
                        return DateTime.Today.AddDays(-(int)dayOfWeek);
                    }

                case DateTimeVariable.FirstOfLastWeek:
                    {
                        var dayOfWeek = CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(DateTime.Today) - 1;
                        return DateTime.Today.AddDays(-(int)dayOfWeek - 7);
                    }

                case DateTimeVariable.FirstOfThisMonth:
                    return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                case DateTimeVariable.FirstOfLastMonth:
                    return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);

                case DateTimeVariable.FirstOfThisYear:
                    return new DateTime(DateTime.Today.Year, 1, 1);

                case DateTimeVariable.FirstOfLastYear:
                    return new DateTime(DateTime.Today.Year, 1, 1).AddYears(-1);

                default:
                    throw new NotSupportedException("Unknown date variable: {0}".FormatString(value));
            }
        }

        #endregion
    }

    public enum DateTimeVariable
    {
        None,
        Today,
        Yesterday,
        FirstOfThisWeek,
        FirstOfLastWeek,
        FirstOfThisMonth,
        FirstOfLastMonth,
        FirstOfThisYear,
        FirstOfLastYear
    }
}
