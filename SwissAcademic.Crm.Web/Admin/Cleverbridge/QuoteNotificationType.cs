using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// New quote: 
    /// A customer has requested a price quote.
    /// </summary>
    [Serializable]

    [XmlRoot("QuoteNotification", IsNullable = false)]
    public partial class QuoteNotificationType
        :
        NotificationType
    {
    }
}