using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace SwissAcademic.Crm.Web
{
    public class CrmEntitySerializationBinder
        :
        ISerializationBinder
    {
        public Type BindToType(string assemblyName, string typeName)
        {
            return EntityNameResolver.GetTypeFromName(typeName);
        }
        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }
}
