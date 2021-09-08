using Newtonsoft.Json;
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
    public class SubscriptionRequestResponse
    {
        public string CitaviLicenseKey { get; internal set; }

        public bool Success
        {
            get;
            set;
        }

        /// <summary>
        /// ResultMessage
        /// </summary>
        public string ResultMessage
        {
            get;
            set;
        }

        /// <summary>
        /// Total gross price for alignment interval. Only for CustomerPricePreview-Mode
        /// </summary>
        public decimal AlignmentCustomerGrossPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Total VAT amount for alignment interval. Only for CustomerPricePreview-Mode.
        /// </summary>
        public decimal AlignmentCustomerVatPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Total net price for alignment interval. Only for CustomerPricePreview-Mode.
        /// </summary>
        public decimal AlignmentCustomerNetPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Total gross price for next billing interval. Only for CustomerPricePreview-Mode.
        /// </summary>
        public decimal NextBillingCustomerGrossPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Total VAT amount for next billing interval. Only for CustomerPricePreview-Mode.
        /// </summary>
        public decimal NextBillingCustomerVatPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Total net price next billing interval. Only for CustomerPricePreview-Mode.
        /// </summary>
        public decimal NextBillingCustomerNetPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Currency code in capital letters, see ISO 4217.
        /// </summary>
        public string PriceCurrencyId
        {
            get;
            set;
        }
		
	}
}
