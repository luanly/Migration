using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SwissAcademic.Crm.Web
{
    public class CrmEntityJsonConverter
        :
        JsonConverter
    {
        #region Felder

        static Type _crmEntityType = typeof(CitaviCrmEntity);
        static HashSet<string> _legacyProperties = new HashSet<string>(new[] 
        {
            "CurrentTwoFactorAuthStatus",
            "TwoFactorAuthMode"
        });

        #endregion

        #region Konstruktor

        public CrmEntityJsonConverter(CitaviSerializationContextType serializationType)
        {
            SerializationType = serializationType;
        }

        #endregion

        #region Eigenschaften

        #region CanRead

        public override bool CanRead => true;
        public override bool CanWrite => true;

        public CitaviSerializationContextType SerializationType { get; }

        #endregion

        #endregion

        #region Methoden

        #region CanConvert

        public override bool CanConvert(Type objectType)
        {
            return _crmEntityType.IsAssignableFrom(objectType);
        }

        #endregion

        #region ReadJson

        //ME: Überarbeiten zu https://stackoverflow.com/questions/17660097/is-it-possible-to-speed-this-method-up/17669142#17669142

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var instance = Activator.CreateInstance(objectType, true) as CitaviCrmEntity;

            foreach (var jp in jo.Properties())
            {
                if (jp.Name == "$type")
                {
                    continue;
                }

                if (jp.Name == "@odata.nextLink")
                {
                    continue;
                }

                if (jp.Name == "entityAspect")
                {
                    continue;
                }

                if (jp.Name == "@odata.etag")
                {
                    instance.ETag = jp.Value.ToString();
                    continue;
                }
                if (_legacyProperties.Contains(jp.Name))
                {
                    continue;
                }

                var name = jp.Name;
                var prop = EntityNameResolver.GetJsonProperty(objectType, name);

                if (prop == null)
                {
                    if (name.Contains("."))
                    {
                        var aliased = name.Split('.');
                        var aliasedName = aliased[0];
                        var entityLocalName = aliased[1];
                        var attributeName = aliased[2];

                        var aliasedProperty = EntityNameResolver.GetJsonProperty(entityLocalName, attributeName);
                        if (aliasedProperty == null)
                        {
                            Telemetry.TrackTrace($"ReadJson: aliasedProperty == null: {entityLocalName}|{attributeName}|{name}", SeverityLevel.Warning);
                            continue;
                        }
                        if (RelationshipResolver.IsIntersectType(aliased[0]))
                        {
                            entityLocalName = aliased[0];
                        }
                        AliasedValue aliasedValue;

                        aliasedValue = new AliasedValue(entityLocalName, attributeName, jp.Value.ToObject(aliasedProperty.PropertyType));
                        instance.Attributes.Add(name, aliasedValue);
                        continue;
                    }
                    if (name.StartsWith("_") &&
                        name.EndsWith("_value"))
                    {
                        //_new_basecbproductid_value
                        //Das sind EntityReferenceId (Verweise auf eine andere Entität)
                        name = name.Substring(1, name.Length - 7);

                        if(name == "masterid")
						{
                            //Merged "WinnerId"
                            instance.Attributes.Add(name, jp.Value.ToString());
                            continue;
                        }
                        prop = EntityNameResolver.GetJsonProperty(objectType, name);
                    }
                    else if (name.StartsWith("new_"))
                    {
                        //custom property von uns. Wir fügen das den Attributen hinzu
                        //bsp. skippluginexecution
                        instance.Attributes.Add(name, jp.Value.ToString());
                        continue;
                    }
                    if (prop == null)
                    {
                        continue;
                    }
                }

                if (!prop.CanWrite)
                {
                    continue;
                }
                if (jp.Value.Type == JTokenType.Null &&
                   Nullable.GetUnderlyingType(prop.PropertyType) == null)
                {
                    continue;
                }
                prop.SetValue(instance, jp.Value.ToObject(prop.PropertyType, serializer));
            }

            return instance;
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

        #endregion

        #endregion
    }
}
