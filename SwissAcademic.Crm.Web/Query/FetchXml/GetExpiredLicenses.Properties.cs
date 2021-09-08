
using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetExpiredLicenses
    {
        #region Constructors

        public GetExpiredLicenses()
        {
            Today = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
        }

        #endregion

        #region Properties

        public string Today { get; }

        #endregion
    }
}
