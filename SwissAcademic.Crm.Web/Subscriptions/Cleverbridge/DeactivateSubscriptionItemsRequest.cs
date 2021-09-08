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
    public class DeactivateSubscriptionItemsRequest
    {
        /// <summary>
        /// Set to true if you want to be able to reinstate items at a later date.
        /// If set to false, the item status changes to Finished. If set to false, GenerateMail must also be set to false
        /// </summary>
        public bool AllowReinstate
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
        /// Running numbers of the subscription items you want to deactivate.
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
