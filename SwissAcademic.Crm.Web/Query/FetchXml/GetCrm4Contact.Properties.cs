using System.Security;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetCrm4Contact
    {
        #region Constructors

        public GetCrm4Contact(string email)
        {
            Email = SecurityElement.Escape(email.RemoveNonStandardWhitespace());
        }

        #endregion

        #region Properties

        public string Email { get; set; }

        #endregion
    }
}
