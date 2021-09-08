using System;
using System.Net;

namespace SwissAcademic.Authorization
{
    public class ConflictException
        :
        Exception
    {
        #region Constructors

        public ConflictException()
            :
            this(null, null)
        {
        }

        public ConflictException(string message)
            :
            this(message, null)
        {
        }

        public ConflictException(string message, Exception innerException)
            :
            base(message, innerException)
        {
            Data[nameof(HttpStatusCode)] = HttpStatusCode.Conflict;
            this.TreatAsWarning();
        }

        #endregion
    }
}
