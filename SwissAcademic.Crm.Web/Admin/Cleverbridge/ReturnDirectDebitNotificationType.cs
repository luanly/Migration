using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    /// <summary>
    /// Returned direct debit:
    /// A return of a direct debit has been issued for the amount paid using the direct debit payment method.
    /// </summary>
    [Serializable]

    [XmlRoot("ReturnDirectDebitNotification", IsNullable = false)]
    public partial class ReturnDirectDebitNotificationType : ReimbursementNotificationType
    {
        private string remarkField;

        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string Remark
        {
            get
            {
                return remarkField;
            }
            set
            {
                remarkField = value;
            }
        }
    }
}