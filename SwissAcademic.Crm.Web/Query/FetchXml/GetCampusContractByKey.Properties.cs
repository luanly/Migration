namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetCampusContractByKey
    {
        #region Constructors

        public GetCampusContractByKey(string campusContractKey)
        {
            CampusContractKey = campusContractKey;
        }

        #endregion

        #region Properties

        public string CampusContractKey { get; private set; }

        #endregion
    }
}
