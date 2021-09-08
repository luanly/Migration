using System;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.CampusContractRenewal)]
    public class CampusContractRenewal
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public CampusContractRenewal()
            :
            base(CrmEntityNames.CampusContractRenewal)
        {

        }

        #endregion

        #region Eigenschaften

        #region AISessionId

        [CrmProperty]
        public string AISessionId
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

        #region BouncedMails

        [CrmProperty]
        public int BouncedMails
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

        #region BlockedMails

        [CrmProperty]
        public int BlockedMails
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

        #region ContractNr

        [CrmProperty]
        public string ContractNr
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

        #region EndDate

        [CrmProperty]
        public DateTime? EndDate
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

        #region LicenceMigState

        [CrmProperty]
        public CampusContractRenewalStateType? LicenceMigState
        {
            get
            {
                return GetValue<CampusContractRenewalStateType?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region MailsSentCount

        [CrmProperty]
        public int MailsSentCount
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

        #region MigratedLicensesCount

        [CrmProperty]
        public int MigratedLicensesCount
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

        #region NewCampusContract

        ManyToOneRelationship<CampusContractRenewal, CampusContract> _newCampusContract;
        public ManyToOneRelationship<CampusContractRenewal, CampusContract> NewCampusContract
        {
            get
            {
                if (_newCampusContract == null)
                {
                    _newCampusContract = new ManyToOneRelationship<CampusContractRenewal, CampusContract>(this, CrmRelationshipNames.CampusContractRenewalCampusContract_New, "new_NewContractId");
                    Observe(_newCampusContract, true);
                }
                return _newCampusContract;
            }
        }

        #endregion

        #region OldCampusContract

        ManyToOneRelationship<CampusContractRenewal, CampusContract> _oldCampusContract;
        public ManyToOneRelationship<CampusContractRenewal, CampusContract> OldCampusContract
        {
            get
            {
                if (_oldCampusContract == null)
                {
                    _oldCampusContract = new ManyToOneRelationship<CampusContractRenewal, CampusContract>(this, CrmRelationshipNames.CampusContractRenewalCampusContract_Old, "new_OldContractId");
                    Observe(_oldCampusContract, true);
                }
                return _oldCampusContract;
            }
        }

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(CampusContractRenewalPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region QueueItemId

        [CrmProperty]
        public string QueueItemId
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

        #region QueueItemPopReceipt

        [CrmProperty]
        public string QueueItemPopReceipt
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

        #region RenewStatus

        [CrmProperty]
        public CampusContractRenewalStateType? RenewStatus
        {
            get
            {
                return GetValue<CampusContractRenewalStateType?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region SendMailDateTime

        [CrmProperty]
        public DateTime? SendMailDateTime
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

        #region SendMailState

        [CrmProperty]
        public CampusContractRenewalStateType? SendMailState
        {
            get
            {
                return GetValue<CampusContractRenewalStateType?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region StartDate

        [CrmProperty]
        public DateTime? StartDate
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

        #region Summiglic

        /// <summary>
        /// Klingonisch für "Anzahl Lizenzen alter Vertrag" ;-)
        /// </summary>
        [CrmProperty]
        public int Summiglic
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

        #endregion

        #region Methoden

        #region GetCampusContracts

        public async Task<(CampusContract OldCampusContract, CampusContract NewCampusContract)> GetCampusContracts(CrmDbContext context)
        {
            var oldKey = (await OldCampusContract.Get()).Key;
            var newKey = (await NewCampusContract.Get()).Key;
            var oldCC = await CampusContract.GetAsync(context, oldKey);
            var newCC = await CampusContract.GetAsync(context, newKey);
            return (oldCC, newCC);
        }

        #endregion

        #endregion
    }
}
