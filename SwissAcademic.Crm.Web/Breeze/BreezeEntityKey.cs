using System;

namespace SwissAcademic.Crm.Web.Breeze
{
    public class BreezeEntityKey
    {
        public BreezeEntityKey(object entity, object key)
        {
            var t = entity.GetType();
            EntityTypeName = t.Name + ":#" + t.Namespace;
            KeyValue = key;
        }
        public string EntityTypeName { get; set; }
        public object KeyValue { get; set; }
    }
}
