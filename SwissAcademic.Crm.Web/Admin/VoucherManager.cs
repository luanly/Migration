using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class VoucherManager
    {
        #region Konstruktor

        public VoucherManager(CrmDbContext dbContext)
        {
            DbContext = dbContext;
            LicenseManager = new LicenseManager(DbContext);
        }

        #endregion

        #region Eigenschaften

        CrmDbContext DbContext { get; set; }
        LicenseManager LicenseManager { get; set; }

        VoucherCodeGenerator CodeGenerator { get; } = new VoucherCodeGenerator();

        #endregion

        #region Methoden

        #region CreateVoucher

        internal async Task<Voucher> CreateVoucher(VoucherBlock voucherBlock)
        {
            var voucher = DbContext.Create<Voucher>();
            voucher.VoucherBlock.Set(voucherBlock);

            VoucherCodeInfo voucherCodeInfo = null;
            var unique = false;
            var exit = 100;
            while (!unique)
            {
                voucherCodeInfo = CodeGenerator.CreateVoucherCode();
                if (exit == 0)
                {
                    //CodeGenerator.CreateVoucherCode arbeitet mit Random -> Generiert u.U. Duplikate
                    //-> RNGCryptoServiceProvider verwenden

                    throw new NotSupportedException("Cannot generate unique voucher code");
                }
                exit--;
                if (await DbContext.Exists<Voucher>(VoucherPropertyId.VoucherCode, voucherCodeInfo.VoucherCode))
                {
                    continue;
                }
                break;
            }

            voucher.VoucherCode = voucherCodeInfo.VoucherCode;
            voucher.VoucherCodeInt = voucherCodeInfo.VoucherCodeInt;
            voucher.VoucherCodePre = voucherCodeInfo.VoucherCodePre;
            voucher.VoucherStatus = VoucherStatus.Active;
            voucher.DataContractVoucherBlockKey = voucherBlock.Key;
            return voucher;
        }

        #endregion

        #region CreateVoucherCode

        internal VoucherCodeInfo CreateVoucherCode() => CodeGenerator.CreateVoucherCode();

        #endregion

        #region CreateGiftCardVoucher

        internal async Task<Voucher> CreateGiftCardVoucher(Contact contact, CleverbridgeProduct product, OrderProcess orderProcess = null)
        {
            var voucherBlock = DbContext.Create<VoucherBlock>();
            voucherBlock.Contact.Set(contact);
            voucherBlock.LicenseType.Set(product.LicenseTypeResolved);
            voucherBlock.Pricing.Set(product.PricingResolved);
            voucherBlock.Product.Set(product.ProductResolved);
            voucherBlock.CampusUseVoucherBlockProduct = true;
            voucherBlock.ShowInLicenseManagement = false;

            if(orderProcess != null)
			{
                orderProcess.VoucherBlocks.Add(voucherBlock);
                voucherBlock.CbOrderNummer = orderProcess.CleverBridgeOrderNr;
			}

            return await CreateVoucher(voucherBlock);
        }

        #endregion

        #region Redeem

        public async Task<VoucherRedeemResult> RedeemAsync(CrmUser user, string voucherCode)
        {
            var resetCloudSpaceCache = false;

            try
            {
                await using (var tslock = new TableStorageLock(voucherCode.ToLowerInvariant()))
                {
                    if(!await tslock.TryEnter())
					{
                        if (CrmConfig.IsUnittest)
                        {
                            return new VoucherRedeemResult { Status = VoucherRedeemResultType.Error };
                        }
                        throw new RateLimitException();
                    }

                    try
                    {
                        var query = new Query.FetchXml.GetVoucher(voucherCode).TransformText();

                        var entities = await DbContext.Fetch(FetchXmlExpression.Create<Voucher>(query));
                        var result = new CrmSet(entities);
                        DbContext.Attach(result);
                        var voucher = result.Vouchers?.FirstOrDefault();
                        if (voucher == null ||
                            voucher.VoucherStatus != VoucherStatus.Active)
                        {
                            user.Contact.FailedRedeemVoucherCount++;
                            if (user.Contact.FailedRedeemVoucherCount >= 10)
                            {
                                if (user.Contact.LastFailedRedeemVoucher < DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(10)))
                                {
                                    user.Contact.FailedRedeemVoucherCount = 1;
                                }
                                else
                                {
                                    return new VoucherRedeemResult { Status = VoucherRedeemResultType.TooManyAttempts };
                                }
                            }
                            user.Contact.LastFailedRedeemVoucher = DateTime.UtcNow;
                            if (voucher == null)
                            {
                                return new VoucherRedeemResult { Status = VoucherRedeemResultType.VoucherNotFound };
                            }
                            else
                            {
                                return new VoucherRedeemResult { Status = VoucherRedeemResultType.VoucherIsNotActive };
                            }
                        }

                        user.Contact.FailedRedeemVoucherCount = 0;
                        user.Contact.LastFailedRedeemVoucher = null;

                        var voucherBlock = result.VoucherBlocks.FirstOrDefault(i => i.Key == voucher.DataContractVoucherBlockKey);
                        if (voucherBlock == null)
                        {
                            Telemetry.TrackTrace($"{nameof(voucherBlock)} not found {voucher.Key}");
                            return new VoucherRedeemResult { Status = VoucherRedeemResultType.VoucherNotFound };
                        }

                        //VoucherLizenzen welche schon beim User sind, aber noch nicht verifziert
                        //Bei diesen muss am Schluss
                        var existingVoucherLicences = new List<CitaviLicense>();
                        CampusContract campusContract = null;
                        if (voucherBlock.CampusContractVoucher)
                        {
                            var campusContracts = CrmCache.CampusContracts.Where(i => i.DataContractAccountKey == voucherBlock.DataContractAccountKey);
                            if (campusContracts == null ||
                                !campusContracts.Any())
                            {
                                Telemetry.TrackTrace($"CampusContract not found for VoucherBlock: {voucherBlock.Key}", SeverityLevel.Warning);
                                return new VoucherRedeemResult { Status = VoucherRedeemResultType.Error };
                            }
                            campusContracts = campusContracts.Where(i => i.VerifyStVoucher || i.VerifyMaVoucher);
                            if (campusContracts.Count() > 1)
                            {
                                campusContracts = campusContracts.Where(i => !i.NewContractAvailable);
                            }
                            if (!campusContracts.Any())
                            {
                                campusContracts = CrmCache.CampusContracts.Where(i => i.DataContractAccountKey == voucherBlock.DataContractAccountKey)
                                                                                .Where(i => !i.NewContractAvailable);
                            }
                            campusContract = campusContracts.FirstOrDefault();
                            voucherBlock.DataContractCampusContractContractDuration = campusContract.ContractDuration;
                            voucherBlock.DataContractCampusContractInfoWebsite = campusContract.InfoWebsite;
                            voucherBlock.DataContractCampusContractKey = campusContract.Key;

                            foreach (var product in campusContract.ProductsResolved)
                            {
                                if (!product.IsCampusContractProduct)
                                {
                                    continue;
                                }

                                foreach (var existing in user.Licenses.ToList())
                                {
                                    if (existing.IsVerified)
                                    {
                                        continue;
                                    }

                                    if (existing.DataContractCampusContractKey != campusContract.Key)
                                    {
                                        continue;
                                    }

                                    if (existing.DataContractProductKey != product.Key)
                                    {
                                        continue;
                                    }

                                    //Voucher via Campus-Verlängerung
                                    //Der Kunde hat eine "Nicht verfifizierte Voucher-Lizenz" -> Verifizieren
                                    existing.IsVerified = true;
                                    existing.Voucher.Set(voucher);
                                    existing.OrganizationName = string.IsNullOrEmpty(voucherBlock.OrganizationName) ? campusContract.OrganizationName : voucherBlock.OrganizationName;
                                    voucher.RedeemedOn = DateTime.UtcNow;
                                    voucher.Contact.Set(user.Contact);
                                    voucher.VoucherStatus = VoucherStatus.Redeemed;
                                    existingVoucherLicences.Add(existing);
                                    if (voucherBlock.VoucherValidityInMonths > 0)
                                    {
                                        //Lizenzschlüssel aktualisieren
                                        existing.ExpiryDate = DateTime.UtcNow.AddMonths((int)voucherBlock.VoucherValidityInMonths);
                                    }

                                    if (existing.IsCitaviSpace)
                                    {
                                        resetCloudSpaceCache = true;
                                    }

                                    LicenseManager.UpdateLicenseKey(user, existing);
                                }

                                //Alte Lizenzen suchen und entfernen
                                var expiredContracts = CrmCache.CampusContracts.Where(i => i.AccountResolved.Key == campusContract.DataContractAccountKey && i.NewContractAvailable).ToList();
                                foreach (var expiredContract in expiredContracts)
                                {
                                    var expiredLicenses = user.Licenses.Where(i => i.DataContractCampusContractKey == expiredContract.Key).ToList();
                                    foreach (var expiredLicense in expiredLicenses)
                                    {
                                        if (expiredLicense != null)
                                        {
                                            expiredLicense.StatusCode = StatusCode.Inactive;
                                            user.Licenses.Remove(expiredLicense);
                                            if (expiredLicense.IsCitaviSpace)
                                            {
                                                resetCloudSpaceCache = true;
                                            }
                                        }
                                    }
                                }
                            }

                            if (existingVoucherLicences.Any())
                            {
                                return new VoucherRedeemResult
                                {
                                    ExpiryDate = existingVoucherLicences.First().ExpiryDate,
                                    LicenseKey = existingVoucherLicences.First().Key,
                                    Status = VoucherRedeemResultType.Success
                                };

                            }
                        }
                        //User hat noch keine Voucherlizenz
                        var organizationName = voucherBlock.OrganizationName;
                        if (string.IsNullOrEmpty(organizationName) && campusContract != null)
                        {
                            organizationName = campusContract.OrganizationName;
                        }

                        var licenses = LicenseManager.CreateLicensesWithVoucherCode(user.Contact, voucher, voucherBlock, organizationName);

                        foreach (var license in licenses)
                        {
                            user.AddOwnerLicense(license);
                            user.AddEndUserLicense(license);
                            if (license.IsCitaviSpace)
                            {
                                resetCloudSpaceCache = true;
                            }
                        }

                        if (licenses.Any())
                        {
                            Telemetry.TrackDiagnostics($"Redeem voucher: {voucherCode}");
                            return new VoucherRedeemResult
                            {
                                ExpiryDate = licenses.First().ExpiryDate,
                                LicenseKey = licenses.First().Key,
                                Status = VoucherRedeemResultType.Success
                            };
                        }

                        return new VoucherRedeemResult { Status = VoucherRedeemResultType.Error };
					}
					finally
					{
                        await DbContext.SaveAndUpdateUserCacheAsync(user);
                        if (resetCloudSpaceCache)
                        {
                            await CitaviSpaceCache.RefreshAsync(user, DbContext);
                        }
                    }
                }
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
                return new VoucherRedeemResult { Status = VoucherRedeemResultType.Error };
            }
        }

        #endregion

        #region SaveVoucherBlock

        public async Task<string> SaveVoucherBlockAsCsv(CrmUser user, string voucherBlockKey)
        {
            var voucherBlock = user.VoucherBlocks.FirstOrDefault(i => i.Key == voucherBlockKey);

            if (voucherBlock == null)
            {
                Telemetry.TrackTrace($"Voucherbloch not found: {voucherBlockKey}", SeverityLevel.Warning);
                return null;
            }

            return await SaveVoucherBlockAsCsv(voucherBlock);
        }

        internal async Task<string> SaveVoucherBlockAsCsv(VoucherBlock voucherBlock)
        {
            var s = new StringBuilder();
            DbContext.Attach(voucherBlock);
            s.AppendLine($"VoucherCode;VoucherStatus;Contact;RedeemedOn");
            foreach (var voucher in await voucherBlock.Vouchers.Get(null, EntityPropertySets.Voucher))
            {
                var contact = await voucher.Contact.Get(EntityPropertySets.Contact);
                s.AppendLine($"{voucher.VoucherCode};{voucher.VoucherStatus?.ToString()};{contact?.FullName};{voucher.RedeemedOn}");
            }

            return s.ToString();
        }

        #endregion

        #endregion
    }
}
