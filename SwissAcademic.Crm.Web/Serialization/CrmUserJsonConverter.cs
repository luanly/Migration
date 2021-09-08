using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SwissAcademic.Crm.Web
{
    public class CrmUserJsonConverter
        :
        JsonConverter
    {
        #region Felder

        static Type _crmUserType = typeof(CrmUser);

        #endregion

        #region Konstruktor

        public CrmUserJsonConverter(CitaviSerializationContextType serializationType)
        {
            SerializationType = serializationType;
        }

        #endregion

        #region Eigenschaften

        #region CanRead

        public override bool CanRead => false;
        public override bool CanWrite => true;

        public CitaviSerializationContextType SerializationType { get; }

        #endregion

        #endregion

        #region Methoden

        #region CanConvert

        public override bool CanConvert(Type objectType)
        {
            return objectType == _crmUserType;
        }

        #endregion

        #region WriteJson

        public override void WriteJson(JsonWriter writer, object crmEntity, JsonSerializer serializer)
        {
            var type = crmEntity.GetType();

            var propertiesToSerialize = EntityNameResolver.GetJsonProperties(type, SerializationType);

            writer.WriteStartObject();

            var typeName = type.Name;
            var @namespace = type.Namespace;

            writer.WritePropertyName("$type");
            writer.WriteValue(string.Format("{0}.{1}, {0}", @namespace, typeName));

            foreach (var item in propertiesToSerialize)
            {
                var property = item.Property;
                var value = property.GetValue(crmEntity);
                switch (value)
                {
                    case null:
                        continue;

                    case DateTime date:
                        if (date == DateTime.MinValue)
                        {
                            continue;
                        }

                        break;

                    case Guid id:
                        if (id == Guid.Empty)
                        {
                            continue;
                        }

                        break;

                    case int i:
                        if (i == 0)
                        {
                            continue;
                        }

                        break;
                }
                var name = property.Name;
                if (item.DataMemberAttribute != null &&
                    !string.IsNullOrEmpty(item.DataMemberAttribute.Name))
                {
                    name = item.DataMemberAttribute.Name;
                }

                writer.WritePropertyName(name);
                serializer.Serialize(writer, value);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotImplementedException();

        #endregion

        #endregion
    }
}
