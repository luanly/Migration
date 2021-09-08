using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web.Cleverbridge
{

    [SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(IsNullable = false)]
    public partial class SubscriptionsForCustomerResponse
    {

        private string resultMessageField;

        private List<SubscriptionInfo> subscriptionField;

        /// <remarks/>
        public string ResultMessage
        {
            get
            {
                return resultMessageField;
            }
            set
            {
                resultMessageField = value;
            }
        }


        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Ausstehend>")]
        public List<SubscriptionInfo> Subscriptions
        {
            get
            {
                return subscriptionField;
            }
            set
            {
                subscriptionField = value;
            }
        }
    }
}
