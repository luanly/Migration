using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    //Es wird eine Intersect-Tabelle erstellt mit dem Namen der Relation
    //Diese Tabelle enthält folgende Attribute:
    //Enity1: Name des ID-Attributes + "one"
    //Enity2: Name des ID-Attributes + "two"
    //Als Beispiel für FetchXML -> GetCleverbridgeProducts

    public class ManyToManyRelationship<TSource, TTarget>
        :
        CrmEntityRelationship<TSource, TTarget>,
        ICrmRelationship
        where TTarget : CitaviCrmEntity
        where TSource : CitaviCrmEntity
    {
        #region Konstruktor

        public ManyToManyRelationship(string relationshipName)
            :
            this(null, relationshipName)
        {

        }
        public ManyToManyRelationship(TSource source, string relationshipName)
            :
            base(source, CrmEntityRelationshipType.ManyToMany, relationshipName)
        {

        }

        #endregion

        #region Methoden

        #region Add

        public void Add(TTarget item)
        {
            if (RelationshipType == CrmEntityRelationshipType.ManyToOne)
            {
                var exception = new NotSupportedException($"{nameof(RelationshipType)} == ({CrmEntityRelationshipType.ManyToOne})");
                Telemetry.TrackException(exception);
            }

            OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        #endregion

        #region Get

        /// <summary>
        /// Wenn keine Attribute angegeben werden, wird NUR der Key übernommen
        /// </summary>
        public Task<IEnumerable<TTarget>> Get(params Enum[] includeAttributes)
          => Get(null, includeAttributes);

        /// <summary>
        /// Wenn keine Attribute angegeben werden, wird NUR der Key übernommen
        /// </summary>
        public async Task<IEnumerable<TTarget>> Get(CrmQueryInfo queryInfo, params Enum[] includeAttributes)
        {
            return await GetRelatedItems(queryInfo, includeAttributes);
        }

        #endregion

        #region Remove

        public void Remove(TTarget item)
        {
            if (RelationshipType == CrmEntityRelationshipType.ManyToOne)
            {
                throw new NotSupportedException();
            }

            OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }

        #endregion

        #endregion
    }
}
