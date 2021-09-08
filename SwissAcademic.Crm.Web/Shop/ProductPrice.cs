using System.Collections.Generic;

namespace SwissAcademic.Crm.Web
{

    public class ProductPrice
    {
        public string CurrencyId { get; set; }
        public int Quantity { get; set; }
        public double SinglePriceNet { get; set; }
        public double SinglePriceVat { get; set; }
        public double SinglePriceGross { get; set; }
        public double TotalPriceNet { get; set; }
        public double TotalPriceVat { get; set; }
        public double TotalPriceGross { get; set; }
    }

    public class ProductPriceInfo
    {
        public int Id { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
        public List<ProductPrice> ProductPrices { get; set; }
    }

    public class ProductPriceResponse
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
        public List<ProductPriceInfo> Products { get; set; }
        public string ResultMessage { get; set; }
    }
}
