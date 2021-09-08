using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Subscription reminder charge: 
    /// A reminder that the subscription renewal is approaching for online payments
    /// </summary>
    [Serializable]

    [XmlRoot("SubscriptionReminderChargeNotification", IsNullable = false)]
    public partial class SubscriptionReminderChargeNotificationType
        :
        NotificationType
    {
    }
}