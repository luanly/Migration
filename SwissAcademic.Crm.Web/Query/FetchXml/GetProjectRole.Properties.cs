
namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetProjectRole
    {
        #region Constructors

        public GetProjectRole(string contactKey, string projectKey)
        {
            ContactKey = contactKey;
            ProjectKey = projectKey;
        }

        #endregion

        #region Properties

        public string ContactKey { get; set; }
        public string ProjectKey { get; set; }

        #endregion
    }
}
