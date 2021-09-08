namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetCampusContractLicensesByOrgaName
    {
        #region Constructors

        public GetCampusContractLicensesByOrgaName(string accountKey, string licenseOrgaName)
        {
            AccountKey = accountKey;
            LicenseOrgaName = licenseOrgaName;
        }

        #endregion

        #region Properties

        public string AccountKey { get; }
        public string LicenseOrgaName { get; }

        #endregion
    }
}
