using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Refunded: 
    /// A full refund has been issued. The total gross price of the order has been refunded back to the customer
    /// </summary>
    [Serializable]

    [XmlRoot("RefundNotification", IsNullable = false)]
    public partial class RefundNotificationType
        :
        ReimbursementNotificationType
    {
        private string remarkField;

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