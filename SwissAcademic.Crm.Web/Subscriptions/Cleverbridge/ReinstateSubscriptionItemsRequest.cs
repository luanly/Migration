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
    public class ReinstateSubscriptionItemsRequest
    {
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
        /// Running numbers of the subscription items you want to reinstate.
        /// </summary>
        [XmlArrayItem("Item", IsNullable = false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Ausstehend>")]
        public int[] Items
        {
            get;
            set;
        }

        /// <summary>
        /// Unique ID of the subscription, with or without an initial 'S'.
        /// </summary>
        public string SubscriptionId
        {
            get;
            set;
        }
    }
}
