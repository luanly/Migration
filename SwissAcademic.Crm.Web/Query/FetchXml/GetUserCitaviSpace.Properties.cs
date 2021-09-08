
using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetUserCitaviSpace
    {
        #region Constructors

        public GetUserCitaviSpace(string contactKey)
        {
            ContactKey = contactKey;
            Today = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
        }

        #endregion

        #region Properties

        public string ContactKey { get; }
        public string Today { get; }

        #endregion
    }
}
