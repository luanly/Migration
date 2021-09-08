using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{

    [Serializable]
    public partial class AffiliateParameterType
    {

        private string keyField;

        private string valueField;

        [System.Xml.Serialization.XmlElementAttribute()]
        public string Key
        {
            get
            {
                return keyField;
            }
            set
            {
                keyField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute()]
        public string Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }
}
