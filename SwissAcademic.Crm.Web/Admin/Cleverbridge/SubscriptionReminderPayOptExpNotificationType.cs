using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Subscription reminder payment expired: 
    /// The payment of the subscription of the product is due but the credit or debit card used in the previous transaction will expire before the payment due date.
    /// </summary>
    [Serializable]

    [XmlRoot("SubscriptionReminderPayOptExpNotification", IsNullable = false)]
    public partial class SubscriptionReminderPayOptExpNotificationType
        :
        NotificationType
    {
    }
}