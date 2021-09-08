
using System.Security;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetResellerByExclusiveCountries
    {
        #region Constructors

        public GetResellerByExclusiveCountries(string exclusiveCountries)
        {
            ExclusiveCountries = SecurityElement.Escape(exclusiveCountries);
        }

        #endregion

        #region Properties

        public string ExclusiveCountries { get; set; }

        #endregion
    }
}
