namespace SwissAcademic.Crm.Web.Cleverbridge
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class PartnerType
    {

        private string usernameField;

        private string statusField;

        private PartnerStatusIdType statusIdField;

        private string typeField;

        private PartnerTypeIdType typeIdField;

        private string partnergroupField;

        private ContactType contactField;

        private PartnerParameterListType extraParametersField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Username
        {
            get
            {
                return this.usernameField;
            }
            set
            {
                this.usernameField = value;
            }
        }

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
        public PartnerStatusIdType StatusId
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
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PartnerTypeIdType TypeId
        {
            get
            {
                return this.typeIdField;
            }
            set
            {
                this.typeIdField = value;
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
        public PartnerParameterListType ExtraParameters
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
