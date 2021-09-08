using Sustainsys.Saml2;
using SwissAcademic.ApplicationInsights;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    public class ShibbolethLogger
        :
        ILoggerAdapter

    {
        public void WriteInformation(string message)
        {
            Telemetry.TrackTrace(message, SeverityLevel.Information);
        }

        public void WriteError(string message, Exception ex)
        {
            if (ex == null)
            {
                Telemetry.TrackTrace(message, SeverityLevel.Error);
            }
            else
            {
                if (ex.Message.Contains("No Saml2 Response found in the http request."))
                {
                    Telemetry.TrackException(ex, SeverityLevel.Warning, ExceptionFlow.Eat, property1: (nameof(TelemetryProperty.Description), message));
                }
                else
                {
                    Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat, property1: (nameof(TelemetryProperty.Description), message));
                }

            }
        }

        public void WriteVerbose(string message)
        {
            Telemetry.TrackTrace(message, SeverityLevel.Verbose);
        }

        public static bool IsIgnorableSaml2Exception(Exception ex)
        {
            if (ex == null)
            {
                return false;
            }

            switch (ex)
            {
                case Sustainsys.Saml2.Exceptions.NoSamlResponseFoundException e:
                    return true;
            }
            return false;
        }
    }
}
