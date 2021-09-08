using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace SwissAcademic.Crm.Web
{
    public class CrmPropertyAttribute
        :
        CacheDataMemberAttribute
    {
        public CrmPropertyAttribute([CallerMemberName] string propertyName = null)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; set; }

        public bool IsBuiltInAttribute { get; set; }
        public bool IsPrimaryAttribute { get; set; }

        public bool NoCache { get; set; }
    }
}
