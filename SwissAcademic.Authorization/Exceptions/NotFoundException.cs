using System;
using System.Net;

namespace SwissAcademic.Authorization
{
    public class NoLongerAvailableException
        :
        Exception
    {
        #region Constructors

        public NoLongerAvailableException()
            :
            this(null, null)
        {
        }

        public NoLongerAvailableException(string message)
            :
            this(message, null)
        {
        }

        public NoLongerAvailableException(string message, Exception innerException)
            :
            base(message, innerException)
        {
            Data[nameof(HttpStatusCode)] = HttpStatusCode.Gone;
            this.TreatAsWarning();
        }

        #endregion
    }
}
