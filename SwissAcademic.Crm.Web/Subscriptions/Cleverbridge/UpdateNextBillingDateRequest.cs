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
    public partial class UpdateNextBillingDateRequest
    {
        public DateTime NextBillingDate
        {
            get;
            set;
        }
        public string SubscriptionId
        {
            get;
            set;
        }
    }
}
