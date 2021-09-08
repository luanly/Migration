using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure.Swagger;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [SwaggerIgnore]
    public class CitaviCrmEntity
        :
        IHasCacheETag
    {
        #region Ereignisse

        #region RelationshipChanged

        public event NotifyCollectionChangedEventHandler RelationshipChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #endregion

        #region Felder

        internal CrmDbContext _context;
        internal string _idAttributeName;

        #endregion

        #region Konstruktor

        public CitaviCrmEntity(string logicalName)
        {
            LogicalName = logicalName;
            _idAttributeName = LogicalName + "id";
        }

        #endregion

        #region Eigenschaften

        #region Attributes

        public Dictionary<string, object> Attributes { get; } = new Dictionary<string, object>();
        public object this[string attributeName]
        {
            get => Attributes[attributeName];
            private set => Attributes[attributeName] = value;
        }

        #endregion

        #region CreatedOn

        //FYI: Wird von VoucherBlock überschrieben, da Serialisiert werden muss

        [CrmProperty(IsBuiltInAttribute = true)]
        public DateTime CreatedOn
        {
            get
            {
                return GetValue<DateTime>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region CacheETag

        [CacheDataMember]
        public string CacheETag { get; set; }

        #endregion

        #region Crm4Id

        [CrmProperty]
        public string Crm4Id
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Context

        public CrmDbContext Context
        {
            get { return _context; }
        }

        #endregion

        #region EntityState

        [CrmProperty(IsBuiltInAttribute = true, NoCache = true)]
        public EntityState? EntityState
        {
            get
            {
                return GetValue<EntityState?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ETag

        [CacheDataMember]
        public string ETag
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Id

        [CrmProperty(IsBuiltInAttribute = true)]
        public Guid Id
        {
            get
            {
                return GetValue<Guid>();
            }

            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Key

        internal string _key;
        [CrmProperty]
        [DataMember]
        public string Key
        {
            get
            {
                if (!string.IsNullOrEmpty(_key))
                {
                    return _key;
                }

                _key = GetValue<string>();
                return _key;
            }
            set
            {
                _key = value;
                SetValueByRef(ref _key);
            }
        }

        #endregion

        #region ModifiedOn

        [CrmProperty(IsBuiltInAttribute = true)]
        public DateTime ModifiedOn
        {
            get
            {
                return GetValue<DateTime>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region PropertyTypeEnum

        protected virtual Type PropertyEnumType { get; }


        #endregion

        #region StateCode

        [CrmProperty(IsBuiltInAttribute = true, NoCache = true)]
        public StateCode? StateCode
        {
            get
            {
                return GetValue<StateCode?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region StatusCode

        [CrmProperty(IsBuiltInAttribute = true, NoCache = true)]
        public StatusCode? StatusCode
        {
            get
            {
                return GetValue<StatusCode?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #endregion

        #region Methoden

        #region Activate

        public void Activate() => StatusCode = Web.StatusCode.Active;

        #endregion

        #region AddIntersectAttribute

        //N-N Relationen
        //Campusvertrag <-> Product via Intersect Tabelle "new_campuscontract_n_citaviproduct"
        //Da müssen wir die Werte (Ids der Produkte) in einer Hilfsliste uns merken
        internal virtual void AddIntersectAttribute(string name, object value)
        {
            //Muss überschrieben werden vom "Parent"
        }

        #endregion

        #region CanSetValue

        internal virtual bool CanSetBreezeValue(string entity, string attributeName)
        {
            return CanSetBreezeValue(attributeName);
        }
        internal virtual bool CanSetBreezeValue(string attributeName)
        {
            var isDefined = EntityPropertySets.IsWritableBreezeProperty(PropertyEnumType, attributeName);
            if (!isDefined)
            {
                return false;
            }

            switch (attributeName)
            {
                case CrmAttributeNames.Key:
                case CrmAttributeNames.LastModifiedBy:
                case CrmAttributeNames.LastModifiedOn:
                    return false;

                default:
                    return true;
            }
        }

        #endregion

        #region CreateKey

        internal void CreateKey()
        {
            if (!string.IsNullOrEmpty(Key))
            {
                var exception = new NotSupportedException($"{nameof(Key)} is not empty");
                Telemetry.TrackException(exception);
            }

            Key = SwissAcademic.Security.PasswordGenerator.WebKey.Generate();
        }

        #endregion

        #region Clone

        protected T Clone<T>(params Enum[] attributesToClone)
            where T : CitaviCrmEntity
        {
            if (Context == null)
            {
                throw new NotSupportedException("Context must not be null");
            }

            var attributesToCloneNames = attributesToClone.Select(p => EntityNameResolver.ResolveAttributeName(LogicalName, p.ToString()));
            var clone = Context.Create<T>();

            if (!attributesToCloneNames.Any())
            {
                attributesToCloneNames = EntityPropertySets.GetFullPropertySet<T>().Select(p => EntityNameResolver.ResolveAttributeName(LogicalName, p.ToString()));
            }
            foreach (var attribute in attributesToCloneNames)
            {
                if (!Attributes.ContainsKey(attribute))
                {
                    continue;
                }

                if (clone.Attributes.ContainsKey(attribute))
                {
                    continue; //bsp. Key, Id, Status
                }

                clone.SetValue(Attributes[attribute], attribute);
            }

            return clone;
        }

        #endregion

        #region Deactivate

        public void Deactivate() => StatusCode = Web.StatusCode.Inactive;

        #endregion

        #region ExportSchema

        public DataTable ExportSchema()
        {
            var table = new DataTable();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();


            var type = GetType();
            table.Rows.Add("$Name", "$InternalName", "", "");
            table.Rows.Add(type.Name, LogicalName, "", "");
            table.Rows.Add();
            table.Rows.Add("$Properties");
            table.Rows.Add();

            var props = EntityPropertySets.GetFullPropertySet(type);
            table.Rows.Add("$Name", "$InternalName", "$Type");
            foreach (var prop in props)
            {
                var fld = type.GetProperty(prop.ToString(), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (fld == null)
                {
                    continue;
                }
                var propertyType = fld.PropertyType;

                if (propertyType.IsGenericType &&
                    propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propertyType = propertyType.GetGenericArguments()[0];
                }

                if (propertyType.IsEnum)
                {
                    propertyType = typeof(Enum);
                }

                table.Rows.Add(fld.Name, EntityNameResolver.ResolveAttributeName(LogicalName, fld.Name), propertyType.Name, "");
            }


            table.Rows.Add();
            table.Rows.Add("$Relationships");
            table.Rows.Add();
            table.Rows.Add("$Name", "$InternalName", "$To", "$Type");

            foreach (var prop in type.GetProperties())
            {
                if (prop.PropertyType.GetInterface(nameof(ICrmRelationship)) != null)
                {
                    var relationship = (ICrmRelationship)prop.GetValue(this);
                    var rel_type = relationship.RelationshipType;
                    var to = relationship.TargetEntityLogicalName + "#" + relationship.TargetEntityType.Name;
                    table.Rows.Add(prop.Name, relationship.RelationshipLogicalName, to, rel_type);
                }
            }

            return table;
        }

        #endregion

        #region GetValue

        protected T GetValue<T>([CallerMemberName] string prop = "")
        {
            var propertyName = EntityNameResolver.ResolveAttributeName(LogicalName, prop);
            return GetAttributeValue<T>(propertyName);
        }

        #endregion

        #region GetAliasedValue

        internal T GetAliasedValue<T>(string relationshipname, string entityLogicalName, Enum attribute)
        {
            return GetAliasedValue<T>(relationshipname, entityLogicalName, attribute.ToString());
        }

        internal T GetAliasedValue<T>(string relationshipname, string entityLogicalName, string attributeName)
        {
            attributeName = attributeName.ToLowerInvariant();
            object aliased;
            var aliasedName = string.Format("{0}.{1}.{2}", EntityNameResolver.GetEntityAliasePrefix(relationshipname), entityLogicalName, EntityNameResolver.ResolveAttributeName(entityLogicalName, attributeName));
            if (!Attributes.TryGetValue(aliasedName, out aliased))
            {
                return default(T);
            }

            if (aliased is AliasedValue)
            {
                var value = ((AliasedValue)aliased).Value;
                if (value is JToken)
                {
                    var jv = (T)((JToken)value).ToObject(typeof(T));
                    Attributes[aliasedName] = jv;
                    return jv;
                }
                if (value == null)
                {
                    return default(T);
                }
                return (T)value;
            }
            if (aliased == null)
            {
                return default(T);
            }
            return (T)aliased;
        }

        #endregion

        #region GetAttributeValue

        T GetAttributeValue<T>(string name)
        {
            if (!Attributes.ContainsKey(name))
            {
                return default(T);
            }
            var val = Attributes[name];
            if (val == null)
            {
                return default(T);
            }
            return (T)val;
        }

        #endregion

        #region HasAttribute

        internal bool HasAttribute(Enum prop)
            => HasAttribute(prop.ToString());
        internal bool HasAttribute(string prop)
        {
            var propertyName = EntityNameResolver.ResolveAttributeName(LogicalName, prop);
            return Attributes.ContainsKey(propertyName);
        }

        #endregion

        #region HasDbUpdateAsync

        public Task<bool> HasDbUpdateAsync()
         => CrmWebApi.HasUpdate(this);

        #endregion

        #region LogicalName
        public string LogicalName { get; set; }

        #endregion

        #region Observe

        protected void Observe<T, TParent>(CrmEntityRelationship<T, TParent> entities, bool start)
            where T : CitaviCrmEntity
            where TParent : CitaviCrmEntity
        {
            if (start)
            {
                entities.Changed += Entity_RelationshipChanged;
            }
            else
            {
                entities.Changed -= Entity_RelationshipChanged;
            }
        }

        #endregion

        #region SetAliasedValue

        internal void SetAliasedValue(string aliasedName, AliasedValue value)
        {
            SetAttributeValue(aliasedName, value);
        }
        internal void SetAliasedValue(string relationshipname, string entityLogicalName, Enum attribute, object value)
        {
            SetAliasedValue(relationshipname, entityLogicalName, EntityNameResolver.ResolveAttributeName(entityLogicalName, attribute.ToString()), value);
        }
        internal void SetAliasedValue(string relationshipname, string entityLogicalName, string attributeName, object value)
        {
            attributeName = attributeName.ToLowerInvariant();
            var name = EntityNameResolver.GetAliasedAttributeName(relationshipname, entityLogicalName, attributeName);
            SetAttributeValue(name, new AliasedValue(entityLogicalName, attributeName, value));
        }

        #endregion

        #region SetAttributeValue

        protected void SetAttributeValue(string name, object value)
        {
            Attributes[name] = value;
        }

        #endregion

        #region SetCreatedOn

        /// <summary>
        /// Can only be used during record creation
        /// </summary>
        /// <param name="dateTime"></param>
        internal void SetCreatedOn(DateTime dateTime)
		{
            SetAttributeValue("overriddencreatedon", dateTime);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("overriddencreatedon"));
        }

        #endregion

        #region SetValue

        public virtual void SetValueWithChecks(string attributeLogicalName, object value, bool raisePropertyChangedEvent = false)
        {
            if (attributeLogicalName == _idAttributeName)
            {
                Id = (Guid)value;
                return;
            }

            if (EntityNameResolver.IsBuiltInAttribute(LogicalName, attributeLogicalName))
            {
                SetAttributeValue(attributeLogicalName, value);
            }
            else if (value is DateTime)
            {
                if ((DateTime)value < CrmDataTypeConstants.MinDate)
                {
                    value = CrmDataTypeConstants.MinDate;
                }

                SetAttributeValue(attributeLogicalName, value);
            }
            else
            {
                SetAttributeValue(attributeLogicalName, value);
            }

            if (raisePropertyChangedEvent)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attributeLogicalName));
            }
        }

        protected void SetValue<T>(T value, [CallerMemberName] string prop = "")
        {
            var propertyName = EntityNameResolver.ResolveAttributeName(LogicalName, prop);

			switch (value)
			{
                case string val:
                    SetAttributeValue(propertyName, val.RemoveNonStandardWhitespace());
                    break;

                default:
                    SetAttributeValue(propertyName, value);
                    break;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetValueByRef(ref string value, [CallerMemberName] string prop = "")
        {
            var propertyName = EntityNameResolver.ResolveAttributeName(LogicalName, prop);
            SetAttributeValue(propertyName, value.RemoveNonStandardWhitespace());
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion

        #region Ereignishandler

        #region RelationshipChanged

        void Entity_RelationshipChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnRelationshipChanged(sender, e);
            RelationshipChanged?.Invoke(sender, e);
        }

        protected virtual void OnRelationshipChanged(object sender, NotifyCollectionChangedEventArgs e) { }

        #endregion

        #endregion
    }
}
