using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Awaiting release: An order has been placed that needs to be accepted by you before it can be processed.
    /// </summary>
    [Serializable]

    [XmlRoot("AwaitingNotification", IsNullable = false)]
    public partial class AwaitingNotificationType
        :
        NotificationType
    {
    }
}