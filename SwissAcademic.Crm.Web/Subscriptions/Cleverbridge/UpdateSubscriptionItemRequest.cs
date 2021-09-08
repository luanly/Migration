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
    [XmlRoot(IsNullable = false)]
    public class UpdateSubscriptionItemRequest
    {
        /// <summary>
        /// Unique ID of the subscription, with or without an initial 'S'.
        /// </summary>
        public string SubscriptionId
        {
            get;
            set;
        }

        /// <summary>
        /// Running number of the item in the subscription.
        /// </summary>
        public int RunningNumber
        {
            get;
            set;
        }

        /// <summary>
        /// ID of the new product you want to replace the existing product with.
        /// </summary>
        public int ProductId
        {
            get;
            set;
        }

        /// <summary>
        /// Total number of items after the update.
        /// If you want to only increase the item quantity, use increasesubscriptionitemquantity instead.
        /// </summary>
        public int Quantity
        {
            get;
            set;
        }

        /// <summary>
        /// Define the behavior when adding or updating a subscription item.
        /// </summary>
        public AlignmentSettings AlignmentSettings
        {
            get;
            set;
        }

        /// <summary>
        /// Type of update. This is used for reporting only. It does not affect the subscription.
        /// </summary>
        public UpdateSubscriptionItemAction UpdateAction
        {
            get;
            set;
        }

        /// <summary>
        /// New product name to be used in customer communication.
        /// </summary>
        public string ProductName
        {
            get;
            set;
        }

        /// <summary>
        /// New English product name. Only used in internal communication and for reporting purposes.
        /// </summary>
        public string ProductNameEn
        {
            get;
            set;
        }

        /// <summary>
        /// New product description. Corresponds to the Additional name information in the Commerce Assistant.
        /// </summary>
        public string ProductNameExtension
        {
            get;
            set;
        }

        /// <summary>
        /// Coupon code for a promotion. 
        /// The customer will be given a discount on the next renewal or a future renewal, depending on the configuration of the coupon code.
        /// </summary>
        public string Couponcode
        {
            get;
            set;
        }

        /// <summary>
        /// ID of the 'recommendation set' to which the product belongs.
        /// Corresponds to the Recommendation ID in the Commerce Assistant. Used for reporting purposes only.
        /// </summary>
        public int RecommendationsetId
        {
            get;
            set;
        }

        /// <summary>
        /// Set to false if you want to suppress the automatically generated email that informs the customer about the subscription update.
        /// Important: If set to false, you must send the email yourself
        /// </summary>
        public bool GenerateMail
        {
            get;
            set;
        }

        /// <summary>
        /// Pricing information that the customer receives during the purchase process.
        /// </summary>
        public CustomerPrice CustomerPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Triggers a renewal immediately. As a result, it also extends the next billing cycle by remaining time of current cycle.
        /// You should use this with caution because it is not a standard action for subscription upgrades.
        /// Renewals are allowed between start of current billing interval and next billing date.
        /// </summary>
        public bool TriggerImmediateRenewal
        {
            get;
            set;
        }
    }
}
