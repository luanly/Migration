using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SwissAcademic.ApplicationInsights
{
    public interface ITelemetryClient
    {
        #region Properties

        Version ApplicationVersion { get; set; }
        TelemetryScope GlobalScope { get; }

        #endregion

        #region Methods

        void Flush();
        TelemetryScope GetCurrentScope();
        string GetCurrentClientVersion();
        string GetCurrentSessionId();
        string GetCurrentOperationId();
        string GetCurrentOperationName();
        void SetCurrentClientVersion(string value);
        void SetCurrentOperationId(string value);
        void SetCurrentOperationName(string value);
        void SetCurrentScope(TelemetryScope value);
        void SetCurrentSessionId(string value);

        #region Track

        SasDependencyOperationHolder StartOperation(SasDependencyTelemetry telemetry);
        SasRequestOperationHolder StartOperation(SasRequestTelemetry telemetry);
        SasDependencyOperationHolder StartOperation(string operationName);

        SasDependencyOperationHolder StartOperation(string operationName, string operationId, string parentOperationId = null);

        void StopOperation(SasDependencyOperationHolder operation);
        void StopOperation(SasRequestOperationHolder operation);

        // Track Dependency

        void TrackDependency(SasDependencyTelemetry telemetry, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);


        // Track Diagnostics

        void TrackDiagnostics(string message, (string Key, object Value) property1 = default, (string Key, object Value) property2 = default, (string Key, object Value) property3 = default, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);

        void TrackDiagnostics(SasTraceTelemetry telemetry, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);

        void TrackDiagnostics(Exception exception, SeverityLevel severityLevel = SeverityLevel.Error, ExceptionFlow flow = ExceptionFlow.Rethrow, (string Key, object Value) property1 = default, (string Key, object Value) property2 = default, (string Key, object Value) property3 = default, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);

        void TrackDiagnostics(SasExceptionTelemetry telemetry, ExceptionFlow flow = ExceptionFlow.Rethrow, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);

        // Track Exception

        void TrackException(Exception exception, SeverityLevel severityLevel = SeverityLevel.Error, ExceptionFlow flow = ExceptionFlow.Rethrow, (string Key, object Value) property1 = default, (string Key, object Value) property2 = default, (string Key, object Value) property3 = default, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);

        void TrackException(SasExceptionTelemetry telemetry, ExceptionFlow flow = ExceptionFlow.Rethrow, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);

        // Track Metrics

        void TrackMetric(SasMetricTelemetry telemetry, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);

        // Track Trace

        void TrackTrace(string message, SeverityLevel severityLevel = SeverityLevel.Verbose, (string Key, object Value) property1 = default, (string Key, object Value) property2 = default, (string Key, object Value) property3 = default, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);

        void TrackTrace(SasTraceTelemetry telemetry, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);

        // Track Event

        void TrackEvent(SasEventTelemetry telemetry, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);

        // Track PageView

        void TrackPageView(SasPageViewTelemetry telemetry, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);

        // Track Request

        void TrackRequest(SasRequestTelemetry telemetry, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);



        #endregion

        #endregion
    }

    public interface IDesktopTelemetryClient
        :
        ITelemetryClient
    {
        string GlobalSessionId { get; }
        void SetGlobalScope(TelemetryScope value);
    }

    public interface IWebTelemetryClient
        :
        ITelemetryClient
    {
        void AddTelemetryContext(IDictionary<string, string> properties);
        void SetTelemetryContext(IDictionary<string, string> properties, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);
    }

    public class SasDependencyTelemetry
    {
        public string Data { get; set; }
        public TimeSpan Duration { get; set; }
        public string Id { get; set; }
        internal Dictionary<string, double> _metrics;
        public IDictionary<string, double> Metrics => LazyInitializer.EnsureInitialized(ref _metrics);
        public string Name { get; set; }
        internal Dictionary<string, string> _properties;
        public IDictionary<string, string> Properties => LazyInitializer.EnsureInitialized(ref _properties);
        public string ResultCode { get; set; }
        public string Sequence { get; set; }
        public bool? Success { get; set; }
        public string Target { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string Type { get; set; }
    }

    public class SasExceptionTelemetry
    {
        public SasExceptionTelemetry()
        {
        }

        public SasExceptionTelemetry(Exception exception, SeverityLevel severityLevel, IDictionary<string, string> properties)
        {
            Exception = exception;
            SeverityLevel = severityLevel;

            if (properties != null)
            {
                var dict = properties as Dictionary<string, string>;
                if (dict == null)
                {
                    foreach (var item in properties)
                    {
                        Properties.Add(item);
                    }
                }
                else
                {
                    _properties = dict;
                }
            }
        }

        public Exception Exception { get; set; }
        public string Message { get; set; }
        internal Dictionary<string, double> _metrics;
        public IDictionary<string, double> Metrics => LazyInitializer.EnsureInitialized(ref _metrics);
        public string ProblemId { get; set; }
        internal Dictionary<string, string> _properties;
        public IDictionary<string, string> Properties => LazyInitializer.EnsureInitialized(ref _properties);
        public string Sequence { get; set; }
        public SeverityLevel SeverityLevel { get; set; } = SeverityLevel.Error;
        public DateTimeOffset TimeStamp { get; set; }

        public ExceptionTracking ExceptionTracking { get; set; }
    }

    public class SasMetricTelemetry
    {
        public int? Count { get; set; }
        public double? Max { get; set; }
        public double? Min { get; set; }
        public string Name { get; set; }
        internal Dictionary<string, string> _properties;
        public IDictionary<string, string> Properties => LazyInitializer.EnsureInitialized(ref _properties);
        public string Sequence { get; set; }
        public double? StandardDeviation { get; set; }
        public double Sum { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }

    public class SasTraceTelemetry
    {
        public string Message { get; set; }
        internal Dictionary<string, string> _properties;
        public IDictionary<string, string> Properties => LazyInitializer.EnsureInitialized(ref _properties);
        public string Sequence { get; set; }
        public SeverityLevel? SeverityLevel { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }

    public class SasEventTelemetry
    {
        internal Dictionary<string, double> _metrics;
        public IDictionary<string, double> Metrics => LazyInitializer.EnsureInitialized(ref _metrics);
        public string Name { get; set; }
        internal Dictionary<string, string> _properties;
        public IDictionary<string, string> Properties => LazyInitializer.EnsureInitialized(ref _properties);
        public string Sequence { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }

    public class SasPageViewTelemetry
    {
        public TimeSpan Duration { get; set; }
        public string Id { get; set; }
        internal Dictionary<string, double> _metrics;
        public IDictionary<string, double> Metrics => LazyInitializer.EnsureInitialized(ref _metrics);
        public string Name { get; set; }
        internal Dictionary<string, string> _properties;
        public IDictionary<string, string> Properties => LazyInitializer.EnsureInitialized(ref _properties);
        public string Sequence { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public Uri Url { get; set; }
    }

    public class SasRequestTelemetry
    {
        public TimeSpan Duration { get; set; }
        public string Id { get; set; }
        internal Dictionary<string, double> _metrics;
        public IDictionary<string, double> Metrics => LazyInitializer.EnsureInitialized(ref _metrics);
        public string Name { get; set; }
        internal Dictionary<string, string> _properties;
        public IDictionary<string, string> Properties => LazyInitializer.EnsureInitialized(ref _properties);
        public string ResponseCode { get; set; }
        public string Sequence { get; set; }
        public string Source { get; set; }
        public bool? Success { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public Uri Url { get; set; }
    }

    public class SasDependencyOperationHolder
        :
        IDisposable
    {
        internal SasDependencyOperationHolder() { }
        public SasDependencyTelemetry Telemetry { get; internal set; }
        internal object OperationHolder { get; set; }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (OperationHolder != null)
                {
                    ((IDisposable)OperationHolder).Dispose();
                }
            }
        }
    }

    public class SasRequestOperationHolder
    {
        internal SasRequestOperationHolder() { }
        public SasRequestTelemetry Telemetry { get; internal set; }
        internal object OperationHolder { get; set; }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (OperationHolder != null)
                {
                    ((IDisposable)OperationHolder).Dispose();
                }
            }
        }
    }
}
