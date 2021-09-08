using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetCampusContractUserStatistic
    {
        #region Constructors

        public GetCampusContractUserStatistic(string campusContractKey)
        {
            CampusContractKey = campusContractKey;
        }

        #endregion

        #region Properties

        public string CampusContractKey { get;  }

        #endregion
    }
}
