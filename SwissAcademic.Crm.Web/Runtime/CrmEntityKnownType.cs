using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class CrmEntityKnownType
    {
        public string EntityLogicalName { get; set; }
        public Type Type { get; set; }
        public Type IEnumerableType { get; set; }
    }
}
