using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.CleverbridgeProduct)]
    [DataContract]
    public class CleverbridgeProduct
        :
        CitaviCrmEntity
    {
        #region Konstanten

        public const int GiftCardProductIdC5 = 152063;
        public const int GiftCardProductIdC6 = 201574;

        #endregion

        #region Konstruktor

        public CleverbridgeProduct()
            :
            base(CrmEntityNames.CleverbridgeProduct)
        {

        }

        #endregion

        #region Eigenschaften

        #region AllowedProductsForUpgrade

        ManyToManyRelationship<CleverbridgeProduct, CleverbridgeProduct> _allowedProductsForUpgrade;
        public ManyToManyRelationship<CleverbridgeProduct, CleverbridgeProduct> AllowedProductsForUpgrade
        {
            get
            {
                if (_allowedProductsForUpgrade == null)
                {
                    _allowedProductsForUpgrade = new ManyToManyRelationship<CleverbridgeProduct, CleverbridgeProduct>(this, CrmRelationshipNames.CleverbridgeProductAllowedUpgrades);
                    Observe(_allowedProductsForUpgrade, true);
                }
                return _allowedProductsForUpgrade;
            }
        }

        #endregion

        #region AllowedUpgradeFromProductResolved

        public List<CleverbridgeProduct> AllowedUpgradeFromProductResolved { get; private set; } = new List<CleverbridgeProduct>();

        #endregion

        #region AllowedUpgradeToProductResolved

        public List<CleverbridgeProduct> AllowedUpgradeToProductResolved { get; private set; } = new List<CleverbridgeProduct>();

        #endregion

        #region BaseProduct

        ManyToOneRelationship<CleverbridgeProduct, CleverbridgeProduct> _baseProduct;
        public ManyToOneRelationship<CleverbridgeProduct, CleverbridgeProduct> BaseProduct
        {
            get
            {
                if (_baseProduct == null)
                {
                    _baseProduct = new ManyToOneRelationship<CleverbridgeProduct, CleverbridgeProduct>(this, CrmRelationshipNames.CleverbridgeProductBaseProduct, "new_basecbproductid");
                    Observe(_baseProduct, true);
                }
                return _baseProduct;
            }
        }

        #endregion

        #region BaseCbProductId

        [CrmProperty]
        public Guid BaseCbProductId
        {
            get
            {
                return GetValue<Guid>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region CitaviSpaceInGB

        [CrmProperty]
        [DataMember]
        public int? CitaviSpaceInGB
        {
            get
            {
                return GetValue<int?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Edition

        public ShopProductEdition Edition
        {
            get
            {
                if(AllowedUpgradeFromProductResolved.Any())
                {
                    return ShopProductEdition.Upgrade;
                }

                if (ProductResolved.IsSqlServerProduct)
                {
                    return ShopProductEdition.DbServer;
                }

                if (ProductResolved.IsCitaviWeb)
                {
                    return ShopProductEdition.Web;
                }

                if (ProductResolved.IsCitaviSpace)
                {
                    return ShopProductEdition.Cloudspace;
                }

                return ShopProductEdition.Windows;
            }
        }

        #endregion

        #region IndexForSorting

        [CrmProperty]
        [DataMember]
        public int IndexForSorting
        {
            get
            {
                return GetValue<int>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region IsGiftCard

        public bool IsGiftCard
        {
            get
            {
                return (ProductId == GiftCardProductIdC5 || ProductId == GiftCardProductIdC6);
            }
        }

        #endregion

        #region IsResellerProduct

        [CrmProperty]
        [DataMember]
        public bool IsResellerProduct
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region LicenseType

        ManyToOneRelationship<CleverbridgeProduct, LicenseType> _licenseType;
        public ManyToOneRelationship<CleverbridgeProduct, LicenseType> LicenseType
        {
            get
            {
                if (_licenseType == null)
                {
                    _licenseType = new ManyToOneRelationship<CleverbridgeProduct, LicenseType>(this, CrmRelationshipNames.CleverbridgeProductLicenseType, "new_licensetypid");
                    Observe(_licenseType, true);
                }
                return _licenseType;
            }
        }

        #endregion

        #region LicenseTypeResolved

        public LicenseType LicenseTypeResolved { get; private set; }

        #endregion

        #region MaintenanceProductKey

        //Cleverbridge Maintenance Product
        //BSP: Citavi 6 for Windows - Home hat
        //Citavi for Windows Maintenance - Home (12 months)
        //als MaintenanceProductKey 
        public string MaintenanceProductKey { get; private set; }

        #endregion

        #region MaxQuantity

        [CrmProperty]
        [DataMember]
        public int? MaxQuantity
        {
            get
            {
                return GetValue<int?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region MonthsValid

        /// <summary>
        /// Laufzeit der Subscription in Monaten
        /// </summary>
        [CrmProperty]
        [DataMember]
        public int? MonthsValid
        {
            get
            {
                return GetValue<int?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Name

        [CrmProperty]
        [DataMember]
        public string Name
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Pricing

        ManyToOneRelationship<CleverbridgeProduct, Pricing> _pricing;
        public ManyToOneRelationship<CleverbridgeProduct, Pricing> Pricing
        {
            get
            {
                if (_pricing == null)
                {
                    _pricing = new ManyToOneRelationship<CleverbridgeProduct, Pricing>(this, CrmRelationshipNames.CleverbridgeProductPricing, "new_pricingid");
                    Observe(_pricing, true);
                }
                return _pricing;
            }
        }

        #endregion

        #region PricingResolved

        public Pricing PricingResolved { get; private set; }

        #endregion

        #region Product

        ManyToOneRelationship<CleverbridgeProduct, Product> _product;
        public ManyToOneRelationship<CleverbridgeProduct, Product> Product
        {
            get
            {
                if (_product == null)
                {
                    _product = new ManyToOneRelationship<CleverbridgeProduct, Product>(this, CrmRelationshipNames.CleverbridgeProductCitaviProduct, "new_citaviproductid");
                    Observe(_product, true);
                }
                return _product;
            }
        }

        #endregion

        #region ProductResolved

        public Product ProductResolved { get; private set; }

        #endregion

        #region ProductGroup

        internal CleverbridgeProduct ProductGroup { get; set; }

        #endregion

        #region ProductId

        [CrmProperty]
        [DataMember]
        public int ProductId
        {
            get
            {
                return GetValue<int>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(CleverbridgeProductPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region ShowInShop

        [CrmProperty]
        [DataMember]
        public bool ShowInShop
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #endregion

        #region DataContract

        #region DataContractGroupProductKey

        [DataMember(Name = "GroupProductKey")]
        public string DataContractGroupProductKey
        {
            get
            {
                return ProductGroup?.Key;
            }
        }

        #endregion

        #region DataContractGroupProductName

        [DataMember(Name = "GroupProductName")]
        public string DataContractGroupProductName
        {
            get
            {
                return ProductGroup?.Name;
            }
        }

        #endregion

        #endregion

        #region Methoden

        #region GetAll

        internal static async Task<List<CleverbridgeProduct>> GetAll()
        {
            var list = new List<CleverbridgeProduct>();

            using (var context = new CrmDbContext())
            {
                var query = new Query.FetchXml.GetCleverbridgeProducts().TransformText();
                var result = await context.Fetch<CleverbridgeProduct>(query);
                var helper = new Dictionary<CleverbridgeProduct, IEnumerable<string>>();
                foreach (var entity in result.GroupBy(i => i.Id))
                {
                    if (entity.Count() > 1)
                    {
                        var first = entity.Where(i => !string.IsNullOrEmpty(i.Name)).First();
                        helper.Add(first as CleverbridgeProduct, entity
                                                                .SelectMany(i => i.Attributes)
                                                                .Where(i => i.Key.StartsWith(CrmRelationshipLookupNames.CleverbridgeUpgrade_To))
                                                                .Select(i => (string)((AliasedValue)i.Value).Value));
                    }
                    else
                    {
                        helper.Add(entity.First() as CleverbridgeProduct, entity
                                                                .SelectMany(i => i.Attributes)
                                                                .Where(i => i.Key.StartsWith(CrmRelationshipLookupNames.CleverbridgeUpgrade_To))
                                                                .Select(i => (string)((AliasedValue)i.Value).Value));
                    }
                }

                foreach (var item in helper)
                {
                    if (item.Value.Any())
                    {
                        foreach (var cbKey in item.Value)
                        {
                            var existing = helper.Keys.FirstOrDefault(i => i.Key == cbKey);
                            item.Key.AllowedUpgradeToProductResolved.Add(existing);
                            existing.AllowedUpgradeFromProductResolved.Add(item.Key);
                        }
                    }
                    var cb = item.Key;
                    var key = cb.GetAliasedValue<string>(CrmRelationshipNames.CleverbridgeProductLicenseType, CrmEntityNames.LicenseType, ContactPropertyId.Key);
                    cb.LicenseTypeResolved = CrmCache.LicenseTypesByKey[key];

                    key = cb.GetAliasedValue<string>(CrmRelationshipNames.CleverbridgeProductPricing, CrmEntityNames.Pricing, ContactPropertyId.Key);
                    cb.PricingResolved = CrmCache.PricingsByKey[key];

                    key = cb.GetAliasedValue<string>(CrmRelationshipNames.CleverbridgeProductCitaviProduct, CrmEntityNames.Product, ContactPropertyId.Key); ;
                    cb.ProductResolved = CrmCache.Products[key];

                    if (cb.BaseCbProductId != Guid.Empty)
                    {
                        cb.ProductGroup = result.FirstOrDefault(i => i.Id == cb.BaseCbProductId);
                        cb.ProductGroup.ProductGroup = cb.ProductGroup; //Produkt-Gruppe auch beim "Basis-Produkt" setzten (sich selber).
                    }

                    if (!cb.ShowInShop)
                    {
                        switch (cb.Edition)
                        {
                            case ShopProductEdition.Cloudspace:
                            case ShopProductEdition.Web:
                            case ShopProductEdition.Maintenance:
                                cb.ShowInShop = CrmConfig.IsShopWebAppSubscriptionAvailable;
                                break;
                        }
                    }

                }

                foreach (var item in helper)
                {
                    var cb = item.Key;

                    if (cb.Edition == ShopProductEdition.Maintenance && cb.ShowInShop)
                    {
                        Product citaviProduct = null;

                        foreach (var product in helper)
                        {
                            if (product.Key.IsGiftCard) continue;

                            if(product.Key.ProductResolved.Key == citaviProduct.Key)
                            {
                                if (cb.PricingResolved.Key == product.Key.PricingResolved.Key)
                                {
                                    product.Key.MaintenanceProductKey = cb.Key;
                                }
                            }
                        }
                    }

                    list.Add(item.Key);
                }

                //Alle Produkte welche keine Produkt-Gruppe haben
                //z.B. Citavi 5 DBServer Reader 1000
                //Damit wir konsistent bleiben, auch hier Produkt-Gruppe "sich selber" setzen
                foreach (var item in list.Where(i => i.ProductGroup == null && i.ShowInShop))
                {
                    item.ProductGroup = item;
                }
            }
            return list;
        }

        #endregion

        #region IsGiftCardProduct

        public static bool IsGiftCardProduct(string cbProductId)
        {
            return (cbProductId == GiftCardProductIdC5.ToString() ||
                    cbProductId == GiftCardProductIdC6.ToString());
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return Name;
        }

        #endregion

        #endregion
    }
}
