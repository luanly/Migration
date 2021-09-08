namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetEmailDomainUsers
    {
        #region Constructors

        public GetEmailDomainUsers(string emaildomain)
        {
            EmailDomain = emaildomain;
        }

        #endregion

        #region Properties

        public string EmailDomain { get; }

        #endregion
    }
}
