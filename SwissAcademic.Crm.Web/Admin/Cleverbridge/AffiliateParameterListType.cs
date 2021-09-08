using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    [Serializable]
    public partial class AffiliateParameterListType
    {
        private AffiliateParameterListTypeExtraParameter[] extraParameterField;

        [System.Xml.Serialization.XmlElementAttribute("ExtraParameter")]
        public AffiliateParameterListTypeExtraParameter[] ExtraParameter
        {
            get
            {
                return extraParameterField;
            }
            set
            {
                extraParameterField = value;
            }
        }
    }
}
