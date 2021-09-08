using System;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.Team)]
    public class Team
        :
        CitaviCrmEntity
    {
        #region Konstanten

        public const string GL = "SAS GL";
        public const string IT = "SAS IT Admin";
        public const string Support = "SAS Sales & Support";

        #endregion

        #region Konstruktor

        public Team()
            :
            base(CrmEntityNames.Team)
        {

        }

        #endregion

        #region Eigenschaften

        #region Name

        [CrmProperty(IsBuiltInAttribute = true)]
        public string Name
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

        static Type _properyEnumType = typeof(TeamPropertyId);
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

        #region GetAccess

        public async Task<CrmAccessRights> GetAccess(ExternalAccount externerAccount)
        {
            var accessRights = await CrmWebApi.RetrieveSharedPrincipalsAndAccess(externerAccount);
            if (accessRights.Any(a => a.TargetId == Id))
            {
                return accessRights.First(a => a.TargetId == Id).AccessRights;
            }
            return CrmAccessRights.None;
        }

        #endregion

        #region GrantAccess

        public async Task GrantAccess(ExternalAccount externalAccount)
            => await GrantAccess(externalAccount, CrmAccessRights.ReadAccess | CrmAccessRights.WriteAccess | CrmAccessRights.DeleteAccess);
        public async Task GrantAccess(ExternalAccount externalAccount, CrmAccessRights accessRights)
         => await CrmWebApi.GrantOrModifyAccess(this, externalAccount, accessRights, modify:false);

        #endregion

        #region ModifyAccess

        public async Task ModifyAccess(ExternalAccount externalAccount, CrmAccessRights accessRights)
         => await CrmWebApi.GrantOrModifyAccess(this, externalAccount, accessRights, modify: true);

        #endregion

        #region ToString

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }

            return base.ToString();
        }

        #endregion

        #endregion
    }
}
