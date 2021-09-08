namespace SwissAcademic.Crm.Web.Cleverbridge
{

    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RecurringBillingCanceledNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReturnDirectDebitNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ChargebackNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ChargebackLetterNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RefundNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PartialRefundNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RefundRequestNotificationType))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public abstract partial class ReimbursementNotificationType : NotificationType
    {

        private string reimbursementTypeIdField;

        private string reimbursementReasonIdField;

        private System.Nullable<bool> terminateRecurringBillingField;

        private bool terminateRecurringBillingFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string ReimbursementTypeId
        {
            get
            {
                return this.reimbursementTypeIdField;
            }
            set
            {
                this.reimbursementTypeIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string ReimbursementReasonId
        {
            get
            {
                return this.reimbursementReasonIdField;
            }
            set
            {
                this.reimbursementReasonIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public System.Nullable<bool> TerminateRecurringBilling
        {
            get
            {
                return this.terminateRecurringBillingField;
            }
            set
            {
                this.terminateRecurringBillingField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TerminateRecurringBillingSpecified
        {
            get
            {
                return this.terminateRecurringBillingFieldSpecified;
            }
            set
            {
                this.terminateRecurringBillingFieldSpecified = value;
            }
        }
    }
}