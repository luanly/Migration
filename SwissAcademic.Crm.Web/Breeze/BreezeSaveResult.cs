using System;
using System.Collections.Generic;

namespace SwissAcademic.Crm.Web.Breeze
{
    public class BreezeSaveResult
    {
        public List<CitaviCrmEntity> Entities { get; set; }
        public List<BreezeKeyMapping> KeyMappings { get; set; }
        public List<BreezeEntityKey> DeletedKeys { get; set; }
        public List<object> Errors { get; set; }
    }
}
