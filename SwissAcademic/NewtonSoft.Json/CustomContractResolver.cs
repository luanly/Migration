using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Newtonsoft.Json
{
    public class CustomContractResolver
        :
        DefaultContractResolver
    {
        #region Constructors

        public CustomContractResolver()
        {
        }

        public CustomContractResolver(Type serializationAttributeType)
        {
            SerializationAttributeType = serializationAttributeType;
        }

        #endregion

        #region Properties

        protected Type SerializationAttributeType { get; set; }

        #endregion

        #region Methods

        #region CreateMemberValueProvider

        protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
        {
            var baseValueProvider = base.CreateMemberValueProvider(member);
            var isString = false;

            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    isString = ((FieldInfo)member).FieldType == typeof(string);
                    break;

                case MemberTypes.Property:
                    isString = ((PropertyInfo)member).PropertyType == typeof(string);
                    break;
            }

            return isString ?
                new StringValueProvider(baseValueProvider) :
                baseValueProvider;
        }

        #endregion

        #region CreateProperties

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            if (type.GetCustomAttribute<DataContractAttribute>() == null) return base.CreateProperties(type, memberSerialization);

            var loopType = type;
            var properties = new List<PropertyInfo>();

            while (loopType != null)
            {
                properties.AddRange(loopType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
                loopType = loopType.BaseType;
            }

            var list = (from property in properties
                        let dataMemberAttribute = property.GetCustomAttribute<DataMemberAttribute>(true)
                        let customDataMemberAttribute = SerializationAttributeType == null ? null :
                            (from attribute in Attribute.GetCustomAttributes(property, true)
                             where SerializationAttributeType.IsAssignableFrom(attribute.GetType())
                             select (CustomDataMemberAttribute)attribute).FirstOrDefault()
                        where dataMemberAttribute != null || customDataMemberAttribute != null
                        select customDataMemberAttribute != null ?
                            CreateProperty(property, memberSerialization, customDataMemberAttribute) :
                            CreateProperty(property, memberSerialization)).ToList();

            return list;
        }

        #endregion

        #region CreateProperty

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);

            if (jsonProperty.PropertyName.StartsWith("DataContract", StringComparison.Ordinal))
            {
                jsonProperty.PropertyName = jsonProperty.PropertyName.Substring("DataContract".Length);
            }

            return jsonProperty;
        }

        protected JsonProperty CreateProperty(PropertyInfo member, MemberSerialization memberSerialization, CustomDataMemberAttribute customDataMemberAttribute)
        {
            var jsonProperty = CreateProperty(member, memberSerialization);
            jsonProperty.Readable = member.CanRead;
            jsonProperty.Writable = member.CanWrite;

            jsonProperty.Ignored = false;
            if (customDataMemberAttribute.Name != null) jsonProperty.PropertyName = customDataMemberAttribute.Name;
            jsonProperty.ShouldSerialize = instance => customDataMemberAttribute.ShouldSerialize(member, jsonProperty, instance);

            return jsonProperty;
        }

        #endregion 

        #endregion
    }
}
