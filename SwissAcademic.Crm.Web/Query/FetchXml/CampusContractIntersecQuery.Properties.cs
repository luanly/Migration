using System.Security;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class CampusContractIntersecQuery
    {
        #region Constructors

        public CampusContractIntersecQuery(string relationshipname)
        {
            RelationshipName = relationshipname;
        }

        #endregion

        #region Properties

        string RelationshipName { get; set; }

        #endregion
    }
}
