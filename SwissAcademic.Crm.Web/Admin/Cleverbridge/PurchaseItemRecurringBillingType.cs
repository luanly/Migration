namespace SwissAcademic.Crm.Web.Cleverbridge
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class PurchaseItemRecurringBillingType
    {

        private string originalPurchaseIdField;

        private string originalPurchaseItemRunningNoField;

        private string intervalNoField;

        private string recurrenceCountField;

        private string gracePeriodDaysField;

        private SubscriptionStatusType statusIdField;

        private string statusField;

        private SubscriptionItemStatusType itemStatusIdField;

        private string itemStatusField;

        private System.DateTime nextBillingDateField;

        private bool nextBillingDateFieldSpecified;

        private string cancellationUrlField;

        private string changePaymentSubscriptionUrlField;

        private ProfitCalculationType nextBillingProfitField;

        private string subscriptionIdField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string OriginalPurchaseId
        {
            get
            {
                return this.originalPurchaseIdField;
            }
            set
            {
                this.originalPurchaseIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string OriginalPurchaseItemRunningNo
        {
            get
            {
                return this.originalPurchaseItemRunningNoField;
            }
            set
            {
                this.originalPurchaseItemRunningNoField = value;
            }
        }


        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string IntervalNo
        {
            get
            {
                return this.intervalNoField;
            }
            set
            {
                this.intervalNoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string RecurrenceCount
        {
            get
            {
                return this.recurrenceCountField;
            }
            set
            {
                this.recurrenceCountField = value;
            }
        }

        [System.Xml.Serialization.XmlElement]
        public string SubscriptionEventtypeId
        {
            get;
            set;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string GracePeriodDays
        {
            get
            {
                return this.gracePeriodDaysField;
            }
            set
            {
                this.gracePeriodDaysField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public SubscriptionStatusType StatusId
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
        public SubscriptionItemStatusType ItemStatusId
        {
            get
            {
                return this.itemStatusIdField;
            }
            set
            {
                this.itemStatusIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string ItemStatus
        {
            get
            {
                return this.itemStatusField;
            }
            set
            {
                this.itemStatusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public System.DateTime NextBillingDate
        {
            get
            {
                return this.nextBillingDateField;
            }
            set
            {
                this.nextBillingDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NextBillingDateSpecified
        {
            get
            {
                return this.nextBillingDateFieldSpecified;
            }
            set
            {
                this.nextBillingDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string CancellationUrl
        {
            get
            {
                return this.cancellationUrlField;
            }
            set
            {
                this.cancellationUrlField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string ChangePaymentSubscriptionUrl
        {
            get
            {
                return this.changePaymentSubscriptionUrlField;
            }
            set
            {
                this.changePaymentSubscriptionUrlField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute()]
        public SubscriptionRenewalType RenewalType
        {
            get;
            set;
        }

        [System.Xml.Serialization.XmlElementAttribute()]
        public int IntervalLengthInMonths
        {
            get;
            set;
        }

        [System.Xml.Serialization.XmlElementAttribute()]
        public int IntervalLengthInDays
        {
            get;
            set;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public ProfitCalculationType NextBillingProfit
        {
            get
            {
                return this.nextBillingProfitField;
            }
            set
            {
                this.nextBillingProfitField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string SubscriptionId
        {
            get
            {
                return this.subscriptionIdField;
            }
            set
            {
                this.subscriptionIdField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string SubscriptionItemRunningNo
        {
            get;
            set;
        }
    }
}