using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Customer contact data changed:
    /// The customer's data has been updated for a specific order. New customer data is sent in this notification.
    /// </summary>
    [Serializable]

    [XmlRoot("CustomerContactDataChangedNotification", IsNullable = false)]
    public partial class CustomerContactDataChangedNotificationType
        :
        NotificationType
    {
    }
}