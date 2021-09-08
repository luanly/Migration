using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class BulkMailQueryPreview
    {
        public bool Success { get; set; }
        public string ExceptionMessage { get; set; }
        public int ReceiverCount { get; set; }
        public IEnumerable<BulkMailQueryPreviewContact> Contacts { get; set; }
    }
}
