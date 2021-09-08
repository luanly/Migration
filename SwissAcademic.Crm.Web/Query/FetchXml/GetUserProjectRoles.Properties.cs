
using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetUserProjectRoles
    {
        #region Constructors

        public GetUserProjectRoles(Guid contactId)
        {
            ContactId = contactId.ToString();
        }

        #endregion

        #region Properties

        public string ContactId { get; private set; }

        #endregion
    }
}
