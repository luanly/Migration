namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class PurchaseItemAdditionalListType
    {

        private PurchaseItemAdditionalListTypeAdditional[] additionalField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Additional")]
        public PurchaseItemAdditionalListTypeAdditional[] Additional
        {
            get
            {
                return this.additionalField;
            }
            set
            {
                this.additionalField = value;
            }
        }
    }
}