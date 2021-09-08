using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    [XmlInclude(typeof(NewAffiliateSignupType))]
    [Serializable]
    public abstract partial class AffiliateNotificationType
    {
        private AffiliateType affiliateField;

        [System.Xml.Serialization.XmlElementAttribute()]
        public AffiliateType Affiliate
        {
            get
            {
                return affiliateField;
            }
            set
            {
                affiliateField = value;
            }
        }
    }
}
