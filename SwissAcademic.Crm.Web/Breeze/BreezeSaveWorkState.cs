using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SwissAcademic.Crm.Web.Breeze
{
    public class BreezeSaveWorkState
    {

        public BreezeSaveWorkState(CrmBreezeContextProvider contextProvider, JArray entitiesArray)
        {
            ContextProvider = contextProvider;
            var jObjects = entitiesArray.Select(jt => (dynamic)jt).ToList();
            var groups = jObjects.GroupBy(jo => (string)jo.entityAspect.entityTypeName).ToList();

            EntityInfoGroups = groups.Select(g =>
            {
                var entityType = EntityNameResolver.GetTypeFromBreezeName(g.Key);
                var entityInfos = g.Select(jo => ContextProvider.CreateEntityInfoFromJson(jo, entityType)).Cast<BreezeEntityInfo>().ToList();
                return new EntityGroup() { EntityType = entityType, EntityInfos = entityInfos };
            }).ToList();
        }

        public void BeforeSave()
        {
            SaveMap = new Dictionary<Type, List<BreezeEntityInfo>>();
            foreach (var eg in EntityInfoGroups)
            {
                var entityInfos = new List<BreezeEntityInfo>();
                foreach (var ei in eg.EntityInfos)
                {
                    entityInfos.Add(ei);
                }
                SaveMap.Add(eg.EntityType, entityInfos);
            }
        }

        public CrmBreezeContextProvider ContextProvider { get; set; }
        List<EntityGroup> EntityInfoGroups { get; set; }
        public Dictionary<Type, List<BreezeEntityInfo>> SaveMap { get; set; }
        public List<BreezeKeyMapping> KeyMappings { get; set; }
        public List<BreezeEntityError> EntityErrors { get; set; }
        public bool WasUsed { get; internal set; }
       
        public BreezeSaveResult ToSaveResult()
        {
            WasUsed = true; // try to prevent reuse in subsequent SaveChanges
            if (EntityErrors != null)
            {
                return new BreezeSaveResult() { Errors = EntityErrors.Cast<object>().ToList() };
            }
            else
            {
                var entities = SaveMap.SelectMany(kvp => kvp.Value.Where(ei => (ei.EntityState != BreezeEntityState.Detached))
                  .Select(entityInfo => entityInfo.Entity)).ToList();
                var deletes = SaveMap.SelectMany(kvp => kvp.Value.Where(ei => (ei.EntityState == BreezeEntityState.Deleted || ei.EntityState == BreezeEntityState.Detached))
                  .Select(entityInfo => new BreezeEntityKey(entityInfo.Entity, ContextProvider.GetKeyValues(entityInfo)))).ToList();
                return new BreezeSaveResult() { Entities = entities, KeyMappings = KeyMappings, DeletedKeys = deletes };
            }
        }
    }

    public class EntityGroup
    {
        public Type EntityType { get; set; }
        public List<BreezeEntityInfo> EntityInfos { get; set; }
    }

}
