using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    public static class EntityNameResolver
    {
        #region Felder

        static HashSet<string> _idAttributes;
        static HashSet<string> _builtInAttributes;
        static ConcurrentDictionary<(string entityName, string attributeName), string> _attributeNames = new ConcurrentDictionary<(string entityName, string attributeName), string>();
        static ConcurrentDictionary<(string relationshipname, string entityLogicalName, string attributeName), string> _aliasedAttributeNames = new ConcurrentDictionary<(string relationshipname, string entityLogicalName, string attributeName), string>();
        static Type _crmEntityType = typeof(CitaviCrmEntity);
        static Type _crmUserType = typeof(CrmUser);
        static Dictionary<Type, List<(PropertyInfo Property, DataMemberAttribute DataMemberAttribute)>> _jsonProperties_all = new Dictionary<Type, List<(PropertyInfo Property, DataMemberAttribute DataMemberAttribute)>>();
        static Dictionary<Type, List<(PropertyInfo Property, DataMemberAttribute DataMemberAttribute)>> _jsonProperties_writable = new Dictionary<Type, List<(PropertyInfo Property, DataMemberAttribute DataMemberAttribute)>>();
        static Dictionary<Type, List<(PropertyInfo Property, DataMemberAttribute DataMemberAttribute)>> _jsonProperties_breeze = new Dictionary<Type, List<(PropertyInfo Property, DataMemberAttribute DataMemberAttribute)>>();
        static Dictionary<string, string> _typeNamesSignlarPlural = new Dictionary<string, string>();
        static Dictionary<string, string> _typeNamesPluralSignlar = new Dictionary<string, string>();

        static Dictionary<string, Type> _breezeTypes = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
        static Dictionary<string, Type> _types = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

        #endregion

        #region Eigenschaften

        public static IEnumerable<CrmEntityKnownType> KnownTypes { get; private set; }

        #endregion

        #region Methoden

        #region GetAliasedAttributeName

        public static string GetAliasedAttributeName(string relationshipname, string entityLogicalName, string attributeName)
        {
            string name = "";
            if (_aliasedAttributeNames.TryGetValue((relationshipname, entityLogicalName, attributeName), out name))
            {
                return name;
            }

            if (attributeName.StartsWith(CrmConstants.CustomAttributePrefix))
            {
                name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", GetEntityAliasePrefix(relationshipname), entityLogicalName, attributeName);
            }
            else if (IsBuiltInAttribute(entityLogicalName, attributeName))
            {
                name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", GetEntityAliasePrefix(relationshipname), entityLogicalName, attributeName);
            }
            else
            {
                name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.new_{2}", GetEntityAliasePrefix(relationshipname), entityLogicalName, attributeName);
            }

            _aliasedAttributeNames.AddOrUpdate((relationshipname, entityLogicalName, attributeName), name, (e, a) => name);
            return name;
        }

        #endregion

        #region GetTypeFromBreezeName

        public static Type GetTypeFromBreezeName(string name) => _breezeTypes[name];

        #endregion

        #region GetTypeFromBreezeName

        public static Type GetTypeFromName(string name) => _types[name];

        #endregion

        #region GetEntityAliasePrefix

        internal static string GetEntityAliasePrefix(string relationshipname)
        {
            //Die Aliase sind zulang (Max. 128 Zeichnen)
            return relationshipname.Replace(CrmConstants.CustomAttributePrefix, string.Empty);
        }

        #endregion

        #region GetSignlar/PluralTypeName

        public static string GetSignlarTypeName(string pluralname) => _typeNamesPluralSignlar[pluralname];
        public static string GetPluralTypeName(string pluralname) => _typeNamesSignlarPlural[pluralname];

        #endregion

        #region GetEntityLogicalName

        public static string GetEntityLogicalName<T>()
        {
            var type = typeof(T);
            return KnownTypes.First(t => t.Type == type).EntityLogicalName;
        }
        public static string GetEntityLogicalName(Type type)
        {
            return KnownTypes.First(t => t.Type == type).EntityLogicalName;
        }

        #endregion

        #region HasKeyAttribute

        public static bool HasKeyAttribute(string entityName)
        {
            return entityName != CrmEntityNames.Annotation &&
                   entityName != CrmEntityNames.Email &&
                   entityName != CrmEntityNames.SystemUser &&
                   entityName != CrmEntityNames.Team &&
                   entityName != CrmEntityNames.TransactionCurrency &&
                   entityName != CrmEntityNames.Workflow;
        }

        #endregion

        #region HasStateCodeAttribute

        public static bool HasStateCodeAttribute(string entityName)
        {
            return entityName != CrmEntityNames.Annotation &&
                   entityName != CrmEntityNames.Workflow &&
                   entityName != CrmEntityNames.SystemUser &&
                   entityName != CrmEntityNames.Team;
        }

        #endregion

        #region IsBuiltInAttribute

        internal static bool IsBuiltInAttribute(string entityName, string propertyName)
        {
            propertyName = propertyName.ToLowerInvariant();
            entityName = entityName.ToLowerInvariant();
            return _builtInAttributes.Contains(entityName + propertyName);
        }

        #endregion

        #region IsIdAttribute

        public static bool IsIdAttribute(string attributeName) => _idAttributes.Contains(attributeName);

        #endregion

        #region Initialize

        internal static void Initialize()
        {
            if (_builtInAttributes?.Any() == true)
            {
                return;
            }

            _builtInAttributes = new HashSet<string>();
            _idAttributes = new HashSet<string>();

            try
            {
                var typesInAssembly = GetTypesSave();
                var types = from type in typesInAssembly
                            where type.BaseType == typeof(CitaviCrmEntity)
                            select type;

                var crmProperties = from type in types
                                    from property in type.GetProperties()
                                    from attr in property.GetCustomAttributes(true)
                                    where attr.GetType() == typeof(CrmPropertyAttribute) &&
                                          ((CrmPropertyAttribute)attr).IsBuiltInAttribute
                                    select new
                                    {
                                        PropertyName = ((CrmPropertyAttribute)attr).PropertyName,
                                        EntityLogicalName = (type.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true).FirstOrDefault() as EntityLogicalNameAttribute).LogicalName
                                    };

                _builtInAttributes.AddRange(crmProperties.Select(i => i.EntityLogicalName.ToLowerInvariant() + i.PropertyName.ToLowerInvariant()));

                var genericListType = typeof(List<>);
                KnownTypes = (from type in typesInAssembly
                              from attribute in type.GetCustomAttributes(typeof(EntityLogicalNameAttribute))
                              select new CrmEntityKnownType
                              {
                                  Type = type,
                                  EntityLogicalName = ((EntityLogicalNameAttribute)attribute).LogicalName,
                                  IEnumerableType = genericListType.MakeGenericType(type)

                              }).ToList();

                foreach (var type in KnownTypes)
                {
                    _breezeTypes.Add($"{type.Type.Name}:#SwissAcademic.Crm.Web", type.Type);
                    _types.Add(type.Type.FullName, type.Type);
                }
                _types.Add(typeof(CrmUser).FullName, typeof(CrmUser));

                foreach (var type in types)
                {
                    LoadJsonProperties(type);
                }

                foreach (var prop in _jsonProperties_all)
                {
                    var dic = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
                    var kt = KnownTypes.First(t => t.Type == prop.Key);
                    foreach (var pi in prop.Value)
                    {
                        if (pi.DataMemberAttribute != null &&
                            !string.IsNullOrEmpty(pi.DataMemberAttribute.Name))
                        {
                            dic.Add(pi.DataMemberAttribute.Name, pi.Property);
                        }
                        if (!dic.ContainsKey(pi.Property.Name))
                        {
                            dic.Add(pi.Property.Name, pi.Property);
                        }
                        if (prop.Key == _crmUserType)
                        {
                            continue;
                        }

                        var name = ResolveAttributeName(kt.EntityLogicalName, pi.Property.Name.ToLowerInvariant());
                        if (!dic.ContainsKey(name))
                        {
                            dic.Add(name, pi.Property);
                        }

                    }
                    _jsonPropertyNamesByType.Add(prop.Key, dic);
                    _jsonPropertyNamesByEntityLogicalName.Add(kt.EntityLogicalName, dic);
                }

                foreach (var kt in KnownTypes)
                {
                    var singular = kt.EntityLogicalName;
                    var plural = kt.EntityLogicalName + "s";
                    if (kt.EntityLogicalName == CrmEntityNames.OrderProcess ||
                        kt.EntityLogicalName == CrmEntityNames.CampusContractStatistic)
                    {
                        plural = kt.EntityLogicalName + "es";
                    }
                    else if (kt.EntityLogicalName == CrmEntityNames.BulkMailQuery)
                    {
                        plural = "new_bulkmailqueries";
                    }
                    else if (kt.EntityLogicalName == CrmEntityNames.Delivery)
                    {
                        plural = "new_deliveries";
                    }
                    else if (kt.EntityLogicalName == CrmEntityNames.TransactionCurrency)
                    {
                        plural = "transactioncurrencies";
                    }
                    _typeNamesPluralSignlar[plural] = singular;
                    _typeNamesSignlarPlural[singular] = plural;
                    _idAttributes.Add(ResolveAttributeName(kt.EntityLogicalName, "id"));
                }

                LoadJsonProperties(_crmUserType);

            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var loaderEx in ex.LoaderExceptions)
                {
                    Telemetry.TrackException(loaderEx, SeverityLevel.Error, flow: ExceptionFlow.Eat);
                }
                Telemetry.TrackException(ex, SeverityLevel.Error, flow: ExceptionFlow.Rethrow);
            }
        }

        #endregion

        #region LoadJsonProperties

        static Dictionary<Type, Dictionary<string, PropertyInfo>> _jsonPropertyNamesByType = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
        static Dictionary<string, Dictionary<string, PropertyInfo>> _jsonPropertyNamesByEntityLogicalName = new Dictionary<string, Dictionary<string, PropertyInfo>>();
        static void LoadJsonProperties(Type type)
        {
            _jsonProperties_all[type] = new List<(PropertyInfo Property, DataMemberAttribute DataMemberAttribute)>();
            _jsonProperties_writable[type] = new List<(PropertyInfo Property, DataMemberAttribute DataMemberAttribute)>();
            _jsonProperties_breeze[type] = new List<(PropertyInfo Property, DataMemberAttribute DataMemberAttribute)>();

            foreach (var property in type.GetProperties())
            {
                if (property.DeclaringType != _crmUserType && !_crmEntityType.IsAssignableFrom(property.DeclaringType))
                {
                    continue;
                }

                if (property.CustomAttributes.Any(ca => ca.AttributeType == typeof(CrmPropertyAttribute)) ||
                   property.CustomAttributes.Any(ca => ca.AttributeType == typeof(CacheDataMemberAttribute)) ||
                   property.CustomAttributes.Any(ca => ca.AttributeType == typeof(DataMemberAttribute)))
                {
                    var attribute = property.GetCustomAttributes(typeof(DataMemberAttribute)).FirstOrDefault();
                    var cattribute = property.GetCustomAttributes(typeof(CrmPropertyAttribute)).FirstOrDefault() as CrmPropertyAttribute;
                    _jsonProperties_all[type].Add((property, attribute as DataMemberAttribute));
                    if (cattribute?.NoCache == true)
                    {
                        continue;
                    }

                    _jsonProperties_writable[type].Add((property, attribute as DataMemberAttribute));
                    if (attribute != null)
                    {
                        _jsonProperties_breeze[type].Add((property, attribute as DataMemberAttribute));
                    }
                }

            }
        }

        #endregion

        #region GetJsonProperties

        internal static List<(PropertyInfo Property, DataMemberAttribute DataMemberAttribute)> GetJsonProperties(Type type, CitaviSerializationContextType serializationType)
        {
            if (serializationType == CitaviSerializationContextType.Breeze)
            {
                return _jsonProperties_breeze[type];
            }
            return _jsonProperties_writable[type];
        }

        internal static PropertyInfo GetJsonProperty(Type entityType, string propertyName)
        {
            if (_jsonPropertyNamesByType.ContainsKey(entityType) &&
                _jsonPropertyNamesByType[entityType].ContainsKey(propertyName))
            {
                return _jsonPropertyNamesByType[entityType][propertyName];
            }
            return null;
        }

        internal static PropertyInfo GetJsonProperty(string entityLogicalName, string propertyName)
        {
            if (_jsonPropertyNamesByEntityLogicalName.ContainsKey(entityLogicalName) &&
                _jsonPropertyNamesByEntityLogicalName[entityLogicalName].ContainsKey(propertyName))
            {
                return _jsonPropertyNamesByEntityLogicalName[entityLogicalName][propertyName];
            }
            return null;
        }

        #endregion

        #region GetTypesSave

        static IEnumerable<Type> GetTypesSave()
        {
            var types = new List<Type>();
            try
            {
                types = Assembly.GetExecutingAssembly().GetTypes().ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.ToList();
            }

            return types.Where(t => t != null);
        }

        #endregion

        #region ResolveAttributeName

        public static string ResolveAttributeName(string entityName_caller, string attributeName_caller)
        {
            var fullName = "";
            if (_attributeNames.TryGetValue((entityName_caller, attributeName_caller), out fullName))
            {
                return fullName;
            }

            try
            {
                if (_builtInAttributes == null)
                {
                    Initialize();
                }

                var attributeName = attributeName_caller.ToLowerInvariant();
                
                if(attributeName.EndsWith("@odata.bind"))
				{
                    fullName = attributeName;
                    return fullName;
                }

                var entityName = entityName_caller.ToLowerInvariant();

                if (attributeName == "id")
                {
                    fullName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", entityName, attributeName);
                    return fullName;
                }

                if (IsBuiltInAttribute(entityName, attributeName))
                {
                    fullName = attributeName;
                    return fullName;
                }

                switch (attributeName)
                {
                    case "createdby":
                    case "modifiedby":
                        return attributeName;

                    case CrmAttributeNames.LastModifiedBy:
                        {
                            fullName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_lookup", entityName, CrmAttributeNames.LastModifiedBy);
                            return fullName;
                        }
                }

                if (attributeName.StartsWith(CrmConstants.CustomAttributePrefix))
                {
                    fullName = attributeName;
                    return fullName;
                }

                fullName = string.Format(CultureInfo.InvariantCulture, "new_{0}", attributeName);
                return fullName;
            }
            finally
            {
                _attributeNames.AddOrUpdate((entityName_caller, attributeName_caller), fullName, (e, a) => fullName);
            }
        }

        #endregion

        #endregion
    }
}
