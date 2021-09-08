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
    public class IncreaseSubscriptionItemQuantityRequest
    {
        /// <summary>
        /// Define the behavior when adding or updating a subscription item
        /// </summary>
        public AlignmentSettings AlignmentSettings
        {
            get;
            set;
        }

        /// <summary>
        /// Set to false if you want to suppress the automatically generated email that informs the customer about the subscription update.
        /// Important: If set to false, you must send the email yourself.
        /// </summary>
        public bool GenerateMail
        {
            get;
            set;
        }

        /// <summary>
        /// Number of additional items. For example 2, if you want to increase the quantity from 1 to 3.
        /// </summary>
        public int Quantity
        {
            get;
            set;
        }

        /// <summary>
        /// Running number of the item in the subscription.
        /// </summary>
        public int RunningNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Unique ID of the subscription, with or without an initial 'S'
        /// </summary>
        public string SubscriptionId
        {
            get;
            set;
        }
    }
}
