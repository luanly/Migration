using SwissAcademic.Resources;
using System;
using System.Collections.Generic;

namespace SwissAcademic.Azure
{
    public class CitaviWebJobException
        :
        Exception
    {
        #region Properties

        #region JobId

        public string JobId { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public CitaviWebJobException(string message, string jobId)
            :
            base(message)
        {
            JobId = jobId;
        }

        #endregion

        public static CitaviWebJobException FromProperties(IDictionary<string, string> properties)
        {
            if (!properties.TryGetValue(MessageKey.ErrorMessage, out var message))
            {
                message = Strings.CitaviCloudException_Unknown;
            }
            if (!properties.TryGetValue(MessageKey.JobId, out var jobId))
            {
                throw new ArgumentNullException(nameof(MessageKey.JobId));
            }
            return new CitaviWebJobException(message, jobId);
        }
    }
}
