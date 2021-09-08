using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure.Swagger;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [SwaggerIgnore]
    public partial class CrmDbContext
        :
        IDisposable
    {
        #region Felder

        internal List<CitaviCrmEntity> _tracked = new List<CitaviCrmEntity>();
        bool _disposed = false;

        #endregion

        #region Konstruktor

        public CrmDbContext()
        {
        }
        ~CrmDbContext()
        {
            Dispose(false);
        }

        #endregion

        #region Eigenschaften

        #region HasTransaction

        public bool HasTransaction => ActiveTransaction != null;

        #endregion

        #region PendingChanges

        public int PendingChangesCount => PendingChanges.Count;

        List<CrmEntityChanged> PendingChanges = new List<CrmEntityChanged>();

        #endregion

        #region SaveRequestCount

        public int SaveRequestCount { get; set; }

        #endregion

        #endregion

        #region Methoden

        #region Add

        internal void Add(CitaviCrmEntity entity)
        {
            entity.EntityState = EntityState.Created;

            if (string.IsNullOrEmpty(entity.Key))
            {
                entity.CreateKey();
            }
            var changed = PendingChanges.FirstOrDefault(p => p.Entity.Key == entity.Key);
            if (changed == null)
            {
                changed = new CrmEntityChanged { Entity = entity };
                changed.TransactionId = ActiveTransaction?.Id;
                PendingChanges.Add(changed);
            }

            foreach (var prop in entity.Attributes)
            {
                if (changed.Properties.Contains(prop.Key))
                {
                    continue;
                }

                if (prop.Value is AliasedValue)
                {
                    continue;
                }

                changed.Properties.Add(prop.Key);
            }
        }

        #endregion

        #region AddIfNew

        void AddIfNew(IEnumerable<CitaviCrmEntity> entities)
        {
            foreach (var entity in entities)
            {
                AddIfNew(entity);
            }
        }

        void AddIfNew(CitaviCrmEntity entity)
        {
            if (entity.Id != Guid.Empty)
            {
                return;
            }

            if (entity.EntityState != null)
            {
                return;
            }

            entity.Id = Guid.NewGuid();
            Add(entity);
        }

        #endregion

        #region Associate

        void Associate(ICrmRelationship relationship, IEnumerable<CitaviCrmEntity> relatatedEntities)
        {
            if (!relatatedEntities.Any())
            {
                return;
            }

            var e = new CrmEntityChanged { Entity = relationship.Source };
            e.Associate.Add((relationship, relatatedEntities));
            e.TransactionId = ActiveTransaction?.Id;
            PendingChanges.Add(e);
        }

        #endregion

        #region Attach
        internal void Attach(CrmSet set)
        {
            if (set == null)
            {
                return;
            }

            Attach(set.Accounts);
            Attach(set.CampusContracts);
            Attach(set.Contacts);
            Attach(set.CrmLinkedAccounts);
            Attach(set.CrmLinkedEMailAccounts);
            Attach(set.EMailDomains);
            Attach(set.IPRanges);
            Attach(set.Licenses);
            Attach(set.LicenseTypes);
            Attach(set.OrderProcesses);
            Attach(set.OrganizationSettings);
            Attach(set.Pricings);
            Attach(set.Products);
            Attach(set.ProjectEntries);
            Attach(set.ProjectRoles);
            Attach(set.VoucherBlocks);
            Attach(set.Vouchers);
        }
        internal void Attach(IEnumerable<CitaviCrmEntity> entities)
        {
            if (entities == null)
            {
                return;
            }

            foreach (var entity in entities)
            {
                Attach(entity);
            }
        }
        public void Attach(CitaviCrmEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            entity._context = this;

            if (!_tracked.Contains(entity))
            {
                Observe(entity, true);
            }
        }
        public void Attach(CrmUser user)
        {
            if (user == null)
            {
                return;
            }

            Attach(user.Contact);
            Attach(user.CrmLinkedAccounts);
            Attach(user.CrmLinkedEMailAccounts);
            Attach(user.Licenses);
            Attach(user.VoucherBlocks);
            Attach(user.ProjectRoles);
        }

        #endregion

        #region BeginTransaction

        CrmTransaction ActiveTransaction;
        public CrmTransaction BeginTransaction()
        {
            if (ActiveTransaction != null &&
               !ActiveTransaction.IsDisposed)
            {
                throw new NotSupportedException("Transaction already open");
            }
            ActiveTransaction = new CrmTransaction(this);
            Observe(ActiveTransaction, true);
            return ActiveTransaction;
        }


        #endregion

        #region Create

        public T Create<T>()
            where T : CitaviCrmEntity
        {
            return (T)Create(typeof(T));
        }
        public CitaviCrmEntity Create(Type type)
        {
            var entity = Activator.CreateInstance(type) as CitaviCrmEntity;
            entity.Id = Guid.NewGuid();
            entity.EntityState = EntityState.Created;
            entity.CreateKey();
            entity.CreatedOn = DateTime.UtcNow;
            Add(entity);
            Observe(entity, true);

            return entity;
        }

        #endregion

        #region Count 

        internal async Task<int> Count<T>(int statuscode = -1)

            where T : CitaviCrmEntity
        {
            return await Count(typeof(T), statuscode);
        }
        internal async Task<int> Count(Type entityType, int statuscode = -1)
        {
            var entityLogicalName = EntityNameResolver.GetEntityLogicalName(entityType);
            return await Count(entityLogicalName, statuscode);
        }

        internal async Task<int> Count(string entityTypeLocalName, int statuscode)
        {
            string xml;

            if (statuscode != -1)
            {
                xml = string.Format(CultureInfo.InvariantCulture, FetchXmlExpression.FetchXml_Count_StatusCode,
                                            entityTypeLocalName, statuscode);
            }
            else
            {
                return await CrmWebApi.RetrieveCount(entityTypeLocalName);
            }

            return await Count(new FetchXmlExpression(entityTypeLocalName, xml));
        }

        internal async Task<int> Count(FetchXmlExpression fetch)
        {
            try
            {
                return await CrmWebApi.RetrieveCount(fetch);
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
                return 0;
            }
        }

        #endregion

        #region Delete

        public void Delete(CitaviCrmEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            entity.EntityState = EntityState.Changed;

            PendingChanges.RemoveAll(p => p.Entity?.Key == entity.Key);

            var changed = new CrmEntityChanged { Entity = entity, Deleted = true };
            changed.TransactionId = ActiveTransaction?.Id;
            PendingChanges.Add(changed);

            Detach(entity);
        }

        //TODO: Hier müssen wir auch deaktivierte Element zurückbekommen! Betrifft aktuell nur Unittests
        public async Task Delete<T>(string key)
            where T : CitaviCrmEntity
        {
            var entity = await GetInternal<T>(key, Array.Empty<string>());
            Delete(entity);
        }

        #endregion

        #region Detach

        void Detach()
        {
            Detach(_tracked);
        }

        void Detach(IEnumerable<CitaviCrmEntity> entites)
        {
            foreach (var entity in entites.ToList())
            {
                Detach(entity);
            }
        }

        void Detach(CitaviCrmEntity entity)
        {
            Observe(entity, false);

        }

        void Detach(CrmUser user)
        {
            Detach(user.Contact);
            Detach(user.CrmLinkedAccounts);
            Detach(user.CrmLinkedEMailAccounts);
            Detach(user.VoucherBlocks);
            Detach(user.Licenses);
            Detach(user.ProjectRoles);
        }

        #endregion

        #region Disassociate

        void Disassociate(ICrmRelationship relationship, IEnumerable<CitaviCrmEntity> relatatedEntities)
        {
            if (!relatatedEntities.Any())
            {
                return;
            }

            var changed = new CrmEntityChanged { Entity = relationship.Source };
            changed.Disassociate.Add((relationship, relatatedEntities));
            changed.TransactionId = ActiveTransaction?.Id;
            PendingChanges.Add(changed);
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Detach();
                    if (ActiveTransaction != null)
                    {
                        Observe(ActiveTransaction, false);
                        ActiveTransaction.Dispose();
                    }
                }

                _disposed = true;
            }
        }

        #endregion

        #region Exists

        public async Task<bool> Exists<T>(string key)
            where T : CitaviCrmEntity
        {
            var entityLogicalName = EntityNameResolver.GetEntityLogicalName<T>();
            var attributeName = EntityNameResolver.ResolveAttributeName(entityLogicalName, "key");
            return await Exists<T>(attributeName, key);
        }

        public async Task<bool> Exists<T>(Enum attribute, string value)
            where T : CitaviCrmEntity
        {
            var entityLogicalName = EntityNameResolver.GetEntityLogicalName<T>();

            var attributeName = EntityNameResolver.ResolveAttributeName(entityLogicalName, attribute.ToString());
            return await Exists<T>(attributeName, value);
        }

        async Task<bool> Exists<T>(string attribute, string value)
            where T : CitaviCrmEntity
        {
            var entityLogicalName = EntityNameResolver.GetEntityLogicalName<T>();

            var query = new QueryExpression(entityLogicalName);
            query.ColumnSet.Add(CrmAttributeNames.Key);

            query.AddCondition(attribute, ConditionOperator.Equal, value);
            query.AddCondition(CrmAttributeNames.StatusCode, ConditionOperator.Equal, (int)StatusCode.Active);

            var response = await RetrieveMultiple<T>(query, false);
            return response != null && response.Any();
        }

        internal async Task<bool> Exists(CitaviCrmEntity entity)
        {
            var query = new QueryExpression(entity.LogicalName);
            query.AddCondition(entity._idAttributeName, ConditionOperator.Equal, entity.Id);
            var response = await RetrieveMultiple<CitaviCrmEntity>(query, false);
            return response != null && response.Any();
        }

        #endregion

        #region ExecuteWorkflow

        /// <summary>
        /// Return: AsyncOperationId
        /// </summary>
        public Task<string> ExecuteWorkflow(Workflow workflow, CitaviCrmEntity entity)
         => ExecuteWorkflow(workflow.Id, entity.Id);

        public Task<string> ExecuteWorkflow(Guid workflowId, Guid entityId)
         => CrmWebApi.ExecuteWorkflow(workflowId.ToString(), entityId.ToString());

        #endregion

        #region EndTransaction

        public void EndTransaction()
        {
            if (ActiveTransaction == null)
            {
                throw new NotSupportedException("Transaction is null");
            }
            ActiveTransaction.Dispose();
        }

        #endregion

        #region Fetch

        public async Task<IEnumerable<T>> Fetch<T>(string fetchXml, bool observe = false)
            where T : CitaviCrmEntity
        {
            var result = await Fetch(FetchXmlExpression.Create<T>(fetchXml), observe);
            if (result == null || !result.Any())
            {
                return Enumerable.Empty<T>();
            }

            return result.Cast<T>();
        }
        public async Task<IEnumerable<T>> Fetch<T>(ExpressionBase fetchXml, bool observe = false)
           where T : CitaviCrmEntity
        {
            var result = await Fetch(fetchXml, observe);
            if (result == null || !result.Any())
            {
                return Enumerable.Empty<T>();
            }

            return result.Cast<T>();
        }
        public async Task<IEnumerable<CitaviCrmEntity>> Fetch(ExpressionBase expression, bool observe = false)
        {
            var result = await CrmWebApi.RetrieveMultiple(expression);
            if (observe)
            {
                Observe(result, true);
            }
            return result;
        }
        public async Task<IEnumerable<CitaviCrmEntity>> FetchMultiple(params FetchXmlExpression[] fetchXmlQueries)
        {
            return await CrmWebApi.RetrieveMultiple(fetchXmlQueries.AsEnumerable());
        }

        #endregion

        #region FetchFirstOrDefault

        internal async Task<T> FetchFirstOrDefault<T>(string fetchXml)
            where T : CitaviCrmEntity
        {
            var result = await Fetch(FetchXmlExpression.Create<T>(fetchXml), false);
            if (result == null || !result.Any())
            {
                return null;
            }

            return (T)result.First();
        }

        #endregion

        #region FetchUsers

        public async Task<IEnumerable<CrmUser>> FetchUsers(FetchXmlExpression fetchXml, bool observe = false)
        {
            var users = new List<CrmUser>();
            var results = await CrmWebApi.RetrieveMultiple(fetchXml);
            if (results == null || !results.Any())
            {
                return users;
            }
            var cached = fetchXml.Bag.Cast<CrmUser>();
            foreach (var r in results.GroupBy(u => u.Key))
            {
                var cs = new CrmSet(r);
                var user = new CrmUser(cs.Contacts.First());
                if (cached.Any(c => c.Id == user.Id))
                {
                    user = cached.First(c => c.Id == user.Id);
                }
                else if (users.Any(c => c.Id == user.Id))
                {
                    user = users.First(c => c.Id == user.Id);
                }
                else
                {
                    users.Add(user);
                    fetchXml.Bag.Add(user);
                }

                foreach (var item in cs.CrmLinkedAccounts)
                {
                    user.CrmLinkedAccounts.AddIfCrmEntityNotExists(item);
                }

                foreach (var item in cs.CrmLinkedEMailAccounts)
                {
                    user.CrmLinkedEMailAccounts.AddIfCrmEntityNotExists(item);
                }

                foreach (var item in cs.Licenses)
                {
                    user.Licenses.AddIfCrmEntityNotExists(item);
                }

                foreach (var item in cs.ProjectRoles)
                {
                    user.ProjectRoles.AddIfCrmEntityNotExists(item);
                }

                if (observe)
                {
                    Attach(user);
                }
            }
            return users;
        }

        #endregion

        #region Get

        public Task<T> Get<T>(Guid id, params Enum[] includeAttributes)
            where T : CitaviCrmEntity
            => Get<T>(id, false, includeAttributes);

        public async Task<T> Get<T>(Guid id, bool includeDeactivatedEntities, params Enum[] includeAttributes)
            where T : CitaviCrmEntity
        {
            var entityLogicalName = EntityNameResolver.GetEntityLogicalName<T>();
            var query = new QueryExpression(entityLogicalName);
            query.AddCondition($"{entityLogicalName}id", ConditionOperator.Equal, id);
            if (EntityNameResolver.HasKeyAttribute(entityLogicalName))
            {
                query.ColumnSet.Add(EntityNameResolver.ResolveAttributeName(entityLogicalName, "key"));
            }
            if (!includeDeactivatedEntities)
            {
                if (EntityNameResolver.HasStateCodeAttribute(entityLogicalName))
                {
                    query.AddCondition(CrmAttributeNames.StatusCode, ConditionOperator.Equal, (int)StatusCode.Active);
                }
            }
            else
            {
                query.IncludeDeactivatedEntities = true;
            }
            var attr = includeAttributes.Select(p => EntityNameResolver.ResolveAttributeName(entityLogicalName, p.ToString())).ToArray();
            foreach (var a in attr)
            {
                if (query.ColumnSet.Contains(a))
                {
                    continue;
                }

                query.ColumnSet.Add(a);
            }

            var results = await RetrieveMultiple<T>(query, observe: false);
            if (results == null)
            {
                return null;
            }
            var entity = results.FirstOrDefault();
            Observe(entity, true);
            _tracked.Add(entity);
            return entity;
        }

        public async Task<T> Get<T>(string key, params Enum[] includeAttributes)
            where T : CitaviCrmEntity
        {
            var entityLogicalName = EntityNameResolver.GetEntityLogicalName<T>();
            return await GetInternal<T>(key, includeAttributes.Select(p => EntityNameResolver.ResolveAttributeName(entityLogicalName, p.ToString())).ToArray());
        }

        public async Task<T> Get<T>(Enum propertyId, object value, params Enum[] includeAttributes)
            where T : CitaviCrmEntity
        {
            var entityLogicalName = EntityNameResolver.GetEntityLogicalName<T>();
            return await GetPrivate<T>(entityLogicalName, propertyId.ToString(), value, includeAttributes.Select(p => EntityNameResolver.ResolveAttributeName(entityLogicalName, p.ToString())).ToArray());
        }

        internal async Task<T> GetInternal<T>(string key, params string[] includeAttributes)
            where T : CitaviCrmEntity
        {
            var entityLogicalName = EntityNameResolver.GetEntityLogicalName<T>();
            return await GetPrivate<T>(entityLogicalName, "key", key, includeAttributes);
        }

        async Task<T> GetPrivate<T>(string entityLogicalName, string attribute, object value, params string[] includeAttributes)
            where T : CitaviCrmEntity
        {
            var query = new QueryExpression(entityLogicalName);
            if (EntityNameResolver.HasKeyAttribute(entityLogicalName))
            {
                query.ColumnSet.Add(EntityNameResolver.ResolveAttributeName(entityLogicalName, "key"));
            }
            if (includeAttributes == null ||
                !includeAttributes.Any())
            {
                var all = new List<string>();
                foreach (var prop in EntityPropertySets.GetFullPropertySet<T>())
                {
                    all.Add(EntityNameResolver.ResolveAttributeName(entityLogicalName, prop.ToString()));
                }
                includeAttributes = all.ToArray();
            }

            foreach (var a in includeAttributes)
            {
                if (query.ColumnSet.Contains(a))
                {
                    continue;
                }

                query.ColumnSet.Add(a);
            }

            if (value != null &&
                value is string)
            {
                value = value.ToString().RemoveNonStandardWhitespace();
            }


            query.AddCondition(EntityNameResolver.ResolveAttributeName(entityLogicalName, attribute), ConditionOperator.Equal, value);

            if (EntityNameResolver.HasStateCodeAttribute(entityLogicalName))
            {
                query.AddCondition(CrmAttributeNames.StatusCode, ConditionOperator.Equal, (int)StatusCode.Active);
            }

            var result = await RetrieveMultiple<T>(query);
            if (result == null)
            {
                return null;
            }

            var entity = result.FirstOrDefault();
            if (entity == null)
            {
                return null;
            }

            Observe(entity, true);
            _tracked.Add(entity);
            return entity;
        }

        #endregion

        #region GetAll

        internal async Task<IEnumerable<T>> GetAll<T>(params ConditionExpression[] expressions)
            where T : CitaviCrmEntity
        {
            var entityLogicalName = EntityNameResolver.GetEntityLogicalName<T>();
            var query = new QueryExpression(entityLogicalName);
            foreach (var attribute in EntityPropertySets.GetFullPropertySet<T>())
            {
                var attributeLocialName = EntityNameResolver.ResolveAttributeName(entityLogicalName, attribute.ToString());
                query.ColumnSet.Add(attributeLocialName);
            }
            if (EntityNameResolver.HasStateCodeAttribute(entityLogicalName))
            {
                query.AddCondition(CrmAttributeNames.StatusCode, ConditionOperator.Equal, (int)StatusCode.Active);
            }
            foreach (var expression in expressions)
            {
                query.AddCondition(expression);
            }
            return await RetrieveMultiple<T>(query, false);
        }

        #endregion

        #region GetAsyncOperationStatus


        public Task<AsyncOperationStatus> GetAsyncOperationStatus(string asyncoperationid)
                                          => CrmWebApi.GetAsyncOperationStatus(asyncoperationid);


        #endregion

        #region GetMany

        public async Task<IEnumerable<T>> GetMany<T>(IEnumerable<string> keys, bool observe = false)
            where T : CitaviCrmEntity
        {
            var entityLogicalName = EntityNameResolver.GetEntityLogicalName<T>();
            var queries = new List<QueryExpression>();
            foreach (var key in keys)
            {
                var query = new QueryExpression(entityLogicalName);
                foreach (var attribute in EntityPropertySets.GetFullPropertySet<T>())
                {
                    var attributeLocialName = EntityNameResolver.ResolveAttributeName(entityLogicalName, attribute.ToString());
                    query.ColumnSet.Add(attributeLocialName);
                }
                if (EntityNameResolver.HasStateCodeAttribute(entityLogicalName))
                {
                    query.AddCondition(CrmAttributeNames.StatusCode, ConditionOperator.Equal, (int)StatusCode.Active);
                }
                query.AddCondition(CrmAttributeNames.Key, ConditionOperator.Equal, key);
                queries.Add(query);
            }
            try
            {
                var result = await CrmWebApi.RetrieveMultiple(queries);
                if (observe)
                {
                    Observe(result, true);
                }
                return result.Cast<T>();
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception);
            }
            return Enumerable.Empty<T>();
        }

        #endregion

        #region GetWithCustomAttribues

        /// <summary>
        /// Die CustomAttribues werden IMMER als Strings im Entity.Attributes Array gespeichert
        /// </summary>
        public async Task<T> GetWithCustomAttribues<T>(string key, params string[] includeAttributes)
            where T : CitaviCrmEntity
        {
            var entityLogicalName = EntityNameResolver.GetEntityLogicalName<T>();
            return await GetInternal<T>(key, includeAttributes.Select(p => EntityNameResolver.ResolveAttributeName(entityLogicalName, p)).ToArray());
        }

        #endregion

        #region GetMergedContact

        public async Task<(Contact MergedContact, CrmUser Master)> GetMergedContact(string contactKey)
		{
            var exp = new Query.FetchXml.GetMergedContactByKey(contactKey);
            var mergedContact = await FetchFirstOrDefault<Contact>(exp.TransformText());
            if (mergedContact == null)
			{
                return (null, null);
			}
            var masterId = Guid.Parse(mergedContact["masterid"].ToString());
            var masterContact = await Get<Contact>(masterId, true, ContactPropertyId.Key);
            CrmUser master = null;
            if(masterContact != null)
			{
                master = await GetByKeyAsync(masterContact.Key);
                if(master == null)
				{
                    (_, CrmUser master2) = await GetMergedContact(masterContact.Key);
                    return (mergedContact, master2);
                }
                return (mergedContact, master);
			}
            return (null, null);
        }

		#endregion

		#region GetSystemInfo(CitaviCrmEntity entity)

        public async Task<CitaviCrmEntitySystemInfo> GetSystemInfo(CitaviCrmEntity entity) => await CrmWebApi.GetCitaviCrmEntitySystemInfo(entity);

        #endregion

        #region MergeCrm4Contact

        internal async Task MergeCrm4ContactAsync(Contact loser, CrmUser winner)
        {
            await winner.Load(this, attachToContext: true, loadContext: UserLoadContexts.Licenses);
            var existing = winner.Licenses.ToList();

            var changed = new CrmEntityChanged();
            changed.Merge = new CrmEntityMergeRequest();
            changed.Merge.SubordinateId = loser.Id;
            changed.Merge.Target = winner.Contact;
            PendingChanges.Add(changed);
            await SaveAsync();

            await winner.Load(this, attachToContext: true, loadContext: UserLoadContexts.Licenses);

            var loserCampusBenefitEligibilityType = loser.CampusBenefitEligibility;
            var winnerCampusBenefitEligibilityType = winner.Contact.CampusBenefitEligibility;

            if ((!winnerCampusBenefitEligibilityType.HasValue ||
                  winnerCampusBenefitEligibilityType.Value == CampusBenefitEligibilityType.NotApplicable) &&
                loserCampusBenefitEligibilityType.HasValue)
            {
                winner.Contact.CampusBenefitEligibility = loserCampusBenefitEligibilityType.Value;
            }
            if (loserCampusBenefitEligibilityType.HasValue &&
                loserCampusBenefitEligibilityType.Value == CampusBenefitEligibilityType.Redeemed)
            {
                winner.Contact.CampusBenefitEligibility = CampusBenefitEligibilityType.Redeemed;
            }

            foreach (var license in winner.Licenses.ToList())
            {
                if (string.IsNullOrEmpty(license.DataContractCampusContractKey))
                {
                    continue;
                }

                if (existing.Any(i => i.Key == license.Key))
                {
                    continue;
                }

                if (existing.Any(i => i.DataContractCampusContractKey == license.DataContractCampusContractKey &&
                                      i.DataContractProductKey == license.DataContractProductKey))
                {
                    //Wir haben diese Campuslizenz bereits -> Entfernen
                    license.StatusCode = StatusCode.Inactive;
                    winner.Licenses.Remove(license);
                }
            }
            if (PendingChanges.Count > 0)
            {
                await SaveAsync();
                await winner.Load(this, attachToContext: true, loadContext: UserLoadContexts.Licenses);
            }
        }

        #endregion

        #region MergeAsync

        internal async Task MergeAsync(string loserKey, string winnerKey)
        {
            var loser = await GetByKeyAsync(loserKey);
            var winner = await GetByKeyAsync(winnerKey);
            await MergeAsync(loser, winner);
        }
        internal async Task MergeAsync(CrmUser loser, CrmUser winner)
        {
            var changed = new CrmEntityChanged();
            changed.Merge = new CrmEntityMergeRequest();
            changed.Merge.SubordinateId = loser.Contact.Id;
            changed.Merge.Target = winner.Contact;
            PendingChanges.Add(changed);
            await SaveAsync();

            await CrmUserCache.RemoveAsync(loser);

            await CrmAuthenticationService.RevokeUserAccessTokensAndCookies(loser.Key);

            await CrmUserCache.RemoveAsync(winner);

            var mergedWinner = await GetByKeyAsync(winner.Key);

            #region ProjectRoles

            foreach (var loserProjectRole in loser.ProjectRoles)
            {
                if (mergedWinner.ProjectRoles.Count(i => i.DataContractProjectKey == loserProjectRole.DataContractProjectKey) <= 1)
                {
                    continue;
                }

                var toRemove = mergedWinner.ProjectRoles.FirstOrDefault(i => i.DataContractProjectKey == loserProjectRole.DataContractProjectKey && i.ProjectRoleType < Azure.ProjectRoleType.Owner);
                mergedWinner.RemoveProjectRole(toRemove);
                toRemove.Deactivate();
            }

            #endregion

            #region LinkedAccounts

            foreach (var loserItem in loser.CrmLinkedAccounts)
            {
                if (mergedWinner.CrmLinkedAccounts.Count(i => i.IdentityProviderId == loserItem.IdentityProviderId) <= 1)
                {
                    continue;
                }

                var toRemove = mergedWinner.CrmLinkedAccounts.FirstOrDefault(i => i.Key == loserItem.Key);
                mergedWinner.RemoveLinkedAccount(toRemove);
                toRemove.Deactivate();
            }

            #endregion

            var campusContractLicenses = mergedWinner.Licenses.Where(lic => !string.IsNullOrEmpty(lic.DataContractCampusContractKey)).ToList();
            foreach (var campusLicense1 in campusContractLicenses)
            {
                foreach (var campusLicense2 in campusContractLicenses)
                {
                    if (campusLicense1.Key == campusLicense2.Key)
                    {
                        continue;
                    }

                    if (campusLicense1.DataContractCampusContractKey != campusLicense2.DataContractCampusContractKey)
                    {
                        continue;
                    }

                    if (campusLicense1.DataContractProductKey != campusLicense2.DataContractProductKey)
                    {
                        continue;
                    }

                    if (mergedWinner.Licenses.Count(lic => lic.DataContractCampusContractKey == campusLicense1.DataContractCampusContractKey && lic.DataContractProductKey == campusLicense1.DataContractProductKey) == 1)
                    {
                        continue;
                    }

                    //Campuslizenz bereits vorhanden
                    mergedWinner.Licenses.Remove(campusLicense1);
                    campusLicense1.Deactivate();
                    if (campusLicense1.IsVerified && !campusLicense2.IsVerified)
                    {
                        //Campuslizenz war im LoserAccount verifiziert -> wir übernehmen das für alle Lizenzen
                        campusLicense2.IsVerified = true;
                        foreach (var cc in campusContractLicenses.Where(lic => lic.DataContractCampusContractKey == campusLicense1.DataContractCampusContractKey))
                        {
                            if (!cc.IsVerified)
                            {
                                cc.IsVerified = true;
                            }
                        }
                    }
                }
            }

            campusContractLicenses = mergedWinner.Licenses.Where(lic => !string.IsNullOrEmpty(lic.DataContractCampusContractKey)).ToList();
            foreach (var campusLicense1 in campusContractLicenses)
            {
                var campusContract = CrmCache.CampusContracts.FirstOrDefault(cc => cc.Key == campusLicense1.DataContractCampusContractKey);
                if (campusContract == null)
                {
                    continue;
                }

                if (campusContract.NewContractAvailable)
                {
                    //Prüfen ob wir eine verfizierte CC-Lizenz vom neuen Vertrag haben
                    foreach (var campusLicense2 in campusContractLicenses)
                    {
                        if (campusLicense1.Key != campusLicense2.Key &&
                           campusLicense2.DataContractAccountKey == campusLicense1.DataContractAccountKey &&
                           campusLicense2.IsVerified)
                        {
                            //User hat bereits eine verifzierte CC-Lizenz -> Lizenz entfernen
                            mergedWinner.Licenses.Remove(campusLicense1);
                            campusLicense1.Deactivate();
                            break;
                        }
                    }
                }
            }

            foreach(var lic in loser.Licenses)
            {
				if (!lic.Free)
				{
                    continue;
				}
                var freeLicense = mergedWinner.Licenses.FirstOrDefault(l => l.Key == lic.Key);
                if(freeLicense != null)
				{
                    freeLicense.Deactivate();
                    mergedWinner.Licenses.Remove(freeLicense);
                }
			}

            var loserCampusBenefitEligibilityType = loser.Contact.CampusBenefitEligibility;
            var winnerCampusBenefitEligibilityType = mergedWinner.Contact.CampusBenefitEligibility;

            if ((!winnerCampusBenefitEligibilityType.HasValue ||
                  winnerCampusBenefitEligibilityType.Value == CampusBenefitEligibilityType.NotApplicable) &&
                loserCampusBenefitEligibilityType.HasValue)
            {
                mergedWinner.Contact.CampusBenefitEligibility = loserCampusBenefitEligibilityType.Value;
            }
            if (loserCampusBenefitEligibilityType.HasValue &&
                loserCampusBenefitEligibilityType.Value == CampusBenefitEligibilityType.Redeemed)
            {
                mergedWinner.Contact.CampusBenefitEligibility = CampusBenefitEligibilityType.Redeemed;
            }

            mergedWinner.Contact.MergeAccountVerificationKey = null;
            mergedWinner.Contact.MergeAccountVerificationKeySent = null;

            if (string.IsNullOrEmpty(mergedWinner.Contact.EMailAddress1) && mergedWinner.CrmLinkedEMailAccounts.Any())
            {
                foreach (var la in mergedWinner.CrmLinkedEMailAccounts)
                {
                    if (la.IsVerified.GetValueOrDefault())
                    {
                        mergedWinner.Contact.EMailAddress1 = la.Email;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(mergedWinner.Contact.FirstName) && !string.IsNullOrEmpty(loser.Contact.FirstName))
            {
                mergedWinner.Contact.FirstName = loser.Contact.FirstName;
            }

            if (string.IsNullOrEmpty(mergedWinner.Contact.LastName) && !string.IsNullOrEmpty(loser.Contact.LastName))
            {
                mergedWinner.Contact.LastName = loser.Contact.LastName;
            }

            await SaveAndUpdateUserCacheAsync(mergedWinner);

            await CrmAuthenticationService.RevokeUserAccessTokensAndCookies(loser.Key);
        }

        #endregion

        #region Observe

        void Observe(IEnumerable<CitaviCrmEntity> entities, bool start)
        {
            foreach (var entity in entities)
            {
                Observe(entity, start);
            }
        }

        void Observe(CitaviCrmEntity entity, bool start)
        {
            if (entity == null)
            {
                return;
            }

            if (start)
            {
                if (!_tracked.Contains(entity))
                {
                    entity._context = this;
                    _tracked.Add(entity);
                    entity.PropertyChanged += Entity_PropertyChanged;
                    entity.RelationshipChanged += Entity_RelationshipChanged;
                }
            }
            else
            {
                entity.PropertyChanged -= Entity_PropertyChanged;
                entity.RelationshipChanged -= Entity_RelationshipChanged;
                entity._context = null;
                _tracked.Remove(entity);
            }
        }

        void Observe(CrmTransaction transaction, bool start)
        {
            if (start)
            {
                transaction.Disposed += Transaction_Disposed;
            }
            else
            {
                transaction.Disposed -= Transaction_Disposed;
            }
        }

        #endregion

        #region OverrideKey

        /// <summary>
        /// Darf nur aus den CRM Functions verwendet werden.
        /// Szenario: Neue Entität im CRM -> Function -> Key muss gesetzt werden
        /// </summary>
        public async Task<string> OverrideKey(string entityName, string entityId)
        {
            var key = SwissAcademic.Security.PasswordGenerator.WebKey.Generate();
            await CrmWebApi.UpdateProperty(entityName, entityId, CrmAttributeNames.Key, key);
            return key;
        }

        #endregion

        #region RetrieveEntityChanges

        Dictionary<string, QueryExpression> _retrieveEntityChangesCache = new Dictionary<string, QueryExpression>();
        //Zum Prüfen: https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/use-change-tracking-synchronize-data-external-systems

        /// <summary>
        /// Wenn keine DateTime angegeben wird, wird beim ersten Request die letzte aktualisierte Entät zurückgegeben
        /// </summary>
        public async Task<IEnumerable<T>> RetrieveEntityChanges<T>(DateTime? dateTime = null)
            where T : CitaviCrmEntity
        {
            var entityName = EntityNameResolver.GetEntityLogicalName<T>();

            if (!_retrieveEntityChangesCache.TryGetValue(entityName, out var expr))
            {
                expr = QueryExpression.Create<T>();
                expr.IncludeDeactivatedEntities = true;
                if (dateTime != null)
                {
                    expr.AddCondition(CrmAttributeNames.ModifiedOn, ConditionOperator.GreaterThan, dateTime);
                }
                else
                {
                    //Initialer Request - das zu letzte geänderte Entität wird gesucht und als Condition übernommen
                    expr.PageSize = 1;
                }
                expr.ColumnSet.Add(CrmAttributeNames.Key);
                expr.ColumnSet.Add(CrmAttributeNames.ModifiedOn);
                expr.OrderBy = $"{CrmAttributeNames.ModifiedOn} desc";
                _retrieveEntityChangesCache[entityName] = expr;
            }
            else if (dateTime.HasValue)
            {
                expr.Conditions[0].Value = dateTime;
            }

            if (expr.Conditions.Any())
            {
                //Wenn in der gleichen Sekunde Änderungen gemacht werden erhalten wir diese nicht.
                //Wir hier 1 Sekunde warten, damit wir nicht in eine "racecondition" reinlaufen
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            var result = await CrmWebApi.RetrieveMultiple(expr) as IEnumerable<T>;
            expr.PageSize = null;
            expr.NextLink = null;

            if (result == null || !result.Any())
            {
                return Enumerable.Empty<T>();
            }

            if (expr.Conditions.Any())
            {
                expr.Conditions[0].Value = result.Max(r => r.ModifiedOn);
            }
            else
            {
                //Initialer Request
                expr.AddCondition(CrmAttributeNames.ModifiedOn, ConditionOperator.GreaterThan, result.Max(r => r.ModifiedOn));
            }

            return result;
        }

        #endregion

        #region RetrieveMultiple

        public async Task<IEnumerable<T>> RetrieveMultiple<T>(QueryExpression query, bool observe = true)
            where T : CitaviCrmEntity
        {
            var result = await RetrieveMultiplePrivate(query, observe);
            if (!result.Any())
            {
                return Enumerable.Empty<T>();
            }
            return result as IEnumerable<T>;
        }
        async Task<IEnumerable<CitaviCrmEntity>> RetrieveMultiplePrivate(QueryExpression query, bool observe = true)
        {
            try
            {
                var result = await CrmWebApi.RetrieveMultiple(query);

                if (observe)
                {
                    Observe(result, true);
                }
                return result;
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception);
            }
            return Enumerable.Empty<CitaviCrmEntity>();
        }

        #endregion

        #region Save

        internal async Task SaveAsync(CrmUser user, bool continueOnError = false, string impersonatedUserID = null)
        {
            if (SuspendUpdateUser)
            {
                return;
            }

            await SaveAndUpdateUserCacheAsync(user, continueOnError, impersonatedUserID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="continueOnError">Betrifft den Speichervorgang im CRM. Wenn true wird das CRM bei einem Fehler nicht abbrechen. Es wird IMMER eine CrmServerException geworfen wenn ein Fehler auftritt.</param>
        /// <returns></returns>
        public Task<int> SaveAsync(bool continueOnError = false, string impersonatedUserID = null) => SaveAndUpdateUserCacheAsync(null, continueOnError, impersonatedUserID);

        public async Task<int> SaveAndUpdateUserCacheAsync(CrmUser user, bool continueOnError = false, string impersonatedUserID = null)
        {
            if (!PendingChanges.Any())
            {
                return 0;
            }

            try
            {
                SaveRequestCount++;
                if (user != null)
                {
                    await CrmWebApi.SaveAsync(PendingChanges, withSaveChangesQueueCheck: true, continueOnError: continueOnError, impersonatedUserID);
                    await CrmUserCache.AddOrUpdateAsync(user);
                }
                else
                {
                    await CrmWebApi.SaveAsync(PendingChanges, withSaveChangesQueueCheck: true, continueOnError: continueOnError, impersonatedUserID);
                }
                return PendingChanges.Count;
            }
            catch
            {
                if (user != null)
                {
                    await CrmUserCache.RemoveAsync(user);
                }
                throw;
            }
            finally
            {
                PendingChanges.Clear();
            }
        }

        #endregion

        #region Sum 

        internal async Task<int> Sum(FetchXmlExpression fetch)
        {
            try
            {
                return await CrmWebApi.RetrieveSum(fetch);
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
                return 0;
            }
        }

        #endregion

        #region UpdateEntity

        public void UpdateEntity(CitaviCrmEntity entity, string propertyName, object value)
        {
            entity.Attributes.Add(propertyName, value);
            UpdateObject(entity, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region UpdateObject

        void UpdateObject(CitaviCrmEntity entity, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == CrmAttributeNames.EntityState)
            {
                return;
            }

            var changed = PendingChanges.FirstOrDefault(i => i.Entity.Key == entity.Key);
            if (changed == null)
            {
                changed = CrmEntityChanged.FromPropertyChangedEventArgs(entity, e);
                changed.TransactionId = ActiveTransaction?.Id;
                PendingChanges.Add(changed);
            }

            if (!_tracked.Contains(entity))
            {
                _tracked.Add(entity);
            }
            if (changed.Properties.Contains(e.PropertyName))
            {
                return;
            }
            changed.Properties.Add(e.PropertyName);
        }

        #endregion

        #endregion

        #region Ereignishandler

        #region Entity_RelationshipChanged

        void Entity_RelationshipChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var crmReleationship = sender as ICrmRelationship;
            var source = crmReleationship.Source;
            var newItems = (e.NewItems == null || e.NewItems.Count == 0) ? Enumerable.Empty<CitaviCrmEntity>() : e.NewItems.Cast<CitaviCrmEntity>().Where(i => i != null).ToList();
            var oldItems = (e.OldItems == null || e.OldItems.Count == 0) ? Enumerable.Empty<CitaviCrmEntity>() : e.OldItems.Cast<CitaviCrmEntity>().Where(i => i != null).ToList();

            Attach(newItems);
            Attach(oldItems);
            Attach(source);

            AddIfNew(newItems);
            AddIfNew(source);

            switch (e.Action)
            {
                #region Add

                case NotifyCollectionChangedAction.Add:
                    {
                        Associate(crmReleationship, newItems);
                    }
                    break;

                #endregion

                #region Remove

                case NotifyCollectionChangedAction.Remove:
                    {
                        Disassociate(crmReleationship, oldItems);
                    }
                    break;

                #endregion

                #region Replace

                case NotifyCollectionChangedAction.Replace:
                    {
                        Disassociate(crmReleationship, oldItems);
                        Associate(crmReleationship, newItems);
                    }
                    break;

                    #endregion
            }
        }

        #endregion

        #region Entity_PropertyChanged

        void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var crmEntity = sender as CitaviCrmEntity;

            if (crmEntity.EntityState == null)
            {
                crmEntity.EntityState = EntityState.Changed;
                if (crmEntity.Id == Guid.Empty)
                {
                    AddIfNew(crmEntity);
                    Attach(crmEntity);
                }
            }
            UpdateObject(crmEntity, e);
        }

        #endregion

        #region Transaction Disposed

        void Transaction_Disposed(object sender, EventArgs e)
        {
            Observe(ActiveTransaction, false);
            ActiveTransaction = null;
        }

        #endregion

        #endregion
    }
}
