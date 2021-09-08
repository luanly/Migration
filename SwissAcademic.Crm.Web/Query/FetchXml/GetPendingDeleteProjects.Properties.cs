
using System;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetPendingDeleteProjects
    {
        #region Constructors

        public GetPendingDeleteProjects(TimeSpan substract)
        {
            DeletedOnBefore = DateTime.UtcNow.Subtract(substract).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
        }

        #endregion

        #region Properties

        public string DeletedOnBefore { get; set; }


        #endregion
    }
}
