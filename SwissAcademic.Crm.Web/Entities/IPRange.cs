using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.IPRange)]
    [DataContract]
    public class IPRange
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public IPRange()
            :
            base(CrmEntityNames.IPRange)
        {

        }

        #endregion

        #region Eigenschaften

        #region Account

        ManyToOneRelationship<IPRange, Account> _account;
        public ManyToOneRelationship<IPRange, Account> Account
        {
            get
            {
                if (_account == null)
                {
                    _account = new ManyToOneRelationship<IPRange, Account>(this, CrmRelationshipNames.AccountIPRange, "new_accountid");
                    Observe(_account, true);
                }
                return _account;
            }
        }

        #endregion

        #region End

        [CrmProperty]
        [DataMember]
        public decimal End
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

        #region IPEnd

        [CrmProperty]
        [DataMember]
        public string IPEnd
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

        #region IPStart

        [CrmProperty]
        [DataMember]
        public string IPStart
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

        #region PropertyEnumType

        static Type _properyEnumType = typeof(IPRangePropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region Range

        [CrmProperty]
        [DataMember]
        public string Range
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

        #region Start

        [CrmProperty]
        [DataMember]
        public decimal Start
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

        #endregion

        #region Methoden

        #region IsInRange

        public bool IsInRange(string ipString)
        {
            if (string.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            var ipValue = IPAddress.Parse(ipString).ConvertToDecimal();
            return Start <= ipValue && End >= ipValue;
        }

        #endregion

        #region ToString

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Range))
            {
                return Range;
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

                _dataContractAccountKey = GetAliasedValue<string>(CrmRelationshipNames.AccountIPRange, CrmEntityNames.Account, AccountPropertyId.Key);
                return _dataContractAccountKey;
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.AccountIPRange, CrmEntityNames.Account, AccountPropertyId.Key, value);
                _dataContractAccountKey = value;
            }
        }

        #endregion

        #endregion

        #region Statische Methoden

        #region Exists

        public static async Task<bool> ExistsAsync(string ipRange, string accountKey, CrmDbContext context)
        {
            var query = new Query.FetchXml.IPRangeExists(ipRange, accountKey).TransformText();
            return await context.Count(FetchXmlExpression.Create<IPRange>(query)) > 0;
        }

        #endregion

        #endregion
    }
}
