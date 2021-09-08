using System.Security;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetMergedContactByKey
    {
        #region Constructors

        public GetMergedContactByKey(string key)
        {
            Value = key;
        }

        #endregion

        #region Properties

        public string Value { get; private set; }

        #endregion
    }
}
