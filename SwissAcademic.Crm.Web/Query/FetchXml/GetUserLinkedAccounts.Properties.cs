
using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetUserLinkedEmailAccounts
    {
        #region Constructors

        public GetUserLinkedEmailAccounts(Guid contactId)
        {
            ContactId = contactId.ToString();
        }

        #endregion

        #region Properties

        public string ContactId { get; private set; }

        #endregion
    }
}
