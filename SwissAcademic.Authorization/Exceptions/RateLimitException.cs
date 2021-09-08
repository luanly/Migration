using System;
using System.Net;

namespace SwissAcademic
{
    public class RateLimitException
        :
        Exception
    {
        #region Constructors

        public RateLimitException()
            :
            this(null, null)
        {
        }

        public RateLimitException(string message)
            :
            this(message, null)
        {
        }

        public RateLimitException(string message, Exception innerException)
            :
            base(message, innerException)
        {
            Data[nameof(HttpStatusCode)] = 429;
            this.TreatAsWarning();
        }

        #endregion
    }
}
