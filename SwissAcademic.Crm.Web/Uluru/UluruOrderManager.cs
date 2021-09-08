using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
	public class UluruOrderManager
	{
        #region Fields

        readonly List<CrmUser> _orderUserCache = new List<CrmUser>();
        readonly List<Subscription> _subscriptionCache = new List<Subscription>();
        readonly List<SubscriptionItem> _subscriptionItemCache = new List<SubscriptionItem>();

        #endregion

        #region Constructor

        public UluruOrderManager(CrmDbContext context)
		{
			DbContext = context;
            UserManager = new CrmUserManager(context);
            VoucherManager = new VoucherManager(context);
            LicenseManager = new LicenseManager(context);
        }

		#endregion

		#region Properties

		CrmDbContext DbContext { get; set; }
		public CrmUserManager UserManager { get; }
		public VoucherManager VoucherManager { get; }
		public LicenseManager LicenseManager { get; }

		#endregion

		#region Methods

		#region ClearCache

        internal void ClearCache()
		{
            _orderUserCache.Clear();
            _subscriptionCache.Clear();
            _subscriptionItemCache.Clear();
        }

		#endregion

		#region EligibilityCheck

		public async Task<UluruEligibilityCheckResult> EligibilityCheck(UluruEligibilityCheckRequest uluruEligibilityCheck)
		{
            var result = new UluruEligibilityCheckResult();

            var user = await DbContext.GetByEmailAsync(uluruEligibilityCheck.Email);

            if (user == null)
            {
                Telemetry.TrackTrace($"EligibilityCheck user not found: {uluruEligibilityCheck.Email}", SeverityLevel.Warning);
                foreach (var product in uluruEligibilityCheck.Products)
                {
                    result.Products.Add(new UluruEligibilityCheckResultItem
                    {
                        AllowedUpgrades = 0,
                        ProductCode = product.ProductCode,
                        UpgradeType = product.UpgradeType,
                        LicenseType = product.LicenseType,
                        Sequence = product.Sequence

                    });
                }
                return result;
            }

            result.CitaviId = user.Key;

            foreach (var product in uluruEligibilityCheck.Products)
			{
                result.Products.Add(EligibilityCheck(user, product));
			}

            return result;
		}

        public UluruEligibilityCheckResultItem EligibilityCheck(CrmUser user, UluruEligibilityCheckRequestItem requestItem)
        {
            var productCode = requestItem.ProductCode;
            var upgradeType = requestItem.UpgradeType;
            var licenseType = requestItem.LicenseType;

            int quantity = 0;

            var ownerLicenses = user.Licenses.Where(lic => lic.DataContractOwnerContactKey == user.Key &&
                                                           lic.DataContractPricingKey != Pricing.None.Key &&
                                                           !lic.Free &&
                                                           !lic.ReadOnly &&
                                                           !lic.ProductResolved.IsUpgradeProduct);

            var ownerLicenses_Upgrades = user.Licenses.Where(lic => lic.DataContractOwnerContactKey == user.Key &&
                                                             lic.ProductResolved.IsUpgradeProduct);

            var product = CrmCache.ProductsByCode[productCode];

            switch (upgradeType)
            {
                #region Benefit

                case UluruUpgradeType.Benefit:
                    {
                        if (user.Contact.CampusBenefitEligibility == CampusBenefitEligibilityType.Eligible &&
                           !ownerLicenses.Any(lic => lic.DataContractPricingKey == Pricing.Benefit.Key))
                        {
                            quantity = !product.IsSqlServerProduct && !product.IsSubscription ? 1 : 0;
                        }
                    }
                    break;

                #endregion

                #region Crossgrade

                case UluruUpgradeType.Crossgrade:
                    {
                        if (product.Key == Product.CitaviWindowsAndWeb.Key)
                        {
                            var crossgradableLicenses = GetCrossgradableLicenses(user);
                            quantity = crossgradableLicenses.Count();
                        }
                        else
                        {
                            quantity = 0;
                        }
                    }
                    break;

                #endregion

                #region OneVersion, TwoVersion

                case UluruUpgradeType.OneVersion:
                case UluruUpgradeType.TwoVersion:
                    {
                        var upgradeFromVersion = product.CitaviMajorVersion - (int)upgradeType;

                        if (product.IsSqlServerProduct)
                        {
                            var upgradable = ownerLicenses.Where(lic => lic.CitaviMajorVersion == upgradeFromVersion &&
                                                                        !lic.ProductResolved.IsSqlServerProduct &&
                                                                        !lic.ProductResolved.IsSubscription).Count();
                            upgradable = 0;

                            var upgradable_server = ownerLicenses.Where(lic => lic.CitaviMajorVersion == upgradeFromVersion &&
                                                                        lic.ProductResolved.IsSqlServerProduct &&
                                                                        lic.ProductResolved.IsEqualDBServerProduct(product))
                                                                           .Sum(lic => lic.ServerAmount + lic.ConcurrentReaderCount);

                            var upgrades = ownerLicenses_Upgrades.Where(lic => lic.ProductResolved.UpgradeFromCitaviMajorVersion == upgradeFromVersion).Count();

                            quantity = upgradable + upgradable_server - upgrades;
                        }
                        else if (product.IsSubscription)
                        {
                            quantity = 0;
                        }
                        else
                        {
                            var upgradable = ownerLicenses.Where(lic => lic.CitaviMajorVersion == upgradeFromVersion &&
                                                                        !lic.ProductResolved.IsSqlServerProduct &&
                                                                        !lic.ProductResolved.IsSubscription).Count();

                            var upgrades = ownerLicenses_Upgrades.Where(lic => lic.ProductResolved.UpgradeFromCitaviMajorVersion == upgradeFromVersion && !lic.ProductResolved.IsSqlServerProduct).Count();

                            quantity = upgradable - upgrades;
                        }
                    }
                    break;

                #endregion

                #region None

                case UluruUpgradeType.None:
					{
                        if (licenseType.HasValue && 
                            licenseType.Value == UluruLicenseType.AUTO_SUBSCRIPTION)
                        {
                            quantity = 1;
                        }
                    }
                    break;

				#endregion
			}

			if (licenseType.HasValue &&
                licenseType.Value == UluruLicenseType.AUTO_SUBSCRIPTION)
            {
                if (ownerLicenses.Any(lic => lic.ProductResolved == product &&
                                             lic.PricingResolved.IsPersonalPricing()))
                {
                    quantity = 0;
                }
            }

            return new UluruEligibilityCheckResultItem
            {
                AllowedUpgrades = quantity < 0 ? 0 : quantity,
                ProductCode = productCode,
                UpgradeType = upgradeType,
                LicenseType = licenseType,
                Sequence = requestItem.Sequence
            };
        }

        #endregion

        #region GetOrderProcess

        public async Task<OrderProcess> GetOrderProcess(string transactionId)
            => await DbContext.Get<OrderProcess>(OrderProcessPropertyId.CleverBridgeOrderNr, transactionId, EntityPropertySets.OrderProcess);


        #endregion

        #region GetCrossgradableLicenses

        static IEnumerable<CitaviLicense> GetCrossgradableLicenses(CrmUser user)
		{
            var ownerLicenses = user.Licenses.Where(lic => lic.DataContractOwnerContactKey == user.Key &&
                                                          lic.DataContractPricingKey != Pricing.None.Key &&
                                                          !lic.Free &&
                                                          !lic.ReadOnly &&
                                                          !lic.ProductResolved.IsUpgradeProduct);

            var crossgradableMinVersion = CrmConfig.CurrentLicenseMajorVersion;

            return ownerLicenses.Where(lic => lic.CitaviMajorVersion == crossgradableMinVersion &&
                                                            !lic.ProductResolved.IsSqlServerProduct &&
                                                            !lic.ProductResolved.IsSubscription).ToList();
        }

        #endregion

        #region GetOrCreateUser

        async Task<CrmUser> GetOrCreateUserAsync(UluruOrder order)
        {
            var email = order.GetContactField(ContactPropertyId.EMailAddress1);

            var exsiting = _orderUserCache.FirstOrDefault(i => string.Equals(i.Email, email, StringComparison.InvariantCultureIgnoreCase));
            if (exsiting != null)
            {
                return exsiting;
            }

            email = email.RemoveNonStandardWhitespace();

            CrmUser user;
            var contactKey = order.CitaviId;
            if (!string.IsNullOrEmpty(contactKey))
            {
                user = await DbContext.GetByKeyAsync(contactKey);
                if(user == null)
				{
                    //Merged Account - lookup for inactive contact
                    (_, user) = await DbContext.GetMergedContact(contactKey);
				}
                if (user.GetLinkedEmailAccount(email) == null)
                {
                    if (await DbContext.GetByEmailAsync(email) == null)
                    {
                        user.AddLinkedEMailAccount(email);
                    }
                    else
                    {
                        //ContactKey Email Missmatch
                        return null;
                    }
                }
            }
            else
            {
                user = await DbContext.GetByEmailAsync(email);
            }

            if (user == null)
            {
                var contact = DbContext.Create<Contact>();
                contact.FirstName = order.GetContactField(ContactPropertyId.FirstName);
                contact.LastName = order.GetContactField(ContactPropertyId.LastName);
                contact.Firm = order.GetContactField(ContactPropertyId.Firm);

				var dataCenter = AzureRegionResolver.Instance.GetDataCenterByCountryIsoCode(order.SoldToCountryCode);
				if (dataCenter.HasValue)
				{
					contact.DataCenter = dataCenter.Value;
				}

				user = new CrmUser(contact);
                user.AddLinkedEMailAccount(email);
                contact.ContactCreatedByOrdermail = true;
            }

            user.Contact.StatusCode = StatusCode.Active;
            _orderUserCache.Add(user);
            return user;
        }

        #endregion

        #region GetOrCreateSubscription

        async Task<Subscription> GetOrCreateSubscription(UluruOrder uluruOrder, OrderProcess orderProcess, CrmUser subscriptionOwner)
        {
            var subscription = _subscriptionCache.FirstOrDefault(i => i.CbSubscriptionId == uluruOrder.SubscriptionRef);
            if (subscription != null)
            {
                return subscription;
            }

            subscription = await DbContext.Get<Subscription>(SubscriptionPropertyId.CbSubscriptionId, uluruOrder.SubscriptionRef, EntityPropertySets.Subscription);
            if (subscription == null)
            {
                subscription = DbContext.Create<Subscription>();
                subscription.CbSubscriptionId = uluruOrder.SubscriptionRef;
                subscription.Owner.Set(subscriptionOwner.Contact);
                _subscriptionCache.Add(subscription);
            }
            //Im Falle einer Verlängerung wird der neue OrderProcess der bestehenden Subscription hinzugefügt
            subscription.OrderProcesses.Add(orderProcess);
            return subscription;
        }

        #endregion

        #region GetOrCreateSubscriptionItem

        async Task<SubscriptionItem> GetOrCreateSubscriptionItem(UluruOrderItem uluruOrderItem, Subscription subscription)
        {
            var subscriptionItem = _subscriptionItemCache.FirstOrDefault(i => i.CbSubscriptionItemId == uluruOrderItem.SubscriptionLineRef && i.DataContractSubscriptionKey == subscription.Key);
            if (subscriptionItem != null)
            {
                return subscriptionItem;
            }

            if (subscription.EntityState != EntityState.Created)
            {
                var subscriptionItems = await subscription.SubscriptionItems.Get(EntityPropertySets.SubscriptionItem);
                foreach (var item in subscriptionItems)
                {
                    if (item.CbSubscriptionItemId == uluruOrderItem.SubscriptionLineRef)
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
                subscriptionItem.CbSubscriptionItemId = uluruOrderItem.SubscriptionLineRef;
                subscriptionItem.NextBillingDate = uluruOrderItem.EndDateUtc.Value;

                _subscriptionItemCache.Add(subscriptionItem);
            }
            return subscriptionItem;
        }

        #endregion

        #region ProcessOrderNotification

        public async Task<UluruOrderResult> ProcessOrderNotificationAsync(string uluruOrderJsonString)
        {
            UluruOrderResult result = null;
            UluruOrder order = null;
            try
            {
				if (string.IsNullOrEmpty(uluruOrderJsonString))
				{
                    throw new NotSupportedException($"{nameof(uluruOrderJsonString)} must not be empty");
				}
                order = JsonConvert.DeserializeObject<UluruOrder>(uluruOrderJsonString);
                order.Raw = uluruOrderJsonString;
                result = await ProcessOrderNotificationAsync(order);
            }
            catch (CrmServerException ex)
            {
                Telemetry.TrackException(ex, severityLevel: SeverityLevel.Error, flow: ExceptionFlow.Eat);
                result = new UluruOrderResult();
                result.Status = UluruOrderStatus.CrmException;
                result.ExceptionMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, severityLevel: SeverityLevel.Error, flow: ExceptionFlow.Eat);
                result = new UluruOrderResult();
                result.Status = UluruOrderStatus.UnknownException;
                result.ExceptionMessage = ex.Message;
            }

            if (result.Status != UluruOrderStatus.Ok)
            {
                try
                {
                    var orderId = order == null ? "-order is null-" : order.SubscriptionRef;
                    var body = Properties.Resources.CleverbridgeOrderFailedErrorMail.Replace("$ERROR$", result.ExceptionMessage).Replace("$CBORDERNUMMER$", orderId);

                    var attachment = new EmailAttachment
                    {
                        BodyBase64 = uluruOrderJsonString.EncodeToBase64(),
                        Content = Encoding.UTF8.GetBytes(uluruOrderJsonString),
                        FileName = $"Order{orderId}.json",
                        MimeType = "application/json"
                    };

                    var serviceEmail = Environment.Build == BuildType.Alpha ? "marc.eichenberger@citavi.com" : "service@citavi.com";
                    await EmailService.SendEmailWithAttachementAsync(body, serviceEmail, "Uluru order could not be processed", false, attachment);
                }
                catch (Exception ignored)
                {
                    Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
                }
            }

            return result;
        }
        internal async Task<UluruOrderResult> ProcessOrderNotificationAsync(UluruOrder uluruOrder)
        {
            var result = new UluruOrderResult();

            var licenseeUser = await GetOrCreateUserAsync(uluruOrder);
            if (licenseeUser == null)
            {
                result.Status = UluruOrderStatus.ContactKeyEmailMissmatch;
                return result;
            }

            result.CitaviId = licenseeUser.Key;

            var emailsSent = new List<string>();
            var purchased = new List<CitaviLicense>();
            var affectedContactKeys = new List<string>();
            affectedContactKeys.Add(licenseeUser.Key);

            var refreshCitaviSpace = false;

            var isSubscriptionRenewal = false;
            var isSubscriptionAlignment = false;

            foreach (var orderItem in uluruOrder.Products)
            {
                if(orderItem.LicenceType == UluruLicenseType.ELA_FTE)
				{
                    continue;
				}

                var addAsEndUser = true;

                if (orderItem.LicenceType != UluruLicenseType.AUTO_SUBSCRIPTION &&
                    orderItem.LicenceType != UluruLicenseType.TRIAL)
                {
                    addAsEndUser = false;
                }
                else if (orderItem.QuantityInt > 1)
                {
                    addAsEndUser = false;
                }

                CrmCache.ProductsByCode.TryGetValue(orderItem.CitaviProductId, out var citaviProduct);

                var licensesCount = orderItem.QuantityInt;
                var sqlServerLicensesAmount = 0;
                if (citaviProduct.IsSqlServerProduct)
                {
                    licensesCount = 1;
                    sqlServerLicensesAmount = orderItem.QuantityInt;

                    if (citaviProduct.IsSubscription &&
                        !orderItem.EndDateUtc.HasValue)
                    {
                        if (citaviProduct.CitaviProductCode == Product.CitaviDBServerCONCURRENTSubscription.CitaviProductCode)
                        {
                            citaviProduct = Product.C6DBServerCONCURRENT;
                        }
                        else if (citaviProduct.CitaviProductCode == Product.CitaviDBServerPERSEATSubscription.CitaviProductCode)
                        {
                            citaviProduct = Product.C6DBServerPerSeat;
                        }
                        else if (citaviProduct.CitaviProductCode == Product.CitaviDBServerREADERSubscription.CitaviProductCode)
                        {
                            citaviProduct = Product.C6DBServerReader;
                        }
                    }
                }

                var orderProcess = await GetOrderProcess(orderItem.TransactionId);

                if (orderProcess != null)
                {
                    var licenseAmount = sqlServerLicensesAmount > 0 ? sqlServerLicensesAmount : licensesCount;
                    var currentQuantity = licenseAmount - orderProcess.LicenseAmmount;
                    
                    licensesCount = currentQuantity;
                    if(licenseAmount == 0)
					{
                        orderProcess.Deactivate();
                        var subscription = await GetOrCreateSubscription(uluruOrder, orderProcess, licenseeUser);
                        if(subscription != null)
						{
                            subscription.Deactivate();
                        }
                        var licenses = licenseeUser.Licenses.Where(lic => lic.DataContractOrderKey == orderProcess.Key).ToList();
                        foreach (var license in licenses)
						{
                            license.Deactivate();
                            affectedContactKeys.Add(license.DataContractEndUserContactKey);
                        }
                        continue;
                    }
					else
					{
                        orderProcess.LicenseAmmount = licenseAmount;
                        if (citaviProduct != Product.CitaviSpace && licensesCount > 0)
                        {
                            addAsEndUser = false;
                        }
                    }

                    if (citaviProduct.IsSqlServerProduct)
                    {
                        licensesCount = 1;
                        var sqlServerLicenses = licenseeUser.Licenses.Where(lic => lic.DataContractOrderKey == orderProcess.Key).ToList();
                        if(sqlServerLicenses.Any())
						{
                            //update orderprocess & license, but no licenses to add.
                            licensesCount = 0;
                            if (citaviProduct.IsSqlServerReader)
                            {
                                sqlServerLicenses.First().ConcurrentReaderCount = licenseAmount;
                            }
                            else
                            {
                                sqlServerLicenses.First().ServerAmount = licenseAmount;
                            }
                        }
                    }
                    else if (licensesCount < 0 && !citaviProduct.IsSubscription)
                    {
                        var licenses = licenseeUser.Licenses.Where(lic => lic.DataContractOrderKey == orderProcess.Key).ToList();
                        var deactivateLicenses = licenses.Take(licensesCount * -1).ToList();
                        foreach(var deactivateLicense in deactivateLicenses)
						{
                            deactivateLicense.Deactivate();
                            affectedContactKeys.Add(deactivateLicense.DataContractEndUserContactKey);
                        }
                        licensesCount = 0;
                    }
                    orderProcess.OrderDate = DateTime.UtcNow;
                }
                else
                {
                    orderProcess = DbContext.Create<OrderProcess>();
                    orderProcess.CleverBridgeOrderNr = orderItem.TransactionId;
                    orderProcess.OrderDate = DateTime.UtcNow;
                    orderProcess.LicenseAmmount = sqlServerLicensesAmount > 0 ? sqlServerLicensesAmount : licensesCount;
                    orderProcess.LicenseContact.Set(licenseeUser.Contact);
                }

                orderProcess.XmlRaw = uluruOrder.Raw;

                SubscriptionItem subscriptionItem = null;

                if (citaviProduct.IsSubscription || orderItem.LicenceType == UluruLicenseType.TRIAL)
                {
                    var subscriptionOwner = licenseeUser;

                    var subscription = await GetOrCreateSubscription(uluruOrder, orderProcess, subscriptionOwner);
                    subscriptionItem = await GetOrCreateSubscriptionItem(orderItem, subscription);

                    isSubscriptionRenewal = subscriptionItem.EntityState != EntityState.Created &&
                                            (subscriptionItem.NextBillingDate.Date != orderItem.EndDateUtc.Value.Date ||
                                             subscriptionItem.LicenseAmount != orderProcess.LicenseAmmount);

					if (!isSubscriptionRenewal &&
                        orderItem.ServiceLimit > 0 &&
                        subscriptionItem.EntityState != EntityState.Created)
					{
                        var licenses = licenseeUser.Licenses.Where(lic => lic.DataContractOrderKey == orderProcess.Key).ToList();
						if (licenses.Any() &&
                            licenses.First().CitaviSpaceInMB != orderItem.ServiceLimit) 
						{
                            isSubscriptionAlignment = true;
                            refreshCitaviSpace = true;
                        }
                    }

                    if (isSubscriptionRenewal || isSubscriptionAlignment)
                    {
                        isSubscriptionRenewal = true;
                        subscriptionItem.NextBillingDate = orderItem.EndDateUtc.Value;
                        var licenses = licenseeUser.Licenses.Where(lic => lic.DataContractSubscriptionItemKey == subscriptionItem.Key).ToList();

                        if (!citaviProduct.IsSqlServerProduct)
                        {
                            if (licenses.Count > orderProcess.LicenseAmmount)
                            {
                                isSubscriptionAlignment = true;
                                licenses = licenses.OrderByDescending(lic => lic.DataContractEndUserContactKey).ToList();
                                var deactivateLicenses = licenses.Skip(orderProcess.LicenseAmmount);
                                licenses = licenses.Take(orderProcess.LicenseAmmount).ToList();
                                foreach (var deactivateLicense in deactivateLicenses)
                                {
                                    deactivateLicense.SubscriptionStatus = LicenseSubscriptionStatus.Deactivated;
                                }
                            }

                            foreach (var license in licenses)
                            {
                                license.ReadOnly = false;
                                license.SubscriptionStatus = LicenseSubscriptionStatus.Active;
                                license.ExpiryDate = subscriptionItem.NextBillingDate;
                                license.CitaviSpaceInMB = orderItem.ServiceLimit;
                            }

                            subscriptionItem.LicenseAmount = licenses.Count();

                            licensesCount = orderProcess.LicenseAmmount - subscriptionItem.LicenseAmount;
                            if (licensesCount <= 0)
                            {
                                //isSubscriptionRenewal
                                //if greater than 0 -> isSubscriptionAlignment, process new licences, continue
                                continue;
                            }
                            isSubscriptionAlignment = true;
                        }
						else
						{
                            var sqlServerLicenses = licenseeUser.Licenses.Where(lic => lic.DataContractOrderKey == orderProcess.Key).ToList();
                            if (sqlServerLicenses.Any())
                            {
                                licensesCount = 0;
                                var sqlServerLicense = sqlServerLicenses.First();
                                sqlServerLicense.SubscriptionStatus = LicenseSubscriptionStatus.Active;
                                sqlServerLicense.ExpiryDate = subscriptionItem.NextBillingDate;
                            }
                            subscriptionItem.LicenseAmount = orderItem.QuantityInt;
                        }
                    }
                    else
                    {
                        subscriptionItem.LicenseAmount = orderItem.QuantityInt;
                    }
                }

                for (var i = 0; i < licensesCount; i++)
                {
                    var companyName = uluruOrder.SoldToOrganisation;
                    var pricing = orderItem.LicenceType == UluruLicenseType.AUTO_SUBSCRIPTION ? Pricing.Personal : Pricing.Standard;

                    LicenseType licenseType;
					switch (orderItem.LicenceType)
					{
                        case UluruLicenseType.SUBSCRIPTION:
                        case UluruLicenseType.SUBSCRIPTION_Concurrent:
                        case UluruLicenseType.SUBSCRIPTION_Seat:
                        case UluruLicenseType.AUTO_SUBSCRIPTION:
                            licenseType = LicenseType.Subscription;
                            break;

                        default:
                            licenseType = LicenseType.Purchase;
                            break;

					}

                    var license = LicenseManager.CreateLicenseWithPurchaseItem(licenseeUser.Contact, pricing, licenseType, citaviProduct, orderProcess, companyName, sqlServerLicensesAmount);
                    purchased.Add(license);
                    if (!string.IsNullOrEmpty(uluruOrder.SubscriptionRef))
                    {
                        license.InvoiceNumber = uluruOrder.SubscriptionRef;
                    }
                    else
                    {
                        license.InvoiceNumber = orderItem.TransactionId;
                    }
                    licenseeUser.AddOwnerLicense(license);

                    if (orderItem.UpgradeType == UluruUpgradeType.Crossgrade)
                    {
                        var crossgradableLicenses = GetCrossgradableLicenses(licenseeUser);
                        var crossgradableLicense = crossgradableLicenses.OrderBy(lic => lic.DataContractEndUserContactKey).LastOrDefault();
                        if (crossgradableLicense == null)
                        {
                            Telemetry.TrackTrace($"CrossgradableLicense is null. TransactionId: {orderItem.TransactionId}");
                            continue;
                        }

                        var endUserContact = await crossgradableLicense.EndUser.Get();
                        if (endUserContact != null)
                        {
                            var endUser = await DbContext.GetByKeyAsync(endUserContact.Key);
                            endUser.AddEndUserLicense(license);
                            affectedContactKeys.Add(endUserContact.Key);
                        }
                        crossgradableLicense.Deactivate();
                        licenseeUser.Licenses.Remove(crossgradableLicense);
                    }
                    else if (addAsEndUser)
                    {
                        licenseeUser.AddEndUserLicense(license);
                        if (orderItem.UpgradeType == UluruUpgradeType.Benefit)
                        {
                            licenseeUser.Contact.CampusBenefitEligibility = CampusBenefitEligibilityType.Redeemed;
                        }

                        //if (license.ProductResolved == Product.C6Windows ||
                        //    license.ProductResolved == Product.CitaviWindowsAndWeb)
                        //{
                        //    var legacyFreeLicense = licenseeUser.GetLegacyFreeLicense();
                        //    if (legacyFreeLicense != null)
                        //    {
                        //        legacyFreeLicense.Deactivate();
                        //        licenseeUser.Licenses.Remove(legacyFreeLicense);
                        //    }
                        //}
                    }
                    else
                    {
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
                    }

                    if (orderItem.ServiceLimit > 0)
                    {
                        license.CitaviSpaceInMB = orderItem.ServiceLimit;
                        refreshCitaviSpace = true;
                        affectedContactKeys.Add(license.DataContractEndUserContactKey);
                    }

                    if(orderItem.LicenceType == UluruLicenseType.TRIAL)
					{
                        license.OrderCategory = OrderCategory.Trial;
                    }
                }
            }

            var licenseeUserIsNew = licenseeUser.Contact.EntityState == EntityState.Created;

            await DbContext.SaveAsync();

            if (licenseeUserIsNew)
            {
                var verificationKey = await UserManager.SetVerificationKeyForNewUserAsync(licenseeUser);
                var productGroup = OrderProcessProductGroup.Home;
                if (purchased.Any(p => p.ProductResolved.IsSqlServerProduct))
                {
                    productGroup = OrderProcessProductGroup.DbServer;
                }
                else if (purchased.Any(p => p.PricingResolved.IsBusinessOrAcacemicPricing()))
                {
                    productGroup = OrderProcessProductGroup.Business;
                }
                await EmailService.SendProcessOrder_BillingMail_AccountCreated(licenseeUser, licenseeUser.Email, verificationKey, productGroup);
                emailsSent.Add(licenseeUser.Email.ToUpperInvariant());
            }

            foreach (var contactKey in affectedContactKeys.Distinct())
			{
				if (string.IsNullOrEmpty(contactKey))
				{
                    continue;
				}

                await CrmUserCache.RemoveAsync(contactKey);

                if (refreshCitaviSpace)
                {
                    await CitaviSpaceCache.RefreshAsync(contactKey, DbContext);
                    await CitaviSpaceCache.QueueUpdateCitaviSpace(contactKey);
                }

                if (isSubscriptionAlignment)
                {
                    await AzureHelper.Ably.Invoke(MessageKey.SubscriptionChanged, CollectionUtility.ToDictionary(MessageKey.ContactKey, contactKey));
                }
            }

            result.Status = UluruOrderStatus.Ok;

            return result;
        }

        #endregion

        #endregion
    }
}
