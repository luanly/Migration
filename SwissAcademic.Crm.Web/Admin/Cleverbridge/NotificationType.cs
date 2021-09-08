namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomerContactDataChangedNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SubscriptionReminderChargeNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RecurringBillingReinstatedNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SubscriptionReminderOfflinePayNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SubscriptionReminderPayOptExpNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(OtherNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RejectedNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PendingNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(HoldNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CanceledNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AwaitingNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RegistrationNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RecurringBillingGracePeriodNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RecurringBillingOnHoldNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReimbursementNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RecurringBillingCanceledNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReturnDirectDebitNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ChargebackNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ChargebackLetterNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RefundNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PartialRefundNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RefundRequestNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomerDataChangedNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DeliveryReminderNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(OnlinePaymentDeclinedType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NewPurchaseOrderNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NewOfflinePaymentPurchaseNotificationType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PaidOrderNotificationType))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public abstract partial class NotificationType
    {

        private System.DateTime notificationDateField;

        private PurchaseType purchaseField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public System.DateTime NotificationDate
        {
            get
            {
                return this.notificationDateField;
            }
            set
            {
                this.notificationDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PurchaseType Purchase
        {
            get
            {
                return this.purchaseField;
            }
            set
            {
                this.purchaseField = value;
            }
        }
    }
}
