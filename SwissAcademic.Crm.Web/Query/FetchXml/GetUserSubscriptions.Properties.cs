
using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetUserSubscriptions
    {
        #region Constructors

        public GetUserSubscriptions(Guid contactId)
        {
            ContactId = contactId.ToString();
        }

        #endregion

        #region Properties

        public string ContactId { get; private set; }

        #endregion
    }
}
