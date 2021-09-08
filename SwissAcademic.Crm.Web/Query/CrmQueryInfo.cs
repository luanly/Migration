using System;
using System.Collections.Generic;

namespace SwissAcademic.Crm.Web
{
    public class CrmQueryInfo
    {
        public CrmQueryInfo(int pageSize)
        {
            PageSize = pageSize;
        }
        public CrmQueryInfo(int pageSize, params Enum[] include)
        {
            PageSize = pageSize;
            Attributes.AddRange(include);
        }


        public bool HasMoreResults => !string.IsNullOrEmpty(NextLink);
        public string NextLink { get; internal set; }
        public int PageSize { get; }

        internal List<Enum> Attributes { get; set; } = new List<Enum>();
        public void Include(params Enum[] attributes) => Attributes.AddRange(attributes);
    }
}
