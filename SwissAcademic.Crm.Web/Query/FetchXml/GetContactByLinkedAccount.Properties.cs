using System.Security;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetContactByLinkedAccount
    {
        #region Constructors

        public GetContactByLinkedAccount(string identityproviderid, string nameIdentifier)
        {
            NameIdentifier = SecurityElement.Escape(nameIdentifier);
            IdentityProviderId = SecurityElement.Escape(identityproviderid);
        }

        #endregion

        #region Properties

        public string NameIdentifier { get; private set; }
        public string IdentityProviderId { get; private set; }

        #endregion
    }
}
