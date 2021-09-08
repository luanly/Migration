namespace SwissAcademic.Crm.Web.Cleverbridge
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class PurchaseExtraParameterListType
    {

        private PurchaseExtraParameterListTypeExtraParameter[] extraParameterField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ExtraParameter")]
        public PurchaseExtraParameterListTypeExtraParameter[] ExtraParameter
        {
            get
            {
                return this.extraParameterField;
            }
            set
            {
                this.extraParameterField = value;
            }
        }
    }
}