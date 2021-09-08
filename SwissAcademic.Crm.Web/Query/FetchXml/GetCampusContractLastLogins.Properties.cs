using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetCampusContractLastLogins
    {
        #region Constructors

        public GetCampusContractLastLogins(string campusContractKey, DateTime date, ClientType clientType)
        {
            CampusContractKey = campusContractKey;
            LastLogin = date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
			switch (clientType)
			{
                case ClientType.Assistant:
                    LastLoginField = EntityNameResolver.ResolveAttributeName(CrmEntityNames.Contact, ContactPropertyId.LastLoginWordAssistant.ToString());
                    break;

                case ClientType.Desktop:
                    LastLoginField = EntityNameResolver.ResolveAttributeName(CrmEntityNames.Contact, ContactPropertyId.LastLogin.ToString());
                    break;

                case ClientType.Picker:
                    LastLoginField = EntityNameResolver.ResolveAttributeName(CrmEntityNames.Contact, ContactPropertyId.LastLoginPicker.ToString());
                    break;

                case ClientType.Web:
                    LastLoginField = EntityNameResolver.ResolveAttributeName(CrmEntityNames.Contact, ContactPropertyId.LastLoginCitaviWeb.ToString());
                    break;
            }
        }

        #endregion

        #region Properties

        public string CampusContractKey { get; }

        public string LastLogin { get; }

        public string LastLoginField { get; }

        #endregion
    }
}
