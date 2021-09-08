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
    public class AddSubscriptionItemRequest
    {
        /// <summary>
        /// ID of the affiliate that receives the commission.
        /// </summary>
        public int? AffiliateId
        {
            get;
            set;
        }

        /// <summary>
        /// Define the behavior when adding or updating a subscription item
        /// </summary>
        public AlignmentSettings AlignmentSettings
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
        /// Pricing information that the customer receives during the purchase process.
        /// </summary>
        public CustomerPrice CustomerPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Set to false if you want to suppress the automatically generated email that informs the customer about the subscription update.
        /// Important: If set to false, you must send the email yourself.
        /// </summary>
        public bool GenerateMail
        {
            get;
            set;
        }

        /// <summary>
        /// ID of the product you want to add to the subscription.
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
        /// English product name. Only used in internal communication and for reporting purposes.
        /// </summary>
        public string ProductNameEn
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
        /// Number of items. If you want to only increase the quantity of an existing subscription item, use /subscription/increasesubscriptionitemquantity instead.
        /// </summary>
        public int Quantity
        {
            get;
            set;
        }

        /// <summary>
        /// ID of the 'recommendation set' to which the product belongs.
        /// Corresponds to the 'Recommendation ID' in the Commerce Assistant. Used for reporting purposes only.
        /// </summary>
        public int? RecommendationsetId
        {
            get;
            set;
        }

        /// <summary>
        /// Unique ID of the subscription, with or without an initial 'S'.
        /// </summary>
        public string SubscriptionId
        {
            get;
            set;
        }
    }
}
