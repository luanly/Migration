using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class CampusContractUserStatistic
    {
        #region Konstruktor

        internal CampusContractUserStatistic(CitaviLicense license)
         => Parse(license);

        #endregion

        #region Eigenschaften

        public string ContactEmailAddress { get; private set; }
        public string ContactKey { get; private set; }
        public int? ContactSoftBounceCounter { get; private set; }
        public DateTime? ContactLastLogin { get; private set; }
        public string LicenseProductKey { get; private set; }
        public string LicenseLicenseTypeKey { get; private set; }
        public string LicensePricingKey { get; private set; }
        public CampusGroup? LicenseCampusGroup { get; private set; }
        public DateTime LicenseCreatedOn { get; private set; }
        public bool LicenseIsVerifed { get; private set; }
        public OrderCategory? LicenseOrderCategory { get; private set; }

        #endregion

        #region Methoden

        #region Parse

        void Parse(CitaviLicense license)
        {
            LicenseCampusGroup = license.CampusGroup;
            LicenseIsVerifed = license.IsVerified;
            LicenseCreatedOn = license.CreatedOn;
            LicenseOrderCategory = license.OrderCategory;
            LicenseProductKey = license.DataContractProductKey;
            LicenseLicenseTypeKey = license.DataContractLicenseTypeKey;
            LicensePricingKey = license.DataContractPricingKey;

            ContactKey = license.DataContractEndUserContactKey;
            ContactEmailAddress = license.DataContractEndUserEmailAddress;
            ContactSoftBounceCounter = license.GetAliasedValue<int?>(CrmRelationshipNames.ContactEndUserLicense, CrmEntityNames.Contact, ContactPropertyId.SoftBounceCounter);
            ContactLastLogin = license.GetAliasedValue<DateTime?>(CrmRelationshipNames.ContactEndUserLicense, CrmEntityNames.Contact, ContactPropertyId.LastLogin);
        }

        #endregion

        #endregion
    }
}
