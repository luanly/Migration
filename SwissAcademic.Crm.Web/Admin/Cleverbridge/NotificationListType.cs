namespace SwissAcademic.Crm.Web.Cleverbridge
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://xml.cleverbridge.com/3.10.0.0/cleverbridgeNotification.xsd")]
    [System.Xml.Serialization.XmlRootAttribute("NotificationList", IsNullable = false)]
    public partial class NotificationListType
    {

        private object[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AwaitingNotification", typeof(AwaitingNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("CanceledNotification", typeof(CanceledNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("ChargebackLetterNotification", typeof(ChargebackLetterNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("ChargebackNotification", typeof(ChargebackNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("CustomerContactDataChangedNotification", typeof(CustomerContactDataChangedNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("CustomerDataChangedNotification", typeof(CustomerDataChangedNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("CustomerProfileUpdateNotification", typeof(CustomerProfileUpdateNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("DeliveryReminderNotification", typeof(DeliveryReminderNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("ErrorNotification", typeof(ErrorNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("HoldNotification", typeof(HoldNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("NewAffiliateSignup", typeof(NewAffiliateSignupType))]
        [System.Xml.Serialization.XmlElementAttribute("NewOfflinePaymentPurchaseNotification", typeof(NewOfflinePaymentPurchaseNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("NewPartnerSignup", typeof(NewPartnerSignupType))]
        [System.Xml.Serialization.XmlElementAttribute("NewPurchaseOrderNotification", typeof(NewPurchaseOrderNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("OnlinePaymentDeclined", typeof(OnlinePaymentDeclinedType))]
        [System.Xml.Serialization.XmlElementAttribute("OtherNotification", typeof(OtherNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("PaidOrderNotification", typeof(PaidOrderNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("PartialRefundNotification", typeof(PartialRefundNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("PendingNotification", typeof(PendingNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("QuoteNotification", typeof(QuoteNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("RecurringBillingCanceledNotification", typeof(RecurringBillingCanceledNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("RecurringBillingGracePeriodNotification", typeof(RecurringBillingGracePeriodNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("RecurringBillingOnHoldNotification", typeof(RecurringBillingOnHoldNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("RecurringBillingReinstatedNotification", typeof(RecurringBillingReinstatedNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("RefundNotification", typeof(RefundNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("RefundRequestNotification", typeof(RefundRequestNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("RegistrationNotification", typeof(RegistrationNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("RejectedNotification", typeof(RejectedNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("ReturnDirectDebitNotification", typeof(ReturnDirectDebitNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("SubscriptionReminderChargeNotification", typeof(SubscriptionReminderChargeNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("SubscriptionReminderOfflinePayNotification", typeof(SubscriptionReminderOfflinePayNotificationType))]
        [System.Xml.Serialization.XmlElementAttribute("SubscriptionReminderPayOptExpNotification", typeof(SubscriptionReminderPayOptExpNotificationType))]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }
    }


}