using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ODataQueryAttribute
        :
        Attribute
    {
        public ODataQueryAttribute([CallerMemberName] string queryName = null)
        {
            QueryName = queryName;
        }

        public string QueryName { get; set; }
    }
}
