using SwissAcademic.ApplicationInsights;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace SwissAcademic.Azure
{
    public class CitaviWebApiException
        :
        Exception
    {
        #region Properties

        #region HttpStatusCode
        public HttpStatusCode StatusCode => Response.StatusCode;
        #endregion

        #region Method
        public HttpMethod Method => Response.RequestMessage.Method;
        #endregion

        #region OperationId
        public string OperationId => Response.RequestMessage.Headers.GetValues(nameof(TelemetryProperty.OperationId)).FirstOrDefault();
        #endregion

        #region Response
        public HttpResponseMessage Response { get; private set; }
        #endregion

        #region RequestUri
        public string RequestUri => Response.RequestMessage.RequestUri.ToString();
        #endregion

        #endregion

        #region Constructors

        public CitaviWebApiException(HttpResponseMessage response)
        {
            Response = response ?? throw new ArgumentNullException(nameof(response));
        }

        #endregion
    }
}
