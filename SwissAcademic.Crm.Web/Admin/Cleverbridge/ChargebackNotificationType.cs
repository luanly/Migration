using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{

    /// <summary>
    /// Chargeback: 
    /// cleverbridge received a notification from our credit card processor that the customer is disputing the purchase.
    /// </summary>
    [Serializable]

    [XmlRoot("ChargebackNotification", IsNullable = false)]
    public partial class ChargebackNotificationType
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