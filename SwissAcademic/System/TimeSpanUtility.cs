namespace System
{
    public static class TimeSpanUtility
    {

        #region Extension Methods

        #region ToReadableString

        public static string ToReadableString(this TimeSpan timeSpan)
        {
            string formatted = string.Format("{0}{1}{2}{3}{4}",
                timeSpan.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", timeSpan.Days, timeSpan.Days == 1 ? String.Empty : "s") : string.Empty,
                timeSpan.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", timeSpan.Hours, timeSpan.Hours == 1 ? String.Empty : "s") : string.Empty,
                timeSpan.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", timeSpan.Minutes, timeSpan.Minutes == 1 ? String.Empty : "s") : string.Empty,
                timeSpan.Duration().Seconds > 0 ? string.Format("{0:0} second{1}, ", timeSpan.Seconds, timeSpan.Seconds == 1 ? String.Empty : "s") : string.Empty,
                timeSpan.Duration().Milliseconds > 0 ? string.Format("{0:0} millisecond{1}", timeSpan.Milliseconds, timeSpan.Milliseconds == 1 ? String.Empty : "s") : string.Empty
            );

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            //if (string.IsNullOrEmpty(formatted)) formatted = "0 milliseconds";
            //if (string.IsNullOrEmpty(formatted)) formatted = string.Format("{0:0} ticks", timeSpan.Ticks);

            // A single tick represents one hundred nanoseconds (!) or one ten-millionth of a second. 
            if (string.IsNullOrEmpty(formatted)) formatted = string.Format("{0} nanoseconds", timeSpan.TotalMilliseconds * 1000000);

            return formatted;
        }

        #endregion ToReadableString


        #endregion Extension Methods

        #region Static Methods


        #endregion Static Methods

        #region Private Methods


        #endregion Private Methods

    }
}
