using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class SaveChangesQueue
    {
        #region Classes

#pragma warning disable CS0659 // Typ überschreibt Object.Equals(object o), überschreibt jedoch nicht Object.GetHashCode()
        class SaveChangesMessage
#pragma warning restore CS0659 // Typ überschreibt Object.Equals(object o), überschreibt jedoch nicht Object.GetHashCode()
            :
            IEquatable<SaveChangesMessage>
        {
            public EntityState EntityState { get; set; }
            public Guid Id { get; set; }
            public string EntityLocicalName { get; set; }
            public Dictionary<string, object> Properties { get; set; }
            public string Key { get; set; }

            public DateTime Time { get; set; }

            public bool Equals(SaveChangesMessage other)
            {
                if(other == null)
                {
                    return false;
                }
                return other.Key == Key && Properties.SequenceEqual(other.Properties);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as SaveChangesMessage);
            }
        }

        #endregion

        #region Felder

        readonly static HashSet<string> _allowedProperties = new HashSet<string>();

        #endregion

        #region Eigenschaften

        static CloudQueue Queue { get; set; }


		#endregion

		#region Methoden

		#region Initalize

        public static async Task InitalizeAsync()
		{
            var queueName = CrmQueueConstants.PendingChanges.GetFullQueueNameFromBaseName();
            

            _allowedProperties.Add(EntityNameResolver.ResolveAttributeName(CrmEntityNames.Contact, nameof(Contact.LastLogin)));
            _allowedProperties.Add(EntityNameResolver.ResolveAttributeName(CrmEntityNames.Contact, nameof(Contact.LastLoginWordAssistant)));
            _allowedProperties.Add(EntityNameResolver.ResolveAttributeName(CrmEntityNames.Contact, nameof(Contact.LastLoginPicker)));
            _allowedProperties.Add(EntityNameResolver.ResolveAttributeName(CrmEntityNames.Contact, nameof(Contact.LastLoginCitaviWeb)));
            _allowedProperties.Add(EntityNameResolver.ResolveAttributeName(CrmEntityNames.Project, nameof(ProjectEntry.ProjectLastModifiedOn)));

            
            Queue = AzureHelper.Queues[queueName];
            await Queue.CreateIfNotExistsAsync();
        }

        #endregion

        #region ExecuteAsync

        public static async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var updated = 0;

            try
            {
                var changes = new List<(CrmEntityChanged EntityChanged, DateTime Time)>();
                var cloudQueueMessages = new List<CloudQueueMessage>();

                using (var context = new CrmDbContext())
                {
                    try
                    {
                        changes.Clear();
                        cloudQueueMessages.Clear();
                        for (var i = 0; i < 3; i++)
                        {
                            var msgs = await Queue.GetMessagesAsync(32, TimeSpan.FromMinutes(5), null, null, cancellationToken);
                            if (!msgs.Any())
                            {
                                break;
                            }
                            updated += msgs.Count();
                            foreach (var msg in msgs)
                            {
                                try
                                {
                                    cloudQueueMessages.Add(msg);

                                    var saveChangesMsg = JsonConvert.DeserializeObject<SaveChangesMessage>(msg.AsString);
                                   
                                    var entity = new CitaviCrmEntity(saveChangesMsg.EntityLocicalName);
                                    entity.Id = saveChangesMsg.Id;
                                    entity.EntityState = saveChangesMsg.EntityState;
                                    entity.Key = saveChangesMsg.Key;
                                    var exists = await context.Exists(entity);
                                    if(!exists)
									{
                                        Telemetry.TrackDiagnostics($"{nameof(SaveChangesQueue)} called, but entity does not exists: {saveChangesMsg.Id} ({saveChangesMsg.EntityLocicalName})");
                                        continue;
									}
                                    foreach (var prop in saveChangesMsg.Properties)
                                    {
                                        if (entity.Attributes.ContainsKey(prop.Key))
                                        {
                                            continue;
                                        }

                                        entity.Attributes.Add(prop.Key, prop.Value);
                                    }

                                    var ec = new CrmEntityChanged { Entity = entity };
                                    ec.Properties.AddRange(saveChangesMsg.Properties.Keys.ToList());
                                    if (changes.Any(c => c.EntityChanged.Equals(ec)))
                                    {
                                        //Bsp: Der User melde sich schnell an und ab
                                        //Cannot obtain lock on resource. This can occur when there are multiple update requests on the same record
                                        var existing = changes.FirstOrDefault(c => c.EntityChanged.Equals(ec));
                                        if (existing.Time < saveChangesMsg.Time)
                                        {
                                            //Neuere Änderung, alte wird ignoriert
                                            Telemetry.TrackTrace($"Skip entitychange: {ec.Entity.Key} ({saveChangesMsg.Time}). Existing: {existing.EntityChanged.Entity.Key} ({existing.Time})");
                                            continue;
                                        }
                                    }
                                    changes.Add((ec, saveChangesMsg.Time));
                                }
                                catch (Exception ignored)
                                {
                                    Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
                                    await Queue.DeleteMessageAsync(msg);
                                }
                            }
                        }
                        if (changes.Count > 0)
                        {
                            try
                            {
                                await CrmWebApi.SaveAsync(changes.Select(ec => ec.EntityChanged), withSaveChangesQueueCheck: false);
                            }
                            catch (Exception ex1)
                            {
                                Telemetry.TrackException(ex1, SeverityLevel.Warning, ExceptionFlow.Eat);

                                foreach (var change in changes)
                                {
                                    try
                                    {
                                        await CrmWebApi.SaveAsync(new[] { change.EntityChanged }, withSaveChangesQueueCheck: false);
                                    }
                                    catch (Exception ex2)
                                    {
                                        Telemetry.TrackException(ex2, SeverityLevel.Error, ExceptionFlow.Eat);
                                    }
                                }
                            }
                        }
                        foreach (var msg in cloudQueueMessages)
                        {
                            await Queue.DeleteMessageAsync(msg);
                        }
                    }
                    catch (TimeoutException timeout)
                    {
                        Telemetry.TrackException(timeout, SeverityLevel.Warning, ExceptionFlow.Eat);
                    }
                    catch (Exception ex)
                    {
                        Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
                    }
                }
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return updated;
        }

        #endregion

        #region ExecuteAllAsync

        /// <summary>
        /// Nur für Unittests
        /// </summary>
        internal static async Task<int> ExecuteAllAsync(CancellationToken cancellation = default)
		{
            var changes = 0;
            do
            {
                changes = await SaveChangesQueue.ExecuteAsync(cancellation);
            }
            while (changes != 0);

            return changes;
        }

        #endregion

        #region TryAdd

        public static async Task<IEnumerable<CrmEntityChanged>> TryAdd(IEnumerable<CrmEntityChanged> entitiesChanged)
        {
            var list = new List<CrmEntityChanged>();

            try
            {
                foreach (var entityChanged in entitiesChanged)
                {
                    if (!Validate(entityChanged))
                    {
                        return list;
                    }
                }
                foreach (var entityChanged in entitiesChanged)
                {
                    if (await TryAdd(entityChanged))
                    {
                        list.Add(entityChanged);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return list;
        }
        static async Task<bool> TryAdd(CrmEntityChanged entityChanged)
        {
            try
            {
                var sm = new SaveChangesMessage
                {
                    Id = entityChanged.Entity.Id,
                    EntityState = entityChanged.Entity.EntityState.GetValueOrDefault(),
                    EntityLocicalName = entityChanged.Entity.LogicalName,
                    Properties = new Dictionary<string, object>(),
                    Key = entityChanged.Entity.Key,
                    Time = DateTime.UtcNow
                };
                foreach (var p in entityChanged.Properties)
                {
                    sm.Properties[p] = entityChanged.Entity[p];
                }
                var msg = new CloudQueueMessage(JsonConvert.SerializeObject(sm));
                await Queue.AddMessageAsync(msg);
                entityChanged.Reset();
                return true;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
                return false;
            }
        }

        #endregion

        #region Validate

        public static bool Validate(CrmEntityChanged entityChanged)
        {
            if (entityChanged.Deleted)
            {
                return false;
            }

            if (entityChanged.Merge != null)
            {
                return false;
            }

            if (entityChanged.HasRelationshipChanges)
            {
                return false;
            }

            if (!entityChanged.Properties.TrueForAll(p => _allowedProperties.Contains(p)))
            {
                return false;
            }

            return true;
        }

        

        #endregion

        #endregion
    }
}
