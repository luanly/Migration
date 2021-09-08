using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Payment declined
    /// A payment has been declined.
    /// </summary>
    [Serializable]

    [XmlRoot("OnlinePaymentDeclined", IsNullable = false)]
    public partial class OnlinePaymentDeclinedType
        :
        NotificationType
    {
    }
}