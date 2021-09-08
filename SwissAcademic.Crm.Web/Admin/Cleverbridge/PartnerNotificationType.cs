namespace SwissAcademic.Crm.Web.Cleverbridge
{

    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NewPartnerSignupType))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public abstract partial class PartnerNotificationType
    {

        private PartnerType partnerField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PartnerType Partner
        {
            get
            {
                return this.partnerField;
            }
            set
            {
                this.partnerField = value;
            }
        }
    }
}
