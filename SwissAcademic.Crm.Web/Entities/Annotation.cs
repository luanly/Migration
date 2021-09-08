using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.Annotation)]
    [DataContract]
    public class Annotation
        :
        CitaviCrmEntity
    {
        public Annotation()
            :
            base(CrmEntityNames.Annotation)
        {

        }

        /// <summary>
        ///  Account Note
        /// </summary>
        [CrmProperty(NoCache = true, IsBuiltInAttribute = true, PropertyName = "objectid_account@odata.bind")]
        public string AccountId
        {
            get
            {
                return GetValue<string>("objectid_account@odata.bind");
            }
            set
            {
                SetValue($"/accounts({value})", "objectid_account@odata.bind");
            }
        }

        /// <summary>
        /// Contact Note
        /// </summary>
        [CrmProperty(NoCache = true, IsBuiltInAttribute = true, PropertyName = "objectid_contact@odata.bind")]
        public string ContactId
        {
            get
            {
                return GetValue<string>("objectid_contact@odata.bind");
            }
            set
            {
                SetValue($"/contacts({value})", "objectid_contact@odata.bind");
            }
        }

        /// <summary>
        ///  Delivery Note
        /// </summary>
        [CrmProperty(NoCache = true, IsBuiltInAttribute = true, PropertyName = "objectid_new_delivery@odata.bind")]
        public string DeliveryId
        {
            get
            {
                return GetValue<string>("objectid_new_delivery@odata.bind");
            }
            set
            {
                SetValue($"/new_deliveries({value})", "objectid_new_delivery@odata.bind");
            }
        }

        /// <summary>
        ///  EmailDomain Note
        /// </summary>
        [CrmProperty(NoCache = true, IsBuiltInAttribute = true, PropertyName = "objectid_new_emaildomainorcampusname@odata.bind")]
        public string EmaildomainId
        {
            get
            {
                return GetValue<string>("objectid_new_emaildomainorcampusname@odata.bind");
            }
            set
            {
                SetValue($"/new_emaildomainorcampusnames({value})", "objectid_new_emaildomainorcampusname@odata.bind");
            }
        }


        /// <summary>
        ///  CampusContractRenewal Note
        /// </summary>
        [CrmProperty(NoCache = true, IsBuiltInAttribute = true, PropertyName = "objectid_new_ccoldnewcontract@odata.bind")]
        public string CampusContractRenewalId
        {
            get
            {
                return GetValue<string>("objectid_new_ccoldnewcontract@odata.bind");
            }
            set
            {
                SetValue($"/new_ccoldnewcontracts({value})", "objectid_new_ccoldnewcontract@odata.bind");
            }
        }

        /// <summary>
        ///  CampusContractRenewal Note
        /// </summary>
        [CrmProperty(NoCache = true, IsBuiltInAttribute = true, PropertyName = "objectid_new_citavilicense@odata.bind")]
        public string CitaviLicenseId
        {
            get
            {
                return GetValue<string>("objectid_new_citavilicense@odata.bind");
            }
            set
            {
                SetValue($"/new_citavilicenses({value})", "objectid_new_citavilicense@odata.bind");
            }
        }

       

        [CrmProperty(IsBuiltInAttribute = true)]
        public string NoteText
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        [CrmProperty(IsBuiltInAttribute = true)]
        public string Subject
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }
    }
}
