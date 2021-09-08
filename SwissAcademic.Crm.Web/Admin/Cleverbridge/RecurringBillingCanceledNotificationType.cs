using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Subscription deactivated:
    /// The customer's subscription has been canceled and he will not be rebilled at the next billing interval.
    /// This does not mean a refund has been issued for the current billing interval.
    /// </summary>
    [Serializable]

    [XmlRoot("RecurringBillingCanceledNotification", IsNullable = false)]
    public partial class RecurringBillingCanceledNotificationType
        :
        ReimbursementNotificationType
    {
    }
}