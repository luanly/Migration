using Microsoft.Azure.KeyVault.Models;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Crm.Web.Authorization;
using SwissAcademic.Crm.Web.Cleverbridge;
using SwissAcademic.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SwissAcademic.Crm.Web
{
	public class LicenseManager
	{
		#region Constants

		const int Legacy_FreeSpace_Prod = 5120;
		const int Legacy_FreeSpace_Beta = 5120;

		#endregion

		#region Konstruktor

		public LicenseManager(CrmDbContext context)
		{
			DbContext = context;
			UserManager = new CrmUserManager(DbContext);
		}

		#endregion

		#region Eigenschaften

		CrmDbContext DbContext { get; set; }
		CrmUserManager UserManager { get; set; }

		#endregion

		#region Methoden

		#region AddLegacyFreeLicense
		public async Task<bool> AddLegacyFreeLicense(CrmUser user, bool skipCreatedOnCheck = false)
		{
			if (!CrmConfig.IsShopWebAppSubscriptionAvailable)
			{
				return false;
			}

			if (user == null)
			{
				return false;
			}

			if (user.Contact.CreatedOn > CrmConfig.LegacyFreeLicensePeriodEndDate &&
				!skipCreatedOnCheck)
			{
				return false;
			}

			if (user.HasCitavi6EndUserLicense() &&
				user.HasEndUserLicense(Product.CitaviSpace))
			{
				return false;
			}

			await using (var tslock = new TableStorageLock(user.Contact.Key, "AddLegacyFreeLicense"))
			{
				if (!await tslock.TryEnter(10))
				{
					return false;
				}

				try
				{
					if (!user.HasCitavi6EndUserLicense())
					{
						var freeLicense = CreateLegacyFreeLicense(user.Contact);
						user.AddEndUserLicense(freeLicense);
						user.AddOwnerLicense(freeLicense);
					}

					var updateCitaviSpace = false;

					if (!user.HasEndUserLicense(Product.CitaviSpace))
					{
						updateCitaviSpace = true;
						var citaviSpaceLicense = await CreateLegacyCitaviSpaceLicense(user.Contact);
						user.AddEndUserLicense(citaviSpaceLicense);
						user.AddOwnerLicense(citaviSpaceLicense);
					}

					await DbContext.SaveAndUpdateUserCacheAsync(user);

					if (updateCitaviSpace)
					{
						await CitaviSpaceCache.RefreshAsync(user, DbContext, skipCloudSpaceWarningSentCheck: true);
					}
					return true;
				}
				catch (Exception ex)
				{
					Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
				}

				return false;
			}
		}

		#endregion

		#region AssignLicense

		public async Task<Dictionary<string, string>> AssignLicenseAsync(string ownerContactKey, string licenseKey, string endUserEmailAddress)
		{
			var owner = await DbContext.GetByKeyAsync(ownerContactKey);
			if (owner == null)
			{
				Telemetry.TrackTrace($"User not found: {ownerContactKey} / {licenseKey}", SeverityLevel.Warning);
				return CollectionUtility.ToDictionary(MessageKey.Result, AssignLicenseResult.NotAllowed.ToString());
			}
			return await AssignLicenseAsync(owner, licenseKey, endUserEmailAddress);
		}

		internal async Task<Dictionary<string, string>> AssignLicenseAsync(CrmUser owner, string licenseKey, string endUserEmailAddress)
		{
			var result = AssignLicenseResult.NotAllowed;
			CrmUser resultUser = null;
			endUserEmailAddress = endUserEmailAddress.RemoveNonStandardWhitespace();

			if (new AuthorizationManager().CheckAssignCitaviLicenseAccess(owner, licenseKey))
			{
				var license = owner.Licenses.FirstOrDefault(i => i.Key == licenseKey);
				if (license == null)
				{
					throw new Exception($"License with key: '{licenseKey}' has not been found");
				}

				var user = await DbContext.GetByEmailAsync(endUserEmailAddress);
				if (user != null)
				{
					resultUser = user;
				}

				var endUser = GetOrCreateUser(user, endUserEmailAddress);
				result = await AssignLicenseAsync(owner, license, endUser, checkAccess: false);
			}

			return resultUser == null ? CollectionUtility.ToDictionary(MessageKey.Result, result.ToString()) : CollectionUtility.ToDictionary(MessageKey.Result, result.ToString(), MessageKey.EndUserContactKey, resultUser.Contact.Key, MessageKey.EndUserContactName, resultUser.Contact.FullName, MessageKey.EndUserIsVerified, resultUser.IsAccountVerified);
		}

		internal async Task<AssignLicenseResult> AssignLicenseAsync(CrmUser owner, CitaviLicense license, CrmUser endUser, bool checkAccess = true)
		{
			if (checkAccess &&
				!new AuthorizationManager().CheckAssignCitaviLicenseAccess(owner, license))
			{
				return AssignLicenseResult.NotAllowed;
			}

			var activeEndUserKey = license.DataContractEndUserContactKey;

			await license.EndUser.SetOrReplace(endUser.Contact);

			if (endUser.Contact.IsVerified.GetValueOrDefault(false))
			{
				UpdateLicenseKey(endUser, license);
			}

			if (endUser.Contact.EntityState == EntityState.Created ||
				!endUser.Contact.IsVerified.GetValueOrDefault(false))
			{
				endUser.Contact.ChangeLanguage(owner.Contact.Language);
				var verificationKey = await UserManager.SetVerificationKeyForNewUserAsync(endUser);
				await EmailService.SendAssignLicenseMail_CreatedAccount(endUser, endUser.Email, verificationKey);
			}
			else if (owner.Key != endUser.Key)
			{
				await EmailService.SendAssignLicenseMailAsync(endUser, endUser.Email);
			}

			await DbContext.SaveAndUpdateUserCacheAsync(owner);

			if (!string.IsNullOrEmpty(activeEndUserKey) &&
				activeEndUserKey != owner.Key)
			{
				var activeUser = await DbContext.GetByKeyAsync(activeEndUserKey);
				if (activeUser != null)
				{
					if (!activeUser.Contact.IsVerified.GetValueOrDefault(false))
					{
						if (activeEndUserKey == endUser.Key)
						{
							//Mail nochmals senden
							await UserManager.SendVerificationKeyMailAsync(activeUser, activeUser.Email);
						}
					}
					await CrmUserCache.RemoveAsync(activeUser);
				}
			}
			await CrmUserCache.RemoveAsync(endUser);
			await CrmUserCache.RemoveAsync(owner);

			if (license.CitaviSpaceInMB.HasValue &&
				license.CitaviSpaceInMB.Value > 0)
			{
				await CitaviSpaceCache.RefreshAsync(endUser, DbContext);
			}

			return AssignLicenseResult.Success;
		}

		#endregion

		#region CreatePreviousVersionCampusContractLicense

		public async Task<CreatePreviousVersionCampusContractLicenseResult> CreatePreviousVersionCampusContractLicenseAsync(CrmUser user, int requestedCitaviMajorVersionLicense)
		{
			var result = new CreatePreviousVersionCampusContractLicenseResult();
			var existingCampusLicenses = (from lic in user.Licenses
										  where !string.IsNullOrEmpty(lic.DataContractCampusContractKey) && lic.IsVerified
										  let campusContract = CrmCache.CampusContracts.FirstOrDefault(i => i.Key == lic.DataContractCampusContractKey)
										  where campusContract != null &&
												campusContract.ContractDuration > DateTime.UtcNow &&
												!campusContract.NewContractAvailable
										  select new
										  {
											  License = lic,
											  Contract = campusContract
										  }).OrderByDescending(i => i.License.ExpiryDate.Value);

			if (!existingCampusLicenses.Any())
			{
				result.Status = CreatePreviousVersionCampusContractLicenseResultType.NoCampusContractLicenseFound;
				return result;
			}
			var existingCampusLicense = existingCampusLicenses.First();

			foreach (var existingLicense in user.Licenses)
			{
				if (existingLicense.CitaviMajorVersion != requestedCitaviMajorVersionLicense)
				{
					continue;
				}
				if (string.Equals(existingLicense.DataContractLicenseTypeKey, LicenseType.Purchase.Key, StringComparison.InvariantCultureIgnoreCase))
				{
					continue;
				}
				var existingLicenseLicenseProduct = CrmCache.Products[existingLicense.DataContractProductKey];
				if (existingLicenseLicenseProduct.IsSqlServerProduct)
				{
					continue;
				}
				if (existingLicense.ExpiryDate == existingCampusLicense.License.ExpiryDate || //Campuslizenz mit dieser Version schon vorhanden
					existingLicense.ExpiryDate == null) //Gekaufte Campus Lizenz mit dieser Version
				{
					result.Status = CreatePreviousVersionCampusContractLicenseResultType.LicenseAlreadyExists;
					return result;
				}
				//Ältere Lizenz von einem Vorgängervertrag. Ist ok, wir erstellen eine neue Lizenz und entfernen diese.
				existingLicense.StatusCode = StatusCode.Inactive;
				user.Licenses.Remove(existingLicense);
				break;
			}

			Product product = null;
			switch (requestedCitaviMajorVersionLicense)
			{
				case 2:
					product = Product.C2Pro;
					break;

				case 3:
					product = Product.C3Team;
					break;

				case 4:
					product = Product.C4Team;
					break;

				case 5:
					product = Product.C5Windows;
					break;

				default:
					throw new NotSupportedException($"Unsupported major license version: {requestedCitaviMajorVersionLicense}");
			}

			var license = CreateLicense(user.Contact, Pricing.None, LicenseType.Subscription, product, existingCampusLicense.License.OrganizationName, existingCampusLicense.License.ExpiryDate);
			license.IsVerified = true;
			license.DataContractEndUserIsVerified = true;
			license.CampusContract.Set(existingCampusLicense.Contract);
			license.DataContractCampusContractKey = existingCampusLicense.Contract.Key;
			license.DataContractCampusContractInfoWebsite = existingCampusLicense.Contract.InfoWebsite;

			user.AddEndUserLicense(license);
			user.AddOwnerLicense(license);

			await DbContext.SaveAndUpdateUserCacheAsync(user);

			result.License = license;
			result.Status = CreatePreviousVersionCampusContractLicenseResultType.Success;
			return result;
		}

		#endregion

		#region CreateLegacyFreeLicense

		internal CitaviLicense CreateLegacyFreeLicense(Contact contact)
		{
			var product = Product.C6Windows;
			var pricing = Pricing.Personal;
			var licenseType = LicenseType.Purchase;

			var freeLicense = DbContext.Create<CitaviLicense>();

			freeLicense.Free = true;
			freeLicense.LicenseType.Set(licenseType);
			freeLicense.Product.Set(product);
			freeLicense.Pricing.Set(pricing);
			freeLicense.OrderDate = DateTime.UtcNow;

			SetLicenseKey(freeLicense, contact, product, pricing, string.Empty);

			freeLicense.DataContractLicenseTypeKey = licenseType.Key;
			freeLicense.DataContractLicenseTypeCode = licenseType.LicenseCode;

			freeLicense.DataContractPricingKey = pricing.Key;
			freeLicense.DataContractPricingCode = pricing.PricingCode;

			freeLicense.DataContractProductName = product.CitaviProductName;
			freeLicense.DataContractProductKey = product.Key;

			return freeLicense;
		}

		#endregion

		#region CreateLegacyCitaviSpaceLicense

		internal async Task<CitaviLicense> CreateLegacyCitaviSpaceLicense(Contact contact)
		{
			var product = Product.CitaviSpace;
			var pricing = Pricing.Personal;
			var licenseType = LicenseType.Subscription;

			var citaviSpaceLicense = DbContext.Create<CitaviLicense>();

			citaviSpaceLicense.LicenseType.Set(licenseType);
			citaviSpaceLicense.Product.Set(product);
			citaviSpaceLicense.Pricing.Set(pricing);
			citaviSpaceLicense.OrderDate = DateTime.UtcNow;
			citaviSpaceLicense.Free = true;

			citaviSpaceLicense.ExpiryDate = CrmConfig.LegacyFreeLicensePeriodEndDate.AddYears(1);

			SetLicenseKey(citaviSpaceLicense, contact, product, pricing, string.Empty);

			citaviSpaceLicense.DataContractLicenseTypeKey = licenseType.Key;
			citaviSpaceLicense.DataContractLicenseTypeCode = licenseType.LicenseCode;

			citaviSpaceLicense.DataContractPricingKey = pricing.Key;
			citaviSpaceLicense.DataContractPricingCode = pricing.PricingCode;

			citaviSpaceLicense.DataContractProductName = product.CitaviProductName;
			citaviSpaceLicense.DataContractProductKey = product.Key;

			if (!await CitaviSpaceCache.HasCitaviSpaceCalculated(contact.Key) || Environment.Build == BuildType.Alpha)
			{
				try
				{
					using (var op = Telemetry.StartOperation("CalculateCitaviSpace"))
					{
						await CrmConfig.CalculateCitaviSpace(contact.Key, true);
					}
				}
				catch(Exception ex)
				{
					Telemetry.TrackException(ex, flow: ExceptionFlow.Eat);
				}
			}

			var space = await CitaviSpaceCache.GetUsedCitaviSpaceAsync(contact.Key, contact.DataCenter);

			if (Environment.Build != BuildType.Release)
			{
				Telemetry.TrackDiagnostics($"CitaviSpace '{contact.Key}': {space.BytesToMegabytes()} mb used");
			}

			if (space == 0)
			{
				if (Environment.Build == BuildType.Beta)
				{
					citaviSpaceLicense.CitaviSpaceInMB = Legacy_FreeSpace_Beta;
				}
				else
				{
					citaviSpaceLicense.CitaviSpaceInMB = Legacy_FreeSpace_Prod;
				}
			}
			else
			{
				space += space / 10;
				var mb_space = (int)Math.Ceiling(space.BytesToMegabytes());
				if (mb_space < Legacy_FreeSpace_Beta &&
					Environment.Build == BuildType.Beta)
				{
					mb_space = Legacy_FreeSpace_Beta;
				}
				else if (mb_space < Legacy_FreeSpace_Prod)
				{
					mb_space = Legacy_FreeSpace_Prod;
				}
				citaviSpaceLicense.CitaviSpaceInMB = mb_space;
			}

			return citaviSpaceLicense;
		}

		#endregion

		#region CreateLicenseWithPurchaseItem

		internal CitaviLicense CreateLicenseWithPurchaseItem(Contact contact, CleverbridgeProduct cleverbridgeProduct, OrderProcess order, string organisationName, int sqlServerQuantity = 0)
		{
			return CreateLicenseWithPurchaseItem(contact, cleverbridgeProduct.PricingResolved, cleverbridgeProduct.LicenseTypeResolved, cleverbridgeProduct.ProductResolved, order, organisationName, sqlServerQuantity);
		}

		internal CitaviLicense CreateLicenseWithPurchaseItem(Contact contact, Pricing pricing, LicenseType licenseType, Product product, OrderProcess order, string organisationName, int sqlServerQuantity)
		{
			var license = CreateLicense(contact, pricing, licenseType, product, organisationName, null);
			if (product.IsSqlServerProduct)
			{
				license.OrganizationName = organisationName;
				license.ServerId = Guid.NewGuid().ToString();
				license.ServerConcurrent = product.IsSqlServerConcurrentProduct;

				if (product.CitaviMajorVersion == 5)
				{
					license.AllowUnlimitedReaders = product.IsSqlServerReader;
					license.ServerAmount = product.IsSqlServerReader ? 0 : sqlServerQuantity;
				}
				else
				{
					if (product.IsSqlServerReader)
					{
						license.ConcurrentReaderCount = sqlServerQuantity;
					}
					else
					{
						license.ServerAmount = sqlServerQuantity;
					}
				}

				license.ServerXMLLicense = CreateServerLicenseXml(license, product);
			}
			if (order != null)
			{
				license.OrderProcess.Set(order);
			}
			license.IsVerified = true;
			return license;
		}

		#endregion

		#region CreateLicensesWithVoucherCode

		internal IEnumerable<CitaviLicense> CreateLicensesWithVoucherCode(Contact contact, Voucher voucher, VoucherBlock voucherBlock, string organisationName)
		{
			var added = new List<CitaviLicense>();

			if (voucher.VoucherStatus != VoucherStatus.Active)
			{
				var exception = new NotSupportedException($"{nameof(voucher.VoucherStatus)} must be {VoucherStatus.Active}");
				Telemetry.TrackException(exception, property1: (nameof(voucher.VoucherStatus), voucher.VoucherStatus));
				return added;
			}
			voucher.RedeemedOn = DateTime.UtcNow;
			voucher.Contact.Set(contact);
			voucher.VoucherStatus = VoucherStatus.Redeemed;
			LicenseType licenseType = null;
			Pricing pricing = null;
			var products = new List<Product>();
			CampusContract campusContract = null;
			if (voucherBlock.CampusContractVoucher)
			{
				licenseType = LicenseType.Subscription;
				pricing = Pricing.None;
				campusContract = CrmCache.CampusContracts.FirstOrDefault(cc => cc.Key == voucherBlock.DataContractCampusContractKey);
				if (campusContract != null)
				{
					products.AddRange(campusContract.ProductsResolved);
					products.RemoveAll(p => !p.IsCampusContractProduct);
				}
				else
				{
					Telemetry.TrackTrace($"CreateLicensesWithVoucherCode: CampusContract not found: {voucherBlock.DataContractCampusContractKey}", SeverityLevel.Warning);
				}
			}
			else
			{
				licenseType = CrmCache.LicenseTypesByKey[voucherBlock.DataContractLicenseTypeKey];
				pricing = CrmCache.PricingsByKey[voucherBlock.DataContractPricingKey];
				products.Add(CrmCache.Products[voucherBlock.DataContractProductKey]);
			}

			DateTime? expiryDate = null;
			if (voucherBlock.VoucherValidityInMonths > 0)
			{
				expiryDate = DateTime.UtcNow.AddMonths((int)voucherBlock.VoucherValidityInMonths);
			}
			else if (voucherBlock.DataContractCampusContractContractDuration != null &&
					 voucherBlock.DataContractCampusContractContractDuration != DateTime.MinValue)
			{
				expiryDate = voucherBlock.DataContractCampusContractContractDuration;
			}

			foreach (var product in products)
			{
				var license = CreateLicense(contact, pricing, licenseType, product, organisationName, expiryDate);
				license.Voucher.Set(voucher);
				license.IsVerified = true;
				license.DataContractEndUserIsVerified = contact.IsVerified.GetValueOrDefault();
				license.DataContractLicenseTypeKey = licenseType.Key;
				license.DataContractPricingKey = pricing.Key;
				license.DataContractProductKey = product.Key;
				license.DataContractCampusContractInfoWebsite = voucherBlock.DataContractCampusContractInfoWebsite;

				if (campusContract != null)
				{
					license.CampusContract.Set(campusContract);
					if (contact.CampusBenefitEligibility == CampusBenefitEligibilityType.NotApplicable ||
						!contact.CampusBenefitEligibility.HasValue)
					{
						//Der Kunde hatte nie eine Campuslizenz -> Neu Anspruch auf 50% Benefit Product
						contact.CampusBenefitEligibility = CampusBenefitEligibilityType.Eligible;
					}
					if (campusContract.CitaviSpaceInGB.HasValue)
					{
						license.CitaviSpaceInMB = campusContract.CitaviSpaceInGB.Value * 1024;
					}
				}
				else
				{
					if (voucherBlock.CitaviSpaceInGB.HasValue)
					{
						license.CitaviSpaceInMB = voucherBlock.CitaviSpaceInGB.Value * 1024;
					}
				}

				added.Add(license);
			}
			return added;
		}

		#endregion

		#region CreateLicenseWithCampusContract

		internal CitaviLicense CreateLicenseWithCampusContract(Contact contact, CampusContract campusContract, Product product, string organizationName)
		{
			if (campusContract.ContractDuration < DateTime.UtcNow)
			{
				var exception = new NotSupportedException($"{nameof(campusContract.ContractDuration)} is expired");
				Telemetry.TrackException(exception, property1: (nameof(campusContract.ContractDuration), campusContract.ContractDuration));
				return null;
			}

			if (string.IsNullOrEmpty(organizationName))
			{
				organizationName = campusContract.OrganizationName;
			}

			var pricing = Pricing.None;
			var licenseType = LicenseType.Subscription;
			var license = CreateLicense(contact, pricing, licenseType, product, organizationName, campusContract.ContractDuration);
			if (campusContract.CitaviSpaceInGB.HasValue)
			{
				license.CitaviSpaceInMB = campusContract.CitaviSpaceInGB.Value * 1024;
			}
			license.CampusContract.Set(campusContract);
			return license;
		}

		#endregion

		#region CreateLicense

		public CitaviLicense CreateLicense(Contact contact, Pricing pricing, LicenseType licenseType, Product product, string organisationName, DateTime? expiryDate)
		{
			var license = DbContext.Create<CitaviLicense>();
			license.LicenseType.Set(licenseType);
			license.Product.Set(product);
			license.Pricing.Set(pricing);
			license.OrderCategory = OrderCategory.Standard;
			license.OrderDate = DateTime.UtcNow;
			license.OrderMailSentOn = DateTime.UtcNow;

			if (expiryDate.HasValue)
			{
				license.ExpiryDate = expiryDate.Value;
			}
			if (!product.IsSqlServerProduct &&
				!product.IsSubscription)
			{
				SetLicenseKey(license, contact, product, pricing, organisationName);
			}
			else
			{
				//Für "UI" im CRM
				license.CitaviLicenseName = contact.FullName;
				license.OrganizationName = organisationName;
			}
			license.DataContractLicenseTypeKey = licenseType.Key;
			license.DataContractLicenseTypeCode = licenseType.LicenseCode;

			license.DataContractPricingKey = pricing.Key;
			license.DataContractPricingCode = pricing.PricingCode;

			license.DataContractProductName = product.CitaviProductName;
			license.DataContractProductKey = product.Key;

			return license;
		}

		#endregion

		#region CreateLicenseKey

		public static string CreateLicenseKey(Product product, string citaviLicenseName, string organizationName, CitaviLicensePermission permission, DateTime? expiryDate)
		{
			string licenseKey;

			if (!expiryDate.HasValue)
			{
				expiryDate = Environment.NullDate;
			}

			if (product == null)
			{
				throw new ArgumentNullException(nameof(product), "Product must no be null");
			}

			if (product.CitaviMajorVersion < 6 && product.CitaviMajorVersion > 0)
			{
				var licenseType = Type.GetType("SwissAcademic.Citavi.Licensing.License, SwissAcademic.Citavi");
				if (licenseType == null)
				{
					throw new NotSupportedException("SwissAcademic.Citavi.Licensing.License, SwissAcademic.Citavi not found");
				}
				var createKey = licenseType.GetMethod("GetKeyPrivate", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

				var suffix = product.CitaviMajorVersion < 5 && product.CitaviMajorVersion > 2 ? "Team" : "Pro";

				if (expiryDate == Environment.NullDate)
				{
					//Lizenzen welche via CRM erzeugt
					if (product.Key == Product.C3Team.Key ||
						product.Key == Product.C4Team.Key)
					{
						suffix = "Team";
					}
					else if (product.Key == Product.C3Reader.Key ||
							product.Key == Product.C4Reader.Key)
					{
						suffix = "Reader";
					}
					else
					{
						suffix = "Pro";
					}
				}

				licenseKey = (string)createKey.Invoke(null, new object[] {    citaviLicenseName,
																			  organizationName,
																			  expiryDate,
																			  suffix,
																			  (int)permission,
																			  product.CitaviMajorVersion });
			}
			else
			{
				licenseKey = PasswordGenerator.LicenseKey.Generate();
			}
			if (product.CitaviMajorVersion > 2)
			{
				licenseKey = FormatLicenceKey(licenseKey);
			}

			return licenseKey;
		}

		#endregion

		#region CreateServerLicenseXml

		public string CreateServerLicenseXml(CitaviLicense license, Product product)
		{
			if (license == null)
			{
				throw new ArgumentNullException(nameof(license), "License must not be null");
			}
			if (product == null)
			{
				throw new ArgumentNullException(nameof(product), "Product must not be null");
			}

			var stringBuilder = new StringBuilder();
			var dbServerType = license.ServerConcurrent.GetValueOrDefault(false) ? "concurrent" : "per seat";

			using (var writer = XmlWriterSafe.Create(stringBuilder))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("CitaviLicense");

				writer.WriteStartElement("License");
				writer.WriteAttributeString("id", license.ServerId);
				writer.WriteAttributeString("type", $"DBServer {dbServerType}");

				if (product.CitaviMajorVersion == 5)
				{
					//AllowUnlimitedReaders gibt es nur in C5. Ab C6 ConcurrentReaderCount
					//#21338 Es gibt ConcurrentLizenzen mit AllowUnlimitedReaders. Kann nur via CRM direkt gesetzt werden.
					var allowUnlimitedReaders = product.IsSqlServerReader || license.AllowUnlimitedReaders.GetValueOrDefault();
					writer.WriteAttributeString("allowUnlimitedReaders", allowUnlimitedReaders.ToString());
				}

				writer.WriteAttributeString("version", product.CitaviMajorVersion.ToString(CultureInfo.InvariantCulture));

				writer.WriteStartElement("Organization");
				writer.WriteAttributeString("name", license.OrganizationName ?? string.Empty);
				writer.WriteEndElement();

				if (license.ExpiryDate.HasValue)
				{
					writer.WriteStartElement("ExpiryDate");
					writer.WriteAttributeString("value", license.ExpiryDate.Value.ToString(DateTimeFormatInfo.InvariantInfo));
					writer.WriteEndElement();
				}

				if (license.ConcurrentReaderCount > 0)
				{
					writer.WriteStartElement("ConcurrentReaderCount");
					writer.WriteAttributeString("value", license.ConcurrentReaderCount.ToString(CultureInfo.InvariantCulture));
					writer.WriteEndElement();
				}

				writer.WriteStartElement("Count");
				writer.WriteAttributeString("value", license.ServerAmount.ToString(CultureInfo.InvariantCulture));
				writer.WriteEndElement();

				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadFromString(stringBuilder.ToString());
			xmlDocument.Sign("320327b22d7e64ea34f4b0d6389f0c7e027b37eb");
			return xmlDocument.SaveToString();
		}

		#endregion

		#region DeactivateExpiredLicenses

		public async Task<int> DeactivateExpiredLicenses()
		{
			var deactived = 0;

			var xml = new Query.FetchXml.GetExpiredLicenses().TransformText();
			var result = await DbContext.Fetch<CitaviLicense>(FetchXmlExpression.Create<CitaviLicense>(xml), observe: true);

			if (result == null || !result.Any())
			{
				Telemetry.TrackDiagnostics("No expired licenses found.");
				return deactived;
			}

			Telemetry.TrackDiagnostics($"Expired licenses found: {result.Count()}");

			foreach (var license in result)
			{
				if (license.ExpiryDate > DateTime.UtcNow)
				{
					//on-or-before in FetchXml
					continue;
				}
				if (!string.IsNullOrEmpty(license.DataContractCampusContractKey))
				{
					//Werden von CrmFunction deaktiviert
					continue;
				}

				if (license.ReadOnly || !CrmConfig.IsShopWebAppSubscriptionAvailable)
				{
					deactived++;
					license.Deactivate();
					if (!string.IsNullOrEmpty(license.DataContractEndUserContactKey))
					{
						var user = await DbContext.GetByKeyAsync(license.DataContractEndUserContactKey);
						if (user != null)
						{
							//await EmailService.SendSubscriptionLicenseDeactivatedAsync(user);
						}
					}
				}
				else
				{
					license.ReadOnly = true;
					license.ExpiryDate = license.ExpiryDate.Value.AddDays(CrmConfig.SubscriptionReadOnlyPeriodInDays);
					if (!string.IsNullOrEmpty(license.DataContractEndUserContactKey))
					{
						var user = await DbContext.GetByKeyAsync(license.DataContractEndUserContactKey);
						if (user != null)
						{
							//await EmailService.SendSubscriptionLicenseReadOnlyAsync(user);
						}
					}
				}
				if (!string.IsNullOrEmpty(license.DataContractEndUserContactKey))
				{
					await CrmUserCache.RemoveAsync(license.DataContractEndUserContactKey);
				}
				if (license.DataContractEndUserContactKey != license.DataContractOwnerContactKey &&
					!string.IsNullOrEmpty(license.DataContractOwnerContactKey))
				{
					await CrmUserCache.RemoveAsync(license.DataContractOwnerContactKey);
				}
				if (license.CitaviSpaceInMB > 0)
				{
					await CitaviSpaceCache.RefreshAsync(license.DataContractEndUserContactKey, DbContext);
				}
			}

			await DbContext.SaveAsync();

			return deactived;
		}

		#endregion

		#region FormatLicenceKey

		public static string FormatLicenceKey(string licenseKey)
		{
			licenseKey = licenseKey.Insert(2, "-");
			licenseKey = licenseKey.Insert(6, "-");
			licenseKey = licenseKey.Insert(10, "-");
			licenseKey = licenseKey.Insert(14, "-");
			return licenseKey;
		}

		#endregion

		#region GetOrCreateUser

		CrmUser GetOrCreateUser(CrmUser user, string emailAddress)
		{
			if (user == null)
			{
				var contact = DbContext.Create<Contact>();
				user = new CrmUser(contact);
				user.AddLinkedEMailAccount(emailAddress);
			}
			else
			{
				user.Contact.StatusCode = StatusCode.Active;
			}

			return user;
		}

		#endregion

		#region ResendAssignLicenseMail

		public async Task<Dictionary<string, string>> ResendAssignLicenseMailAsync(string ownerContactKey, string licenseKey, string endUserEmailAddress)
		{
			var owner = await DbContext.GetByKeyAsync(ownerContactKey);
			if (owner == null)
			{
				Telemetry.TrackTrace($"Owner not found: {ownerContactKey} / {licenseKey}", SeverityLevel.Warning);
				return CollectionUtility.ToDictionary(MessageKey.Result, AssignLicenseResult.NotAllowed.ToString());
			}
			if (!owner.Licenses.Any(i => i.Key == licenseKey && i.DataContractOwnerContactKey == owner.Key))
			{
				Telemetry.TrackTrace($"Owner-License not found: {ownerContactKey} / {licenseKey}", SeverityLevel.Warning);
				return CollectionUtility.ToDictionary(MessageKey.Result, AssignLicenseResult.NotAllowed.ToString());
			}

			var endUser = await DbContext.GetByEmailAsync(endUserEmailAddress);
			if (endUser == null)
			{
				Telemetry.TrackTrace($"EndUser not found: {endUserEmailAddress} / {licenseKey}", SeverityLevel.Warning);
				return CollectionUtility.ToDictionary(MessageKey.Result, AssignLicenseResult.NotAllowed.ToString());
			}
			if (!endUser.Licenses.Any(i => i.Key == licenseKey && i.DataContractEndUserContactKey == endUser.Key))
			{
				Telemetry.TrackTrace($"EndUser-License not found: {endUserEmailAddress} / {licenseKey}", SeverityLevel.Warning);
				return CollectionUtility.ToDictionary(MessageKey.Result, AssignLicenseResult.NotAllowed.ToString());
			}

			if (endUser.IsAccountVerified)
			{
				await EmailService.SendAssignLicenseMailAsync(endUser, endUser.Email);
			}
			else
			{
				await UserManager.SendVerificationKeyMailAsync(endUser, endUser.Email);
			}
			return CollectionUtility.ToDictionary(MessageKey.Result, AssignLicenseResult.Success.ToString());
		}

		#endregion

		#region SetLicenseKey

		void SetLicenseKey(CitaviLicense license, Contact contact, Product product, Pricing pricing, string organisationName)
		{
			if (string.IsNullOrEmpty(organisationName))
			{
				if (pricing.IsPersonalPricing())
				{
					organisationName = Resources.Strings.ResourceManager.GetString(nameof(Resources.Strings.PersonalLicenseName), contact.GetCultureInfo());
				}
				else if (pricing.IsStudentPricing())
				{
					organisationName = Resources.Strings.ResourceManager.GetString(nameof(Resources.Strings.StudentLicenseName), contact.GetCultureInfo());
				}
				else
				{
					organisationName = Resources.Strings.ResourceManager.GetString(nameof(Resources.Strings.CompanyLicenseName), contact.GetCultureInfo());
				}
			}

			license.OrganizationName = organisationName.Trim();
			license.CitaviLicenseName = contact.BuildLicenseName();

			if (product.IsCitaviWeb)
			{
				return;
			}

			license.CitaviKey = CreateLicenseKey(product, license.CitaviLicenseName, license.OrganizationName, CitaviLicensePermission.Default, license.ExpiryDate);
		}

		#endregion

		#region WithdrawLicense

		public async Task WithdrawLicenseAsync(CrmUser owner, string licenseKey)
		{
			if (new AuthorizationManager().CheckAssignCitaviLicenseAccess(owner, licenseKey))
			{
				var lic = owner.Licenses.First(lic => string.Equals(lic.Key, licenseKey, StringComparison.InvariantCultureIgnoreCase));
				var endUserKey = (await lic.EndUser.Get())?.Key;
				if (string.IsNullOrEmpty(endUserKey))
				{
					return;
				}
				var endUser = await DbContext.GetByKeyAsync(endUserKey);
				if (endUser == null)
				{
					return;
				}
				endUser.Contact.EndUserLicenses.Remove(lic);
				await DbContext.SaveAsync();
				await EmailService.SendLicenseWithdrawalMailAsync(endUser, endUser.Email);

				await CrmUserCache.RemoveAsync(owner);
				await CrmUserCache.RemoveAsync(endUser);

				if (lic.IsCitaviSpace)
				{
					await CitaviSpaceCache.RefreshAsync(endUser, DbContext);
				}
			}
		}

		#endregion

		#region UpdateLicenseKey

		internal void UpdateLicenseKey(CrmUser endUser, CitaviLicense license)
		{
			var product = CrmCache.Products[license.DataContractProductKey];
			var pricing = CrmCache.PricingsByKey[license.DataContractPricingKey];
			SetLicenseKey(license, endUser.Contact, product, pricing, license.OrganizationName);
		}

		#endregion

		#endregion

		#region Statische Methoden

		[Obsolete("CreateLicenseKey verwenden")]
		public static string CreateCitavi2LicenseKey(string username, string organization, string expiryDate)
		{
			var licenseType = Type.GetType("SwissAcademic.Citavi.Licensing.License, SwissAcademic.Citavi");
			var createKey = licenseType.GetMethod("GetKeyPrivate", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

			var suffix = "Pro";

			DateTime expiry;
			if (!string.IsNullOrEmpty(expiryDate))
			{
				expiry = DateTime.Parse(expiryDate);
			}
			else
			{
				expiry = Environment.NullDate;
			}

			return (string)createKey.Invoke(null, new object[] { username,
																		   organization,
																		   expiry,
																		   suffix,
																		   (int)CitaviLicensePermission.Default,
																		   2});
		}

		public static string CreateBatchLicenseXml(string organization, string accountKey, string citaviLicenseVersion, string expiryDate = "")
		{
			#region CreateXmlDocument

			var xmlDocument = new XmlDocument
			{
				PreserveWhitespace = true
			};
			xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);

			//root
			var rootElement = xmlDocument.CreateElement("License");

			var typeElement = xmlDocument.CreateAttribute("type");
			typeElement.InnerText = "Batch";
			rootElement.Attributes.Append(typeElement);

			var versionElement = xmlDocument.CreateAttribute("version");
			versionElement.InnerText = citaviLicenseVersion;
			rootElement.Attributes.Append(versionElement);

			xmlDocument.AppendChild(rootElement);

			//organization
			var orgnameElement = xmlDocument.CreateElement("Organization");

			var orgnameValue = xmlDocument.CreateAttribute("name");
			orgnameValue.InnerText = organization;
			orgnameElement.Attributes.Append(orgnameValue);

			var accountKeyValue = xmlDocument.CreateAttribute("accountKey");
			accountKeyValue.InnerText = accountKey;
			orgnameElement.Attributes.Append(accountKeyValue);

			rootElement.AppendChild(orgnameElement);

			//expirydate
			if (!string.IsNullOrEmpty(expiryDate))
			{
				var dateElement = xmlDocument.CreateElement("ExpiryDate");
				var dateValueAttribute = xmlDocument.CreateAttribute("value");
				dateValueAttribute.InnerText = expiryDate;
				dateElement.Attributes.Append(dateValueAttribute);
				rootElement.AppendChild(dateElement);
			}

			#endregion

			//sign
			xmlDocument.Sign("320327b22d7e64ea34f4b0d6389f0c7e027b37eb");

			#region Write to string with UTF-8

			string xmlString = null;

			//using (MemoryStream ms = new MemoryStream())
			//{
			//    XmlWriterSettings settings = new XmlWriterSettings();
			//    settings.Encoding = new UTF8Encoding(false);

			//    using (XmlWriter writer = XmlWriter.Create(ms, settings))
			//    {
			//        xmlDocument.Save(writer);
			//    }

			//    xmlString = Encoding.UTF8.GetString(ms.ToArray());
			//}

			using (var ms = new MemoryStream())
			{
				using (var xmlTextWriter = new XmlTextWriter(ms, Encoding.UTF8))
				{
					xmlTextWriter.Formatting = Formatting.Indented;
					xmlDocument.Save(xmlTextWriter);
					ms.Seek(0, SeekOrigin.Begin);
					using (var sr = new StreamReader(ms, Encoding.UTF8))
					{
						xmlString = sr.ReadToEnd();
					}
				}
			}

			#endregion

			#region return string

			return xmlString;

			#endregion
		}

		#endregion
	}
}
