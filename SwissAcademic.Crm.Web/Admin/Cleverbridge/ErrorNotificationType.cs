using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Error: 
    /// An error occurred when trying to generate a license key for an order.
    /// </summary>
    [Serializable]

    [XmlRoot("ErrorNotification", IsNullable = false)]
    public partial class ErrorNotificationType
    {
        private PurchaseType purchaseField;
        private string errorTypeField;

        [System.Xml.Serialization.XmlElementAttribute()]
        public PurchaseType Purchase
        {
            get
            {
                return purchaseField;
            }
            set
            {
                purchaseField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute()]
        public string ErrorType
        {
            get
            {
                return errorTypeField;
            }
            set
            {
                errorTypeField = value;
            }
        }
    }
}