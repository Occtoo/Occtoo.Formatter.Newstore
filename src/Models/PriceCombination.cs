namespace Occtoo.Formatter.Newstore.Models
{
    public class PriceCombination
    {
        public string PriceList { get; set; }
        public string Currency { get; set; }

        /// <summary>
        /// Regular price straight from SAP
        /// </summary>
        public double RegularPrice { get; set; }

        /// <summary>
        /// Promotion price, if any, straight from SAP
        /// </summary>
        public double? PromotionPrice { get; set; }

        /// <summary>
        /// Set to active promotion price if one is active, otherwise will be regular price
        /// </summary>
        public double SalesPrice { get; set; }

        /// <summary>
        /// Calculated based on regular price - promotion price
        /// </summary>
        public double Discount { get; set; }

        public OcctooPrice RegularPriceSource { get; set; }
        public OcctooPrice PromotionPriceSource { get; set; }
    }
}
