namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class PurchaseItemPriceType
    {

        private decimal netPriceField;

        private decimal vatPriceField;

        private decimal grossPriceField;

        private decimal vatPercentageField;

        private bool vatPercentageFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal NetPrice
        {
            get
            {
                return this.netPriceField;
            }
            set
            {
                this.netPriceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal VatPrice
        {
            get
            {
                return this.vatPriceField;
            }
            set
            {
                this.vatPriceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal GrossPrice
        {
            get
            {
                return this.grossPriceField;
            }
            set
            {
                this.grossPriceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal VatPercentage
        {
            get
            {
                return this.vatPercentageField;
            }
            set
            {
                this.vatPercentageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool VatPercentageSpecified
        {
            get
            {
                return this.vatPercentageFieldSpecified;
            }
            set
            {
                this.vatPercentageFieldSpecified = value;
            }
        }
    }
}