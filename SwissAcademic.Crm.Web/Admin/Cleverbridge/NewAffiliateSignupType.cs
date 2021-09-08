using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// New affiliate: 
    /// A new affiliate has signed up through the cleverbridge platform and has to be reviewed prior to being accepted.
    /// </summary>
    [Serializable]

    [XmlRoot("NewAffiliateSignup", IsNullable = false)]
    public partial class NewAffiliateSignupType
        :
        AffiliateNotificationType
    {
    }
}