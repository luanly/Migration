
namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetProjectMembers
    {

        #region Constructors

        public GetProjectMembers(string projectKey)
        {
            ProjectKey = projectKey;
        }

        #endregion

        #region Properties

        public string ProjectKey { get; set; }

        #endregion
    }
}
