using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    public class ManyToOneRelationship<TSource, TTarget>
        :
        CrmEntityRelationship<TSource, TTarget>,
        ICrmRelationship
        where TTarget : CitaviCrmEntity
        where TSource : CitaviCrmEntity
    {
        #region Konstruktor

        public ManyToOneRelationship(string relationshipName)
            :
            this(null, relationshipName)
        {

        }
        public ManyToOneRelationship(TSource source, string relationshipName, string buildInLookupFieldName = null)
            :
            base(source, CrmEntityRelationshipType.ManyToOne, relationshipName, buildInLookupFieldName)
        {

        }

        #endregion

        #region Methoden

        #region Get

        /// <summary>
        /// Wenn keine Attribute angegeben werden, wird NUR der Key übernommen. Alle Props einer Enität via EntityPropertySets abfragen
        /// </summary>
        public async Task<TTarget> Get(params Enum[] includeAttributes)
         => await Get(null, includeAttributes);

        /// <summary>
        /// Wenn keine Attribute angegeben werden, wird NUR der Key übernommen. Alle Props einer Enität via EntityPropertySets abfragen
        /// </summary>
        public async Task<TTarget> Get(CrmQueryInfo queryInfo, params Enum[] includeAttributes)
        {
            return await GetRelatedItem(queryInfo, includeAttributes);
        }

        #endregion

        #region Set

        /// <summary>
        /// Die Relelation wird direkt erstellt, ohne zu Prüfen ob es bereites eine Relaltion gibt.
        /// Wenn es ein Replace sein könnte, IMMER SetOrReplace verwenden
        /// </summary>
        public void Set(TTarget target)
        {
            OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, target, null));
        }

        public async Task SetOrReplace(TTarget target)
        {
            var checkForExistingRelationship = true;
            if (Source.EntityState == EntityState.Created)
            {
                checkForExistingRelationship = false;
            }

            OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, target, checkForExistingRelationship ? await Get() : null));
        }

        #endregion

        #endregion
    }
}
