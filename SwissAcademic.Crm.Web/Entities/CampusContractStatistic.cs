using System;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.CampusContractStatistic)]
    public class CampusContractStatistic
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public CampusContractStatistic()
            :
            base(CrmEntityNames.CampusContractStatistic)
        {

        }

        #endregion

        #region Eigenschaften

        #region Bounces

        [CrmProperty]
        public int Bounces
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

        #region BouncesTotal

        [CrmProperty]
        public int BouncesTotal
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

        #region CampusContract

        ManyToOneRelationship<CampusContractStatistic, CampusContract> _campusContract;
        public ManyToOneRelationship<CampusContractStatistic, CampusContract> CampusContract
        {
            get
            {
                if (_campusContract == null)
                {
                    _campusContract = new ManyToOneRelationship<CampusContractStatistic, CampusContract>(this, CrmRelationshipNames.CampusContractCampusContractStatistic, "new_campuscontractid");
                    Observe(_campusContract, true);
                }
                return _campusContract;
            }
        }

        #endregion

        #region DailyTotal

        [CrmProperty]
        public int DailyTotal
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

        #region Desktop_Lastlogin_Daily

        [CrmProperty]
        public int Desktop_Lastlogin_Daily
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

        #region Desktop_Lastlogin_Monthly

        [CrmProperty]
        public int Desktop_Lastlogin_Monthly
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

        #region MaCount

        /// <summary>
        /// Mitarbeiter Anzahl
        /// </summary>
        [CrmProperty]
        public int MaCount
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

        #region MaVerifiedCount

        /// <summary>
        /// Mitarbeiter Anzahl (Verifiziert)
        /// </summary>
        [CrmProperty]
        public int MaVerifiedCount
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

        #region PropertyEnumType

        static Type _properyEnumType = typeof(CampusContractStatisticPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region StdCount

        /// <summary>
        /// Studenten Anzahl
        /// </summary>
        [CrmProperty]
        public int StdCount
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

        #region StdVerifiedCount

        /// <summary>
        /// Studenten Anzahl (Verifiziert)
        /// </summary>
        [CrmProperty]
        public int StdVerifiedCount
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

        #region TotalCount

        [CrmProperty]
        public int TotalCount
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

        #region TotalHistoric

        [CrmProperty]
        public int TotalHistoric
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

        #region TotalVerifiedCount

        [CrmProperty]
        public int TotalVerifiedCount
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

        #region Web_Lastlogin_Daily

        [CrmProperty]
        public int Web_Lastlogin_Daily
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

        #region Web_Lastlogin_Monthly

        [CrmProperty]
        public int Web_Lastlogin_Monthly
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

        #region WordAssistant_Lastlogin_Daily

        [CrmProperty]
        public int WordAssistant_Lastlogin_Daily
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

        #region WordAssistant_Lastlogin_Monthly

        [CrmProperty]
        public int WordAssistant_Lastlogin_Monthly
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
	}
}
