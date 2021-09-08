using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    [DataContract]
    public class ShopView
    {
        #region Eigenschaften

        [DataMember]
        public string Country { get; set; }

        [DataMember]
        public string Currency { get; set; }

        [DataMember]
        public List<ProductShopItem> Products { get; private set; } = new List<ProductShopItem>();

        [DataMember]
        public List<ProductShopItem> UserProducts { get; private set; } = new List<ProductShopItem>();

        [DataMember]
        public List<ProductShopItem> Upgrades { get; private set; } = new List<ProductShopItem>();

        #region Vat rate

        [DataMember]
        public double VatRate { get; set; }

        #endregion

        #region Vat rate localized

        [DataMember]
        public string VatRateLocalized { get; set; }

        #endregion

        #endregion

        #region Methoden

        public void CalculateDiscounts(ProductPriceResponse p)
        {
            foreach (var productShopItem in Products.Concat(UserProducts).Concat(Upgrades))
            {
                var price = p.Products.Where(i => i.Id == productShopItem.CleverbridgeProductId).FirstOrDefault();
                if (price == null)
                {
                    continue;
                }

                if (price.ProductPrices.Count == 0)
                {
                    continue;
                }

                productShopItem.CalculateDiscounts(price.ProductPrices);
            }
        }

        #endregion
    }
}
