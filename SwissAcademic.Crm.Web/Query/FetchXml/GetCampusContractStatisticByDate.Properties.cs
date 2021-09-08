using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetCampusContractStatisticByDate
    {
        #region Constructors

        public GetCampusContractStatisticByDate(string campusContractKey, DateTime dateTime)
        {
            CampusContractKey = campusContractKey;
            Date = dateTime.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
        }

        #endregion

        #region Properties

        public string CampusContractKey { get; }

        public string Date { get; }

        #endregion
    }
}
