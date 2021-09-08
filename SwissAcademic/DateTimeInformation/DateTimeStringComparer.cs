using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;

namespace SwissAcademic
{
    public class DateTimeStringComparer
        :
        IComparer<string>,
        System.Collections.IComparer
    {
        #region Konstruktoren

        private DateTimeStringComparer()
        {
        }

        #endregion

        #region Eigenschaften

        #region Default

        static DateTimeStringComparer _default;

        public static DateTimeStringComparer Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new DateTimeStringComparer();
                }
                return _default;
            }
        }

        #endregion

        #endregion

        #region Methoden

        public int Compare(string x, string y)
        {
            /*
				return values explained
				0:					x is considered the same as y sorting-wise, so we cannot tell a difference based on the algorithm below
				> 0 (positive):		x should go after y, x is greater than y
				< 0 (negative):		x should go before y, x is less than
			*/

            try
            {
                if (string.IsNullOrEmpty(x))
                {
                    if (string.IsNullOrEmpty(y)) return 0;
                    return -1;
                }
                if (string.IsNullOrEmpty(y)) return 1;

                DateTime xDateTime;
                DateTime yDateTime;

                if (DateTimeInformation.TryParse(x, out xDateTime))
                {
                    if (DateTimeInformation.TryParse(y, out yDateTime)) return xDateTime.CompareTo(yDateTime);

                    // x ist ein Datum, y nicht - also kommt x nach y
                    return 1;
                }

                if (DateTimeInformation.TryParse(y, out yDateTime))
                {
                    // x ist kein Datum, y schon - also kommt x vor y
                    return -1;
                }

                // beides sind keine Daten
                return StringComparer.CurrentCulture.Compare(x.GetCompareValue(), y.GetCompareValue());
            }

            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);

                try
                {
                    return StringComparer.CurrentCulture.Compare(x.GetCompareValue(), y.GetCompareValue());
                }

                catch (Exception ignored2)
                {
                    Telemetry.TrackException(ignored2, SeverityLevel.Warning, ExceptionFlow.Eat);
                    return 0;
                }
            }
        }

        #endregion

        #region IComparer Member

        int System.Collections.IComparer.Compare(object x, object y)
        {
            return Compare(x as string, y as string);
        }

        #endregion
    }
}
