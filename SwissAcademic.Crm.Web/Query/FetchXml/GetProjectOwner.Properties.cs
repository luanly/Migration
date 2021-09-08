
namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetProjectOwner
    {

        #region Constructors

        public GetProjectOwner(string projectKey)
        {
            ProjectKey = projectKey;
        }

        #endregion

        #region Properties

        public string ProjectKey { get; set; }

        #endregion
    }
}
