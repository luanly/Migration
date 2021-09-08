using SwissAcademic.ApplicationInsights;
using System.Diagnostics;

namespace Newtonsoft.Json
{
    public static class JsonConfig
    {
        #region Methods

        #region ConfigureSerializerSettings

        public static void Configure(this JsonSerializerSettings serializerSettings)
        {
            serializerSettings.Formatting = Debugger.IsAttached ? Formatting.Indented : Formatting.None;

            // see http://james.newtonking.com/projects/json/help/index.html?topic=html/DeserializeConstructorHandling.htm
            serializerSettings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            serializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            serializerSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
            serializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;

            serializerSettings.Error = (sender, args) =>
            {
                var et = new SasExceptionTelemetry
                {
                    Exception = args?.ErrorContext?.Error,
                    SeverityLevel = SeverityLevel.Error
                };

                et.Properties[nameof(sender)] = sender?.ToString();
                et.Properties[nameof(args.CurrentObject)] = args.CurrentObject?.ToString();
                et.Properties[$"{nameof(args.ErrorContext)}.{nameof(args.ErrorContext.Handled)}"] = args?.ErrorContext?.Handled.ToString();
                et.Properties[$"{nameof(args.ErrorContext)}.{nameof(args.ErrorContext.OriginalObject)}"] = args?.ErrorContext?.OriginalObject?.ToString();
                et.Properties[$"{nameof(args.ErrorContext)}.{nameof(args.ErrorContext.Path)}"] = args?.ErrorContext?.Path;

                Telemetry.TrackException(et, ExceptionFlow.Rethrow);
            };

            // Default is what we want, we don't have to set these handlings:
            // serializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            // serializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            // serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Error;
        }

        #endregion 

        #endregion
    }
}
