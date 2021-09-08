using Aspose.Words.Drawing;
using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using SwissAcademic.Crm.Web.Query.FetchXml;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.CampusContract)]
    [DataContract]
    public class CampusContract
        :
        CitaviCrmEntity
    {
        #region Ereignisse

        #region OnRelationshipChanged

        protected override void OnRelationshipChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var relationship = sender as ICrmRelationship;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    {
                        if (e.NewItems.Count > 0)
                        {
                            var newItem = e.NewItems[0];
                            switch (newItem)
                            {
                                case Account account:
                                    {
                                        AccountResolved = account;
                                        DataContractAccountKey = account.Key;
                                        DataContractAccountName = account.Name;
                                        DataContractAccountCampusCities = account.CampusCities;
                                        DataContractAccountCity = account.Address1_City;
                                        DataContractAccountDataCenter = account.DataCenter;
                                    }
                                    break;

                                case Product product:
                                    {
                                        if (ProductsResolved.Any(p => p.Key == product.Key))
                                        {
                                            return;
                                        }

                                        ProductsResolved.Add(product);
                                    }
                                    break;

                            }
                        }
                    }
                    break;
            }
        }

        #endregion

        #endregion

        #region Konstruktor

        public CampusContract()
            :
            base(CrmEntityNames.CampusContract)
        {

        }

        #endregion

        #region Eigenschaften

        #region Account

        /// <summary>
        /// Hochschule
        /// </summary>
        ManyToOneRelationship<CampusContract, Account> _account;
        public ManyToOneRelationship<CampusContract, Account> Account
        {
            get
            {
                if (_account == null)
                {
                    _account = new ManyToOneRelationship<CampusContract, Account>(this, CrmRelationshipNames.AccountCampusContract, "new_accountid");
                    Observe(_account, true);
                }
                return _account;
            }
        }

        #endregion

        #region AccountResolved

        public Account AccountResolved { get; set; }

        #endregion

        #region CitaviTeamCosts

        /// <summary>
        /// Kosten p.A. (derBezeichner "citaviteam" istaufgrund historischer Entwicklung)
        /// </summary>
        [CrmProperty(NoCache=true)]
        public decimal CitaviTeamCosts
        {
            get
            {
                return GetValue<decimal>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region CitaviSpaceInGB

        [CrmProperty]
		[DataMember]
		public int? CitaviSpaceInGB
        {
            get
            {
                return GetValue<int?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Contact

        /// <summary>
        /// Hochschule primärer Kontakt
        /// </summary>
        ManyToOneRelationship<CampusContract, Contact> _contact;
        public ManyToOneRelationship<CampusContract, Contact> Contact
        {
            get
            {
                if (_contact == null)
                {
                    _contact = new ManyToOneRelationship<CampusContract, Contact>(this, CrmRelationshipNames.ContactCampusContract, "new_contactid");
                    Observe(_contact, true);
                }
                return _contact;
            }
        }

        #endregion

        #region ContractDuration

        [CrmProperty]
        public DateTime? ContractDuration
        {
            get
            {
                var date = GetValue<DateTime?>();
                if (!date.HasValue)
                {
                    return date;
                }

                return date.Value;
            }
            set
            {
                if (value == DateTime.MinValue)
                {
                    value = CrmDataTypeConstants.MinDate;
                }

                SetValue(value);
            }
        }

        #endregion

        #region ContractNumber

        [CrmProperty]
        public string ContractNumber
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

        #endregion

        #region ContractType

        [DataMember]
        [CrmProperty]
        public ContractType? ContractType
        {
            get
            {
                return GetValue<ContractType?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ContractReceived

        /// <summary>
        /// Vertrag da?
        /// </summary>
        [CrmProperty]
        public ContractReceivedType ContractReceived
        {
            get
            {
                return GetValue<ContractReceivedType>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ContractSignDate

        [CrmProperty]
        public DateTime? ContractSignDate
        {
            get
            {
                return GetValue<DateTime?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region CurrentStudents

        /// <summary>
        /// Anzahl Studenten
        /// </summary>
        [CrmProperty(NoCache = true)]
        public int CurrentStudents
        {
            get
            {
                return GetValue<int>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region EmailsDomainsResolved

        [DataMember]
        public List<EmailDomain> EmailsDomainsResolved
        {
            get;
            set;
        }

        #endregion

        #region EternalEmail

        [CrmProperty]
        [CacheDataMember]
        public YesNoOptionSet EternalEmail
        {
            get
            {
                var val = GetValue<YesNoOptionSet?>();
                if (val == null)
                {
                    return YesNoOptionSet.No;
                }

                return val.Value;
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region FirstRequest

        [CrmProperty]
        public DateTime? FirstRequest
        {
            get
            {
                var date = GetValue<DateTime?>();
                if (!date.HasValue)
                {
                    return date;
                }

                return date.Value;
            }
            set
            {
                if (value == DateTime.MinValue)
                {
                    value = CrmDataTypeConstants.MinDate;
                }

                SetValue(value);
            }
        }

        #endregion

        #region InfoWebsite

        [DataMember]
        [CrmProperty]
        public string InfoWebsite
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

        #endregion

        #region IPRangesResolved

        public List<IPRange> IPRangesResolved { get; } = new List<IPRange>();
        

        #endregion

        #region IsEmailContract

        public bool IsEmailContract => VerifyMaEmail || VerifyStEmail;

        #endregion

        #region IsIPContract

        public bool IsIPContract => VerifyMaIP || VerifyStIP;

        #endregion

        #region IsShibbolethContract

        public bool IsShibbolethContract => !string.IsNullOrEmpty(ShibbolethEntityId);

        #endregion

        #region IsVoucherContract

        public bool IsVoucherContract => VerifyMaVoucher || VerifyStVoucher;

        #endregion

        #region LeaseTimeYears

        [CrmProperty]
        public int LeaseTimeYears
        {
            get
            {
                return GetValue<int>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Licenses

        /// <summary>
        /// Lizenz
        /// </summary>
        OneToManyRelationship<CampusContract, CitaviLicense> _licenses;
        public OneToManyRelationship<CampusContract, CitaviLicense> Licenses
        {
            get
            {
                if (_licenses == null)
                {
                    _licenses = new OneToManyRelationship<CampusContract, CitaviLicense>(this, CrmRelationshipNames.LicenseCampusContract, "new_campuscontractid");
                    Observe(_licenses, true);
                }
                return _licenses;
            }
        }

        #endregion

        #region LicenseFiles

        OneToManyRelationship<CampusContract, LicenseFile> _licenseFiles;
        public OneToManyRelationship<CampusContract, LicenseFile> LicenseFiles
        {
            get
            {
                if (_licenseFiles == null)
                {
                    _licenseFiles = new OneToManyRelationship<CampusContract, LicenseFile>(this, CrmRelationshipNames.CampusContractLicenseFile, "new_campuscontractid");
                    Observe(_licenseFiles, true);
                }
                return _licenseFiles;
            }
        }

        #endregion

        #region MailsSendetAt

        [CrmProperty]
        public DateTime? MailsSendetAt
        {
            get
            {
                return GetValue<DateTime?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region NewContractAvailable

        [CrmProperty]
        public bool NewContractAvailable
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region NumberOfStudents

        [CrmProperty]
        public int NumberOfStudents
        {
            get
            {
                return GetValue<int>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region DelayC6

        [CrmProperty]
        public bool DelayC6
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region OrderUrl

        [CrmProperty]
        [DataMember]
        public string OrderUrl
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

        #endregion

        #region OrganizationName

        [CrmProperty]
        [DataMember]
        public string OrganizationName
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

        #endregion

        #region OrganizationSettings

        OneToManyRelationship<CampusContract, OrganizationSetting> _organizationSettings;
        public OneToManyRelationship<CampusContract, OrganizationSetting> OrganizationSettings
        {
            get
            {
                if (_organizationSettings == null)
                {
                    _organizationSettings = new OneToManyRelationship<CampusContract, OrganizationSetting>(this, CrmRelationshipNames.OrganizationSettingsCampusContract, "new_campuscontractid");
                    Observe(_organizationSettings, true);
                }
                return _organizationSettings;
            }
        }

        #endregion

        #region Products

        ManyToManyRelationship<CampusContract, Product> _products;
        internal ManyToManyRelationship<CampusContract, Product> Products
        {
            get
            {
                if (_products == null)
                {
                    _products = new ManyToManyRelationship<CampusContract, Product>(this, CrmRelationshipNames.CampusContractProduct);
                    Observe(_products, true);
                }
                return _products;
            }
        }
        public List<Product> ProductsResolved { get; set; } = new List<Product>();

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(CampusContractPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region RelevantRenewalInfos

        [CrmProperty]
        public string RelevantRenewalInfos
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

        #endregion

        #region RSSFeedUrl

        [CrmProperty]
        [DataMember]
        public string RSSFeedUrl
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

        #endregion

        #region ShibbolethEntityId

        [CrmProperty]
        [DataMember]
        public string ShibbolethEntityId
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

        #endregion

        #region ShibbolethAlum

        [CrmProperty]
        [CacheDataMember]
        public bool ShibbolethAlum
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ShibbolethAffiliate

        [CrmProperty]
        [CacheDataMember]
        public bool ShibbolethAffiliate
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ShibbolethEmployee

        [CrmProperty]
        [CacheDataMember]
        public bool ShibbolethEmployee
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ShibbolethFaculty

        [CrmProperty]
        [CacheDataMember]
        public bool ShibbolethFaculty
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ShibbolethLibraryWalkIn

        [CrmProperty]
        [CacheDataMember]
        public bool ShibbolethLibraryWalkIn
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ShibbolethMember

        [CrmProperty]
        [CacheDataMember]
        public bool ShibbolethMember
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ShibbolethStudent

        [CrmProperty]
        [CacheDataMember]
        public bool ShibbolethStudent
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ShibbolethStaff

        [CrmProperty]
        [CacheDataMember]
        public bool ShibbolethStaff
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ShibbolethPersonAffiliation

        public PersonAffiliationType ShibbolethPersonAffiliation
        {
            get
            {
                var affiliation = PersonAffiliationType.None;
                if (ShibbolethAffiliate)
                {
                    affiliation |= PersonAffiliationType.Affiliate;
                }

                if (ShibbolethAlum)
                {
                    affiliation |= PersonAffiliationType.Alum;
                }

                if (ShibbolethEmployee)
                {
                    affiliation |= PersonAffiliationType.Employee;
                }

                if (ShibbolethFaculty)
                {
                    affiliation |= PersonAffiliationType.Faculty;
                }

                if (ShibbolethLibraryWalkIn)
                {
                    affiliation |= PersonAffiliationType.LibraryWalkIn;
                }

                if (ShibbolethMember)
                {
                    affiliation |= PersonAffiliationType.Member;
                }

                if (ShibbolethStaff)
                {
                    affiliation |= PersonAffiliationType.Staff;
                }

                if (ShibbolethStudent)
                {
                    affiliation |= PersonAffiliationType.Student;
                }

                return affiliation;
            }
        }

        #endregion

        #region ShibbolethPersonEntitlement

        [CrmProperty]
        [DataMember]
        public string ShibbolethPersonEntitlement
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


        #endregion

        #region ShowOnWeb

        [CrmProperty]
        [CacheDataMember]
        public bool ShowOnWeb
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Sponsor

        /// <summary>
        /// Dynamisches OptionSet in CRM
        /// </summary>
        [CrmProperty]
        [DataMember]
        public int Sponsor
        {
            get
            {
                return GetValue<int>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region SourceText

        /// <summary>
        /// HTML Text für Campusverlängerung
        /// </summary>
        [CrmProperty]
        public string SourceText
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

        #endregion

        #region Statistics

        OneToManyRelationship<CampusContract, CampusContractStatistic> _campusContractStatistics;
        public OneToManyRelationship<CampusContract, CampusContractStatistic> Statistics
        {
            get
            {
                if (_campusContractStatistics == null)
                {
                    _campusContractStatistics = new OneToManyRelationship<CampusContract, CampusContractStatistic>(this, CrmRelationshipNames.CampusContractCampusContractStatistic, "new_campuscontractid");
                    Observe(_campusContractStatistics, true);
                }
                return _campusContractStatistics;
            }
        }

        #endregion

        #region VerifyMaEmail

        /// <summary>
        /// Mitarbeiter Email Verification
        /// </summary>
        [CrmProperty]
        [DataMember]
        public bool VerifyMaEmail
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region VerifyMaIP

        /// <summary>
        /// Mitarbeiter IP Verification
        /// </summary>
        [CrmProperty]
        [DataMember]
        public bool VerifyMaIP
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region VerifyMaVoucher

        /// <summary>
        /// Mitarbeiter Voucher Verification
        /// </summary>
        [CrmProperty]
        [DataMember]
        public bool VerifyMaVoucher
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region VerifyStEmail

        /// <summary>
        /// Student EMail Verification
        /// </summary>
        [CrmProperty]
        [DataMember]
        public bool VerifyStEmail
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region VerifyStIP

        /// <summary>
        /// Student IP Verification
        /// </summary>
        [CrmProperty]
        [DataMember]
        public bool VerifyStIP
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region VerifyStVoucher

        /// <summary>
        /// Student Voucher Verification
        /// </summary>
        [CrmProperty]
        [DataMember]
        public bool VerifyStVoucher
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region VoucherOrderUrl

        /// <summary>
        /// Gutschein Bestell-URL
        /// </summary>
        [CrmProperty]
        [DataMember]
        public string VoucherOrderUrl
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

        #endregion

        #endregion

        #region Methoden

        #region AddIntersectAttribute

        internal override void AddIntersectAttribute(string name, object value)
        {
            if (name == CrmRelationshipNames.CampusContractProduct)
            {
                var productId = (Guid)value;
                if (ProductsResolved.Any(p => p.Id == productId))
                {
                    return;
                }

                ProductsResolved.Add(CrmCache.Products.First(p => p.Value.Id == productId).Value);
            }
            base.AddIntersectAttribute(name, value);
        }

        #endregion

        #region AddProduct

        public void AddProduct(Product product)
        {
            if (ProductsResolved.Any(p => p.Key == product.Key))
            {
                return;
            }

            ProductsResolved.Add(product);
            Products.Add(product);
        }

        #endregion

        #region Clone

        public async Task<CampusContract> Clone()
        {
            var props = EntityPropertySets.CampusContract.Cast<CampusContractPropertyId>().Except(
                                                        CampusContractPropertyId.ContractNumber,
                                                        CampusContractPropertyId.ContractDuration,
                                                        CampusContractPropertyId.ContractReceived,
                                                        CampusContractPropertyId.NewContractAvailable,
                                                        CampusContractPropertyId.MailsSendetAt
                                                        );
            return await Clone(props.ToArray());
        }

        internal async Task<CampusContract> Clone(params CampusContractPropertyId[] attributesToClone)
		{
            var clone = Clone<CampusContract>(attributesToClone.Cast<Enum>().ToArray());

            var account = await Account.Get();
            if (account != null)
            {
                clone.Account.Set(account);
            }

            var contact = await Contact.Get();
            if(contact != null)
			{
                clone.Contact.Set(contact);
			}

            var products = await Products.Get();
            foreach(var product in products)
			{
                clone.Products.Add(product);
			}

            var currency = await GetTransactioncurrencyId();
            if (!string.IsNullOrEmpty(currency))
            {
                clone.SetTransactionCurrencyId(currency);
            }

            return clone;
        }

        #endregion

        #region GetLinkedEmailAccountsAsync

        public async Task<List<LinkedEmailAccount>> GetLinkedEmailAccountsAsync(CrmDbContext context)
        {
            if(EmailsDomainsResolved?.Count == 0)
            {
                var account = await Account.Get();
                var emailDomains = await account.EMailDomains.Get(EntityPropertySets.EMailDomain);
                EmailsDomainsResolved = emailDomains.ToList();
            }
            var linkedEmailAccounts = new List<LinkedEmailAccount>();

            foreach (var emailDomain in EmailsDomainsResolved)
            {
                var expr = QueryExpression.Create<LinkedEmailAccount>();
                expr.AddCondition(LinkedEmailAccountPropertyId.Email, ConditionOperator.EndsWith, emailDomain.Email_Domain_Or_Campus_Name);
                expr.PageSize = 1000;
                do
                {
                    linkedEmailAccounts.AddRange(await context.RetrieveMultiple<LinkedEmailAccount>(expr, true));
                }
                while (expr.HasMoreResults);
            }

            return linkedEmailAccounts;
        }

        #endregion

        #region GetLastLogins

        public async Task<int> GetLastLogins(DateTime dateTime, ClientType clientType, CrmDbContext context)
        {
            var count = 0;
            var expr = FetchXmlExpression.Create<CitaviLicense>(new Query.FetchXml.GetCampusContractLastLogins(Key, dateTime, clientType).TransformText());
            expr.PageSize = 5000;
            do
            {
                var response = await context.Fetch<CitaviLicense>(expr, false);
                count += response.Count();
            }
            while (expr.HasMoreResults);

            return count;
        }

        #endregion

        #region GetCampusContractStatisticByDate

        public async Task<IEnumerable<CampusContractStatistic>> GetCampusContractStatisticByDate(CrmDbContext context, DateTime date)
            => await GetCampusContractStatisticByDate(context, date, 1000);

        internal async Task<IEnumerable<CampusContractStatistic>> GetCampusContractStatisticByDate(CrmDbContext context, DateTime date, int pageSize)
        {
            var result = new List<CampusContractStatistic>();

            var expr = FetchXmlExpression.Create<CampusContractStatistic>(new Query.FetchXml.GetCampusContractStatisticByDate(Key, date).TransformText());
            expr.PageSize = pageSize;
            do
            {
                var response = await context.Fetch<CampusContractStatistic>(expr, false);
                result.AddRange(response);
            }
            while (expr.HasMoreResults);

            return result;
        }

        #endregion

        #region GetUserStatistic

        public async Task<IEnumerable<CampusContractUserStatistic>> GetUserStatistic(CrmDbContext context)
            => await GetUserStatistic(context, 1000);

        internal async Task<IEnumerable<CampusContractUserStatistic>> GetUserStatistic(CrmDbContext context, int pageSize)
        {
            var result = new List<CampusContractUserStatistic>();

            var expr = FetchXmlExpression.Create<CitaviLicense>(new Query.FetchXml.GetCampusContractUserStatistic(Key).TransformText());
            expr.PageSize = pageSize;
            do
            {
                var response = await context.Fetch<CitaviLicense>(expr, false);
                result.AddRange(response.Select(r => new CampusContractUserStatistic(r)));
            }
            while (expr.HasMoreResults);

            return result;
        }

        #endregion

        #region GetTransactioncurrencyId

        public async Task<string> GetTransactioncurrencyId()
		 => await CrmWebApi.RetrieveProperty<string>(CrmEntityNames.CampusContract, Id, "_transactioncurrencyid_value", null);

        #endregion

        #region GetUsersAsync

        /// <summary>
        /// Die User haben NUR die Lizenzen von diesem CC
        /// Wenn alle Lizenzen benötigten werden, müssen die nachgeladen werden
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<List<CrmUser>> GetUsersAsync(CrmDbContext context)
             => await GetUsersAsync(context, FetchXmlExpression.DefaultPageSize);

        /// <summary>
        /// Die User haben NUR die Lizenzen von diesem CC
        /// Wenn alle Lizenzen benötigten werden, müssen die nachgeladen werden
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal async Task<List<CrmUser>> GetUsersAsync(CrmDbContext context, int pageSize)
        {
            var users = new List<CrmUser>();
            var xml = new GetCampusContractLicenses(Key).TransformText();
            var expr = FetchXmlExpression.Create<CitaviLicense>(xml);
            expr.PageSize = pageSize;
            do
            {
                users.AddRange(await context.FetchUsers(expr, true));
            }
            while (expr.HasMoreResults);

            return users;
        }

        #endregion

        #region LookupLegacyProduct

        internal async Task<Product> LookupLegacyProduct()
        {
            try
            {
                var productId = await CrmWebApi.RetrieveProperty<string>(CrmEntityNames.CampusContract, Id, "new_citaviproductid", "new_citaviproductid");
                if (string.IsNullOrEmpty(productId))
                {
                    return null;
                }
                var id = Guid.Parse(productId);
                return CrmCache.Products.First(product => product.Value.Id == id).Value;
            }
            catch (Exception ex)
            {
                ex.Data["CampusContractKey"] = Key;
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return null;
        }

        #endregion

        #region ReplaceProduct

        internal void ReplaceProduct(Product existingProduct, Product newProduct)
        {
            RemoveProduct(existingProduct);
            AddProduct(newProduct);
        }

        #endregion

        #region RemoveProduct

        internal void RemoveProduct(Product existingProduct)
        {
            ProductsResolved.RemoveAll(p => p.Key == existingProduct.Key);
            Products.Remove(existingProduct);
        }

        #endregion

        #region SetTransactioncurrency

        public void SetTransactionCurrency(TransactionCurrency transactioncurrency)
           => SetTransactionCurrencyId(transactioncurrency.Id.ToString());
        public void SetTransactionCurrencyId(string transactioncurrencyid)
            => SetValue($"/transactioncurrencies({transactioncurrencyid})", "transactioncurrencyid@odata.bind");

        #endregion

        #region ToString

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (AccountResolved != null)
            {
                return AccountResolved.Name;
            }

            if (!string.IsNullOrEmpty(DataContractAccountName))
            {
                return DataContractAccountName;
            }

            if (!string.IsNullOrEmpty(ContractNumber))
            {
                return ContractNumber;
            }

            return base.ToString();
        }

        #endregion

        #endregion

        #region DataContract

        #region DataContractAccountKey

        internal string _dataContractAccountKey;
        [DataMember(Name = "AccountKey")]
        public string DataContractAccountKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractAccountKey))
                {
                    return _dataContractAccountKey;
                }

                _dataContractAccountKey = GetAliasedValue<string>(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.Account, AccountPropertyId.Key);
                return _dataContractAccountKey;

            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.Account, AccountPropertyId.Key, value);
                _dataContractAccountKey = value;
            }
        }

        #endregion

        #region DataContractAccountName

        string _dataContractAccountName;

        [DataMember(Name = "AccountName")]
        public string DataContractAccountName
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractAccountName))
                {
                    return _dataContractAccountName;
                }

                _dataContractAccountName = GetAliasedValue<string>(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.Account, AccountPropertyId.Name);
                return _dataContractAccountName;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.Account, AccountPropertyId.Name, value);
                _dataContractAccountName = value;
            }
        }

        #endregion

        #region DataContractAccountCampusCities

        string _dataContractAccountCampusCities;
        [DataMember(Name = "AccountCampusCities")]
        public string DataContractAccountCampusCities
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractAccountCampusCities))
                {
                    return _dataContractAccountCampusCities;
                }

                _dataContractAccountCampusCities = GetAliasedValue<string>(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.Account, AccountPropertyId.CampusCities);
                return _dataContractAccountCampusCities;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.Account, AccountPropertyId.CampusCities, value);
                _dataContractAccountCampusCities = value;
            }
        }

        #endregion

        #region DataContractAccountCity

        string _dataContractAccountCity;

        [DataMember(Name = "AccountCity")]
        public string DataContractAccountCity
        {
            get
            {
                if (!string.IsNullOrEmpty(_dataContractAccountCity))
                {
                    return _dataContractAccountCity;
                }

                _dataContractAccountCity = GetAliasedValue<string>(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.Account, AccountPropertyId.Address1_City);
                return _dataContractAccountCity;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.Account, AccountPropertyId.Address1_City, value);
                _dataContractAccountCity = value;
            }
        }

        #endregion

        #region DataContractAccountDataCenter

        DataCenter? _dataContractAccountDataCenter;

        [CacheDataMember(Name = "AccountDataCenter")]
        public DataCenter DataContractAccountDataCenter
        {
            get
            {
                if (_dataContractAccountDataCenter.HasValue)
                {
                    return _dataContractAccountDataCenter.Value;
                }
                _dataContractAccountDataCenter = GetAliasedValue<DataCenter?>(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.Account, AccountPropertyId.DataCenter);
				if (!_dataContractAccountDataCenter.HasValue)
				{
                    _dataContractAccountDataCenter = DataCenter.WestEurope;
                }
                return _dataContractAccountDataCenter.Value;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.AccountCampusContract, CrmEntityNames.Account, AccountPropertyId.DataCenter, value);
                _dataContractAccountDataCenter = value;
            }
        }

        #endregion

        #endregion

        #region Statische Methoden

        public async static Task<CampusContract> GetAsync(CrmDbContext context, Guid id)
        {
            var key = (await context.Get<CampusContract>(id)).Key;
            return await GetAsync(context, key);
        }

        public async static Task<CampusContract> GetAsync(CrmDbContext context, string key)
        {
			if (string.IsNullOrEmpty(key))
			{
                throw new NotSupportedException("CampusContractKey must not be null");
			}
            var queryXml = new GetCampusContractByKey(key).TransformText();
            var entities = await context.Fetch(FetchXmlExpression.Create<CampusContract>(queryXml), observe: true);
            var campusContract = new CrmSet(entities).CampusContracts?.FirstOrDefault();
            if(campusContract == null)
			{
                Telemetry.TrackDiagnostics($"CampusContract not found: ${key}");
                return null;
			}
            campusContract.AccountResolved = await campusContract.Account.Get(EntityPropertySets.Account);
            if (campusContract.AccountResolved != null)
            {
                campusContract.EmailsDomainsResolved = (await campusContract.AccountResolved.EMailDomains.Get(EntityPropertySets.EMailDomain)).ToList();

                campusContract.IPRangesResolved.Clear();
                campusContract.IPRangesResolved.AddRange((await campusContract.AccountResolved.IPRanges.Get(EntityPropertySets.IPRange)).ToList());
            }
            return campusContract;
        }

        #endregion
    }
}
