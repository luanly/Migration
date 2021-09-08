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
    public class CustomerPrice
    {
        /// <summary>
        /// Currency code in capital letters, see ISO 4217.
        /// https://en.wikipedia.org/wiki/ISO_4217
        /// </summary>
        public string CurrencyId
        {
            get;
            set;
        }

        /// <summary>
        /// Set to true if taxes should be included in the item price.
        /// Set to false if taxes should be added on top of the item price.
        /// For more information, see Using the IsGross Subparameter.
        /// https://docs.cleverbridge.com/public/all/integrating-your-system/getting-started-with-subscriptions.htm#IsGross?utm_source=developer-docs&utm_medium=api-reference
        /// </summary>
        public bool IsGross
        {
            get;
            set;
        }

        /// <summary>
        /// New price to be paid by the customer.
        /// </summary>
        public decimal Value
        {
            get;
            set;
        }
    }
}
