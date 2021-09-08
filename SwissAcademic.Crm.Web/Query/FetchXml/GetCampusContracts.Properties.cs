using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetCampusContracts
    {
        #region Constructors

        public GetCampusContracts()
        {
            ContractDuration = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
        }

        #endregion

        #region Properties

        public string ContractDuration { get; private set; }

        #endregion
    }
}
