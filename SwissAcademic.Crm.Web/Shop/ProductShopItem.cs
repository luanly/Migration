using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    [DataContract]
    public class ProductShopItem
    {
        #region Felder

        internal List<string> _upgradeToProducts = new List<string>();
        internal List<string> _upgradeFromProducts = new List<string>();

        #endregion

        #region Eigenschaften

        #region CleverbridgeProductId

        [DataMember]
        internal int? CleverbridgeProductId => CleverbridgeProduct?.ProductId;

        #endregion

        #region CleverbridgeProduct

        internal CleverbridgeProduct CleverbridgeProduct { get; set; }

        #endregion

        #region Discounts

        public List<DiscountInfo> Discounts { get; } = new List<DiscountInfo>();

        [DataMember]
        public string DiscountsString => JsonConvert.SerializeObject(Discounts);

        #endregion

        #region Edition

        [DataMember]
        public ShopProductEdition Edition => CleverbridgeProduct.Edition;

        #endregion

        #region IndexForSorting

        [DataMember]
        public int IndexForSorting { get; set; }

        #endregion

        #region Key

        [DataMember]
        public string Key { get; set; }

        #endregion

        #region Name

        [DataMember]
        public string Name { get; set; }

        #endregion

        #region MaintenanceProductKey

        [DataMember]
        public string MaintenanceProductKey => CleverbridgeProduct?.MaintenanceProductKey;

        #endregion

        #region MaxQuantity

        /// <summary>
        /// Maximale Anzahl die "normal" gekauft werden kann
        /// Wird bei Produkt-Ansicht benötigt
        /// </summary>
        [DataMember]
        public int MaxQuantity { get; set; } = -1;

        #endregion

        #region Pricing

        [DataMember]
        public string Pricing { get; set; }

        #endregion

        #region Price

        [DataMember]
        public double Price { get; set; }

        #endregion

        #region ProductGroupKey

        /// <summary>
        /// Produkt-Gruppe. Bsp: 
        /// Citavi 5 for DBServer SEAT - Academic (Upgrade Citavi 3)
        /// Citavi 5 for DBServer SEAT - Academic (Upgrade Citavi 4)
        /// Citavi 5 for DBServer SEAT - Academic (Upgrade Citavi 5)
        /// Citavi 5 for DBServer SEAT - Academic
        /// Alle haben als GroupProductKey den ProductKey von "Citavi 5 for DBServer SEAT - Academic"
        /// </summary>
        [DataMember]
        public string ProductGroupKey => CleverbridgeProduct?.DataContractGroupProductKey;

        #endregion

        #region ProductGroupName

        /// <summary>
        /// Produkt-Gruppe. Bsp: 
        /// Citavi 5 for DBServer SEAT - Academic (Upgrade Citavi 3)
        /// Citavi 5 for DBServer SEAT - Academic (Upgrade Citavi 4)
        /// Citavi 5 for DBServer SEAT - Academic (Upgrade Citavi 5)
        /// Citavi 5 for DBServer SEAT - Academic
        /// Alle haben als GroupProductKey den ProductKey von "Citavi 5 for DBServer SEAT - Academic"
        /// </summary>
        [DataMember]
        public string ProductGroupName => CleverbridgeProduct?.DataContractGroupProductName;

        #endregion

        #region ProductGroupQuantity

        /// <summary>
        /// Produkt-Gruppe. Bsp: 
        /// Citavi 5 for DBServer SEAT - Academic (Upgrade Citavi 3)
        /// Citavi 5 for DBServer SEAT - Academic (Upgrade Citavi 4)
        /// Citavi 5 for DBServer SEAT - Academic (Upgrade Citavi 5)
        /// Citavi 5 for DBServer SEAT - Academic
        /// Summe von ALLEN gekauften Produkten in diesen Gruppe
        /// Wird für die BaseQuantity bei Kauf benötigt (Rabattstufen)
        /// </summary>
        [DataMember]
        public int ProductGroupQuantity { get; set; }

        #endregion

        #region ProductUpgradeMaxQuantity

        /// <summary>
        /// Maximale Anzahle Upgrades
        /// </summary>
        [DataMember]
        public int ProductUpgradeMaxQuantity { get; set; }

        #endregion

        #region ShowInShop

        [DataMember]
        public bool ShowInShop { get; set; }

        #endregion

        #region Quantity

        /// <summary>
        /// Bereits gekaufte Produkte
        /// </summary>
        [DataMember]
        public int Quantity { get; set; }

        #endregion

        #region UpgradeFromProductKeys

        [DataMember]
        public string UpgradeFromProductKeys
        {
            get
            {
                if (_upgradeFromProducts.Any())
                {
                    return _upgradeFromProducts.ToString(";");
                }

                return null;
            }
        }

        #endregion

        #region UpgradeToProductKeys

        [DataMember]
        public string UpgradeToProductKeys
        {
            get
            {
                if (_upgradeToProducts.Any())
                {
                    return _upgradeToProducts.ToString(";");
                }

                return null;
            }
        }

        #endregion

        #region Validity

        /// <summary>
        /// Laufzeit der Subscription in Monaten
        /// </summary>
        [DataMember]
        public int? Validity => CleverbridgeProduct.MonthsValid;

        #endregion

        #endregion

        #region Methoden

        #region CalculateDiscounts

        internal void CalculateDiscounts(List<ProductPrice> prices)
        {
            prices.Sort((a, b) => a.Quantity.CompareTo(b.Quantity));
            var singlePrice = prices.Where(i => i.Quantity == 1).First();
            for (var i = 1; i < prices.Count; i++)
            {
                var price = prices[i];
                var discountInfo = new DiscountInfo();
                discountInfo.Discount = Math.Round(100 / (singlePrice.SinglePriceGross / (singlePrice.SinglePriceGross - price.SinglePriceGross)), 1);
                discountInfo.From = prices[i].Quantity;
                if (i < prices.Count - 1)
                {
                    discountInfo.To = prices[i + 1].Quantity - 1;
                }
                Discounts.Add(discountInfo);
            }
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

    public class DiscountInfo
    {
        public double Discount { get; set; }
        public int From { get; set; }
        public int To { get; set; } = -1;

        public override string ToString()
        {
            if (To == -1)
            {
                return $"{From}+: {Discount} %";
            }

            return $"{From} - {To}: {Discount} %";
        }
    }
}
