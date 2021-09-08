
using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetOrganizationSetting
    {
        #region Constructors

        public GetOrganizationSetting(string settingsKey, DateTime updatedOn)
        {
            if (updatedOn < CrmDataTypeConstants.MinDate)
            {
                updatedOn = CrmDataTypeConstants.MinDate;
            }

            SettingsKey = settingsKey;
            UpdatedOn = updatedOn.AddSeconds(1).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
        }

        #endregion

        #region Properties

        public string SettingsKey { get; set; }
        public string UpdatedOn { get; set; }

        #endregion
    }
}
