using System;
using System.Collections.Generic;
using System.Linq;

namespace SwissAcademic.Crm.Web
{
    public static class EntityPropertySets
    {
        #region Eigenschaften

        #region Account

        public static readonly Enum[] Account = Enum.GetValues(typeof(AccountPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region BulkMailQuery

        public static readonly Enum[] BulkMailQuery = Enum.GetValues(typeof(BulkMailQueryPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region BulkMailTemplate

        public static readonly Enum[] BulkMailTemplate = Enum.GetValues(typeof(BulkMailTemplatePropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region CitaviCampaign

        public static readonly Enum[] CitaviCampaign = Enum.GetValues(typeof(CitaviCampaignPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region AutoNumberSequence

        public static readonly Enum[] AutoNumberSequence = Enum.GetValues(typeof(AutoNumberSequencePropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region CampusContract

        public static readonly Enum[] CampusContract = Enum.GetValues(typeof(CampusContractPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region CampusContractRenewal

        public static readonly Enum[] CampusContractRenewal = Enum.GetValues(typeof(CampusContractRenewalPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region CampusContractStatistic

        public static readonly Enum[] CampusContractStatistic = Enum.GetValues(typeof(CampusContractStatisticPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region CleverbridgeProduct

        public static readonly Enum[] CleverbridgeProduct = Enum.GetValues(typeof(CleverbridgeProductPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region Contact

        public static readonly Enum[] Contact = Enum.GetValues(typeof(ContactPropertyId)).Cast<Enum>().ToArray();

        internal static readonly IEnumerable<string> ContactBreeze = new Enum[]{
                                                                ContactPropertyId.Address1_StateOrProvince,
                                                                ContactPropertyId.Address1_Fax,
                                                                ContactPropertyId.Address1_City,
                                                                ContactPropertyId.Address1_Country,
                                                                ContactPropertyId.Address1_Telephone1,
                                                                ContactPropertyId.Address1_Telephone2,
                                                                ContactPropertyId.Address1_Line1,
                                                                ContactPropertyId.Address1_Line2,
                                                                ContactPropertyId.Address1_PostalCode,
                                                                ContactPropertyId.Firm,
                                                                ContactPropertyId.FirstName,
                                                                ContactPropertyId.GenderCode,
                                                                ContactPropertyId.LastName,
                                                                ContactPropertyId.Language,
                                                                ContactPropertyId.NickName,
                                                                ContactPropertyId.Salutation,
                                                                ContactPropertyId.TitelDefinedByContact,
                                                            }
                                                            .Select(i => i.ToString().ToLowerInvariant());

        #endregion

        #region CitaviLicense

        public static readonly Enum[] CitaviLicense = Enum.GetValues(typeof(LicensePropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region DataOrderProcessing

        public static readonly Enum[] DataOrderProcessing = Enum.GetValues(typeof(DataOrderProcessingPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region Delivery

        public static readonly Enum[] Delivery = Enum.GetValues(typeof(DeliveryPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region EMailDomain

        public static readonly Enum[] EMailDomain = Enum.GetValues(typeof(EmailDomainPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region ExternalAccount

        public static readonly Enum[] ExternalAccount = Enum.GetValues(typeof(ExternalAccountPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region LicenseFile

        public static readonly Enum[] LicenseFile = Enum.GetValues(typeof(LicenseFilePropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region LicenseType

        public static readonly Enum[] LicenseType = Enum.GetValues(typeof(LicenseTypePropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region LinkedAccount

        public static readonly Enum[] LinkedAccount = Enum.GetValues(typeof(LinkedAccountPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region LinkedEMailAccount

        public static readonly Enum[] LinkedEMailAccount = Enum.GetValues(typeof(LinkedEmailAccountPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region LinkedMobilePhone

        public static readonly Enum[] LinkedMobilePhone = Enum.GetValues(typeof(LinkedMobilePhonePropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region IPRange

        public static readonly Enum[] IPRange = Enum.GetValues(typeof(IPRangePropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region OrderProcess

        public static readonly Enum[] OrderProcess = Enum.GetValues(typeof(OrderProcessPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region OrganizationSetting

        public static readonly Enum[] OrganizationSetting = Enum.GetValues(typeof(OrganizationSettingPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region ProjectEntry

        public static readonly Enum[] ProjectEntry = Enum.GetValues(typeof(ProjectEntryPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region ProjectRole

        public static readonly Enum[] ProjectRole = Enum.GetValues(typeof(ProjectRolePropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region Pricing

        public static readonly Enum[] Pricing = Enum.GetValues(typeof(PricingPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region Product

        public static readonly Enum[] Product = Enum.GetValues(typeof(ProductPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region Voucher

        public static readonly Enum[] Voucher = Enum.GetValues(typeof(VoucherPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region VoucherBlock

        public static readonly Enum[] VoucherBlock = Enum.GetValues(typeof(VoucherBlockPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region Workflow

        public static readonly Enum[] Workflow = Enum.GetValues(typeof(WorkflowPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region Subscription

        public static readonly Enum[] Subscription = Enum.GetValues(typeof(SubscriptionPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region SubscriptionItem

        public static readonly Enum[] SubscriptionItem = Enum.GetValues(typeof(SubscriptionItemPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region Team

        public static readonly Enum[] Team = Enum.GetValues(typeof(TeamPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #region TransactionCurrency

        public static readonly Enum[] TransactionCurrency = Enum.GetValues(typeof(TransactionCurrencyPropertyId)).Cast<Enum>().ToArray();

        #endregion

        #endregion

        #region Methoden

        public static Enum[] GetFullPropertySet<T>()
            where T : CitaviCrmEntity
        {
            var type = typeof(T);
            return GetFullPropertySet(type);
        }

        internal static Enum[] GetFullPropertySet(Type entityType)
        {
            if (entityType == typeof(Account))
            {
                return Account;
            }

            if (entityType == typeof(AutoNumberSequence))
            {
                return AutoNumberSequence;
            }

            if (entityType == typeof(BulkMailQuery))
            {
                return BulkMailQuery;
            }

            if (entityType == typeof(BulkMailTemplate))
            {
                return BulkMailTemplate;
            }

            if (entityType == typeof(CitaviCampaign))
            {
                return CitaviCampaign;
            }

            if (entityType == typeof(CampusContract))
            {
                return CampusContract;
            }

            if (entityType == typeof(CampusContractRenewal))
            {
                return CampusContractRenewal;
            }

            if (entityType == typeof(CampusContractStatistic))
            {
                return CampusContractStatistic;
            }

            if (entityType == typeof(Contact))
            {
                return Contact;
            }

            if (entityType == typeof(CitaviLicense))
            {
                return CitaviLicense;
            }

            if (entityType == typeof(CleverbridgeProduct))
            {
                return CleverbridgeProduct;
            }

            if (entityType == typeof(DataOrderProcessing))
            {
                return DataOrderProcessing;
            }

            if (entityType == typeof(Delivery))
            {
                return Delivery;
            }

            if (entityType == typeof(EmailDomain))
            {
                return EMailDomain;
            }

            if (entityType == typeof(ExternalAccount))
            {
                return ExternalAccount;
            }

            if (entityType == typeof(LicenseFile))
            {
                return LicenseFile;
            }

            if (entityType == typeof(LicenseType))
            {
                return LicenseType;
            }

            if (entityType == typeof(LinkedAccount))
            {
                return LinkedAccount;
            }

            if (entityType == typeof(LinkedEmailAccount))
            {
                return LinkedEMailAccount;
            }

            if (entityType == typeof(OrderProcess))
            {
                return OrderProcess;
            }

            if (entityType == typeof(OrganizationSetting))
            {
                return OrganizationSetting;
            }

            if (entityType == typeof(ProjectEntry))
            {
                return ProjectEntry;
            }

            if (entityType == typeof(ProjectRole))
            {
                return ProjectRole;
            }

            if (entityType == typeof(Pricing))
            {
                return Pricing;
            }

            if (entityType == typeof(Product))
            {
                return Product;
            }

            if (entityType == typeof(Voucher))
            {
                return Voucher;
            }

            if (entityType == typeof(IPRange))
            {
                return IPRange;
            }

            if (entityType == typeof(VoucherBlock))
            {
                return VoucherBlock;
            }

            if (entityType == typeof(Workflow))
            {
                return Workflow;
            }

            if (entityType == typeof(Subscription))
            {
                return Subscription;
            }

            if (entityType == typeof(SubscriptionItem))
            {
                return SubscriptionItem;
            }

            if (entityType == typeof(Team))
            {
                return Team;
            }

            if (entityType == typeof(TransactionCurrency))
            {
                return TransactionCurrency;
            }

            throw new NotSupportedException();
        }

        internal static bool IsWritableBreezeProperty(Type enumType, string name)
        {
            if (enumType == null)
            {
                return false;
            }

            if (enumType == typeof(ContactPropertyId))
            {
                return ContactBreeze.Contains(name.ToLowerInvariant());
            }

            return false;
        }

        #endregion
    }
}
