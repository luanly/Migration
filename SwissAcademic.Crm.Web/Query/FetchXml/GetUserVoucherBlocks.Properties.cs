
using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetUserVoucherBlocks
    {
        #region Constructors

        public GetUserVoucherBlocks(Guid contactId)
        {
            ContactId = contactId.ToString();
        }

        #endregion

        #region Properties

        public string ContactId { get; private set; }

        #endregion
    }
}
