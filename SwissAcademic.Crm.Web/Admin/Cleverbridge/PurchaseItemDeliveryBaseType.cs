namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PurchaseItemServiceDeliveryType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PurchaseItemPhysicalDeliveryType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PurchaseItemDownloadDeliveryType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PurchaseItemBinKeyDeliveryType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PurchaseItemTextKeyDeliveryType))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public abstract partial class PurchaseItemDeliveryBaseType
    {

        private string deliveryTypeField;

        private DeliveryTypeIdType deliveryTypeIdField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string DeliveryType
        {
            get
            {
                return this.deliveryTypeField;
            }
            set
            {
                this.deliveryTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public DeliveryTypeIdType DeliveryTypeId
        {
            get
            {
                return this.deliveryTypeIdField;
            }
            set
            {
                this.deliveryTypeIdField = value;
            }
        }
    }
}