using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    public class CrmServerException
        :
        AggregateException
    {
        public CrmServerException()
        {

        }

        public CrmServerException(string message)
            :
            base(message)
        {

        }

        public CrmServerException(string message, Exception innerException)
            :
            base(message, innerException)
        {

        }

        public CrmServerException(string message, IEnumerable<Exception> innerExceptions)
            : base(message, innerExceptions)
        {
        }

        public List<FailedSaveRequest> FailedSaveRequests { get; } = new List<FailedSaveRequest>();

    }

    public class FailedSaveRequest
    {
        public CrmEntityChanged CrmEntityChanged { get; set; }
        public string ContentId { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
    }
}
