using Microsoft.Azure.Cosmos.Table;
using System;

namespace SwissAcademic.Crm.Web
{
    public class PersistedGrantStoreItem
        :
        TableEntity
    {
        public string ClientId { get; set; }

        public DateTime? Expiration { get; set; }

        public string JsonCode { get; set; }

        public string SubjectId { get; set; }

        public int Version { get; set; }
    }
}
