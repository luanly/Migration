namespace System.Data
{
    public static class DatabaseUtility
    {
        #region GetDBValue

        public static object GetDBValue(this Guid value)
        {
            return value == Guid.Empty ? (object)DBNull.Value : value;
        }

        public static object GetDBValue(this Guid? value)
        {
            if (!value.HasValue) return DBNull.Value;
            return value.Value == Guid.Empty ? (object)DBNull.Value : value.Value;
        }

        public static object GetDBValue(this string value)
        {
            //return string.IsNullOrEmpty(value) ? (object)DBNull.Value : value;
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;
        }

        public static object GetDBValue(this DateTime value)
        {
            // Zeiteinheiten unterhalb einer Sekunde abschneiden, falls Zeit automatisch generiert wurde.
            return value == SwissAcademic.Environment.NullDate || value == DateTime.MinValue ? (object)DBNull.Value : value.TrimToSecond();
        }

        public static object GetDBValue(this DateTime? value)
        {
            // Zeiteinheiten unterhalb einer Sekunde abschneiden, falls Zeit automatisch generiert wurde.
            return !value.HasValue || value.Value == SwissAcademic.Environment.NullDate || value.Value == DateTime.MinValue ? (object)DBNull.Value : value.Value.TrimToSecond();
        }

        #endregion
    }
}
