using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Partially refunded (teilweise zurückerstattet):
    /// A partial refund has been issued. A portion of an order/service has been refunded back to the customer.
    /// </summary>
    [Serializable]

    [XmlRoot("PartialRefundNotification", IsNullable = false)]
    public partial class PartialRefundNotificationType
        :
        ReimbursementNotificationType
    {

        private PartialRefundTypeType partialRefundTypeField;

        private string remarkField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PartialRefundTypeType PartialRefundType
        {
            get
            {
                return partialRefundTypeField;
            }
            set
            {
                partialRefundTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string Remark
        {
            get
            {
                return remarkField;
            }
            set
            {
                remarkField = value;
            }
        }
    }
}