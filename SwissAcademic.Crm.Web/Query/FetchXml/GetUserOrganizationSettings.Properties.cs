
namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetUserOrganizationSettings
    {
        #region Constructors

        public GetUserOrganizationSettings(string contactKey)
        {
            ContactKey = contactKey;
        }

        #endregion

        #region Properties

        public string ContactKey { get; set; }

        #endregion
    }
}
