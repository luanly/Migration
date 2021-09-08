
using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetDeletedProjectByContactKey
    {
        #region Constructors

        public GetDeletedProjectByContactKey(string contactKey)
        {
            ContactKey = contactKey;
        }

        #endregion

        #region Properties

        public string ContactKey { get; private set; }

        #endregion
    }
}
