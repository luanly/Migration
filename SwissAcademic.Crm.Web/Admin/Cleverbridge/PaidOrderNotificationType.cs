
using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Paid: 
    /// A customer has placed an order and payment has been received by cleverbridge.
    /// This notification is sent when payment is received for online or offline payments.
    /// </summary>
    [Serializable]
    [XmlRoot("PaidOrderNotification", IsNullable = false)]
    public partial class PaidOrderNotificationType
        :
        NotificationType
    {

    }
}
