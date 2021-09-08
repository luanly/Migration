using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Awaiting offline payment: A new order has been placed using an offline form of payment, such as PayPal or wire transfer.
    /// </summary>
    [Serializable]

    [XmlRoot("NewOfflinePaymentPurchaseNotification", IsNullable = false)]
    public partial class NewOfflinePaymentPurchaseNotificationType : NotificationType
    {
    }
}