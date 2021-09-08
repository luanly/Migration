using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Crm.Web.Cleverbridge;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    //https://reference.cleverbridge.com/reference/subscription
    //https://docs.cleverbridge.com/public/all/integrating-your-system/subscription-api.htm
    public class SubscriptionManager
    {
        #region Felder

        const string BaseUrl = "https://rest.cleverbridge.com/subscription";

        #endregion

        #region Konstruktor

        public SubscriptionManager(CrmDbContext context)
        {
            DbContext = context;
        }

        #endregion

        #region Eigenschaften

        public CrmDbContext DbContext { get; }

        #endregion

        #region Methoden

        #region ActivateSubscriptionItem

        /// <summary>
        /// Aktiviert ein SubscriptionItem bei CB. Nur für internen Gebrauch bei Increase/Decrease von SubscriptionItem
        /// https://reference.cleverbridge.com/reference/subscription#reinstatesubscriptionitems
        /// </summary>
        internal async Task<bool> ActivateSubscriptionItem(string subscriptionId, string subscriptionItemId)
        {
            try
            {
                var request = new ReinstateSubscriptionItemsRequest();
                request.GenerateMail = !CrmConfig.IsUnittest;

                request.SubscriptionId = subscriptionId;
                request.Items = new[] { int.Parse(subscriptionItemId, CultureInfo.InvariantCulture) };

                var response = await Post("reinstatesubscriptionitems", request);
                if (response == null)
                {
                    Telemetry.TrackTrace($"Subscription: ActivateSubscriptionItem failed. SubscriptionId: {subscriptionId} | SubscriptionItemId: {subscriptionItemId}", SeverityLevel.Error);
                }
                return response != null;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return false;
        }

        #endregion

        #region ActivateCitaviLicenseSubscription

        /// <summary>
        /// Die Lizenz erhält den SubscriptionStatus Activated.
        /// Bei CB erhöhen wir die SubscriptionItemQuantity.
        /// Es wird kein Order ausgelöst
        /// </summary>
        public async Task<SubscriptionRequestResponse> ActivateCitaviLicenseSubscription(CitaviLicense citaviLicense, bool getCustomerPricePreviewOnly)
        {
            if (citaviLicense == null)
            {
                throw new NotSupportedException("CitaviLicense must not be null");
            }
            if (citaviLicense.SubscriptionStatus == LicenseSubscriptionStatus.Active)
            {
                throw new NotSupportedException($"SubscriptionStatus must be deactivated: {citaviLicense.Key}");
            }

            var subscriptionItem = await citaviLicense.SubscriptionItem.Get(EntityPropertySets.SubscriptionItem);
            var subscription = await subscriptionItem.Subscription.Get(EntityPropertySets.Subscription);
            var subscriptionInfo = await GetSubscriptionInfo(subscription);
            var subscriptionItemInfo = subscriptionInfo.Subscription.Items.FirstOrDefault(si => si.IsCurrent && si.RunningNo == subscriptionItem.CbSubscriptionItemId);

            SubscriptionRequestResponse response;
            if (subscriptionItemInfo.Status != SubscriptionItemStatusType.Active)
            {
                //Wir müssen das SubscriptionItem aktivieren
                //Das Aktivieren löst eine Notification aus. Da aber der Status bereits auf Active ist, ignorieren wir es

                if (!getCustomerPricePreviewOnly)
                {
                    subscriptionItem.ItemStatus = SubscriptionItemStatusType.Active;
                    await DbContext.SaveAsync();
                }

                if (!await ActivateSubscriptionItem(subscription.CbSubscriptionId, subscriptionItem.CbSubscriptionItemId))
                {
                    //Zurücksetzen
                    if (!getCustomerPricePreviewOnly)
                    {
                        subscriptionItem.ItemStatus = SubscriptionItemStatusType.Deactivated;
                        await DbContext.SaveAsync();
                    }
                    return null;
                }

                response = new SubscriptionRequestResponse();
            }
            else
            {
                //Nur ChangeSubscriptionItemQuantity aufrufen wenn die Subscription aktiv ist.
                response = await ChangeSubscriptionItemQuantity(subscription.CbSubscriptionId, subscriptionItem.CbSubscriptionItemId, subscriptionItemInfo.Quantity + 1, false, getCustomerPricePreviewOnly);
            }
            if (response == null)
            {
                return null;
            }

            if (getCustomerPricePreviewOnly)
            {
                return response;
            }

            var owner = await citaviLicense.Owner.Get(ContactPropertyId.Key);
            var enduser = await citaviLicense.EndUser.Get(ContactPropertyId.Key);

            citaviLicense.SubscriptionStatus = LicenseSubscriptionStatus.Active;
            subscriptionItem.ItemStatus = SubscriptionItemStatusType.Active;

            await DbContext.SaveAsync();

            await CrmUserCache.RemoveAsync(owner.Key);
            if (enduser != null &&
                owner.Key != enduser.Key)
            {
                await CrmUserCache.RemoveAsync(enduser.Key);
            }

            return response;
        }

        #endregion

        #region AddSubscriptionItem

        /// <summary>
        /// Löst eine neue Bestellung aus
        /// Lizenzen werden bei OrderProcess erstellt
        /// Es wird eine neues SubscriptionItem erstellt
        /// https://reference.cleverbridge.com/reference/subscription#addsubscriptionitem
        /// </summary>
        public async Task<SubscriptionRequestResponse> AddSubscriptionItem(Subscription subscription, CleverbridgeProduct product, int quantity = 1, bool getCustomerPricePreviewOnly = false, int? affiliateId = null)
        {
            if (subscription == null)
            {
                throw new NotSupportedException("Subscription must not be null");
            }
            if (product == null)
            {
                throw new NotSupportedException("Product must not be null");
            }
            try
            {
                var request = new AddSubscriptionItemRequest();
                request.AffiliateId = affiliateId;
                request.GenerateMail = !CrmConfig.IsUnittest;
                request.ProductId = product.ProductId;
                request.Quantity = quantity;
                request.SubscriptionId = subscription.CbSubscriptionId;

                request.AlignmentSettings = new AlignmentSettings();
                request.AlignmentSettings.AlignToCurrentInterval = true;
                request.AlignmentSettings.ExtendInterval = false;
                request.AlignmentSettings.GetCustomerPricePreviewOnly = getCustomerPricePreviewOnly;

                var response = await Post("addsubscriptionitem", request);
                if (response == null)
                {
                    Telemetry.TrackTrace($"Subscription: AddSubscriptionItem failed. SubscriptionId {subscription.CbSubscriptionId}", SeverityLevel.Error);
                }
                return response;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return null;
        }

        #endregion

        #region ChangeSubscriptionItemQuantity

        /// <summary>
        /// Ändert die Anzahl von einem SubscriptionItem. Löst keine Bestellung aus.
        /// </summary>
        internal async Task<SubscriptionRequestResponse> ChangeSubscriptionItemQuantity(string subscriptionId, string subscriptionItemId, int newQuantity, bool alignToCurrentInterval, bool getCustomerPricePreviewOnly)
        {
            try
            {
                var subscriptionInfo = await GetSubscriptionInfo(subscriptionId);
                var subscriptionItemInfo = subscriptionInfo.Subscription.Items.FirstOrDefault(c => c.RunningNo == subscriptionItemId);

                var request = new UpdateSubscriptionItemRequest();
                request.AlignmentSettings = new AlignmentSettings();
                request.AlignmentSettings.AlignToCurrentInterval = alignToCurrentInterval;
                request.AlignmentSettings.GetCustomerPricePreviewOnly = getCustomerPricePreviewOnly;

                request.GenerateMail = !CrmConfig.IsUnittest;
                request.SubscriptionId = subscriptionId;
                request.RunningNumber = int.Parse(subscriptionItemId, CultureInfo.InvariantCulture);
                request.Quantity = newQuantity;

                if (subscriptionItemInfo.Quantity > newQuantity)
                {
                    request.UpdateAction = UpdateSubscriptionItemAction.Downgrade;
                }
                else
                {
                    request.UpdateAction = UpdateSubscriptionItemAction.Upgrade;
                }

                request.ProductId = subscriptionItemInfo.ProductId;

                var response = await Post("updatesubscriptionitem", request);

                if (response == null)
                {
                    Telemetry.TrackTrace($"Subscription: ChangeSubscriptionItemQuantity failed. SubscriptionId {subscriptionId} | SubscriptionItemId {subscriptionItemId}", SeverityLevel.Error);
                }

                return response;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return null;
        }

        #endregion

        #region DeactivateCitaviLicenseSubscription

        /// <summary>
        /// Die Lizenz erhält den SubscriptionStatus Deactivated.
        /// Die Lizenz ist weiterhin gültig (ExpiryDate)
        /// Bei CB verringern wir die SubscriptionItemQuantity.
        /// Es wird kein Order ausgelöst
        /// Wenn es die letzte aktive Lizenz ist, wird das SubscriptionItem in CB deaktiviert
        /// </summary>
        public async Task<SubscriptionRequestResponse> DeactivateCitaviLicenseSubscription(CitaviLicense citaviLicense, bool getCustomerPricePreviewOnly)
        {
            if (citaviLicense == null)
            {
                throw new NotSupportedException("CitaviLicense must not be null");
            }

            if (citaviLicense.SubscriptionStatus == LicenseSubscriptionStatus.Deactivated)
            {
                throw new NotSupportedException($"SubscriptionStatus must be active: {citaviLicense.Key}");
            }

            var subscriptionItem = await citaviLicense.SubscriptionItem.Get(EntityPropertySets.SubscriptionItem);
            var subscription = await subscriptionItem.Subscription.Get(EntityPropertySets.Subscription);
            var subscriptionInfo = await GetSubscriptionInfo(subscription);
            var subscriptionItemInfo = subscriptionInfo.Subscription.Items.FirstOrDefault(si => si.IsCurrent && si.RunningNo == subscriptionItem.CbSubscriptionItemId);

            SubscriptionRequestResponse response;
            if (subscriptionItemInfo.Quantity == 1)
            {
                if (!getCustomerPricePreviewOnly)
                {
                    subscriptionItem.ItemStatus = SubscriptionItemStatusType.Deactivated;
                    await DbContext.SaveAsync();
                }
                //Wir müssen das SubscriptionItem deaktivieren
                if (!await DeactivateSubscriptionItem(subscription.CbSubscriptionId, subscriptionItem.CbSubscriptionItemId))
                {
                    if (!getCustomerPricePreviewOnly)
                    {
                        //Undo der Aktion
                        subscriptionItem.ItemStatus = SubscriptionItemStatusType.Active;
                        await DbContext.SaveAsync();
                    }
                    return null;
                }
                response = new SubscriptionRequestResponse { Success = true };

            }
            else
            {
                response = await ChangeSubscriptionItemQuantity(subscription.CbSubscriptionId, subscriptionItem.CbSubscriptionItemId, subscriptionItemInfo.Quantity - 1, false, getCustomerPricePreviewOnly);
                if (response == null)
                {
                    return null;
                }
            }

            if (getCustomerPricePreviewOnly)
            {
                return response;
            }

            var owner = await citaviLicense.Owner.Get(ContactPropertyId.Key);
            var enduser = await citaviLicense.EndUser.Get(ContactPropertyId.Key);

            citaviLicense.SubscriptionStatus = LicenseSubscriptionStatus.Deactivated;

            await DbContext.SaveAsync();

            await CrmUserCache.RemoveAsync(owner.Key);
            if (enduser != null &&
                owner.Key != enduser.Key)
            {
                await CrmUserCache.RemoveAsync(enduser.Key);
            }

            return response;
        }

        #endregion

        #region DeactivateSubscription

        /// <summary>
        /// Deaktiviert alle SubscriptionItems in der Subscription
        /// </summary>
        public async Task<IEnumerable<SubscriptionRequestResponse>> DeactivateSubscription(string subscriptionKey)
        {
            var subscription = await DbContext.Get<Subscription>(subscriptionKey);
            if (subscription == null)
            {
                throw new NotSupportedException($"{nameof(Subscription)} must not be null");
            }

            var responses = new List<SubscriptionRequestResponse>();
            var subscriptionItems = await subscription.SubscriptionItems.Get();
            foreach (var subscriptionItem in subscriptionItems)
            {
                var citaviLicenses = await subscriptionItem.Licenses.Get(EntityPropertySets.CitaviLicense);
                foreach (var citaviLicense in citaviLicenses)
                {
                    if (citaviLicense.SubscriptionStatus == LicenseSubscriptionStatus.Deactivated)
                    {
                        continue;
                    }
                    var result = await DeactivateCitaviLicenseSubscription(citaviLicense, false);
                    result.CitaviLicenseKey = citaviLicense.Key;
                    responses.Add(result);
                }
            }
            return responses;
        }

        #endregion

        #region DeactivateSubscriptionItem

        /// <summary>
        /// Deaktiviert das SubscriptionItem
        /// https://reference.cleverbridge.com/reference/subscription#deactivatesubscriptionitems
        /// </summary>
        internal async Task<bool> DeactivateSubscriptionItem(string subscriptionId, string subscriptionItemId)
        {
            try
            {
                var request = new DeactivateSubscriptionItemsRequest();
                request.AllowReinstate = true;
                request.GenerateMail = !CrmConfig.IsUnittest;
                request.SubscriptionId = subscriptionId;
                request.Items = new[] { int.Parse(subscriptionItemId, CultureInfo.InvariantCulture) };

                var response = await Post("deactivatesubscriptionitems", request);
                if (response == null)
                {
                    Telemetry.TrackTrace($"Subscription: DeactivateSubscriptionItem failed. SubscriptionId {subscriptionId} | SubscriptionItemId {subscriptionItemId}", SeverityLevel.Error);
                }
                return response != null;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return false;
        }

        #endregion

        #region Get

        async Task<T> Get<T>(string method, string urlParams)
        {
            using (var response = await CleverbridgeHttpClient.Instance.GetAsync($"{BaseUrl}/{method}?{urlParams}"))
            {
                var json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    Telemetry.TrackTrace($"{method} failed: " + response.StatusCode, SeverityLevel.Error, property1: ("json", json));
                    return default;
                }

                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        #endregion

        #region GetNextRenewalPrice

        public async Task<SubscriptionRequestResponse> GetNextRenewalPrice(Subscription subscription, SubscriptionItem subscriptionItem)
        {
            if (subscription == null)
            {
                throw new NotSupportedException("Subscription must not be null");
            }
            if (subscriptionItem == null)
            {
                throw new NotSupportedException("SubscriptionItem must not be null");
            }
            return await GetNextRenewalPrice(subscription.CbSubscriptionId, subscriptionItem.CbSubscriptionItemId);
        }

        /// <summary>
        /// Preis bei der nächsten Verlängerung
        /// </summary>
        internal async Task<SubscriptionRequestResponse> GetNextRenewalPrice(string subscriptionId, string subscriptionItemId)
        {
            /// Mail von 14.01.2020:
            /// If a client wants to get the next subscription renewal price via API, they can use a trick to update the price of the product via the API and set "GetCustomerPricePreviewOnly" to 'true'.
            /// This means the system won't apply any changes, it will just return the price of the next renewal if you were to do the update because changing the name doesn't change the price 

            var subscriptionInfo = await GetSubscriptionInfo(subscriptionId);
            var subscriptionItemInfo = subscriptionInfo.Subscription.Items.FirstOrDefault(c => c.RunningNo == subscriptionItemId);

            var request = new UpdateSubscriptionItemRequest();
            request.AlignmentSettings = new AlignmentSettings();
            request.AlignmentSettings.AlignToCurrentInterval = false;
            request.AlignmentSettings.GetCustomerPricePreviewOnly = true;

            request.GenerateMail = false;
            request.SubscriptionId = subscriptionId;
            request.RunningNumber = int.Parse(subscriptionItemId, CultureInfo.InvariantCulture);
            request.ProductNameEn = subscriptionItemInfo.ProductName + "-";
            request.Quantity = subscriptionItemInfo.Quantity;
            request.UpdateAction = UpdateSubscriptionItemAction.Update;

            request.ProductId = subscriptionItemInfo.ProductId;

            var response = await Post("updatesubscriptionitem", request);
            return response;
        }

        #endregion

        #region GetSubscriptionStatus

        internal async Task<SubscriptionStatusType> GetSubscriptionStatus(string subscriptionId)
        {
            var subscriptionResponse = await GetSubscriptionInfo(subscriptionId);
            return subscriptionResponse.Subscription.SubscriptionStatus;
        }

        #endregion

        #region GetSubscriptionItemStatus

        internal async Task<SubscriptionItemStatusType> GetSubscriptionItemStatus(string subscriptionId, string subscriptionItemId)
        {
            var subscriptionResponse = await GetSubscriptionInfo(subscriptionId);

            foreach (var item in subscriptionResponse.Subscription.Items)
            {
                if (item.RunningNo == subscriptionItemId && item.IsCurrent)
                {
                    return item.Status;
                }
            }
            throw new NotSupportedException("SubscriptionItemId does not exists");
        }

        #endregion

        #region GetSubscriptionInfo
        /// <summary>
        /// Retrieves all subscription items for a particular subscription using the subscription ID.
        /// Subscription items contain the current status and a list of all related purchases.
        /// https://reference.cleverbridge.com/reference/subscription#getsubscription
        /// </summary>
        async Task<SubscriptionInfoResponse> GetSubscriptionInfo(Subscription subscription)
            => await GetSubscriptionInfo(subscription.CbSubscriptionId);

        /// <summary>
        /// Retrieves all subscription items for a particular subscription using the subscription ID.
        /// Subscription items contain the current status and a list of all related purchases.
        /// https://reference.cleverbridge.com/reference/subscription#getsubscription
        /// </summary>
        internal async Task<SubscriptionInfoResponse> GetSubscriptionInfo(string subscriptionId)
        {
            try
            {
                var subscriptionResponse = await Get<SubscriptionInfoResponse>("getsubscription", $"subscriptionId={subscriptionId}");
                if (subscriptionResponse == null)
                {
                    throw new NotSupportedException("Subscription does not exists");
                }
                var subscriptionItems = new List<SubscriptionItemInfo>();
                foreach (var item in subscriptionResponse.Subscription.Items.ToList())
                {
                    if (item.IsCurrent)
                    {
                        subscriptionItems.Add(item);
                    }
                }
                subscriptionResponse.Subscription.Items = subscriptionItems.ToArray();

                return subscriptionResponse;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return null;
        }

        #endregion

        #region GetSubscriptionInfoByContactKey

        /// <summary>
        /// Retrieves the subscriptions, including all subscription items, for a particular customer using the customer ID, the customer reference ID , or the customer's email address.
        /// Subscription items contain the current status and a list of all related purchases.
        /// https://reference.cleverbridge.com/reference/subscription#getsubscriptionsforcustomer
        /// </summary>
        internal async Task<SubscriptionsForCustomerResponse> GetSubscriptionInfoByContactKey(string contactKey)
        {
            try
            {
                return await Get<SubscriptionsForCustomerResponse>("getsubscriptionsforcustomer", $"customerReferenceId={contactKey}");
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return null;
        }

        #endregion

        #region GetSubscriptionInfoByContactEmail

        /// <summary>
        /// Retrieves the subscriptions, including all subscription items, for a particular customer using the customer ID, the customer reference ID , or the customer's email address.
        /// Subscription items contain the current status and a list of all related purchases.
        /// https://reference.cleverbridge.com/reference/subscription#getsubscriptionsforcustomer
        /// </summary>
        internal async Task<SubscriptionsForCustomerResponse> GetSubscriptionInfoByContactEmail(string email)
        {
            try
            {
                return await Get<SubscriptionsForCustomerResponse>("getsubscriptionsforcustomer", $"customerEmail={email}");
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }

            return null;
        }

        #endregion

        #region GetUserSubscriptions

        public async Task<IEnumerable<SubscriptionItem>> GetUserSubscriptions(CrmUser user)
        {
            if (user == null)
            {
                throw new NotSupportedException("User must not be null");
            }
            var query = new Query.FetchXml.GetUserSubscriptions(user.Contact.Id).TransformText();
            var result = await DbContext.Fetch<SubscriptionItem>(FetchXmlExpression.Create<SubscriptionItem>(query));
            return result;
        }

        #endregion

        #region IncreaseSubscriptionItem

        /// <summary>
        /// Löst eine neue Bestellung aus. Die Lizenzen werden via OrderProcess erstellt.
        /// Aktiviert das SubscriptionItem sollte es deaktiviert sein
        /// https://reference.cleverbridge.com/reference/subscription#increasesubscriptionitemquantity
        /// </summary>

        public async Task<SubscriptionRequestResponse> IncreaseSubscriptionItem(Subscription subscription, SubscriptionItem subscriptionItem, int quantity, bool getCustomerPricePreviewOnly = false)
        {
            if (subscription == null)
            {
                throw new NotSupportedException("Subscription must not be null");
            }
            if (subscriptionItem == null)
            {
                throw new NotSupportedException("SubscriptionItem must not be null");
            }
            var response = await IncreaseSubscriptionItem(subscription.CbSubscriptionId, subscriptionItem.CbSubscriptionItemId, quantity, getCustomerPricePreviewOnly);
            if (!getCustomerPricePreviewOnly &&
                subscriptionItem.ItemStatus != SubscriptionItemStatusType.Active)
            {
                subscriptionItem.ItemStatus = SubscriptionItemStatusType.Active;
                await DbContext.SaveAsync();
            }
            return response;
        }

        /// <summary>
        /// Löst eine neue Bestellung aus. Die Lizenzen werden via OrderProcess erstellt.
        /// Aktiviert das SubscriptionItem sollte es deaktiviert sein
        /// https://reference.cleverbridge.com/reference/subscription#increasesubscriptionitemquantity
        /// </summary>
        internal async Task<SubscriptionRequestResponse> IncreaseSubscriptionItem(string subscriptionId, string subscriptionItemId, int quantity, bool getCustomerPricePreviewOnly = false)
        {
            try
            {
                var subscriptionInfo = await GetSubscriptionInfo(subscriptionId);
                var subscriptionItemInfo = subscriptionInfo.Subscription.Items.FirstOrDefault(si => si.IsCurrent && si.RunningNo == subscriptionItemId);

                if (subscriptionItemInfo.Status != SubscriptionItemStatusType.Active)
                {
                    if (!await ActivateSubscriptionItem(subscriptionId, subscriptionItemId))
                    {
                        return null;
                    }
                }

                var request = new IncreaseSubscriptionItemQuantityRequest();
                request.AlignmentSettings = new AlignmentSettings();
                request.AlignmentSettings.AlignToCurrentInterval = true;
                request.AlignmentSettings.GetCustomerPricePreviewOnly = getCustomerPricePreviewOnly;

                request.GenerateMail = !CrmConfig.IsUnittest;
                request.SubscriptionId = subscriptionId;
                request.RunningNumber = int.Parse(subscriptionItemId, CultureInfo.InvariantCulture);
                request.Quantity = quantity;

                var response = await Post("increasesubscriptionitemquantity", request);

                if (response == null)
                {
                    Telemetry.TrackTrace($"Subscription: IncreaseSubscriptionItem failed. SubscriptionId {subscriptionId} | SubscriptionItemId {subscriptionItemId}", SeverityLevel.Error);
                }

                return response;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return null;
        }

        #endregion

        #region Post

        async Task<SubscriptionRequestResponse> Post(string method, object request)
        {
            var subscriptionResponse = await Post<SubscriptionRequestResponse>(method, request);
            if (subscriptionResponse == null)
            {
                return null;
            }
            if (subscriptionResponse.ResultMessage != "OK")
            {
                var json = JsonConvert.SerializeObject(request);
                Telemetry.TrackTrace($"{method} failed: " + subscriptionResponse.ResultMessage, SeverityLevel.Error, property1: ("json", json));
                return null;
            }
            subscriptionResponse.Success = true;
            subscriptionResponse.ResultMessage = null;
            return subscriptionResponse;
        }
        async Task<T> Post<T>(string method, object request)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }), Encoding.UTF8, "application/json"))
            {
                using (var response = await CleverbridgeHttpClient.Instance.PostAsync($"{BaseUrl}/{method}", content))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        Telemetry.TrackTrace($"{method} failed: " + response.StatusCode, SeverityLevel.Error, property1: ("json", json));
                        return default(T);
                    }

                    return JsonConvert.DeserializeObject<T>(json);
                }
            }

        }

        #endregion

        #region UpdateNextBillingDate
        /// <summary>
        /// Modifies the next billing date for a subscription. This endpoint is commonly used to extend a billing period.
        /// https://reference.cleverbridge.com/reference/subscription#updatenextbillingdate
        /// </summary>
        public async Task<bool> UpdateNextBillingDate(Subscription subscription, DateTime nextBillingDate)
        {
            if (subscription == null)
            {
                throw new NotSupportedException("Subscription must not be null");
            }
            var success = await UpdateNextBillingDate(subscription.CbSubscriptionId, nextBillingDate);
            if (!success)
            {
                return false;
            }
            var subscriptionItems = await subscription.SubscriptionItems.Get();
            foreach (var item in subscriptionItems)
            {
                item.NextBillingDate = nextBillingDate;
                var licences = await item.Licenses.Get();
                foreach (var license in licences)
                {
                    license.ExpiryDate = nextBillingDate;
                }
            }
            await DbContext.SaveAsync();
            return true;
        }

        /// <summary>
        /// Modifies the next billing date for a subscription. This endpoint is commonly used to extend a billing period.
        /// https://reference.cleverbridge.com/reference/subscription#updatenextbillingdate
        /// </summary>
        internal async Task<bool> UpdateNextBillingDate(string subscriptionId, DateTime nextBillingDate)
        {
            try
            {
                var request = new UpdateNextBillingDateRequest
                {
                    SubscriptionId = subscriptionId,
                    NextBillingDate = nextBillingDate
                };

                var response = await Post("updatenextbillingdate", request);
                if (response == null)
                {
                    Telemetry.TrackTrace($"Subscription: UpdateNextBillingDate failed. SubscriptionId {subscriptionId}", SeverityLevel.Error);
                }
                return response != null;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return false;
        }

        #endregion

        #region RenewSubscription

        /// <summary>
        /// Triggers an instant renewal of a subscription. 
        /// If the renewal payment can be processed with the current payment details, then the subscription is renewed, and the customer receives an email confirmation with the delivery details. 
        /// If the renewal payment cannot be processed with the current payment details, or if an offline payment is required, then the customer receives an email with payment instructions.
        /// You can use this API if a B2B customer has budget left at the end of the year, and they want to renew their auto-renewal subscription before the end of the year.
        /// https://reference.cleverbridge.com/reference/subscription#renewsubscription
        /// </summary>
        [ExcludeFromCodeCoverage]
        internal async Task<RenewSubscriptionResponse> RenewSubscription(string subscriptionId)
        {
            var subscriptionInfo = await GetSubscriptionInfo(subscriptionId);
            if (subscriptionInfo == null)
            {
                throw new NotSupportedException("Subscription does not exists");
            }
            var request = new RenewSubscriptionRequest();
            request.SubscriptionId = subscriptionId;
            var response = await Post<RenewSubscriptionResponse>("renewsubscription", request);
            if (response == null)
            {
                Telemetry.TrackTrace($"Subscription: RenewSubscription failed: SubscriptionId {subscriptionId}");
            }
            if (response.ResultMessage != "OK")
            {
                var json = JsonConvert.SerializeObject(request);
                Telemetry.TrackTrace($"Subscription: RenewSubscription failed: SubscriptionId {subscriptionId}", SeverityLevel.Error, property1: ("json", json));
                return null;
            }
            response.Success = true;
            response.ResultMessage = null;
            return response;
        }

        #endregion

        #endregion
    }
}
