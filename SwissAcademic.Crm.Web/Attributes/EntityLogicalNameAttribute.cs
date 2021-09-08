using System;

namespace SwissAcademic.Crm.Web
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EntityLogicalNameAttribute
        :
        Attribute
    {
        public EntityLogicalNameAttribute(string logicalName) => LogicalName = logicalName;
        public string LogicalName { get; }
    }
}
