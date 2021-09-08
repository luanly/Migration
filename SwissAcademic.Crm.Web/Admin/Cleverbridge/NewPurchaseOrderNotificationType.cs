using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Purchase order
    /// A customer has placed a new order using a purchase order. This order is fulfilled prior to payment being received.
    /// </summary>
    [Serializable]

    [XmlRoot("NewPurchaseOrderNotification", IsNullable = false)]
    public partial class NewPurchaseOrderNotificationType
        :
        NotificationType
    {

    }
}