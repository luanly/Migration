using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]

    [XmlRoot("RegistrationNotification", IsNullable = false)]
    public partial class RegistrationNotificationType
        :
        NotificationType
    {
    }
}