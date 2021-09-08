using System.Security;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class EmailExists
    {
        #region Constructors

        public EmailExists(string email)
        {
            Email = SecurityElement.Escape(email.RemoveNonStandardWhitespace());
        }

        #endregion

        #region Properties

        public string Email { get; set; }

        #endregion
    }
}
