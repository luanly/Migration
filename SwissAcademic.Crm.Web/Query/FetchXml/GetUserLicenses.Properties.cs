
using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetUserLicenses
    {
        #region Constructors

        public GetUserLicenses(Guid contactId)
        {
            ContactId = contactId.ToString();
        }

        #endregion

        #region Properties

        public string ContactId { get; private set; }

        #endregion
    }
}
