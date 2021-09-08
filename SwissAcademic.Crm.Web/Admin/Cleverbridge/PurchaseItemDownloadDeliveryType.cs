namespace SwissAcademic.Crm.Web.Cleverbridge
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class PurchaseItemDownloadDeliveryType : PurchaseItemDeliveryBaseType
    {

        private string linkField;

        private System.DateTime expirationTimeField;

        private string filenameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Link
        {
            get
            {
                return this.linkField;
            }
            set
            {
                this.linkField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public System.DateTime ExpirationTime
        {
            get
            {
                return this.expirationTimeField;
            }
            set
            {
                this.expirationTimeField = value;
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
    }
}