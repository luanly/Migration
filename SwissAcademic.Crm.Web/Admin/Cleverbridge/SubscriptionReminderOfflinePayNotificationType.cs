using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Subscription reminder offline payment: 
    /// A reminder that a renewal is upcoming. This is sent for orders placed with offline payment methods.
    /// The default is seven days prior to renewal.
    /// </summary>
    [Serializable]

    [XmlRoot("SubscriptionReminderOfflinePayNotification", IsNullable = false)]
    public partial class SubscriptionReminderOfflinePayNotificationType : NotificationType
    {
    }
}