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
    public partial class AlignmentSettings
    {
        /// <summary>
        /// Set to true if you want the customer to be charged immediately for the updated item (for example, a quantity increase).
        /// Otherwise, the customer will be charged at the next renewal.
        /// </summary>
        public bool AlignToCurrentInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Set to true if you want to charge a full interval for the updated item (for example, a quantity increase).
        /// Otherwise, the customer will be charged a pro-rated amount for the updated item that corresponds to the time between the update and the next renewal.
        /// Important: AlignToCurrentInterval must be set to true if ExtendInterval is set to true
        /// </summary>
        public bool ExtendInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Set to true if you only want to obtain preview data in the response.
        /// You can use this data to display a price preview to a customer or for testing purposes.
        /// </summary>
        public bool GetCustomerPricePreviewOnly
        {
            get;
            set;
        }
    }
}
