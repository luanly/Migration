using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetIPRanges
    {
        #region Constructors

        public GetIPRanges()
        {
            ContractDuration = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"); ;
        }

        #endregion

        #region Properties

        public string ContractDuration { get; private set; }

        #endregion
    }
}
