using System;
using System.Net;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class IPRangeExists
    {
        #region Constructors

        public IPRangeExists(string ipRange, string accountKey)
        {
            AccountKey = accountKey;
            IP = IPAddress.Parse(ipRange).ConvertToDecimal();
            ContractDuration = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"); ;
        }

        #endregion

        #region Properties

        public string AccountKey { get; set; }
        public decimal IP { get; set; }
        public string ContractDuration { get; private set; }

        #endregion
    }
}
