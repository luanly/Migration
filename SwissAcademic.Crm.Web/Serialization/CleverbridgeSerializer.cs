using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    internal static class CleverbridgeSerializer
    {
        #region Felder

        static Dictionary<string, XmlSerializer> _serializers = new Dictionary<string, XmlSerializer>
        {
            { "cbn:AwaitingNotification", new XmlSerializer(typeof(AwaitingNotificationType)) },

            { "cbn:CanceledNotification", new XmlSerializer(typeof(CanceledNotificationType)) },
            { "cbn:ChargebackLetterNotification", new XmlSerializer(typeof(ChargebackLetterNotificationType)) },
            { "cbn:ChargebackNotification", new XmlSerializer(typeof(ChargebackNotificationType)) },
            { "cbn:CustomerDataChangedNotification", new XmlSerializer(typeof(CustomerDataChangedNotificationType)) },
            { "cbn:CustomerContactDataChangedNotification", new XmlSerializer(typeof(CustomerContactDataChangedNotificationType)) },
            { "cbn:CustomerProfileUpdateNotification", new XmlSerializer(typeof(CustomerProfileUpdateNotificationType)) },

            { "cbn:DeliveryReminderNotification", new XmlSerializer(typeof(DeliveryReminderNotificationType)) },

            { "cbn:ErrorNotification", new XmlSerializer(typeof(ErrorNotificationType)) },

            { "cbn:HoldNotification", new XmlSerializer(typeof(HoldNotificationType)) },

            { "cbn:NewOfflinePaymentPurchaseNotification", new XmlSerializer(typeof(NewOfflinePaymentPurchaseNotificationType)) },
            { "cbn:NewPurchaseOrderNotification", new XmlSerializer(typeof(NewPurchaseOrderNotificationType)) },
            { "cbn:NewPartnerSignup", new XmlSerializer(typeof(NewPartnerSignupType)) },
            { "cbn:NewAffiliateSignup", new XmlSerializer(typeof(NewAffiliateSignupType)) },
            { "cbn:NotificationList", new XmlSerializer(typeof(NotificationListType)) },

            { "cbn:OnlinePaymentDeclined", new XmlSerializer(typeof(OnlinePaymentDeclinedType)) },
            { "cbn:OtherNotification", new XmlSerializer(typeof(OtherNotificationType)) },

            { "cbn:PaidOrderNotification", new XmlSerializer(typeof(PaidOrderNotificationType)) },
            { "cbn:PartialRefundNotification", new XmlSerializer(typeof(PartialRefundNotificationType)) },
            { "cbn:PendingNotification", new XmlSerializer(typeof(PendingNotificationType)) },

            { "cbn:QuoteNotification", new XmlSerializer(typeof(QuoteNotificationType)) },

            { "cbn:RecurringBillingReinstatedNotification", new XmlSerializer(typeof(RecurringBillingReinstatedNotificationType)) },
            { "cbn:RefundNotification", new XmlSerializer(typeof(RefundNotificationType)) },
            { "cbn:RefundRequestNotification", new XmlSerializer(typeof(RefundRequestNotificationType)) },
            { "cbn:ReturnDirectDebitNotification", new XmlSerializer(typeof(ReturnDirectDebitNotificationType)) },
            { "cbn:RecurringBillingCanceledNotification", new XmlSerializer(typeof(RecurringBillingCanceledNotificationType)) },
            { "cbn:RecurringBillingOnHoldNotification", new XmlSerializer(typeof(RecurringBillingOnHoldNotificationType)) },
            { "cbn:RecurringBillingGracePeriodNotification", new XmlSerializer(typeof(RecurringBillingGracePeriodNotificationType)) },
            { "cbn:RegistrationNotification", new XmlSerializer(typeof(RegistrationNotificationType)) },
            { "cbn:RejectedNotification", new XmlSerializer(typeof(RejectedNotificationType)) },

            { "cbn:SubscriptionReminderPayOptExpNotification", new XmlSerializer(typeof(SubscriptionReminderPayOptExpNotificationType)) },
            { "cbn:SubscriptionReminderOfflinePayNotification", new XmlSerializer(typeof(SubscriptionReminderOfflinePayNotificationType)) },
            { "cbn:SubscriptionReminderChargeNotification", new XmlSerializer(typeof(SubscriptionReminderChargeNotificationType)) },
        };

        #endregion

        #region Methoden

        #region Deserialize

        internal static object Deserialize(string data)
        {
            using (var stringReader = new StringReader(data))
            {
                using (var reader = new CleverbridgeXmlTextReader(stringReader))
                {
                    reader.MoveToContent();
                    var r = _serializers[reader.Name].Deserialize(reader);
                    return r;
                }
            }
        }

        internal static T Deserialize<T>(string data)
            where T : class
        {
            using (var stringReader = new StringReader(data))
            {
                using (var reader = new CleverbridgeXmlTextReader(stringReader))
                {
                    reader.MoveToContent();
                    return (T)_serializers[reader.Name].Deserialize(reader);
                }
            }
        }

        #endregion

        #endregion
    }

    public class CleverbridgeXmlTextReader : XmlTextReader
    {
        public CleverbridgeXmlTextReader(System.IO.TextReader reader) : base(reader) { }

        public override string NamespaceURI => "";
    }
}
