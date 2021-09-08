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
    public class SubscriptionItemInfo
    {
        /// <summary>
        /// Coupon code applied to this version of the subscription item. 
        /// This indicates a discount was applied to the customer’s subscription at the initial purchase, on renewal of the subscription or via /subscription/updatesubscriptionitem
        /// </summary>
        public string Couponcode
        {
            get;
            set;
        }

        /// <summary>
        /// Date and time when an item is deactivated. Format according to RFC3339.
        /// </summary>
        public DateTime? DeactivationDate
        {
            get;
            set;
        }

        /// <summary>
        /// Date and time when item is deactivated or number of specified billing intervals is reached. Format according to RFC3339.
        /// </summary>
        public DateTime? EndDate
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether this subscription item is the latest version. 
        /// If true, the item is current. If false, the item is not current
        /// </summary>
        public bool IsCurrent
        {
            get;
            set;
        }

        /// <summary>
        /// Number of the last successfully paid interval of the subscription item.
        /// Counting starts at 0. For each successfully paid interval, the number is increased by one.
        /// </summary>
        public int LastIntervalNo
        {
            get;
            set;
        }

        /// <summary>
        /// ID of the product in the subscription.
        /// </summary>
        public int ProductId
        {
            get;
            set;
        }

        /// <summary>
        /// Product name used in customer communication.
        /// </summary>
        public string ProductName
        {
            get;
            set;
        }

        /// <summary>
        /// Product description. Corresponds to the Additional name information in the Commerce Assistant.
        /// </summary>
        public string ProductNameExtension
        {
            get;
            set;
        }

        /// <summary>
        /// Current number of items.
        /// </summary>
        public int Quantity
        {
            get;
            set;
        }

        /// <summary>
        /// Maximum number of billings that can occur for the item.
        /// </summary>
        public int? RecurrenceCount
        {
            get;
            set;
        }

        /// <summary>
        /// Running number of the item in the subscription.
        /// </summary>
        public string RunningNo
        {
            get;
            set;
        }

        /// <summary>
        /// Date and time when item is paid. 
        /// If a purchase order is used, this is the date and time when the item is processed for the first time. Format according to RFC3339.
        /// </summary>
        public DateTime? StartDate
        {
            get;
            set;
        }

        /// <summary>
        /// Status of subscription item
        /// </summary>
        public SubscriptionItemStatusType Status
        {
            get;
            set;
        }

        /// <summary>
        /// Unique ID of the subscription, with or without an initial 'S'.
        /// </summary>
        public int SubscriptionId
        {
            get;
            set;
        }


        /// <summary>
        /// List of transactions where the item can be found. For descriptions of the response items
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Ausstehend>")]
        public SubscriptionItemPurchaseItemInfo[] SubscriptionPurchaseItems
        {
            get;
            set;
        }

        /// <summary>
        /// ID of the promotion to which the coupon code belongs. Promotions are set up in the Commerce Assistant.
        /// </summary>
        public int? PromotionId
        {
            get;
            set;
        }

        /// <summary>
        /// Version of the item. If a subscription item is updated, for example using
        /// </summary>
        public int? Version
        {
            get;
            set;
        }
    }
}
