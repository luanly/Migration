using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Crm.Web.Cleverbridge;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class CleverbridgeOrderManager
    {
        #region Felder

        readonly List<CrmUser> _orderUserCache = new List<CrmUser>();
        readonly List<Subscription> _subscriptionCache = new List<Subscription>();
        readonly List<SubscriptionItem> _subscriptionItemCache = new List<SubscriptionItem>();

        #endregion

        #region Konstruktor

        public CleverbridgeOrderManager(CrmDbContext dbContext)
        {
            DbContext = dbContext;
            UserManager = new CrmUserManager(DbContext);
            LicenseManager = new LicenseManager(DbContext);
            VoucherManager = new VoucherManager(dbContext);
        }

        #endregion

        #region Eigenschaften

        CrmDbContext DbContext { get; set; }
        LicenseManager LicenseManager { get; set; }
        CrmUserManager UserManager { get; set; }
        VoucherManager VoucherManager { get; set; }

        #endregion

        #region Methoden

        #region GetOrCreateUser

        async Task<CrmUser> GetOrCreateUserAsync(ContactType cleverbridgeContact)
        {
            var exsiting = _orderUserCache.FirstOrDefault(i => string.Equals(i.Email, cleverbridgeContact.Email, StringComparison.InvariantCultureIgnoreCase));
            if (exsiting != null)
            {
                return exsiting;
            }

            var email = cleverbridgeContact.Email.RemoveNonStandardWhitespace();
            var user = await DbContext.GetByEmailAsync(email);

            if (user == null)
            {
                user = UserManager.CreateUserFromCleverbridgeOrder(cleverbridgeContact);
            }
            else
            {
                var linkedEmail = user.GetLinkedEmailAccount(cleverbridgeContact.Email);
                if (linkedEmail != null &&
                   !linkedEmail.IsVerified.GetValueOrDefault(false))
                {
                    //user.Contact.IsLoginAllowed = true;
                    //user.Contact.IsVerified = true;
                    //email.IsVerified = true;
                    //email.ClearVerifcationData();
                }
                else if (linkedEmail == null)
                {
                    user.AddLinkedEMailAccount(email);
                    //user.Contact.IsLoginAllowed = true;
                    //user.Contact.IsVerified = true;
                }
            }

            user.Contact.StatusCode = StatusCode.Active;
            _orderUserCache.Add(user);
            return user;
        }

        #endregion

        #region GetOrCreateSubscription

        async Task<Subscription> GetOrCreateSubscription(PurchaseItemRecurringBillingType recurringBilling, OrderProcess orderProcess, CrmUser subscriptionOwner, bool allowReorder)
        {
            var subscription = _subscriptionCache.FirstOrDefault(i => i.CbSubscriptionId == recurringBilling.SubscriptionId);
            if (subscription != null)
            {
                return subscription;
            }

            subscription = await DbContext.Get<Subscription>(SubscriptionPropertyId.CbSubscriptionId, recurringBilling.SubscriptionId);
            if (subscription == null)
            {
                subscription = DbContext.Create<Subscription>();
                subscription.CbSubscriptionId = recurringBilling.SubscriptionId;
                subscription.AllowReorder = allowReorder;
                
                subscription.CancellationUrl = recurringBilling.CancellationUrl;
                subscription.ChangePaymentSubscriptionUrl = recurringBilling.ChangePaymentSubscriptionUrl;

                subscription.Owner.Set(subscriptionOwner.Contact);
                _subscriptionCache.Add(subscription);
            }
            //Im Falle einer Verlängerung wird der neue OrderProcess der bestehenden Subscription hinzugefügt
            subscription.OrderProcesses.Add(orderProcess);
            return subscription;
        }

        #endregion

        #region GetOrCreateSubscriptionItem

        async Task<SubscriptionItem> GetOrCreateSubscriptionItem(CleverbridgeProduct cleverbridgeProduct, PurchaseItemRecurringBillingType recurringBilling, Subscription subscription)
        {
            var subscriptionItem = _subscriptionItemCache.FirstOrDefault(i => i.CbSubscriptionItemId == recurringBilling.SubscriptionItemRunningNo && i.DataContractSubscriptionKey == subscription.Key);
            if (subscriptionItem != null)
            {
                return subscriptionItem;
            }

            if (subscription.EntityState != EntityState.Created)
            {
                var subscriptionItems = await subscription.SubscriptionItems.Get(EntityPropertySets.SubscriptionItem);
                foreach (var item in subscriptionItems)
                {
                    if (item.CbSubscriptionItemId == recurringBilling.SubscriptionItemRunningNo)
                    {
                        subscriptionItem = item;
                        break;
                    }
                }
            }

            if (subscriptionItem == null)
            {
                subscriptionItem = DbContext.Create<SubscriptionItem>();
                subscriptionItem.Subscription.Set(subscription);
                subscriptionItem.CbSubscriptionItemId = recurringBilling.SubscriptionItemRunningNo;
                subscriptionItem.CleverbridgeProduct.Set(cleverbridgeProduct);
                subscriptionItem.DisplayName = cleverbridgeProduct.Name;
                if (recurringBilling.IntervalLengthInDays > 0)
                {
                    subscriptionItem.IntervalLengthInDays = recurringBilling.IntervalLengthInDays;
                }
                if (recurringBilling.IntervalLengthInMonths > 0)
                {
                    subscriptionItem.IntervalLengthInMonths = recurringBilling.IntervalLengthInMonths;
                }
                subscriptionItem.ItemStatus = recurringBilling.ItemStatusId;
                subscriptionItem.NextBillingDate = recurringBilling.NextBillingDate;
                subscriptionItem.RenewalType = recurringBilling.RenewalType == 0 ? SubscriptionRenewalType.Automatic : recurringBilling.RenewalType;

                _subscriptionItemCache.Add(subscriptionItem);
            }
            return subscriptionItem;
        }

        #endregion

        #region GetOrCreateReseller

        async Task<Account> GetOrCreateReseller(string partnerId, string partnerUserName)
        {
            var account = await DbContext.Get<Account>(AccountPropertyId.CBPartnerId, partnerId);
            if (account != null)
            {
                return account;
            }
           account = DbContext.Create<Account>();
            account.Name = partnerUserName;
            account.CBPartnerId = partnerId;
            account.CBPartnerTyp = AccountPartnerType.Reseller;
            account.CBUserName = partnerUserName;
            return account;
        }

        #endregion

        #region GetOrderProcessWithLicenses

        internal async Task<Tuple<OrderProcess, IEnumerable<CitaviLicense>>> GetOrderProcessWithLicenses(string cleverbridgePurchaseId)
        {
            var query = new Query.FetchXml.GetOrderProcess(cleverbridgePurchaseId).TransformText();
            var result = await DbContext.Fetch(FetchXmlExpression.Create<OrderProcess>(query));
            
            if (result == null || !result.Any())
            {
                return null;
            }

            var set = new CrmSet(result);
            DbContext.Attach(set);
            return new Tuple<OrderProcess, IEnumerable<CitaviLicense>>(set.OrderProcesses.First(), set.Licenses);
        }

        #endregion

        #region OrderProcessExists

        async Task<bool> OrderProcessExists(string processId)
        {
            return await DbContext.Exists<OrderProcess>(OrderProcessPropertyId.CleverBridgeOrderNr, processId);
        }

        #endregion

        #region ProcessCleverbridgeNotification

        public async Task<OrderProcessResult> ProcessCleverbridgeNotificationAsync(string xml)
        {
            OrderProcessResult result = null;

            try
            {
                var notification = CleverbridgeSerializer.Deserialize(xml);
                OrderProcess order = null;

                Telemetry.TrackDiagnostics($"Process CleverbridgeNotification: {notification.GetType().Name}");

                switch (notification.GetType().Name)
                {
                    case nameof(NewPurchaseOrderNotificationType):
                    case nameof(PaidOrderNotificationType):
                        {
                            result = await ProcessOrderNotificationAsync(notification as NotificationType);
                            if (result.OrderProcessAlreadyExists)
                            {
                                return result;
                            }

                            if (result.UnknownProduct)
                            {
                                return result;
                            }

                            order = result.OrderProcess;
                        }
                        break;


                    case nameof(RecurringBillingReinstatedNotificationType):
                    case nameof(RecurringBillingCanceledNotificationType):
                        {
                            var rc_notification = notification as NotificationType;
                            result = await ProcessSubscriptionStatusNotification(rc_notification);
                            order = result.OrderProcess;
                        }
                        break;

                    case nameof(ChargebackNotificationType):
                    case nameof(RefundNotificationType):
                        {
                            result = await ProcessCancelOrderNotification(notification as NotificationType);
                            if (result.OrderProcessAlreadyExists)
                            {
                                return result;
                            }
                            order = result.OrderProcess;
                        }
                        break;

                    //Da machen wir nichts
                    case nameof(SubscriptionBillingDateExtendedNotificationType):
                    case nameof(NewOfflinePaymentPurchaseNotificationType):
                    case nameof(AwaitingNotificationType):
                    case nameof(CustomerContactDataChangedNotificationType):
                    case nameof(ErrorNotificationType):
                    case nameof(NewAffiliateSignupType):
                    case nameof(NewPartnerSignupType):
                    case nameof(QuoteNotificationType):
                    case nameof(OnlinePaymentDeclinedType):

                    case nameof(RegistrationNotificationType):
                    case nameof(ReturnDirectDebitNotificationType):


                    case nameof(RecurringBillingGracePeriodNotificationType):
                    case nameof(RecurringBillingOnHoldNotificationType):

                    case nameof(CustomerProfileUpdateNotificationType):
                    case nameof(SubscriptionReminderChargeNotificationType):
                    case nameof(SubscriptionReminderOfflinePayNotificationType):
                    case nameof(SubscriptionReminderPayOptExpNotificationType):
                    case nameof(PartialRefundNotificationType):
                        break;

                    default:
                        {
                            Telemetry.TrackTrace($"Unsupportet Cleverbridge-Notificationtype: {notification.GetType().Name}", SeverityLevel.Warning);
                        }
                        break;
                }
                if (order != null)
                {
                    order.XmlRaw += xml;
                }
            }
            catch (Exception exception)
            {
                if (!string.IsNullOrEmpty(xml))
                {
                    try
                    {
                        var orderId = Regex.Match(xml, "Purchase.+?Id=\"(?<ID>\\d+)").Groups["ID"].Value;
                        var body = Properties.Resources.CleverbridgeOrderFailedErrorMail.Replace("$ERROR$", exception.Message).Replace("$CBORDERNUMMER$", orderId);

                        var attachment = new EmailAttachment
                        {
                            BodyBase64 = xml.EncodeToBase64(),
                            Content = Encoding.UTF8.GetBytes(xml),
                            FileName = $"Order{orderId}.xml",
                            MimeType = "application/xml"
                        };

                        await EmailService.SendEmailWithAttachementAsync(body, "service@citavi.com", "Cleverbrige Bestellung konnte nicht verarbeitet werden", false, attachment);
                    }
                    catch (Exception ignored)
                    {
                        Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
                    }
                }
                throw;
            }

            return result;
        }

        #endregion

        #region ProcessOrderNotification

        internal async Task<OrderProcessResult> ProcessOrderNotificationAsync(NotificationType paidOrder)
        {
            var result = new OrderProcessResult();

            var purchase = paidOrder.Purchase;

            if (await OrderProcessExists(purchase.Id))
            {
                result.OrderProcessAlreadyExists = true;
                Telemetry.TrackDiagnostics($"ProcessOrderNotification: Order already exists: {purchase.Id}");
                return result;
            }

            var orderProcess = DbContext.Create<OrderProcess>();
            result.OrderProcess = orderProcess;

            orderProcess.CbOrderPdf = purchase.CustomerPdfDocumentUrl;
            orderProcess.CleverBridgeOrderNr = purchase.Id;
            orderProcess.OrderProcessTrackingNr = "Cleverbridge: " + purchase.Id;
            orderProcess.OrderDate = purchase.CreationTime;
            orderProcess.LicenseAmmount = purchase.Items.Item.Select(i => Convert.ToInt32(i.Quantity, CultureInfo.InvariantCulture)).Sum();

            var licenseeUser = await GetOrCreateUserAsync(purchase.LicenseeContact);
            orderProcess.LicenseContact.Set(licenseeUser.Contact);

            var billingUser = await GetOrCreateUserAsync(purchase.BillingContact);
            orderProcess.BillingContact.Set(billingUser.Contact);

            var deliveryContact = await GetOrCreateUserAsync(purchase.DeliveryContact);
            orderProcess.DeliveryContact.Set(deliveryContact.Contact);

            orderProcess.BillingAccountText = purchase.BillingContact.Company;
            orderProcess.DeliveryAccountText = purchase.DeliveryContact.Company;

            orderProcess.PartnerID = purchase.PartnerId;
            orderProcess.PartnerName = purchase.PartnerUsername;

            if (!string.IsNullOrEmpty(purchase.PartnerUsername))
            {
                var reseller = await GetOrCreateReseller(purchase.PartnerId, purchase.PartnerUsername);
                orderProcess.Reseller.Set(reseller);
                orderProcess.IsReseller = true;
                licenseeUser.Contact.ResellerAffiliateId = reseller.CleverbrigeAffiliateId;
            }

            var voucherToSend = new List<Tuple<CrmUser, Voucher>>();
            var emailsSent = new List<string>();
            var sendMails = false; //Wird auf "true" bei einer normalen Bestellung gesetzt. Bei "nur"-Gutschein_Bestellung gibt es keine Mail (auch keine "Account-Erstellt" mail).
            var purchased = new List<CleverbridgeProduct>();
            var hasCitaviSpaceSubscription = false;

            var isSubscriptionRenewal = !string.IsNullOrEmpty(purchase.SubscriptionRevenueCategoryId) && purchase.SubscriptionRevenueCategoryId.EndsWith("Renewal", StringComparison.InvariantCultureIgnoreCase);
            var isSubscriptionAlignment = !string.IsNullOrEmpty(purchase.SubscriptionRevenueCategoryId) && purchase.SubscriptionRevenueCategoryId.Equals(nameof(SubscriptionRevenueCategoryId.Alignment), StringComparison.InvariantCultureIgnoreCase);

            var addAsEndUser = true;
            foreach (var purchaseItem in purchase.Items.Item)
            {
                if (!CrmCache.CleverbridgeProductsByProductId.TryGetValue(Convert.ToInt32(purchaseItem.ProductId, CultureInfo.InvariantCulture), out var cleverbridgeProduct))
                {
                    continue;
                }

                //1 Bestellung, 1 Lizenz -> Owner = EndUser
                //Bei mehr als 1 Lizenz => Kein EndUser.Das muss dann via UI geschehen.
                //Nur bei Privatbestellungen
                //#20100
                if (cleverbridgeProduct.PricingResolved.IsBusinessOrAcacemicPricing())
                {
                    //Business u. Academic müssen immer via UI gesetzt werden
                    addAsEndUser = false;
                    break;
                }
                var quantity = Convert.ToInt32(purchaseItem.Quantity, CultureInfo.InvariantCulture);
                if (quantity > 1 && cleverbridgeProduct.Edition != ShopProductEdition.Cloudspace)
                {
                    //Mehrere Bestellungen vom gleichen Produkt
                    //Bei Cloudspace ist es ok, da diese mehrfach bestellt werden können von einem Home User
                    addAsEndUser = false;
                }

                if (cleverbridgeProduct.Edition == ShopProductEdition.Cloudspace ||
                    cleverbridgeProduct.Edition == ShopProductEdition.Maintenance ||
                    cleverbridgeProduct.Edition == ShopProductEdition.Web)
                {
                    //Diese Produkte kann ein Home User bestellen. In diesem Fall wird er bei allen Lizenzen als EndUser eingetragen
                    continue;
                }

                //Verschiedene Desktop Produkte wie DBServer (1) und Home (1)
                //Der EndUser wird nicht gesetzt
                addAsEndUser = quantity == 1;
            }

            foreach (var purchaseItem in purchase.Items.Item)
            {
                if (purchaseItem.ProductId == ShopConstants.BackupCDProductId)
                {
                    return null;
                }
                if (!CrmCache.CleverbridgeProductsByProductId.TryGetValue(Convert.ToInt32(purchaseItem.ProductId, CultureInfo.InvariantCulture), out var cleverbridgeProduct))
                {
                    if (purchase.Id != "201408" && purchase.Id != "201409")
                    {
                        //Das sind Zitationstil-Aufträge
                        Telemetry.TrackTrace($"No product found for: {purchaseItem.ProductId}. CleverBridgeOrderNr:{purchase.Id}", SeverityLevel.Warning);
                    }
                    return new OrderProcessResult { UnknownProduct = true };
                }

                if (CleverbridgeProduct.IsGiftCardProduct(purchaseItem.ProductId))
                {
                    var voucher = await VoucherManager.CreateGiftCardVoucher(licenseeUser.Contact, cleverbridgeProduct, orderProcess);
                    voucherToSend.Add(new Tuple<CrmUser, Voucher>(licenseeUser, voucher));
                    if (licenseeUser.Contact.EntityState != EntityState.Created)
                    {
                        //Kontakt gibt es bereits - wir senden ihm keine weitere Mail
                        emailsSent.Add(licenseeUser.Email.ToUpperInvariant());
                    }
                }
                else
                {
                    result.Contacts.AddIfNotExists(licenseeUser.Key);
                    sendMails = true;
                    var count = Convert.ToInt32(purchaseItem.Quantity, CultureInfo.InvariantCulture);
                    purchased.Add(cleverbridgeProduct);
                    if (cleverbridgeProduct.ProductResolved.IsSqlServerProduct)
                    {
                        count = 1;
                    }

                    SubscriptionItem subscriptionItem = null;
                    if (cleverbridgeProduct.ProductResolved.IsSubscription)
                    {
                        var subscriptionOwner = deliveryContact;
                        var allowReorder = true;
                        if (deliveryContact.Key != billingUser.Key)
                        {
                            allowReorder = false;
                        }
                        var subscription = await GetOrCreateSubscription(purchaseItem.RecurringBilling, orderProcess, subscriptionOwner, allowReorder);
                        subscriptionItem = await GetOrCreateSubscriptionItem(cleverbridgeProduct, purchaseItem.RecurringBilling, subscription);

                        if (purchaseItem.RecurringBilling.SubscriptionEventtypeId == SubscriptionEventTypeConstants.Renewed ||
                            isSubscriptionRenewal)
                        {
                            Telemetry.TrackDiagnostics($"Renew Subscription {subscription.CbSubscriptionId}");

                            isSubscriptionRenewal = true;
                            subscriptionItem.NextBillingDate = purchaseItem.RecurringBilling.NextBillingDate;
                            var licenses = (await subscriptionItem.Licenses.Get(EntityPropertySets.CitaviLicense)).ToList();
                            
                            foreach (var license in licenses.ToList())
                            {
                                if (license.SubscriptionStatus == LicenseSubscriptionStatus.Deactivated)
                                {
                                    var owner = await license.Owner.Get();
                                    var endUser = await license.EndUser.Get();
                                    await CrmUserCache.RemoveAsync(owner.Key);
                                    if(endUser != null && 
                                       owner.Key != endUser.Key)
									{
                                        await CrmUserCache.RemoveAsync(endUser.Key);
                                    }
                                    licenses.Remove(license);
                                    license.Deactivate();
                                    if (license.IsCitaviSpace)
                                    {
                                        hasCitaviSpaceSubscription = true;
                                    }
                                    continue;
                                }
                                license.ExpiryDate = purchaseItem.RecurringBilling.NextBillingDate;
                            }
                            subscriptionItem.LicenseAmount = licenses.Count();
                            continue;
                        }
                        else
                        {
                            subscriptionItem.LicenseAmount = count;
                        }
                    }

                    for (var i = 0; i < count; i++)
                    {
                        var companyName = orderProcess.IsReseller.GetValueOrDefault() ? purchase.LicenseeContact.Company : purchase.DeliveryContact.Company;
                        var license = LicenseManager.CreateLicenseWithPurchaseItem(licenseeUser.Contact, cleverbridgeProduct, orderProcess, companyName, Convert.ToInt32(purchaseItem.Quantity, CultureInfo.InvariantCulture));
                        license.InvoiceNumber = orderProcess.CleverBridgeOrderNr;
                        licenseeUser.AddOwnerLicense(license);

                        if (addAsEndUser)
                        {
                            licenseeUser.AddEndUserLicense(license);
                        }
                        else
                        {
                            //Reset von Licensedata
                            license.CitaviKey = string.Empty;
                            license.CitaviLicenseName = string.Empty;
                        }

                        if (subscriptionItem != null)
                        {
                            if (subscriptionItem.NextBillingDate != DateTime.MinValue)
                            {
                                license.ExpiryDate = subscriptionItem.NextBillingDate;
                            }
                            license.SubscriptionItem.Set(subscriptionItem);
                            license.SubscriptionStatus = LicenseSubscriptionStatus.Active;

                            if (cleverbridgeProduct.ProductResolved.IsCitaviSpace)
                            {
                                hasCitaviSpaceSubscription = true;
                            }
                        }
                    }
                }

                if (cleverbridgeProduct.PricingResolved.Key == Pricing.Benefit.Key)
                {
                    licenseeUser.Contact.CampusBenefitEligibility = CampusBenefitEligibilityType.Redeemed;
                }

                if (purchase.SubscriptNewsletter())
                {
                    var emailSubscriptionManager = new EmailSubscriptionManager();
                    await emailSubscriptionManager.InitializeAsync();
                    await emailSubscriptionManager.SubscribeNews(licenseeUser.Contact);
                }
            }

            var licenseeUserIsNew = licenseeUser.Contact.EntityState == EntityState.Created;

            await DbContext.SaveAsync();

            #region Send Emails

            if (sendMails && !isSubscriptionRenewal && !isSubscriptionAlignment)
            {
                var productGroup = OrderProcessProductGroup.Home;
                if (purchased.Any(p => p.ProductResolved.IsSqlServerProduct))
                {
                    productGroup = OrderProcessProductGroup.DbServer;
                }
                else if (purchased.Any(p => p.PricingResolved.IsBusinessOrAcacemicPricing()))
                {
                    productGroup = OrderProcessProductGroup.Business;
                }
                else if(hasCitaviSpaceSubscription)
                {
                    //Keine Mail senden
                    emailsSent.Add(licenseeUser.Email.ToUpperInvariant());
                }

                if (licenseeUserIsNew &&
                    !emailsSent.Contains(licenseeUser.Email.ToUpperInvariant()))
                {
                    var verificationKey = await UserManager.SetVerificationKeyForNewUserAsync(licenseeUser);
                    await EmailService.SendProcessOrder_BillingMail_AccountCreated(licenseeUser, licenseeUser.Email, verificationKey, productGroup);
                    emailsSent.Add(licenseeUser.Email.ToUpperInvariant());
                }
                else if (!emailsSent.Contains(licenseeUser.Email.ToUpperInvariant()))
                {
                    await EmailService.SendProcessOrder_BillingMail(licenseeUser, licenseeUser.Email, productGroup);
                    emailsSent.Add(licenseeUser.Email.ToUpperInvariant());
                }
            }

            foreach (var item in voucherToSend)
            {
                await EmailService.SendVoucherGiftMailAsync(item.Item1, item.Item2);
            }

            #endregion

            await CrmUserCache.RemoveAsync(licenseeUser);

            if (addAsEndUser && hasCitaviSpaceSubscription)
            {
                await CitaviSpaceCache.RefreshAsync(licenseeUser, DbContext);
            }

            if (isSubscriptionAlignment && licenseeUser != null)
            {
                await CrmUserCache.RemoveAsync(licenseeUser);
                await AzureHelper.Ably.Invoke(MessageKey.SubscriptionChanged, CollectionUtility.ToDictionary(MessageKey.ContactKey, licenseeUser.Key));
            }

            return result;
        }

        #endregion

        #region ProcessCancelOrderNotification

        async Task<OrderProcessResult> ProcessCancelOrderNotification(NotificationType cancelOrder)
        {
            var result = new OrderProcessResult();
            var purchase = cancelOrder.Purchase;
            var existingOrder = await GetOrderProcessWithLicenses(purchase.Id);
            if (existingOrder == null)
            {
                Telemetry.TrackDiagnostics($"CancelOrderNotification: {nameof(purchase.Id)} not exists: {purchase.Id}");
                return null;
            }
            var orderProcess = existingOrder.Item1;
            result.OrderProcess = orderProcess;
            orderProcess.StatusCode = StatusCode.Inactive;
            var affectedUsers = new List<string>();
            foreach (var license in existingOrder.Item2)
            {
                license.StatusCode = StatusCode.Inactive;
                affectedUsers.AddIfNotExists(license.DataContractEndUserContactKey);
                affectedUsers.AddIfNotExists(license.DataContractOwnerContactKey);

                result.Contacts.AddIfNotExists(license.DataContractEndUserContactKey);
                result.Contacts.AddIfNotExists(license.DataContractOwnerContactKey);
            }

            await DbContext.SaveAsync();

            foreach (var user in affectedUsers.Distinct())
            {
                await CrmUserCache.RemoveAsync(user);
            }
            return result;
        }

        #endregion

        #region ProcessSubscriptionStatusNotification

        internal async Task<OrderProcessResult> ProcessSubscriptionStatusNotification(NotificationType subscriptionEvent)
        {
            var result = new OrderProcessResult();
            var purchase = subscriptionEvent.Purchase;

            var orderProcess = DbContext.Create<OrderProcess>();
            result.OrderProcess = orderProcess;

            orderProcess.CbOrderPdf = purchase.CustomerPdfDocumentUrl;
            orderProcess.CleverBridgeOrderNr = purchase.Id;
            orderProcess.OrderProcessTrackingNr = "Cleverbridge: " + purchase.Id;
            orderProcess.OrderDate = purchase.CreationTime;
            orderProcess.LicenseAmmount = purchase.Items.Item.Select(i => Convert.ToInt32(i.Quantity, CultureInfo.InvariantCulture)).Sum();

            var licenseeUser = await GetOrCreateUserAsync(purchase.LicenseeContact);
            orderProcess.LicenseContact.Set(licenseeUser.Contact);

            var billingUser = await GetOrCreateUserAsync(purchase.BillingContact);
            orderProcess.BillingContact.Set(billingUser.Contact);

            var deliveryContact = await GetOrCreateUserAsync(purchase.DeliveryContact);
            orderProcess.DeliveryContact.Set(deliveryContact.Contact);

            orderProcess.BillingAccountText = purchase.BillingContact.Company;
            orderProcess.DeliveryAccountText = purchase.DeliveryContact.Company;

            orderProcess.PartnerID = purchase.PartnerId;
            orderProcess.PartnerName = purchase.PartnerUsername;

            if (!string.IsNullOrEmpty(purchase.PartnerUsername))
            {
                var reseller = await GetOrCreateReseller(purchase.PartnerId, purchase.PartnerUsername);
                orderProcess.Reseller.Set(reseller);
                orderProcess.IsReseller = true;
                licenseeUser.Contact.ResellerAffiliateId = reseller.CleverbrigeAffiliateId;
            }

            var affectedUsers = new List<string>();
            var resetCloudSpaceCache = false;

            foreach (var item in purchase.Items.Item)
            {
                var subscriptionId = item.RecurringBilling.SubscriptionId;
                var subcription = await DbContext.Get<Subscription>(SubscriptionPropertyId.CbSubscriptionId, subscriptionId);
                if (subcription == null)
                {
                    Telemetry.TrackTrace($"Subscription not found: SubscriptionId: {subscriptionId}. Purchase: {purchase.Id}", SeverityLevel.Warning);
                    return null;
                }
                var subscriptionItems = await subcription.SubscriptionItems.Get(EntityPropertySets.SubscriptionItem);
                var subscriptionItem = subscriptionItems.FirstOrDefault(s => s.CbSubscriptionItemId == item.RecurringBilling.SubscriptionItemRunningNo);
                if (subscriptionItem == null)
                {
                    Telemetry.TrackTrace($"SubscriptionItem not found: SubscriptionItemId: {item.RecurringBilling.SubscriptionItemRunningNo}. SubscriptionId: {subscriptionId}. Purchase: {purchase.Id}", SeverityLevel.Warning);
                    return null;
                }

                LicenseSubscriptionStatus licenseSubscriptionStatus;

                if (item.RecurringBilling.Status == nameof(SubscriptionStatusType.Active))
                {
                    if (subscriptionItem.ItemStatus == SubscriptionItemStatusType.Active)
                    {
                        //Das SI wurde bereits aktiviert. Das ist der Fall, wenn via Account die SI aktiviert wird.
                        //In diesem Fall müssen wir nichts machen
                        continue;
                    }

                    //Subscription wurde aktiviert
                    subscriptionItem.RenewalType = item.RecurringBilling.RenewalType;
                    subscriptionItem.ItemStatus = SubscriptionItemStatusType.Active;
                    licenseSubscriptionStatus = LicenseSubscriptionStatus.Active;


                }
                else if (item.RecurringBilling.Status == nameof(SubscriptionStatusType.Deactivated) ||
                         item.RecurringBilling.Status == nameof(SubscriptionStatusType.Finished))
                {
                    if (subscriptionItem.ItemStatus == SubscriptionItemStatusType.Deactivated)
                    {
                        //Das SI wurde bereits deaktiviert. Das ist der Fall, wenn via Account die SI deaktiviert wird.
                        //In diesem Fall müssen wir nichts machen
                        continue;
                    }
                    //Subscription wurde deaktiviert
                    subscriptionItem.RenewalType = item.RecurringBilling.RenewalType;
                    subscriptionItem.ItemStatus = SubscriptionItemStatusType.Deactivated;
                    licenseSubscriptionStatus = LicenseSubscriptionStatus.Deactivated;
                }
                else
                {
                    Telemetry.TrackDiagnostics($"Unsupported RecurringBilling Status: {item.RecurringBilling.Status}");
                    continue;
                }

                Telemetry.TrackDiagnostics("Process SubscriptionStatusNotification");

                subscriptionItem.NextBillingDate = item.RecurringBilling.NextBillingDate;

                var licenses = await subscriptionItem.Licenses.Get(EntityPropertySets.CitaviLicense);
                subscriptionItem.LicenseAmount = licenses.Count();
                var count = int.Parse(item.Quantity, CultureInfo.InvariantCulture);

                foreach (var license in licenses)
                {
                    if (count == 0 && licenseSubscriptionStatus == LicenseSubscriptionStatus.Active)
                    {
                        Telemetry.TrackDiagnostics($"Cannot activate license. Count == 0");
                        continue;
                    }
                    count--;
                    license.SubscriptionStatus = licenseSubscriptionStatus;
                    license.ExpiryDate = item.RecurringBilling.NextBillingDate;

                    var enduser = await license.EndUser.Get();
                    var owner = await license.Owner.Get();
                    if (enduser != null)
                    {
                        affectedUsers.AddIfNotExists(enduser.Key);
                    }
                    affectedUsers.AddIfNotExists(owner.Key);

                    if (enduser != null)
                    {
                        result.Contacts.AddIfNotExists(enduser.Key);
                    }
                    result.Contacts.AddIfNotExists(owner.Key);

					if (license.IsCitaviSpace)
					{
                        resetCloudSpaceCache = true;
                    }
                }
            }

            await DbContext.SaveAsync();

            foreach (var contactKey in affectedUsers.Distinct())
            {
                await CrmUserCache.RemoveAsync(contactKey);
                await AzureHelper.Ably.Invoke(MessageKey.SubscriptionChanged,CollectionUtility.ToDictionary(MessageKey.ContactKey, contactKey));
                if(resetCloudSpaceCache)
				{
                    await CitaviSpaceCache.RefreshAsync(contactKey, DbContext);
				}
            }

            return result;
        }

        #endregion

        #region SendMailBillingMail

        public async Task SendBillomatMail_Licensee(CrmUser user, OrderProcess orderProcess)
        {
            if (user == null)
            {
                throw new NotSupportedException($"{nameof(user)} must not be null");
            }
            if (orderProcess == null)
            {
                throw new NotSupportedException($"{nameof(orderProcess)} must not be null");
            }

            var productGroup = await orderProcess.GetOrderProcessProductGroup();

            if (!user.IsAccountVerified)
			{
                var verificationKey = await UserManager.SetVerificationKeyForNewUserAsync(user);
                await EmailService.SendProcessOrder_BillingMail_AccountCreated(user, user.Email, verificationKey, productGroup);
            }
			else
			{
                await EmailService.SendProcessOrder_BillingMail(user, user.Email, productGroup);
            }
        }

        public async Task SendBillomatMail_Billing(CrmUser user, OrderProcess orderProcess)
        {
            if (user == null)
            {
                throw new NotSupportedException($"{nameof(user)} must not be null");
            }
            if (orderProcess == null)
            {
                throw new NotSupportedException($"{nameof(orderProcess)} must not be null");
            }
            await EmailService.SendProcessOrder_BillomatBillingMail(user, user.Email, orderProcess, DbContext);
        }

        #endregion

        #endregion
    }

    public class OrderProcessResult
    {
        public bool UnknownProduct { get; set; }
        public bool OrderProcessAlreadyExists { get; set; }
        public OrderProcess OrderProcess { get; set; }
        /// <summary>
        /// Alle Kontakte (Keys) welche neue Lizenzen bekommen haben oder Lizenzen entzogen wurden
        /// </summary>
        public List<string> Contacts { get; } = new List<string>();
    }
}
