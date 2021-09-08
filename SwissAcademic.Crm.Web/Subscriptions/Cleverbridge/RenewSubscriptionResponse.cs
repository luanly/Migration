using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(IsNullable = false)]
    public class RenewSubscriptionResponse
    {
        public bool Success
        {
            get;
            set;
        }

        /// <summary>
        /// ResultMessage
        /// </summary>
        public string ResultMessage
        {
            get;
            set;
        }

        /// <summary>
        /// URL of web page with transaction details or payment instructions.
        /// </summary>
        public string ContinueUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Next billing date according to RFC3339. Specified in UTC
        /// </summary>
        public DateTime NextBillingDate
        {
            get;
            set;
        }

        /// <summary>
        /// Status of the renewal transaction. Possible values are Success, Error, Rejected, Pending, Unknown.
        /// </summary>
        public RenewSubscriptionTransactionStatus TransactionStatus
        {
            get;
            set;
        }
    }
}
