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
    public partial class RenewSubscriptionRequest
    {
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
