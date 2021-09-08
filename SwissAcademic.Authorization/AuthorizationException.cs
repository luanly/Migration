using System;
using System.Net;

namespace SwissAcademic.Authorization
{
    public class AuthorizationException
        :
        Exception
    {
        #region Constructors

        public AuthorizationException()
            :
            this(null, null)
        {

        }

        public AuthorizationException(string message)
            :
            this(message, null)
        {
        }

        public AuthorizationException(string message, Exception innerException)
            :
            base(message, innerException)
        {
            Data[nameof(HttpStatusCode)] = HttpStatusCode.Forbidden;
        }

        #endregion
    }
}
