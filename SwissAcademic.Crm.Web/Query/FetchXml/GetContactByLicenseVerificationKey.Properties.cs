using System.Security;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetContactByLicenseVerificationKey
    {
        #region Constructors

        public GetContactByLicenseVerificationKey(string verificationKey)
        {
            VerificationKey = SecurityElement.Escape(verificationKey);

        }

        #endregion

        #region Properties

        public string VerificationKey { get; private set; }

        #endregion
    }
}
