using System;
using System.Runtime.CompilerServices;

namespace SwissAcademic.ApplicationInsights
{
    public static class Telemetry
    {
        #region Fields and Constants

        public const string ExceptionDataKey = "Telemetry.IsTracked";

        #endregion

        #region Properties

        #region Client

        static ITelemetryClient _client;

        public static ITelemetryClient Client
        {
            get
            {
                //if (_client == null) throw new NullReferenceException("ITelementryClient has not been initialized. Call Initialize on a implementation class like DesktopTelemetryClient or WebTelemetryClient first.");
                return _client;
            }
            internal set
            {
                _client = value;
            }
        }

        #endregion

        #region Facades

        public static Version ApplicationVersion
        {
            get { return Client.ApplicationVersion; }
            set { Client.ApplicationVersion = value; }
        }

        public static TelemetryScope GlobalScope
        {
            get { return Client.GlobalScope; }
        }

        #endregion

        #region IsInitialized

        public static bool IsInitialized => _client != null;

        #endregion

        #endregion

        #region Methods

        public static void Flush()
        {
            Client?.Flush();
        }

        public static TelemetryScope GetCurrentScope()
        {
            if (Client == null) return TelemetryScope.None;
            return Client.GetCurrentScope();
        }

        public static string GetCurrentClientVersion()
        {
            if (Client == null) return string.Empty;
            return Client.GetCurrentClientVersion();
        }

        public static string GetCurrentSessionId()
        {
            if (Client == null) return string.Empty;
            return Client.GetCurrentSessionId();
        }

        public static string GetCurrentOperationId()
        {
            if (Client == null) return string.Empty;
            return Client.GetCurrentOperationId();
        }

        public static string GetCurrentOperationName()
        {
            if (Client == null) return string.Empty;
            return Client.GetCurrentOperationName();
        }
        public static void SetCurrentClientVersion(string value)
        {
            Client?.SetCurrentClientVersion(value);
        }
        public static void SetCurrentOperationId(string value)
        {
            Client?.SetCurrentOperationId(value);
        }

        public static void SetCurrentOperationName(string value)
        {
            Client?.SetCurrentOperationName(value);
        }

        public static void SetCurrentScope(TelemetryScope value)
        {
            Client.SetCurrentScope(value);
        }

        public static void SetCurrentSessionId(string value)
        {
            Client?.SetCurrentSessionId(value);
        }

        public static SasDependencyOperationHolder StartOperation(SasDependencyTelemetry telemetry)
        {
            if (Client == null)
            {
                var sasOperationHolder = new SasDependencyOperationHolder
                {
                    Telemetry = telemetry
                };

                return sasOperationHolder;
            }

            return Client.StartOperation(telemetry);
        }

        public static SasRequestOperationHolder StartOperation(SasRequestTelemetry telemetry)
        {
            if (Client == null)
            {
                var sasOperationHolder = new SasRequestOperationHolder
                {
                    Telemetry = telemetry
                };

                return sasOperationHolder;
            }

            return Client.StartOperation(telemetry);
        }

        public static SasDependencyOperationHolder StartOperation(string operationName)
        {

            if (Client == null)
            {
                var sasOperationHolder = new SasDependencyOperationHolder()
                {
                    Telemetry = new SasDependencyTelemetry()
                };

                return sasOperationHolder;
            }

            return Client.StartOperation(operationName);
        }

        public static SasDependencyOperationHolder StartOperation(string operationName, string operationId, string parentOperationId = null)
        {
            if (Client == null)
            {
                var sasOperationHolder = new SasDependencyOperationHolder()
                {
                    Telemetry = new SasDependencyTelemetry()
                };

                return sasOperationHolder;
            }

            return Client.StartOperation(operationName, operationId, parentOperationId);
        }

        public static void StopOperation(SasRequestOperationHolder operation)
        {
            Client?.StopOperation(operation);
        }

        // Track Exception

        public static void TrackException(Exception exception, SeverityLevel severityLevel = SeverityLevel.Error, ExceptionFlow flow = ExceptionFlow.Rethrow, (string key, object value) property1 = default, (string key, object value) property2 = default, (string key, object value) property3 = default, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null)
        {
            Client?.TrackException(exception, severityLevel, flow, property1, property2, property3, callerFilePath, callerMemberName);
        }

        public static void TrackException(SasExceptionTelemetry telemetry, ExceptionFlow flow = ExceptionFlow.Rethrow, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null)
        {
            Client?.TrackException(telemetry, flow, callerFilePath, callerMemberName);
        }

        // Track Metric

        public static void TrackMetric(SasMetricTelemetry telemetry, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null)
        {
            Client?.TrackMetric(telemetry, callerFilePath, callerMemberName);
        }

        // Track Diagnostics

        public static void TrackDiagnostics(string message, (string key, object value) property1 = default, (string key, object value) property2 = default, (string key, object value) property3 = default, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null)
        {
            Client?.TrackDiagnostics(message, property1, property2, property3, callerFilePath, callerMemberName);
        }

        public static void TrackDiagnostics(SasTraceTelemetry telemetry, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null)
        {
            Client?.TrackDiagnostics(telemetry, callerFilePath, callerMemberName);
        }

        public static void TrackDiagnostics(Exception exception, SeverityLevel severityLevel = SeverityLevel.Error, ExceptionFlow flow = ExceptionFlow.Rethrow, (string Key, object Value) property1 = default, (string Key, object Value) property2 = default, (string Key, object Value) property3 = default, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null)
        {
            Client?.TrackDiagnostics(exception, severityLevel, flow, property1, property2, property3, callerFilePath, callerMemberName);
        }

        public static void TrackDiagnostics(SasExceptionTelemetry telemetry, ExceptionFlow flow = ExceptionFlow.Rethrow, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null)
        {
            Client?.TrackDiagnostics(telemetry, flow, callerFilePath, callerMemberName);
        }

        // Track Trace

        public static void TrackTrace(string message, SeverityLevel severityLevel = SeverityLevel.Verbose, (string key, object value) property1 = default, (string key, object value) property2 = default, (string key, object value) property3 = default, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null)
        {
            Client?.TrackTrace(message, severityLevel, property1, property2, property3, callerFilePath, callerMemberName);
        }

        public static void TrackTrace(SasTraceTelemetry telemetry, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null)
        {
            Client?.TrackTrace(telemetry, callerFilePath, callerMemberName);
        }

        // Track Event

        public static void TrackEvent(SasEventTelemetry telemetry, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null)
        {
            Client?.TrackEvent(telemetry, callerFilePath, callerMemberName);
        }

        // Track PageView

        public static void TrackPageView(SasPageViewTelemetry telemetry, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null)
        {
            Client?.TrackPageView(telemetry, callerFilePath, callerMemberName);
        }

        // Track Request

        public static void TrackRequest(SasRequestTelemetry telemetry, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null)
        {
            Client?.TrackRequest(telemetry, callerFilePath, callerMemberName);
        }

        #endregion

        #region IsTracked

        public static bool IsTracked(this Exception exception) => exception.Data.Contains(ExceptionDataKey);

        #endregion

    }
}
