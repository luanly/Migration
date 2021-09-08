namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class PurchaseItemTextKeyDeliveryType : PurchaseItemDeliveryBaseType
    {

        private string keyField;

        private string keyRawField;

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