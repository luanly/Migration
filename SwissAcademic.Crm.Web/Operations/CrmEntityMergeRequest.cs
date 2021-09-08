using System;

namespace SwissAcademic.Crm.Web
{
    public class CrmEntityMergeRequest
    {
        /// <summary>
        /// Loser
        /// </summary>
        public CitaviCrmEntity Target { get; set; }
        /// <summary>
        /// Id von Winner
        /// </summary>
        public Guid SubordinateId { get; set; }
    }
}
