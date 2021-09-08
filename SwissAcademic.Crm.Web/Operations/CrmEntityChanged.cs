using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SwissAcademic.Crm.Web
{
    public class CrmEntityChanged
    {
        #region Eigenschaften

        public List<(ICrmRelationship relationship, IEnumerable<CitaviCrmEntity> relatatedEntities)> Associate { get; } = new List<(ICrmRelationship relationship, IEnumerable<CitaviCrmEntity> relatatedEntities)>();
        public CitaviCrmEntity Entity { get; set; }
        public bool Deleted { get; set; }
        public bool HasRelationshipChanges => Associate.Any() || Disassociate.Any();
        public CrmEntityMergeRequest Merge { get; set; }
        public List<string> Properties { get; } = new List<string>();
        public string ContentId { get; internal set; }
        public List<(ICrmRelationship relationship, IEnumerable<CitaviCrmEntity> relatatedEntities)> Disassociate { get; } = new List<(ICrmRelationship relationship, IEnumerable<CitaviCrmEntity> relatatedEntities)>();

        public string TransactionId { get; set; }

        public CrmWebApiSaveOperations Operations { get; private set; }

        public int OperationsCount { get; private set; }

        #endregion

        #region Methoden

        public void CalculateEstimatedOperations()
        {
            var operations = CrmWebApiSaveOperations.None;
            if (Entity?.EntityState == EntityState.Created && !HasRelationshipChanges && !Deleted)
            {
                operations |= CrmWebApiSaveOperations.Create;
            }
            if (Entity?.EntityState != EntityState.Created && Properties.Any() && !Deleted)
            {
                operations |= CrmWebApiSaveOperations.Update;
            }
            if (Deleted)
            {
                operations |= CrmWebApiSaveOperations.Delete;
            }
            if (Merge != null)
            {
                operations |= CrmWebApiSaveOperations.Merge;
            }
            if (HasRelationshipChanges)
            {
                operations |= CrmWebApiSaveOperations.Relations;
            }
            Operations = operations;

            int count = 0;
            if (Operations.HasFlag(CrmWebApiSaveOperations.Create))
            {
                count++;
            }
            if (Operations.HasFlag(CrmWebApiSaveOperations.Update))
            {
                count++;
            }
            if (Operations.HasFlag(CrmWebApiSaveOperations.Delete))
            {
                count++;
            }
            if (Operations.HasFlag(CrmWebApiSaveOperations.Merge))
            {
                count++;
            }
            if (Operations.HasFlag(CrmWebApiSaveOperations.Relations))
            {
                foreach (var associate in Associate)
                {
                    count += associate.relatatedEntities.Count();
                }
                foreach (var disassociate in Disassociate)
                {
                    count += disassociate.relatatedEntities.Count();
                }
            }
            OperationsCount = count;
        }

        public void Reset()
        {
            Entity.EntityState = null;
        }

        #endregion

        #region Statische Methoden

        public static CrmEntityChanged FromPropertyChangedEventArgs(CitaviCrmEntity entity, PropertyChangedEventArgs e)
        {
            var c = new CrmEntityChanged { Entity = entity };
            c.Properties.Add(e.PropertyName);
            return c;
        }

        #endregion
    }

    [Flags]
    public enum CrmWebApiSaveOperations
    {
        None = 0,
        Create = 1,
        Update = 2,
        Delete = 4,
        Merge = 8,
        Relations = 16
    }
}
