namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class PurchaseItemBinKeyDeliveryType : PurchaseItemDeliveryBaseType
    {

        private byte[] binKeyField;

        private string filenameField;

        private string keyField;

        private string keyRawField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] BinKey
        {
            get
            {
                return this.binKeyField;
            }
            set
            {
                this.binKeyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Filename
        {
            get
            {
                return this.filenameField;
            }
            set
            {
                this.filenameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string KeyRaw
        {
            get
            {
                return this.keyRawField;
            }
            set
            {
                this.keyRawField = value;
            }
        }
    }
}