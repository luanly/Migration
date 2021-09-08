using System.Collections.Generic;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetUserFullName
    {
        #region Constructors

        public GetUserFullName(IEnumerable<string> contactKeys)
        {
            ContactKeys = contactKeys;
        }

        #endregion

        #region Properties

        public IEnumerable<string> ContactKeys { get; set; }

        #endregion
    }
}
