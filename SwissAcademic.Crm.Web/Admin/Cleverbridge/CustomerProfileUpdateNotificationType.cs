using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Subscription profile data changed: 
    /// A customer has changed his profile data for a subscription, for example the payment details.
    /// </summary>
    [Serializable]

    [XmlRoot("CustomerProfileUpdateNotification", IsNullable = false)]
    public partial class CustomerProfileUpdateNotificationType
    {
        private System.DateTime notificationDateField;
        private CustomerType customerField;

        [System.Xml.Serialization.XmlElementAttribute()]
        public System.DateTime NotificationDate
        {
            get
            {
                return notificationDateField;
            }
            set
            {
                notificationDateField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute()]
        public CustomerType Customer
        {
            get
            {
                return customerField;
            }
            set
            {
                customerField = value;
            }
        }
    }
}