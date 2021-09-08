
namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetProjectRoleByKey
    {
        #region Constructors

        public GetProjectRoleByKey(string projectRoleKey)
        {
            ProjectRoleKey = projectRoleKey;
        }

        #endregion

        #region Properties

        public string ProjectRoleKey { get; set; }

        #endregion
    }
}
