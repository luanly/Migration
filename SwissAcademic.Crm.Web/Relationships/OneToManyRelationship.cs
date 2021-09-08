using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    public class OneToManyRelationship<TSource, TTarget>
        :
        CrmEntityRelationship<TSource, TTarget>,
        ICrmRelationship
        where TTarget : CitaviCrmEntity
        where TSource : CitaviCrmEntity
    {
        #region Konstruktor

        public OneToManyRelationship(string relationshipName)
            :
            this(null, relationshipName, null)
        {

        }

        public OneToManyRelationship(TSource source, string relationshipName, string buildInLookupFieldName = null)
            :
            base(source, CrmEntityRelationshipType.OneToMany, relationshipName, buildInLookupFieldName)
        {

        }

        #endregion

        #region Methoden

        #region Add

        public void Add(TTarget item)
        {
            if (RelationshipType == CrmEntityRelationshipType.ManyToOne)
            {
                throw new NotSupportedException();
            }

            OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
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

        #region Get

        /// <summary>
        /// Wenn keine Attribute angegeben werden, wird NUR der Key übernommen. Alle Props einer Enität via EntityPropertySets abfragen
        /// </summary>
        public Task<IEnumerable<TTarget>> Get(params Enum[] includeAttributes)
          => Get(null, includeAttributes);

        /// <summary>
        /// Wenn keine Attribute angegeben werden, wird NUR der Key übernommen. Alle Props einer Enität via EntityPropertySets abfragen
        /// </summary>
        public async Task<IEnumerable<TTarget>> Get(CrmQueryInfo queryInfo, params Enum[] includeAttributes)
        {
            return await GetRelatedItems(queryInfo, includeAttributes);
        }

        #endregion

        #endregion
    }
}
