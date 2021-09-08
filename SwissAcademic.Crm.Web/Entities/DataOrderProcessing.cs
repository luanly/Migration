using System;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.DataOrderProcessing)]
    public class DataOrderProcessing
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public DataOrderProcessing()
            :
            base(CrmEntityNames.DataOrderProcessing)
        {

        }

        #endregion

        #region Eigenschaften

        #region Account

        ManyToOneRelationship<DataOrderProcessing, Account> _account;
        public ManyToOneRelationship<DataOrderProcessing, Account> Account
        {
            get
            {
                if (_account == null)
                {
                    _account = new ManyToOneRelationship<DataOrderProcessing, Account>(this, CrmRelationshipNames.AccountDataOrderProcessing, "new_accountid");
                    Observe(_account, true);
                }
                return _account;
            }
        }

        #endregion

        #region ClientAndVersion

        [CrmProperty]
        public string ClientAndVersion
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

        static Type _properyEnumType = typeof(DataOrderProcessingPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region SendState

        [CrmProperty]
        public SendState? SendState
        {
            get
            {
                return GetValue<SendState?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Url

        [CrmProperty]
        public string Url
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

        #region Version

        [CrmProperty]
        public string Version
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

        #endregion
    }
}
