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
    public class SubscriptionItemPurchaseItemInfo
    {
        /// <summary>
        /// Unique ID of the purchase.
        /// </summary>
        public int PurchaseId
        {
            get;
            set;
        }

        /// <summary>
        /// Running number of item in the purchase.
        /// </summary>
        public int PurchaseItemRunningNo
        {
            get;
            set;
        }

        /// <summary>
        /// Interval number of subscription at the time of the purchase.
        /// </summary>
        public int SubscriptionIntervalNo
        {
            get;
            set;
        }
    }
}
