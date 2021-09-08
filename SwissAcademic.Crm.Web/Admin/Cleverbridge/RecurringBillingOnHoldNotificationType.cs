using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Subscription on hold: 
    /// The grace period is over and the subscription is still not paid.
    /// The customer must complete payment in the next few days or the subscription will be deactivated.
    /// </summary>
    [Serializable]

    [XmlRoot("RecurringBillingOnHoldNotification", IsNullable = false)]
    public partial class RecurringBillingOnHoldNotificationType
        :
        NotificationType
    {
    }
}