using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// New partner: 
    /// A new partner has signed up through the cleverbridge platform and has to be reviewed prior to being accepted.
    /// </summary>
    [Serializable]

    [XmlRoot("NewPartnerSignup", IsNullable = false)]
    public partial class NewPartnerSignupType
        :
        PartnerNotificationType
    {
    }
}