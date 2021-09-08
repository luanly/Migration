namespace SwissAcademic.Crm.Web.Cleverbridge
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class PurchaseItemAdditionalType
    {

        private string questionField;

        private string questionIdField;

        private string answerField;

        private string answerIdField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Question
        {
            get
            {
                return this.questionField;
            }
            set
            {
                this.questionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string QuestionId
        {
            get
            {
                return this.questionIdField;
            }
            set
            {
                this.questionIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Answer
        {
            get
            {
                return this.answerField;
            }
            set
            {
                this.answerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string AnswerId
        {
            get
            {
                return this.answerIdField;
            }
            set
            {
                this.answerIdField = value;
            }
        }
    }
}