namespace Occtoo.Formatter.Newstore.Models
{
    public class OcctooPrice : IOcctooBase
    {
        public string Id { get; set; }

        public string ConditionUnitCurrencies { get; set; }

        public double? AmountOfPercentageCondition { get; set; }

        public string ValidToDate { get; set; }

        public string ValidFromDate { get; set; }

        public string MaterialNumber { get; set; }

        public string PriceListType { get; set; }

        public string ConditionType { get; set; }

    }
}
