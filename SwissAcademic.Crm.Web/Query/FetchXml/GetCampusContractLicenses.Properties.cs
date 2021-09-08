namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetCampusContractLicenses
    {
        #region Constructors

        public GetCampusContractLicenses(string campusContractKey)
        {
            CampusContractKey = campusContractKey;
        }

        #endregion

        #region Properties

        public string CampusContractKey { get; }

        #endregion
    }
}
