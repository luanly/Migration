using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public partial class CrmDbContext
    {
        #region Eigenschaften

        #region SuspendUpdateUser

        /// <summary>
        /// Wenn true, passiert bei CrmUserManager.Update(user) nichts.
        /// Verwenden, wenn BA zuviele Updates durchführt.
        /// </summary>
        public bool SuspendUpdateUser { get; set; }

        #endregion

        #endregion

        #region Methoden

        #region GetByEmailAsync

        public async Task<CrmUser> GetByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }
            email = email.RemoveNonStandardWhitespace();
            var user = await GetUser(LinkedEmailAccountPropertyId.Email, email);
            if (user == null)
            {
                return null;
            }

            var linkedAccount = user.GetLinkedEmailAccount(email);
            if (linkedAccount == null)
            {
                //Cache Problem. User in Cache doesnt have the requested email
                user = await GetUser(LinkedEmailAccountPropertyId.Email, email, ignoreCache: true);
                linkedAccount = user.GetLinkedEmailAccount(email);
            }
            if (linkedAccount == null)
            {
                throw new ArgumentNullException($"GetByEmail: LinkedEmailAccount not found, but user is not null: {email}, {user.Key} / {user.Email}");
            }
            user.SetVerificationData(linkedAccount);
            return user;
        }

        #endregion

        #region GetByKeyAsync
        public Task<CrmUser> GetByKeyAsync(string key, bool updateCacheIfMissing = false)
        {
            return CrmUserCache.GetAsync(this, key, fromCacheOnly: false, updateCacheIfMissing: updateCacheIfMissing);
        }

        public Task<CitaviCrmEntity> GetCrmEntityByKeyAsync(string key, bool updateCacheIfMissing = false)
        {
            return CrmUserCache.GetCrmEntityAsync(this, key, fromCacheOnly: false, updateCacheIfMissing: updateCacheIfMissing);
        }
        #endregion

        #region GetByLinkedAccountAsync
        public async Task<CrmUser> GetByLinkedAccountAsync(string provider, string id)
        {
            return await GetUser(provider, id);
        }
        #endregion

        #region GetByMergeVerificationKey
        public async Task<CrmUser> GetByMergeVerificationKey(string mergeVerificationKey)
        {
            return await GetUserFromCrm(ContactPropertyId.MergeAccountVerificationKey, mergeVerificationKey);
        }
        #endregion

        #region GetByVerificationKeyAsync

        /// <summary>
        /// WICHTIG: Hier muss der vkey bereits entcrypted sein. UserManager.GetByVKeyAsync
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<CrmUser> GetByVerificationKeyAsync(string key)
        {
            var user = await GetUser(LinkedEmailAccountPropertyId.VerificationKey, key, ignoreCache: true);
            if (user == null)
            {
                return null;
            }
            var linkedEmailAccount = user.CrmLinkedEMailAccounts.FirstOrDefault(i => i.VerificationKey == key);
            if (linkedEmailAccount == null)
            {
                var prop = new Dictionary<string, string>();
                prop.Add(nameof(user.Contact.Key), user.Contact.Key);
                prop.Add(nameof(LinkedEmailAccountPropertyId.VerificationKey), key);
                Telemetry.TrackTrace("GetByVerificationKeyAsync user is not null, but verificationkey not found", SeverityLevel.Warning, property1: ("props", prop));
                return null;
            }
            user.SetVerificationData(linkedEmailAccount);
            return user;
        }

        #endregion

        #region GetUser

        internal async Task<CrmUser> GetUser(string identityProviderId, string nameIdentifier)
        {
            var xml = new Query.FetchXml.GetContactByLinkedAccount(identityProviderId, nameIdentifier).TransformText();
            var result = await Fetch(FetchXmlExpression.Create<Contact>(xml), observe: false);
            if (result == null || !result.Any())
            {
                return null;
            }

            var user = await CrmUserCache.GetAsync(this, result.First().Attributes[CrmAttributeNames.Key].ToString(), true);
            if (user != null)
            {
                return user;
            }
            return await LoadCrmUser(result);
        }

        internal async Task<CrmUser> GetUser(LinkedEmailAccountPropertyId attribute, object value, bool ignoreCache = false)
        {
            var xml = new Query.FetchXml.GetContactByLinkedEmailAccount(attribute, value.ToString()).TransformText();
            var result = await Fetch(FetchXmlExpression.Create<Contact>(xml), observe: false);
            if (result == null || !result.Any())
            {
                return null;
            }

            if (!ignoreCache)
            {
                var user = await CrmUserCache.GetAsync(this, result.First().Attributes[CrmAttributeNames.Key].ToString(), true);
                if (user != null)
                {
                    return user;
                }
            }
            return await LoadCrmUser(result);
        }

        public async Task<CrmUser> GetUserFromCrm(ContactPropertyId attribute, object value)
        {
            if (value == null)
            {
                return null;
            }
            var xml = new Query.FetchXml.GetContactByContactProperty(attribute, value.ToString()).TransformText();
            var result = await Fetch(FetchXmlExpression.Create<Contact>(xml), observe: false);
            if (result == null || !result.Any())
            {
                return null;
            }

            return await LoadCrmUser(result);
        }

        public async Task<CitaviCrmEntity> GetCitaviCrmEntityFromCrm(ContactPropertyId attribute, object value)
        {
            if (value == null)
            {
                return null;
            }
            var xml = new Query.FetchXml.GetContactByContactProperty(attribute, value.ToString()).TransformText();
            var result = await Fetch(FetchXmlExpression.Create<Contact>(xml), observe: false);
            if (result == null || !result.Any())
            {
                return null;
            }

            return result.First();
        }

        async Task<CrmUser> LoadCrmUser(IEnumerable<CitaviCrmEntity> dataCollection)
        {
            try
            {
                var start = DateTimeOffset.Now;

                var crmSet = new CrmSet(dataCollection);
                if (crmSet.Contacts.Count() > 1)
                {
                    var s = new StringBuilder();
                    foreach (var contact in crmSet.Contacts)
                    {
                        s.AppendLine($"{contact.Key}");
                    }
                    var execption = new NotSupportedException($"{nameof(crmSet.Contacts)} Count == {crmSet.Contacts.Count()}");
                    Telemetry.TrackException(execption, property1: (nameof(TelemetryProperty.Description), s.ToString()));
                }

                var user = new CrmUser(crmSet.Contacts.First());
                await user.Load(this, attachToContext: false);
                Attach(user);
                return user;
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception);
            }
            return null;
        }

        #endregion

        #region SetUserImageAsync

        internal async Task SetUserImageAsync(Contact contact, byte[] bytes)
        {
            var entity = new Contact();
            entity.Id = contact.Id;
            entity.EntityImage = bytes;
            entity.EntityState = EntityState.Changed;

            var changed = new CrmEntityChanged();
            changed.Entity = entity;
            changed.Properties.Add("entityimage");

            await CrmWebApi.SaveAsync(new[] { changed });
            //kein Update von UserImage, da wir hier die User-Bytes haben (kann sehr gross sein)
            //beim nächsten Call werden das Image (verkleinert) aus der CRM gespeichert (via GetAsync).
            await CrmUserImageCache.RemoveAsync(contact.Key);
        }

        #endregion

        #endregion
    }
}
