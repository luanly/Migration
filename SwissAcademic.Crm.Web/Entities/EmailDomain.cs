using SwissAcademic.Crm.Web.Query.FetchXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.EMailDomain)]
    [DataContract]
    public class EmailDomain
        :
        CitaviCrmEntity
    {
        #region Felder

        static Regex DomainRegex = new Regex("@(?<DOMAIN>.+)");

        #endregion

        #region Konstruktor

        public EmailDomain()
            :
            base(CrmEntityNames.EMailDomain)
        {

        }

        #endregion

        #region Eigenschaften

        #region Account

        ManyToOneRelationship<EmailDomain, Account> _account;
        public ManyToOneRelationship<EmailDomain, Account> Account
        {
            get
            {
                if (_account == null)
                {
                    _account = new ManyToOneRelationship<EmailDomain, Account>(this, CrmRelationshipNames.AccountEMailDomain, "new_accountid");
                    Observe(_account, true);
                }
                return _account;
            }
        }

        #endregion

        #region Email_Domain_Or_Campus_Name

        [CrmProperty]
        [DataMember]
        public string Email_Domain_Or_Campus_Name
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                if (value != null)
                {
                    value = value.ToLowerInvariant();
                }

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

        #region PropertyEnumType

        static Type _properyEnumType = typeof(EmailDomainPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #endregion

        #region Methoden

        #region GetUsersAsync

        public async Task<List<CrmUser>> GetUsersAsync(CrmDbContext context)
             => await GetUsersAsync(context, FetchXmlExpression.DefaultPageSize);
        internal async Task<List<CrmUser>> GetUsersAsync(CrmDbContext context, int pageSize)
        {
            var users = new List<CrmUser>();
            var xml = new GetEmailDomainUsers(Email_Domain_Or_Campus_Name).TransformText();
            var expr = FetchXmlExpression.Create<Contact>(xml);
            expr.PageSize = pageSize;
            do
            {
                users.AddRange(await context.FetchUsers(expr, true));
            }
            while (expr.HasMoreResults);

            return users;
        }

        #endregion

        #region IsMatch

        public bool IsMatch(string email)
        {
            //Bug 21203
            //Der "." dient dazu, dass wir bei EndsWith nicht in folgendes Problem laufen:
            //EmailDomain Campus: hn.de
            //Email: student@stud.evhn.de
            //Ohne "." haben wir "stud.evhn.de" EntsWith hn.de -> true
            //Mit "." haben wir ".stud.evhn.de" EntsWith .hn.de -> false
            //und ".stud.evhn.de" EntsWith ".evhn.de" -> true
            //und ".evhn.de" EntsWith ".evhn.de" -> true

            var domain = "." + Parse(email);
            return domain.EndsWith("." + Email_Domain_Or_Campus_Name, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Email_Domain_Or_Campus_Name))
            {
                return Email_Domain_Or_Campus_Name;
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

                _dataContractAccountKey = GetAliasedValue<string>(CrmRelationshipNames.AccountEMailDomain, CrmEntityNames.Account, AccountPropertyId.Key);
                return _dataContractAccountKey;

            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.AccountEMailDomain, CrmEntityNames.Account, AccountPropertyId.Key, value);
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

                _dataContractAccountName = GetAliasedValue<string>(CrmRelationshipNames.AccountEMailDomain, CrmEntityNames.Account, AccountPropertyId.Name);
                return _dataContractAccountName;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.AccountEMailDomain, CrmEntityNames.Account, AccountPropertyId.Name, value);
                _dataContractAccountName = value;
            }
        }

        #endregion

        #endregion

        #region Statische Methoden

        #region Exists

        public static bool Exists(string emailAddress, bool isStudent)
        {
            emailAddress = emailAddress.RemoveNonStandardWhitespace();
            var subdomains = Parse(emailAddress).Split('.');
            for (int i = subdomains.Length - 2; i >= 0; i--)
            {
                var domain = string.Join(".", from j in Enumerable.Range(i, subdomains.Length - i)
                                              select subdomains[j]);

                var match = CrmCache.EmailDomains.FirstOrDefault(e => string.Equals(e.Email_Domain_Or_Campus_Name, domain, StringComparison.InvariantCultureIgnoreCase));
                if (match != null)
                {
                    var cc = CrmCache.CampusContracts.FirstOrDefault(c => c._dataContractAccountKey == match._dataContractAccountKey);
                    if (cc != null && (cc.VerifyStEmail && isStudent || cc.VerifyMaEmail && !isStudent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Parse

        public static string Parse(string email)
        {
            email = email.RemoveNonStandardWhitespace();

            if (Environment.Build == BuildType.Alpha &&
                Regex.IsMatch(email, "\\+.+?@(gmail)"))
            {
                return Regex.Match(email, "\\+(?<DOMAIN>.+)@").Groups["DOMAIN"].Value.ToLowerInvariant();
            }
            return DomainRegex.Match(email).Groups["DOMAIN"].Value.ToLowerInvariant();
        }

        #endregion

        #endregion
    }
}
