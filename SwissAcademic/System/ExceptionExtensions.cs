using SwissAcademic.ApplicationInsights;
using System;

namespace SwissAcademic
{
    public static class ExceptionExtensions
    {
        public static void TreatAsWarning(this Exception exception)
        {
            exception.Data[nameof(SeverityLevel)] = "Warning";
        }
    }
}
