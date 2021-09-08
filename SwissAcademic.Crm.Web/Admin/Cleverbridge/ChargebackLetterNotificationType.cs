namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://xml.cleverbridge.com/3.10.0.0/cleverbridgeNotification.xsd")]
    [System.Xml.Serialization.XmlRootAttribute("ChargebackLetterNotification", IsNullable = false)]
    public partial class ChargebackLetterNotificationType : ReimbursementNotificationType
    {

        private string remarkField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string Remark
        {
            get
            {
                return this.remarkField;
            }
            set
            {
                this.remarkField = value;
            }
        }
    }
}