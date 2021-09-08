namespace SwissAcademic.Crm.Web.Cleverbridge
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class CustomerType
    {

        private string internalCustomerField;

        private ContactType billingContactField;

        private ContactType deliveryContactField;

        private ContactType licenseeContactField;

        private PaymentInfoType paymentInfoField;

        private PurchaseExtraParameterListType extraParametersField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string InternalCustomer
        {
            get
            {
                return this.internalCustomerField;
            }
            set
            {
                this.internalCustomerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public ContactType BillingContact
        {
            get
            {
                return this.billingContactField;
            }
            set
            {
                this.billingContactField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public ContactType DeliveryContact
        {
            get
            {
                return this.deliveryContactField;
            }
            set
            {
                this.deliveryContactField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public ContactType LicenseeContact
        {
            get
            {
                return this.licenseeContactField;
            }
            set
            {
                this.licenseeContactField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PaymentInfoType PaymentInfo
        {
            get
            {
                return this.paymentInfoField;
            }
            set
            {
                this.paymentInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PurchaseExtraParameterListType ExtraParameters
        {
            get
            {
                return this.extraParametersField;
            }
            set
            {
                this.extraParametersField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "integer")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }
}
