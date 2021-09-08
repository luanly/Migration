namespace SwissAcademic.Crm.Web.Cleverbridge
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class AffiliateType
    {

        private string statusField;

        private AffiliateStatusIdType statusIdField;

        private string partnergroupField;

        private ContactType contactField;

        private AffiliateParameterListType extraParametersField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public AffiliateStatusIdType StatusId
        {
            get
            {
                return this.statusIdField;
            }
            set
            {
                this.statusIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Partnergroup
        {
            get
            {
                return this.partnergroupField;
            }
            set
            {
                this.partnergroupField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public ContactType Contact
        {
            get
            {
                return this.contactField;
            }
            set
            {
                this.contactField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public AffiliateParameterListType ExtraParameters
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