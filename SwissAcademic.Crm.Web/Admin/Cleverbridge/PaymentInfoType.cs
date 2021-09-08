namespace SwissAcademic.Crm.Web.Cleverbridge
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class PaymentInfoType
    {

        private string currencyField;

        private CurrencyIdType currencyIdField;

        private string paymentTypeField;

        private PaymenttypeIdType paymentTypeIdField;

        private bool isPurchaseOrderField;

        private string cardLastFourDigitsField;

        private PaymentInfoTypeCardExpirationDate cardExpirationDateField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Currency
        {
            get
            {
                return this.currencyField;
            }
            set
            {
                this.currencyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public CurrencyIdType CurrencyId
        {
            get
            {
                return this.currencyIdField;
            }
            set
            {
                this.currencyIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string PaymentType
        {
            get
            {
                return this.paymentTypeField;
            }
            set
            {
                this.paymentTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PaymenttypeIdType PaymentTypeId
        {
            get
            {
                return this.paymentTypeIdField;
            }
            set
            {
                this.paymentTypeIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public bool IsPurchaseOrder
        {
            get
            {
                return this.isPurchaseOrderField;
            }
            set
            {
                this.isPurchaseOrderField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string CardLastFourDigits
        {
            get
            {
                return this.cardLastFourDigitsField;
            }
            set
            {
                this.cardLastFourDigitsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PaymentInfoTypeCardExpirationDate CardExpirationDate
        {
            get
            {
                return this.cardExpirationDateField;
            }
            set
            {
                this.cardExpirationDateField = value;
            }
        }
    }
}
