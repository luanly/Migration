using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class CrmEntityRelationship<TSource, TTarget>
        where TTarget : CitaviCrmEntity
        where TSource : CitaviCrmEntity
    {
        #region Ereignisse

        #region Changed

        public event NotifyCollectionChangedEventHandler Changed;
        protected void OnChanged(NotifyCollectionChangedEventArgs e)
        {
            if (Changed != null)
            {
                Changed(this, e);
            }
        }

        #endregion

        #endregion

        #region Konstruktor

        public CrmEntityRelationship(TSource source, CrmEntityRelationshipType relationshipType, string relationshipName, string buildInLookupFieldName = null)
        {
            SourceEntityType = typeof(TSource);
            TargetEntityType = typeof(TTarget);

            var entityLogicalNameAttributeSource = EntityNameResolver.GetEntityLogicalName(SourceEntityType);
            var entityLogicalNameAttributeTarget = EntityNameResolver.GetEntityLogicalName(TargetEntityType);

            if (source != null)
            {
                Source = source;
                SourceKey = source.Key;
            }
            SourceEntityLogicalName = entityLogicalNameAttributeSource;
            TargetEntityLogicalName = entityLogicalNameAttributeTarget;

            RelationshipLogicalName = relationshipName;
            RelationshipType = relationshipType;
            if (relationshipType == CrmEntityRelationshipType.OneToMany ||
                relationshipType == CrmEntityRelationshipType.ManyToOne)
            {
                RelationshipQueryName = relationshipName + CrmConstants.LookupSuffix;
            }
            else
            {
                RelationshipQueryName = relationshipName;
            }

            if (!string.IsNullOrEmpty(buildInLookupFieldName))
            {
                RelationshipQueryName = buildInLookupFieldName;
            }

            if (RelationshipLogicalName.StartsWith(CrmConstants.CustomAttributePrefix + SourceEntityLogicalName))
            {
                ReferencedEntityLogicalName = SourceEntityLogicalName;
                ReferencingEntityLogicalName = TargetEntityLogicalName;
            }
            else
            {
                ReferencedEntityLogicalName = TargetEntityLogicalName;
                ReferencingEntityLogicalName = SourceEntityLogicalName;
            }
        }

        #endregion

        #region Eigenschaften

        #region RelationshipType

        public CrmEntityRelationshipType RelationshipType { get; private set; }

        #endregion

        #region RelationshipLogicalName

        //Name der Relation in der DB (z.b new_project_new_projectrole)
        public string RelationshipLogicalName { get; protected set; }

        #endregion

        #region RelationshipQueryName

        //Name der Relation für QueryExpression (z.b new_project_new_projectrole_lookup)
        public string RelationshipQueryName { get; protected set; }

        #endregion

        #region Source

        public CitaviCrmEntity Source { get; private set; }

        #endregion

        #region SourceKey

        public string SourceKey { get; private set; }

        #endregion

        #region SourceEntityLogicalName

        public string SourceEntityLogicalName { get; private set; }

        #endregion

        #region SourceEntityType

        public Type SourceEntityType { get; private set; }

        #endregion

        #region ReferencingEntity

        public string ReferencingEntityLogicalName { get; private set; }

        #endregion

        #region ReferencedEntityLogicalName

        public string ReferencedEntityLogicalName { get; private set; }

        #endregion

        #region TargetEntityLogicalName

        public string TargetEntityLogicalName { get; private set; }

        #endregion

        #region TargetEntityType

        public Type TargetEntityType { get; private set; }

        #endregion

        #endregion

        #region Methoden

        #region Count

        public async Task<int> Count(CrmDbContext context = null)
        {
            if (RelationshipType == CrmEntityRelationshipType.ManyToOne)
            {
                var exception = new NotSupportedException($"{nameof(RelationshipType)} == {RelationshipType}");
                Telemetry.TrackException(exception);
            }
            if (Source.Id == Guid.Empty)
            {
                var exception = new NotSupportedException($"{nameof(Source.Id)} == Guid.Empty");
                Telemetry.TrackException(exception);
            }

            string xml = null;
            FetchXmlExpression fetchXml;
            if (RelationshipType == CrmEntityRelationshipType.ManyToMany)
            {
                xml = string.Format(FetchXmlExpression.FetchXml_ManyToMany_Count,
                                    SourceEntityLogicalName, Source.Id.ToString(), RelationshipQueryName);
                fetchXml = new FetchXmlExpression(SourceEntityLogicalName, xml);
            }
            else
            {
                xml = string.Format(FetchXmlExpression.FetchXml_OneToMany_Count,
                                    TargetEntityLogicalName, Source.Id.ToString(), RelationshipQueryName);
                fetchXml = new FetchXmlExpression(TargetEntityLogicalName, xml);
            }

            if (context == null)
            {
                context = Source.Context;
            }

            return await CrmWebApi.RetrieveCount(fetchXml);
        }

        #endregion

        #region GetRelatedItem

        protected async Task<TTarget> GetRelatedItem(CrmQueryInfo queryInfo, Enum[] includeAttributes)
        {
            var includeAttributeNames = includeAttributes?.Select(p => EntityNameResolver.ResolveAttributeName(TargetEntityLogicalName, p.ToString())).ToList();

            if (RelationshipType != CrmEntityRelationshipType.ManyToOne)
            {
                var exception = new NotSupportedException($"{nameof(RelationshipType)} != {CrmEntityRelationshipType.ManyToOne}");
                Telemetry.TrackException(exception);
            }

            var context = Source.Context;
            if (EntityNameResolver.HasKeyAttribute(TargetEntityLogicalName))
            {
                includeAttributeNames.AddIfNotExists(CrmAttributeNames.Key);
            }

            var result = await CrmWebApi.GetRelatedEntities(this, includeAttributeNames, queryInfo);
            if (result == null || !result.Any())
            {
                return null;
            }

            var entity = result.First() as TTarget;
            if (context != null)
            {
                context.Attach(entity);
            }
            return entity;
        }

        #endregion

        #region GetRelatedItems

        protected async Task<IEnumerable<TTarget>> GetRelatedItems(CrmQueryInfo queryInfo, Enum[] includeAttributes)
        {
            var includeAttributeNames = includeAttributes.Select(p => EntityNameResolver.ResolveAttributeName(TargetEntityLogicalName, p.ToString())).ToList();
            if (EntityNameResolver.HasKeyAttribute(TargetEntityLogicalName))
            {
                includeAttributeNames.AddIfNotExists(CrmAttributeNames.Key);
            }

            if (RelationshipType == CrmEntityRelationshipType.ManyToOne)
            {
                var exception = new NotSupportedException($"{nameof(RelationshipType)} == {CrmEntityRelationshipType.ManyToOne}");
                Telemetry.TrackException(exception);
            }

            try
            {
                var result = await CrmWebApi.GetRelatedEntities(this, includeAttributeNames, queryInfo);
                if (result == null || !result.Any())
                {
                    return Enumerable.Empty<TTarget>();
                }

                if (Source.Context != null)
                {
                    result.ForEach(i => Source.Context.Attach(i));
                }
                return result;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
                return Enumerable.Empty<TTarget>();
            }
        }

        #endregion

        #endregion
    }
}
