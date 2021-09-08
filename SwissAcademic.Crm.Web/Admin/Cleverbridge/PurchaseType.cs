using System;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    [Serializable]
    public partial class PurchaseType
    {

        private string statusField;

        private PurchaseStatusIdType statusIdField;

        private ContactType billingContactField;

        private ContactType deliveryContactField;

        private ContactType licenseeContactField;

        private PaymentInfoType paymentInfoField;

        private string customerReferenceNoField;

        private System.DateTime creationTimeField;

        private System.DateTime paymentArriveTimeField;

        private bool paymentArriveTimeFieldSpecified;

        private System.DateTime reimbursementTimeField;

        private bool reimbursementTimeFieldSpecified;

        private System.DateTime lastModificationTimeField;

        private System.DateTime quoteToPurchaseTimeField;

        private bool quoteToPurchaseTimeFieldSpecified;

        private string remoteAddressField;

        private string remoteHostField;

        private string httpUserAgentField;

        private string httpEntryUrlField;

        private string httpRefererField;

        private string httpAcceptLanguageField;

        private string partnerIdField;

        private string partnerUsernameField;

        private string internalCustomerField;

        private string internalPartnerField;

        private string customerConfirmationPageUrlField;

        private string customerPdfDocumentUrlField;

        private string merchantOfRecordField;

        private ErrorType errorField;

        private PurchaseItemListType itemsField;

        private PurchaseExtraParameterListType extraParametersField;

        private string idField;

        private string reimbursementIdField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Status
        {
            get
            {
                return statusField;
            }
            set
            {
                statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PurchaseStatusIdType StatusId
        {
            get
            {
                return statusIdField;
            }
            set
            {
                statusIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public ContactType BillingContact
        {
            get
            {
                return billingContactField;
            }
            set
            {
                billingContactField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public ContactType DeliveryContact
        {
            get
            {
                return deliveryContactField;
            }
            set
            {
                deliveryContactField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public ContactType LicenseeContact
        {
            get
            {
                return licenseeContactField;
            }
            set
            {
                licenseeContactField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PaymentInfoType PaymentInfo
        {
            get
            {
                return paymentInfoField;
            }
            set
            {
                paymentInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string CustomerReferenceNo
        {
            get
            {
                return customerReferenceNoField;
            }
            set
            {
                customerReferenceNoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public System.DateTime CreationTime
        {
            get
            {
                return creationTimeField;
            }
            set
            {
                creationTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public System.DateTime PaymentArriveTime
        {
            get
            {
                return paymentArriveTimeField;
            }
            set
            {
                paymentArriveTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PaymentArriveTimeSpecified
        {
            get
            {
                return paymentArriveTimeFieldSpecified;
            }
            set
            {
                paymentArriveTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public System.DateTime ReimbursementTime
        {
            get
            {
                return reimbursementTimeField;
            }
            set
            {
                reimbursementTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ReimbursementTimeSpecified
        {
            get
            {
                return reimbursementTimeFieldSpecified;
            }
            set
            {
                reimbursementTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public System.DateTime LastModificationTime
        {
            get
            {
                return lastModificationTimeField;
            }
            set
            {
                lastModificationTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public System.DateTime QuoteToPurchaseTime
        {
            get
            {
                return quoteToPurchaseTimeField;
            }
            set
            {
                quoteToPurchaseTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QuoteToPurchaseTimeSpecified
        {
            get
            {
                return quoteToPurchaseTimeFieldSpecified;
            }
            set
            {
                quoteToPurchaseTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string RemoteAddress
        {
            get
            {
                return remoteAddressField;
            }
            set
            {
                remoteAddressField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string RemoteHost
        {
            get
            {
                return remoteHostField;
            }
            set
            {
                remoteHostField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string HttpUserAgent
        {
            get
            {
                return httpUserAgentField;
            }
            set
            {
                httpUserAgentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string HttpEntryUrl
        {
            get
            {
                return httpEntryUrlField;
            }
            set
            {
                httpEntryUrlField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string HttpReferer
        {
            get
            {
                return httpRefererField;
            }
            set
            {
                httpRefererField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string HttpAcceptLanguage
        {
            get
            {
                return httpAcceptLanguageField;
            }
            set
            {
                httpAcceptLanguageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string PartnerId
        {
            get
            {
                return partnerIdField;
            }
            set
            {
                partnerIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string PartnerUsername
        {
            get
            {
                return partnerUsernameField;
            }
            set
            {
                partnerUsernameField = value;
            }
        }

        /// <summary>
        /// Entspricht SubscriptionRevenueCategoryId-Enum
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string SubscriptionRevenueCategoryId
        {
            get;
            set;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string InternalCustomer
        {
            get
            {
                return internalCustomerField;
            }
            set
            {
                internalCustomerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string InternalPartner
        {
            get
            {
                return internalPartnerField;
            }
            set
            {
                internalPartnerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string CustomerConfirmationPageUrl
        {
            get
            {
                return customerConfirmationPageUrlField;
            }
            set
            {
                customerConfirmationPageUrlField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string CustomerPdfDocumentUrl
        {
            get
            {
                return customerPdfDocumentUrlField;
            }
            set
            {
                customerPdfDocumentUrlField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string MerchantOfRecord
        {
            get
            {
                return merchantOfRecordField;
            }
            set
            {
                merchantOfRecordField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public ErrorType Error
        {
            get
            {
                return errorField;
            }
            set
            {
                errorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PurchaseItemListType Items
        {
            get
            {
                return itemsField;
            }
            set
            {
                itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public PurchaseExtraParameterListType ExtraParameters
        {
            get
            {
                return extraParametersField;
            }
            set
            {
                extraParametersField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "integer")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "integer")]
        public string ReimbursementId
        {
            get
            {
                return reimbursementIdField;
            }
            set
            {
                reimbursementIdField = value;
            }
        }


        public bool SubscriptNewsletter()
        {
            try
            {
                if (ExtraParameters == null)
                {
                    return false;
                }

                if(ExtraParameters.ExtraParameter == null)
				{
                    return false;
				}

                foreach (var para in ExtraParameters.ExtraParameter)
                {
                    if (para.Key.ToLowerInvariant() == "x-newsletter" &&
                        para.Value.ToLowerInvariant() == "yes")
                    {
                        return true;
                    }
                }
            }
            catch
            {

            }
            return false;
        }
    }
}
