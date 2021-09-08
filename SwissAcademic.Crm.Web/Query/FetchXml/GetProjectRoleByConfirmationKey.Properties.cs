
using System.Security;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetProjectRoleByConfirmationKey
    {
        #region Constructors

        public GetProjectRoleByConfirmationKey(string confirmationKey)
        {
            ConfirmationKey = SecurityElement.Escape(confirmationKey);
        }

        #endregion

        #region Properties

        public string ConfirmationKey { get; set; }

        #endregion
    }
}
