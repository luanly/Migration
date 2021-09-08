using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class ShopManager
    {
        #region Felder

        static RegionInfo _defaultRegion = new RegionInfo("en-US");
        const string IPRegionGraphQuery = "{\"query\":\"query MyQuery {\n cart(cartInput: {cartItems: {productId: 72708}, remoteIpAddress: \\\"$1\\\", clientId: 635}) {\n    country {\n      isoCode\n    }\n  }\n}\n\",\"variables\":null,\"operationName\":\"MyQuery\"}";

        #endregion

        #region Konstruktor

        public ShopManager()
        {

        }

        #endregion

        #region Eigenschaften

        #region Dictionarry

        public static ConcurrentDictionary<string, Tuple<double, string>> VatRates { get; } = new ConcurrentDictionary<string, Tuple<double, string>>();

        #endregion

        #endregion

        #region Methoden

        #region CreateCheckoutUrlAsync

        //https://www.cleverbridge.com/corporate/docs/default-source/csc-documents/checkout-process-urls.pdf?sfvrsn=12

        public async Task<string> CreateCheckoutUrlAsync(CrmUser user, IEnumerable<KeyValuePair<string, StringValues>> dic, bool generateUsersessionUrl = true)
        {
            var keys = dic.Where(k => k.Key == ShopConstants.ProductKey).Select(k => k.Value).ToList();
            var quantities = dic.Where(k => k.Key == ShopConstants.Quantity).Select(k => k.Value).ToList();
            var baseQuantities = dic.Where(k => k.Key == ShopConstants.BaseQuantity).Select(k => k.Value).ToList();

            var checkoutScope = dic.Where(k => k.Key == ShopConstants.CheckoutScope).Select(k => k.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(checkoutScope))
            {
                checkoutScope = ShopCheckoutScope.Checkout.ToString();
            }

            var language = dic.Where(k => k.Key == ShopConstants.Culture).Select(k => k.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(language))
            {
                language = "en";
            }

            var country = dic.Where(k => k.Key == ShopConstants.Country).Select(k => k.Value).FirstOrDefault();

            var order = new List<Tuple<string, string, string>>();
            var design = ShopConstants.Design_Private;

            ShopView shopView = null;
            if(user != null)
			{
                shopView = await GetShopViewAsync(user, null);
			}

            for (var i = 0; i < quantities.Count; i++)
            {
                if (string.IsNullOrEmpty(quantities[i]))
                {
                    continue;
                }
                //C-Web: xlrhggjef3djenxd1qug8lkrahh1xq54cg5jfxzsfsyyaegxs
                //Cloudspace: yig5dat1zq0ydcm3n82r7zhbpwznrmvgmdh1fdlyctcumag

                var product = CrmCache.CleverbridgeProductsByKey[keys[i]];
                var quantity = Convert.ToInt32(quantities[i], CultureInfo.InvariantCulture);
                if (product.MaxQuantity != -1 && quantity > product.MaxQuantity)
                {
                    quantity = product.MaxQuantity.GetValueOrDefault();
                }
                if (quantity < 0)
                {
                    quantity = 1;
                }

                if(user == null)
				{
                    //#6235
                    baseQuantities[i] = "0";
                }
				else
				{
                    //#6235
                    var showProductItem = shopView.Products.FirstOrDefault(p => p.Key == product.Key);
                    if (showProductItem == null)
                    {
                        if (CrmConfig.IsUnittest)
                        {
                            baseQuantities[i] = "-1";
                        }
                        else
                        {
                            baseQuantities[i] = "0";
                        }
                    }
                    else
                    {
                        baseQuantities[i] = showProductItem.ProductGroupQuantity.ToString();
                    }
                }

                order.Add(new Tuple<string, string, string>(product.ProductId.ToString(CultureInfo.InvariantCulture), quantity.ToString(CultureInfo.InvariantCulture), baseQuantities[i]));

                if (Pricing.IsBusinessOrAcacemicPricing(product.PricingResolved.Key))
                {
                    design = ShopConstants.Design_Institutionen;
                    if (user != null && user.Licenses.Any(lic => !string.IsNullOrEmpty(lic.InvoiceNumber)))
                    {
                        design = ShopConstants.Design_Institutionen_NoFax;
                    }
                }
            }
            var lang = $"language={language.ToString().ToLowerInvariant()}";
            var cart = "cart=" + order.Select(i => i.Item1).ToString(",");
            var quantityUrl = order.Select(i => $"quantity_{i.Item1.ToString()}={i.Item2}" +
                                                 $"&minquantity_{i.Item1.ToString()}={i.Item2}" +
                                                 $"&maxquantity_{i.Item1.ToString()}={i.Item2}" +
                                                 $"&basequantity_{i.Item1.ToString()}={i.Item3}").ToString("&");
            var scope = $"scope={checkoutScope.ToString().ToLowerInvariant()}";
            var checkoutUrl = $"{ShopConstants.CleverbridgeCheckoutUrl}{scope}&{lang}&{cart}&{quantityUrl}&cfg={design}";

            #region DeliveryContact / ResellerAffiliateId

            if (user != null)
            {
                var contact = user.Contact;
                var deliveryContactUrl = $"&deliveryFirstname={contact.FirstName}" +
                                         $"&deliveryLastname={contact.LastName}" +
                                         $"&deliveryStreet1={contact.Address1_Line1}" +
                                         $"&deliveryStreet2={contact.Address1_Line2}" +
                                         $"&deliveryCity={contact.Address1_City}" +
                                         $"&deliveryStateId={contact.Address1_StateOrProvince}" +
                                         $"&deliveryPostalcode={contact.Address1_PostalCode}" +
                                         $"&deliveryCountryId={contact.Address1_Country}" +
                                         $"&deliveryPhone1={contact.Address1_Telephone1}" +
                                         $"&deliveryFax={contact.Address1_Fax}" +
                                         $"&deliveryCompany={contact.Firm}" +
                                         $"&internalcustomer={contact.Key}" +
                                         $"&deliveryEmail={contact.EMailAddress1}";

                if (user.Contact.GenderCode != null &&
                   user.Contact.GenderCode != GenderCodeType.Unknown)
                {
                    if (user.Contact.GenderCode == GenderCodeType.Female)
                    {
                        deliveryContactUrl += "&deliverySalutationId=MRS";
                    }
                    else
                    {
                        deliveryContactUrl += "&deliverySalutationId=MR_";
                    }
                }

                checkoutUrl += deliveryContactUrl;

                if (!string.IsNullOrEmpty(user.Contact.ResellerAffiliateId))
                {
                    checkoutUrl += "&affiliate=" + user.Contact.ResellerAffiliateId;
                }
                else if (!string.IsNullOrEmpty(country))
                {
                    using (var context = new CrmDbContext())
                    {
                        var resellerIdExclusive = await Account.GetAffiliateIdByExclusiveCountries(country, context);
                        if (!string.IsNullOrEmpty(resellerIdExclusive))
                        {
                            checkoutUrl += "&affiliate=" + resellerIdExclusive;
                        }
                    }
                }
            }

            #endregion

            checkoutUrl += "&x-initialprice=initialhide";

            if (!generateUsersessionUrl)
            {
                return checkoutUrl;
            }

            return await GenerateUserSessionUrl(checkoutUrl);
        }

        #endregion

        #region GenerateUserSessionUrl

        public static async Task<string> GenerateUserSessionUrl(string url)
        {
            var obj = new { TargetUrl = WebUtility.UrlEncode(url) };
            using (var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"))
            {
                using (var response = await CleverbridgeHttpClient.Instance.PostAsync(ShopConstants.GenerateUserSessionUrl + "/generateusersessionurl", content))
                {
                    var userSession = JsonConvert.DeserializeObject<GenerateUserSessionUrlResponse>(await response.Content.ReadAsStringAsync());
                    return userSession.Url;
                }
            }
        }
		#endregion

        #region GetShopViewAsync

        public async Task<ShopView> GetShopViewAsync(CrmUser user, RegionInfo regionInfo)
        {
            if (regionInfo == null)
            {
                regionInfo = _defaultRegion;
            }

            var shopView = new ShopView();
            shopView.Country = regionInfo.TwoLetterISORegionName;
            shopView.Currency = regionInfo.ISOCurrencySymbol;
            var products = CrmCache.CleverbridgeProductsByProductId.Select(i => i.Value);
            foreach (var product in products)
            {
                #region Products

                if (!product.ShowInShop)
                {
                    continue;
                }

                if (product.IsResellerProduct)
                {
                    continue;
                }

                if (product.AllowedUpgradeFromProductResolved.Any())
                {
                    continue;
                }

                if (product.PricingResolved.Key == Pricing.Benefit.Key)
                {
                    continue;
                }

                if (product.PricingResolved.Key == Pricing.None.Key)
                {
                    continue;
                }

                shopView.Products.Add(new ProductShopItem
                {
                    CleverbridgeProduct = product,
                    MaxQuantity = product.MaxQuantity.GetValueOrDefault(-1),
                    IndexForSorting = product.IndexForSorting,
                    Key = product.Key,
                    Name = product.Name,
                    Pricing = product.PricingResolved.PricingCode
                });

                #endregion
            }

            if (user != null)
            {
                #region Bereits gekaufte Produkte (ohne Upgrades)

                foreach (var shopProduct in shopView.Products)
                {
                    var product = shopProduct.CleverbridgeProduct;

                    if (product.IsGiftCard)
                    {
                        continue;
                    }

                    if (shopProduct.CleverbridgeProduct.ProductResolved.IsSqlServerProduct)
                    {
                        shopProduct.Quantity = (from lic in user.Licenses
                                                where lic.DataContractOwnerContactKey == user.Key &&
                                                      lic.DataContractLicenseTypeKey == product.LicenseTypeResolved.Key &&
                                                      lic.DataContractPricingKey == product.PricingResolved.Key &&
                                                      lic.DataContractProductKey == product.ProductResolved.Key
                                                select lic.ServerAmount + lic.ConcurrentReaderCount).Sum();
                    }
                    else
                    {
                        shopProduct.Quantity = (from lic in user.Licenses
                                                where lic.DataContractOwnerContactKey == user.Key &&
                                                      lic.DataContractLicenseTypeKey == product.LicenseTypeResolved.Key &&
                                                      lic.DataContractPricingKey == product.PricingResolved.Key &&
                                                      lic.DataContractProductKey == product.ProductResolved.Key
                                                select lic).Count();
                    }

                }

                #endregion

                #region Upgrades: Bereist gekaufte Produkte (ohne Reseller u. Campus)

                foreach (var license in user.Licenses)
                {
                    if (license.DataContractOwnerContactKey != user.Key)
                    {
                        continue;
                    }

                    if (license.IsCampusLicense)
                    {
                        continue;
                    }

                    var product = (from item in products
                                   where item.PricingResolved.Key == license.DataContractPricingKey &&
                                         item.ProductResolved.Key == license.DataContractProductKey &&
                                         !item.IsResellerProduct &&
                                         !item.IsGiftCard
                                   select item).FirstOrDefault();


                    if (product == null)
                    {
                        continue;
                    }

                    var p = shopView.UserProducts.FirstOrDefault(i => i.Key == product.Key);
                    if (p == null)
                    {
                        p = new ProductShopItem
                        {
                            CleverbridgeProduct = product,
                            MaxQuantity = product.MaxQuantity.GetValueOrDefault(-1),
                            IndexForSorting = product.IndexForSorting,
                            Key = product.Key,
                            Name = product.Name,
                            Pricing = product.PricingResolved.PricingCode,

                        };
                        foreach (var upgrade in product.AllowedUpgradeToProductResolved)
                        {
                            p._upgradeToProducts.Add(upgrade.Key);
                        }
                        shopView.UserProducts.Add(p);
                    }
                    if (p.CleverbridgeProduct.ProductResolved.IsSqlServerProduct)
                    {
                        p.Quantity += license.ServerAmount + license.ConcurrentReaderCount;
                    }
                    else
                    {
                        p.Quantity++;
                    }
                }

                foreach (var license in user.Licenses)
                {
                    if (license.DataContractOwnerContactKey != user.Key)
                    {
                        continue;
                    }
                    //if (license.DataContractPricingKey == Pricing.Benefit.Key) continue;
                    if (license.DataContractPricingKey == Pricing.None.Key)
                    {
                        continue;
                    }

                    if (license.IsCampusLicense)
                    {
                        continue;
                    }

                    var product = (from item in products
                                   where item.LicenseTypeResolved.Key == license.DataContractLicenseTypeKey &&
                                         item.PricingResolved.Key == license.DataContractPricingKey &&
                                         item.ProductResolved.Key == license.DataContractProductKey &&
                                         !item.IsResellerProduct &&
                                         item.AllowedUpgradeToProductResolved.Any()
                                   select item).FirstOrDefault();

                    if (product == null)
                    {
                        continue;
                    }

                    foreach (var upgradeTo in product.AllowedUpgradeToProductResolved)
                    {
                        var p = shopView.Upgrades.FirstOrDefault(i => i.Key == upgradeTo.Key);
                        if (p == null)
                        {
                            p = new ProductShopItem
                            {
                                CleverbridgeProduct = upgradeTo,
                                Key = upgradeTo.Key,
                                Name = upgradeTo.Name,
                                MaxQuantity = 0,
                                Pricing = upgradeTo.PricingResolved.PricingCode,
                            };
                            p._upgradeFromProducts.AddRange(upgradeTo.AllowedUpgradeFromProductResolved.Select(i => i.Key));
                            shopView.Upgrades.Add(p);
                        }
                        if (product.ProductResolved.IsSqlServerProduct)
                        {
                            p.ProductUpgradeMaxQuantity += license.ServerAmount + license.ConcurrentReaderCount;
                        }
                        else
                        {
                            p.ProductUpgradeMaxQuantity++;
                        }
                    }
                }

                foreach (var product in shopView.UserProducts)
                {
                    if (shopView.Upgrades.Any(i => i.Key == product.Key))
                    {
                        var upgrade = shopView.Upgrades.First(i => i.Key == product.Key);
                        var upgradedProduct = shopView.UserProducts.First(i => i._upgradeToProducts.Contains(product.Key));
                        foreach (var otherUpgrades in shopView.Upgrades.Where(i => i._upgradeFromProducts.Contains(upgradedProduct.Key)))
                        {
                            if (product.CleverbridgeProduct.ProductResolved.IsSqlServerProduct)
                            {
                                var serverLicence = user.Licenses.FirstOrDefault(p => p.DataContractProductKey == product.CleverbridgeProduct.ProductResolved.Key);
                                if (serverLicence != null)
                                {
                                    otherUpgrades.ProductUpgradeMaxQuantity -= serverLicence.ServerAmount + serverLicence.ConcurrentReaderCount;
                                }
                            }
                            else
                            {
                                otherUpgrades.ProductUpgradeMaxQuantity -= product.Quantity;
                            }
                        }
                        upgrade.Quantity = product.Quantity;
                    }
                }

                shopView.Upgrades.RemoveAll(upgrade => upgrade.ProductUpgradeMaxQuantity <= 0);

                var allProducts = shopView.Products.Concat(shopView.UserProducts).Concat(shopView.Upgrades);
                foreach (var productGroup in shopView.UserProducts.GroupBy(i => i.CleverbridgeProduct.DataContractGroupProductKey))
                {
                    if (string.IsNullOrEmpty(productGroup.Key))
                    {
                        continue;
                    }

                    var productsInGroup = allProducts.Where(i => i.CleverbridgeProduct.DataContractGroupProductKey == productGroup.Key).ToList();
                    var productsInGroupQuantity = productGroup.Sum(i => i.Quantity);
                    foreach (var product in productsInGroup)
                    {
                        //FUNKTIONIERT NICHT
                        //"Konstruierter" Fall #4599
                        //C5 und C6 (keine Upgrades) vorhanden. Wir drüfen die PGQ bei den Upgrades nicht erhöhen.
                        //Er hat diese noch nicht gekauft. Obwohl C6 gleiches Basisprodukt
                        //Wir schliessen hier "Citavi 6" (Vollkauf) aus
                        //                  if (product.Key == product.ProductGroupKey)
                        //{
                        //                      continue;
                        //}
                        product.ProductGroupQuantity = productsInGroupQuantity;
                    }
                }

                #endregion

                #region Upgrades: Campus (Benefit)

                if (user.Contact.CampusBenefitEligibility == CampusBenefitEligibilityType.Eligible)
                {
                    foreach (var product in shopView.UserProducts.Where(i => i.Key == Pricing.Benefit.Key))
                    {
                        if (shopView.Upgrades.Any(i => i.Key == product.Key))
                        {
                            var upgrade = shopView.Upgrades.First(i => i.Key == product.Key);
                            var upgradedProduct = shopView.UserProducts.First(i => i._upgradeToProducts.Contains(product.Key));
                            foreach (var otherUpgrades in shopView.Upgrades.Where(i => i._upgradeFromProducts.Contains(upgradedProduct.Key)))
                            {
                                otherUpgrades.Quantity++;
                            }
                        }
                    }
                }

                #endregion
            }

            #region SpecialOffers

            foreach (var product in shopView.Products.Where(i => i.CleverbridgeProduct.IsGiftCard))
            {
                product.Pricing = "gift";
            }

            #endregion

            #region Checks

            foreach (var p in shopView.Products.ToList())
            {
                var version = p.CleverbridgeProduct.ProductResolved.CitaviMajorVersion;
                if (version != CrmConfig.MaxLicenseMajorVersion && !p.CleverbridgeProduct.ProductResolved.IsSubscription)
                {
                    shopView.Products.Remove(p);
                }
                else
                {
                    p.ShowInShop = CrmCache.CleverbridgeProductsByProductId[p.CleverbridgeProductId.Value].ShowInShop;
                }
            }
            foreach (var p in shopView.Upgrades.ToList())
            {
                var version = p.CleverbridgeProduct.ProductResolved.CitaviMajorVersion;
                if (version != CrmConfig.MaxLicenseMajorVersion && !p.CleverbridgeProduct.ProductResolved.IsSubscription)
                {
                    shopView.Upgrades.Remove(p);
                }
                else
                {
                    p.ShowInShop = true;
                }
            }
            foreach (var p in shopView.UserProducts.ToList())
            {
                if (p.CleverbridgeProduct.ProductResolved.IsSubscription)
                {
                    p.ShowInShop = true;
                }
                else
                {
                    var version = p.CleverbridgeProduct.ProductResolved.CitaviMajorVersion;
                    p.ShowInShop = version == CrmConfig.MaxLicenseMajorVersion &&
                                              CrmCache.CleverbridgeProductsByProductId[p.CleverbridgeProductId.Value].ShowInShop;
                }
            }

            if (user != null)
            {
                var campusBenefitProductChecked = false;
                foreach (var p in shopView.UserProducts)
                {
                    if (shopView.Upgrades.Any(i => i.CleverbridgeProductId == p.CleverbridgeProductId))
                    {
                        continue;
                    }

                    if (p.ShowInShop &&
                        p.CleverbridgeProductId.HasValue)
                    {
                        if (CrmCache.CleverbridgeProductsByProductId[p.CleverbridgeProductId.Value].PricingResolved.Key == Pricing.Benefit.Key)
                        {
                            if (user.Contact.CampusBenefitEligibility != CampusBenefitEligibilityType.Eligible)
                            {
                                p.ShowInShop = false;
                            }
                            campusBenefitProductChecked = true;
                        }
                    }
                }

                if (!campusBenefitProductChecked &&
                    user.Contact.CampusBenefitEligibility == CampusBenefitEligibilityType.Eligible)
                {
                    //#21693
                    //Hat der Kunde "Eligible", dann müssen wir das Produkt hinzufügen und anzeigen lassen.
                    var benefit = CrmCache.CleverbridgeProductsByProductId.Where(i => i.Value.ShowInShop &&
                                                                                            i.Value.PricingResolved.Key == Pricing.Benefit.Key &&
                                                                                            i.Value.ProductResolved.CitaviMajorVersion == CrmConfig.MaxLicenseMajorVersion).FirstOrDefault();
                    if (benefit.Value != null)
                    {
                        shopView.Products.Add(new ProductShopItem
                        {
                            Name = benefit.Value.Name,
                            Key = benefit.Value.Key,
                            Pricing = Pricing.Benefit.PricingCode,
                            Quantity = 0,
                            MaxQuantity = 1,
                            CleverbridgeProduct = benefit.Value,
                            ShowInShop = true
                        });
                    }
                }

            }

            #endregion

            await RefreshPricesAsync(shopView, regionInfo);
            return shopView;
        }

        #endregion

        #region GetClientRegionInfo

        public static async Task<string> GetClientRegionInfo(string remoteIpAddress)
        {
            try
            {
                var query = IPRegionGraphQuery.Replace("$1", remoteIpAddress);
                using (var content = new StringContent(query, Encoding.UTF8, "application/json"))
                {
                    using (var response = await CleverbridgeHttpClient.Instance.PostAsync(ShopConstants.GraphQLUrl, content))
                    {
                        var json = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
                        return json["data"]["cart"]["country"]["isoCode"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Warning, ExceptionFlow.Eat);
            }
            return string.Empty;
        }

        #endregion

        #region RefreshPricesAsync

        public async Task RefreshPricesAsync(ShopView shopView, RegionInfo regionInfo)
        {
            var allProducts = shopView.Products.Concat(shopView.Upgrades).Concat(shopView.UserProducts).Where(i => i.CleverbridgeProduct != null);
            var response = await RefreshPricesAsync(allProducts.Select(i => i.CleverbridgeProduct.ProductId.ToString(CultureInfo.InvariantCulture)), regionInfo);
            if (response == null ||
               !response.Products.Any(i => i.ProductPrices.Count > 0))
            {
                //Aus dem Iran konnten keine Produkte gekauft werden (Shop, Fehler bei Preisabfrage)
                //http://tfs2012:8080/tfs/CITAVICollection/Citavi/_workitems/edit/20734
                regionInfo = _defaultRegion;
                shopView.Country = _defaultRegion.TwoLetterISORegionName;
                shopView.Currency = _defaultRegion.ISOCurrencySymbol;
                response = await RefreshPricesAsync(allProducts.Select(i => i.CleverbridgeProduct.ProductId.ToString(CultureInfo.InvariantCulture)), _defaultRegion);
            }
            var vat = await RefreshVatRateAsync(allProducts.Select(i => i.CleverbridgeProduct.ProductId.ToString(CultureInfo.InvariantCulture)).First(), regionInfo.TwoLetterISORegionName);

            shopView.VatRate = vat.Item1;
            shopView.VatRateLocalized = vat.Item2;
            shopView.CalculateDiscounts(response);

            foreach (var product in allProducts)
            {
                var cbProduct = CrmCache.CleverbridgeProductsByKey[product.Key];
                var price = response.Products.FirstOrDefault(i => i.Id == cbProduct.ProductId)?.ProductPrices.Where(p => p.Quantity == 1)?.FirstOrDefault();

                if (price == null)
                {
                    Telemetry.TrackTrace($"No productprice found for {cbProduct.Name}", SeverityLevel.Warning);
                    continue;
                }
                product.Price = price.SinglePriceNet;
            }
        }

        public async Task<ProductPriceResponse> RefreshPricesAsync(IEnumerable<string> productIds, RegionInfo regionInfo)
        {
            var url = ShopConstants.GetProductPriceUrl + $"getproductprice?currencyids={regionInfo.ISOCurrencySymbol}&productids={string.Join(",", productIds.Distinct())}";
            try
            {
                var response = JsonConvert.DeserializeObject<ProductPriceResponse>(await CleverbridgeHttpClient.Instance.GetStringAsync(url));
                if (response.ResultMessage != "OK")
                {
                    Telemetry.TrackTrace(response.ResultMessage, SeverityLevel.Warning, property1: ("URL", url));
                    return null;
                }
                return response;
            }
            catch (Exception ignored)
            {
                //Response status code does not indicate success: 400 (Bad Request).
                //BSP: Iran u. currencyIds = IRR -> Embargo
                //return null und dann regionInfo = us regioninfo
                Telemetry.TrackTrace($"{ignored.Message}\r\nURL:{url}", SeverityLevel.Warning);
                return null;
            }
        }

        public async Task<Tuple<double, string>> RefreshVatRateAsync(string productId, string country)
        {
            if (VatRates.TryGetValue(country, out var vatrate))
            {
                return vatrate;
            }
            try
            {
                var url = $"https://pricingapi.cleverbridge.com/prices?client_id=635&product_id={productId}&country={country}";
                var response = JsonConvert.DeserializeObject<dynamic>(await CleverbridgeHttpClient.Instance.GetStringAsync(url));
                var vat = (double)response[0].vat_rate.value;
                var vatLocalized = $"{vat} %";
                var tuple = new Tuple<double, string>(vat, vatLocalized);
                VatRates.TryAdd(country, tuple);
                return tuple;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat, property1: ("ProductId", productId), property2: ("Country", country));
            }
            return new Tuple<double, string>(0, string.Empty);
        }

        #endregion

        #endregion
    }
}
