using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwissAcademic.Crm.Web.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web.Breeze
{
    public class CrmBreezeContextProvider
    {
        #region Felder

        readonly List<BreezeKeyMapping> _keyMappings = new List<BreezeKeyMapping>();
        readonly List<BreezeEntityError> _entityErrors = new List<BreezeEntityError>();
        IHttpContextAccessor _httpContextAccessor;
        string _contactKey;

        #endregion

        #region Konstruktor

        public CrmBreezeContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        internal CrmBreezeContextProvider(string contactKey)
        {
            _contactKey = contactKey;
        }

        #endregion

        #region Eigenschaften

        BreezeSaveWorkState SaveWorkState { get; set; }
        JsonSerializer JsonSerializer { get; set; }

        #endregion

        #region Methoden

        #region CreateJsonSerializer

        private static JsonSerializer CreateJsonSerializer()
        {
            return JsonSerializer.Create(CrmJsonConvert.DefaultSettings);
        }

        #endregion

        #region GetKeyValues

        public object[] GetKeyValues(BreezeEntityInfo entityInfo)
        {
            var crmEntity = (CitaviCrmEntity)entityInfo.Entity;
            return new object[]
            {
                crmEntity.Key,
            };
        }

        #endregion

        #region CreateEntityInfo

        BreezeEntityInfo CreateEntityInfo()
        {
            return new BreezeEntityInfo();
        }

        public BreezeEntityInfo CreateEntityInfo(CitaviCrmEntity entity, BreezeEntityState entityState = BreezeEntityState.Added)
        {
            var ei = CreateEntityInfo();
            ei.Entity = entity;
            ei.EntityState = entityState;
            return ei;
        }

        #endregion

        #region CreateEntityInfoFromJson

        internal BreezeEntityInfo CreateEntityInfoFromJson(dynamic jo, Type entityType)
        {
            var entityInfo = CreateEntityInfo();
            using (var reader = new JTokenReader(jo))
            {
                entityInfo.Entity = (CitaviCrmEntity)JsonSerializer.Deserialize(reader, entityType);
                entityInfo.EntityState = (BreezeEntityState)Enum.Parse(typeof(BreezeEntityState), (String)jo.entityAspect.entityState);
                entityInfo.OriginalValuesMap = JsonToDictionary(jo.entityAspect.originalValuesMap);
            }
            return entityInfo;
        }

        #endregion

        #region InitializeSaveState

        void InitializeSaveState(JObject saveBundle)
        {
            JsonSerializer = CreateJsonSerializer();

            var dynSaveBundle = (dynamic)saveBundle;
            var entitiesArray = (JArray)dynSaveBundle.entities;
            SaveWorkState = new BreezeSaveWorkState(this, entitiesArray);
        }
        #endregion

        #region JsonToDictionary
        Dictionary<String, Object> JsonToDictionary(dynamic json)
        {
            if (json == null)
            {
                return null;
            }

            var jprops = ((System.Collections.IEnumerable)json).Cast<JProperty>();
            var dict = jprops.ToDictionary(jprop => jprop.Name, jprop =>
            {
                var val = jprop.Value as JValue;
                if (val != null)
                {
                    return val.Value;
                }
                else if (jprop.Value as JArray != null)
                {
                    return jprop.Value as JArray;
                }
                else
                {
                    return jprop.Value as JObject;
                }
            });
            return dict;
        }

        #endregion

        #region OpenAndSaveAsync
        private async Task OpenAndSaveAsync(BreezeSaveWorkState saveWorkState)
        {
            saveWorkState.BeforeSave();
            await SaveChangesCoreAsync(saveWorkState);
        }
        #endregion

        #region SaveChangesAsync

        public async Task<BreezeSaveResult> SaveChangesAsync(JObject saveBundle)
        {
            if (SaveWorkState == null || SaveWorkState.WasUsed)
            {
                InitializeSaveState(saveBundle);
            }
            await OpenAndSaveAsync(SaveWorkState);
            return SaveWorkState.ToSaveResult();
        }

        #endregion

        #region SaveChangesCoreAsync

        async Task SaveChangesCoreAsync(BreezeSaveWorkState saveWorkState)
        {
            var changedItems = new Dictionary<Type, List<BreezeEntityInfo>>();
            CrmUser user = null;

            using (var context = new CrmDbContext())
            {
                if (_httpContextAccessor != null)
                {
                    user = _httpContextAccessor.GetCrmUser();
                    if (user == null)
                    {
                        user = await _httpContextAccessor.HttpContext.User.GetCrmUserAsync();
                    }
                }
                else
                {
                    user = await context.GetByKeyAsync(_contactKey);
                }
                context.Attach(user);
                var contact = user.Contact;

                foreach (var e in saveWorkState.SaveMap.ToList())
                {
                    foreach (var entityInfo in e.Value)
                    {
                        switch (entityInfo.EntityState)
                        {
                            #region Deleted

                            case BreezeEntityState.Deleted:
                                {
                                    var crmEntityType = entityInfo.Entity.GetType();

                                    #region LinkedAccount

                                    if (crmEntityType == typeof(LinkedAccount))
                                    {
                                        var breezeLinkedAccount = (LinkedAccount)entityInfo.Entity;

                                        var linkedAccount = user.CrmLinkedAccounts.FirstOrDefault(i => i.Key == breezeLinkedAccount.Key);
                                        if (linkedAccount == null)
                                        {
                                            throw new NotSupportedException($"LinkedAccount not exists: {breezeLinkedAccount.Key}");
                                        }
                                        var userManager = new CrmUserManager(context);
                                        await userManager.RemoveLinkedAccount(user, linkedAccount);
                                        changedItems.Add(breezeLinkedAccount.GetType(), new List<BreezeEntityInfo> { CreateEntityInfo(breezeLinkedAccount, BreezeEntityState.Deleted) });
                                    }

                                    #endregion

                                    #region LinkedEmailAccount

                                    else if (crmEntityType == typeof(LinkedEmailAccount))
                                    {
                                        var breezeLinkedEmailAccount = (LinkedEmailAccount)entityInfo.Entity;

                                        var linkedEmailAccount = user.CrmLinkedEMailAccounts.FirstOrDefault(i => i.Key == breezeLinkedEmailAccount.Key);
                                        var userManager = new CrmUserManager(context);

                                        await userManager.RemoveLinkedEMailAccount(user, linkedEmailAccount, true, _httpContextAccessor);
                                        changedItems.Add(breezeLinkedEmailAccount.GetType(), new List<BreezeEntityInfo> { CreateEntityInfo(breezeLinkedEmailAccount, BreezeEntityState.Deleted) });
                                    }

                                    #endregion

                                    #region CitaviLicense

                                    else if (crmEntityType == typeof(CitaviLicense))
                                    {
                                        var breezeCitaviLicense = (CitaviLicense)entityInfo.Entity;

                                        var crmCitaviLicense = user.Licenses.FirstOrDefault(i => i.Key == breezeCitaviLicense.Key);
                                        if (crmCitaviLicense.DataContractOwnerContactKey != user.Contact.Key)
                                        {
                                            throw new UnauthorizedAccessException();
                                        }

                                        context.Delete(crmCitaviLicense);
                                        await context.SaveAsync();
                                        await CrmUserCache.RemoveAsync(user);
                                        if (!string.IsNullOrEmpty(crmCitaviLicense.DataContractEndUserContactKey) &&
                                            crmCitaviLicense.DataContractEndUserContactKey != crmCitaviLicense.DataContractOwnerContactKey)
                                        {
                                            var endUser = await context.GetByKeyAsync(crmCitaviLicense.DataContractEndUserContactKey);
                                            if (endUser != null)
                                            {
                                                await CrmUserCache.RemoveAsync(endUser);
                                            }
                                        }
                                        changedItems.Add(breezeCitaviLicense.GetType(), new List<BreezeEntityInfo> { CreateEntityInfo(breezeCitaviLicense, BreezeEntityState.Deleted) });
                                    }

                                    #endregion
                                }
                                break;

                            #endregion

                            #region Modified

                            case BreezeEntityState.Modified:
                                {
                                    var crmEntityType = entityInfo.Entity.GetType();
                                    if (crmEntityType == typeof(Contact))
                                    {
                                        var breezeEntity = (Contact)entityInfo.Entity;

                                        if (_httpContextAccessor != null)
                                        {
                                            await AuthorizationManager.Instance.CheckAccessAsync(user.Principal, _httpContextAccessor.HttpContext.Items, AuthAction.Update, AuthResource.CrmContact, AuthResource.ContactKey(breezeEntity.Key));
                                        }
                                        else
                                        {
                                            await AuthorizationManager.Instance.CheckAccessAsync(user.Principal, AuthAction.Update, AuthResource.CrmContact, AuthResource.ContactKey(breezeEntity.Key));
                                        }

                                        contact.EntityState = EntityState.Changed;

                                        foreach (var val in entityInfo.OriginalValuesMap)
                                        {
                                            var key = val.Key.ToLowerInvariant();

                                            if (!contact.CanSetBreezeValue(CrmEntityNames.Contact, key))
                                            {
                                                continue;
                                            }

                                            var attribute = EntityNameResolver.ResolveAttributeName(CrmEntityNames.Contact, key);

                                            if (breezeEntity.Attributes.ContainsKey(attribute))
                                            {
                                                contact.SetValueWithChecks(attribute, breezeEntity[attribute], raisePropertyChangedEvent: true);
                                            }
                                            else
                                            {
                                                contact.SetValueWithChecks(attribute, null, raisePropertyChangedEvent: true);
                                            }
                                        }

                                        changedItems.Add(crmEntityType, new List<BreezeEntityInfo> { CreateEntityInfo(contact, BreezeEntityState.Modified) });
                                        await context.SaveAndUpdateUserCacheAsync(user);
                                    }
                                }
                                break;

                                #endregion
                        }
                    }
                }
            }

            saveWorkState.SaveMap = changedItems;
            saveWorkState.KeyMappings = _keyMappings;
            if (_entityErrors.Any())
            {
                saveWorkState.EntityErrors = _entityErrors;
            }
        }

        #endregion

        #endregion
    }
}
