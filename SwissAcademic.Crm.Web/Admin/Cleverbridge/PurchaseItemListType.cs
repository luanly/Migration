namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class PurchaseItemListType
    {

        private PurchaseItemType[] itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Item")]
        public PurchaseItemType[] Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }
    }
}