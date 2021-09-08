using System;
using System.Collections.Generic;

namespace SwissAcademic.Crm.Web.Breeze
{
    public class BreezeEntityInfo
    {
        public CitaviCrmEntity Entity { get; internal set; }
        public BreezeEntityState EntityState { get; set; }
        public Dictionary<string, object> OriginalValuesMap { get; set; }
    }
}
