namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class PurchaseItemDeliveryListType
    {

        private PurchaseItemDeliveryBaseType[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BinKey", typeof(PurchaseItemBinKeyDeliveryType))]
        [System.Xml.Serialization.XmlElementAttribute("Download", typeof(PurchaseItemDownloadDeliveryType))]
        [System.Xml.Serialization.XmlElementAttribute("Key", typeof(PurchaseItemTextKeyDeliveryType))]
        [System.Xml.Serialization.XmlElementAttribute("Physical", typeof(PurchaseItemPhysicalDeliveryType))]
        [System.Xml.Serialization.XmlElementAttribute("Service", typeof(PurchaseItemServiceDeliveryType))]
        public PurchaseItemDeliveryBaseType[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }
    }
}