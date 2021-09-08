using System;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.CitaviCampaign)]
    public class CitaviCampaign
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public CitaviCampaign()
            :
            base(CrmEntityNames.CitaviCampaign)
        {

        }

        #endregion

        #region Eigenschaften

        #region CampaignCode

        [CrmProperty]
        public string CampaignCode
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

        #region CitaviCampagneName

        [CrmProperty]
        public string CitaviCampagneName
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

        #region Licenses

        OneToManyRelationship<CitaviCampaign, CitaviLicense> _licenses;
        public OneToManyRelationship<CitaviCampaign, CitaviLicense> Licenses
        {
            get
            {
                if (_licenses == null)
                {
                    _licenses = new OneToManyRelationship<CitaviCampaign, CitaviLicense>(this, CrmRelationshipNames.LicenseCitaviCampaign, "new_citavicampaignid");
                    Observe(_licenses, true);
                }
                return _licenses;
            }
        }

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(CitaviCampaignPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #endregion
    }
}
