using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// The billing date for a customer's subscription has been changed or extended.
    /// </summary>
    [Serializable]

    [XmlRoot("SubscriptionBillingDateExtendedNotification", IsNullable = false)]
    public partial class SubscriptionBillingDateExtendedNotificationType
        :
        NotificationType
    {
    }
}