using Sustainsys.Saml2;
using System;
using System.Security;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetLinkedAccountStatistics
    {
        #region Constructors

        public GetLinkedAccountStatistics(string identityproviderid, string contactKeyStartsWith, DateTime lastLogin)
{
            IdentityProviderId = identityproviderid;
            ContactKeyStartsWith = contactKeyStartsWith;
            LastLogin = lastLogin.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
        }

        #endregion

        #region Properties

        public string IdentityProviderId { get; private set; }
        public string ContactKeyStartsWith { get; private set; }
        public string LastLogin { get; private set; }

        #endregion
    }
}
