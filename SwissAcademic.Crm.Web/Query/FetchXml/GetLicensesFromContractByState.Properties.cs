using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetLicensesFromContractByState
    {
        #region Constructors

        public GetLicensesFromContractByState(Guid contractId, int state)
        {
            ContractId = contractId.ToString();
            State = state.ToString();
        }

        #endregion

        #region Properties

        public string ContractId { get; private set; }
        public string State { get; private set; }

        #endregion
    }
}
