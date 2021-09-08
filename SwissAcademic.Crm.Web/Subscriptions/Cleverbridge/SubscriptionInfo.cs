using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public class SubscriptionInfo
    {

        /// <summary>
        /// Currency code in capital letters, see ISO 4217. This was selected by the customer during the checkout process.
        /// </summary>
        public string CustomerCurrencyId
        {
            get;
            set;
        }

        /// <summary>
        /// cleverbridge's unique ID for a customer.
        /// </summary>
        public int? CustomerId
        {
            get;
            set;
        }

        /// <summary>
        /// Your unique ID for the customer. This value is submitted by you when the customer initially purchases the subscription and is stored by cleverbridge.
        /// Corresponds to the Internal customer ID in the Commerce Assistant.
        /// </summary>
        public string CustomerReferenceId
        {
            get;
            set;
        }

        /// <summary>
        /// Internal purchase order number submitted by the customer during checkout.
        /// </summary>
        public string CustomerReferenceNo
        {
            get;
            set;
        }

        /// <summary>
        /// Number of grace period days that the customer has before the subscription is put on hold.
        /// </summary>
        public DateTime? EndDate
        {
            get;
            set;
        }

        /// <summary>
        /// Number of grace period days that the customer has before the subscription is put on hold.
        /// </summary>
        public int? GracePeriodDays
        {
            get;
            set;
        }

        /// <summary>
        /// Unique ID of the subscription.
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Number of days between billing events. This is equivalent to the length of subsequent billing intervals.
        /// </summary>
        public int? IntervalDayCount
        {
            get;
            set;
        }

        /// <summary>
        /// Number of months between billing events. This is equivalent to the length of subsequent billing intervals.
        /// </summary>
        public int? IntervalMonthCount
        {
            get;
            set;
        }

        /// <summary>
        /// List of subscription items.
        /// </summary>
        [XmlArrayItem("SubscriptionItem")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Ausstehend>")]
        public SubscriptionItemInfo[] Items
        {
            get;
            set;
        }

        /// <summary>
        /// Number of the last successfully paid interval of the subscription.
        /// Counting starts at 0. For each successfully paid interval, the number is increased by one.
        /// </summary>
        public int? LastIntervalNo
        {
            get;
            set;
        }

        /// <summary>
        /// Next billing date and time according to RFC3339. Specified in UTC.
        /// </summary>
        public DateTime? NextBillingDate
        {
            get;
            set;
        }

        /// <summary>
        /// Date on which the reminder of the next billing date is sent to the customer.
        /// </summary>
        public DateTime? NextBillingDateReminder
        {
            get;
            set;
        }

        /// <summary>
        /// Renewal type of the subscription
        /// </summary>
        public SubscriptionRenewalType? RenewalType
        {
            get;
            set;
        }

        /// <summary>
        /// Date and time when subscription is paid.
        /// If a purchase order is used, this is the date when the subscription is processed for the first time.
        /// </summary>
        public DateTime? StartDate
        {
            get;
            set;
        }

        /// <summary>
        /// Number of days after initial purchase until the first billing event. 
        /// This is equivalent to the length of first interval of the subscription.
        /// </summary>
        public int? StartIntervalDayCount
        {
            get;
            set;
        }

        /// <summary>
        /// Number of months after initial purchase until the first billing event. 
        /// This is equivalent to the length of first interval of the subscription.
        /// </summary>
        public int? StartIntervalMonthCount
        {
            get;
            set;
        }

        /// <summary>
        /// Status of the subscription.
        /// </summary>
        public SubscriptionStatusType SubscriptionStatus
        {
            get;
            set;
        }
    }
}
