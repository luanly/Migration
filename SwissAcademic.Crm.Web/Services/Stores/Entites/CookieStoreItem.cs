using Microsoft.Azure.Cosmos.Table;
using System;

namespace SwissAcademic.Crm.Web
{
    public class CookieStoreItem
        :
        TableEntity
    {

        public DateTimeOffset? Expiration { get; set; }

        public string JsonCode { get; set; }

        public string SubjectId { get; set; }

        public int Version { get; set; }
    }
}
