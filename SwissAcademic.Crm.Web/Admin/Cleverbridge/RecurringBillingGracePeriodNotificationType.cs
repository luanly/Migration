using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Subscription on grace: 
    /// The payment for a subscription is due but could not be processed with the current payment details.
    /// The customer must update the payment details to complete the renewal or the subscription will be deactivated.
    /// </summary>
    [Serializable]

    [XmlRoot("RecurringBillingGracePeriodNotification", IsNullable = false)]
    public partial class RecurringBillingGracePeriodNotificationType
        :
        NotificationType
    {

    }
}