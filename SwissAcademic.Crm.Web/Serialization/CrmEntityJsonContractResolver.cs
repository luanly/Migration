using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace SwissAcademic.Crm.Web
{
    public class CrmEntityJsonContractResolver
        :
        DefaultContractResolver //Nicht von "unserem" erben
    {

        #region Felder



        #endregion

        public CrmEntityJsonContractResolver(CitaviSerializationContextType serializationType)
        {
            SerializationType = serializationType;
            CrmEntityConverter = new CrmEntityJsonConverter(serializationType);
            CrmUserConverter = new CrmUserJsonConverter(serializationType);
        }

        CrmEntityJsonConverter CrmEntityConverter { get; }
        CrmUserJsonConverter CrmUserConverter { get; }
        public CitaviSerializationContextType SerializationType { get; }


        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.Ignored = false;

            return property;
        }

        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (CrmEntityConverter.CanConvert(objectType))
            {
                return CrmEntityConverter;
            }
            if (CrmUserConverter.CanConvert(objectType))
            {
                return CrmUserConverter;
            }

            return base.ResolveContractConverter(objectType);
        }
    }
}
