using SwissAcademic.ApplicationInsights;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace SwissAcademic.Azure
{
    public class ProjectLockedException
        :
        Exception
    {
        #region Constructors

        public ProjectLockedException()
        {
        }

        #endregion
    }
}
