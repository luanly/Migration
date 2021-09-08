using System.ComponentModel;

namespace SwissAcademic.ApplicationInsights
{
    // We mirror these enums here in order to avoid having to set a reference
    // to the MS ApplicationInsights DLL wherever we need them.
    public enum SeverityLevel
    {
        Verbose,
        Information,
        Warning,
        Error,
        Critical
    }

    public enum ExceptionHandledAt
    {
        Platform,
        Unhandled,
        UserCode
    }

    public enum TelemetryScope
    {
        Undefined,
        None,
        CEIP,
        Trace
    }

    public enum ExceptionFlow
    {
        Rethrow,
        Eat
    }

    public enum ExceptionTracking
    {
        IfFirstOccurrence,
        Force
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum TelemetryProperty
    {
        Category,
        OperationId,
        OperationName,
        SessionId,
        CallerInformation,
        Description,
        Locals,
        Language,
        NetworkType,
        ScreenResolution,
        HandledAt,
        ClientType,
        ClientVersion,
        ProjectKey
    }
}
