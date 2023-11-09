using System.Collections.Generic;

namespace Occtoo.Formatter.Newstore.Models
{
    public class OcctooProductVariant : IOcctooBase
    {
        public class Constants
        {
            public const string ArigatoOnline = "Arigato Online";
            public const string Newstore = "NewStore";
            public const string TwentyFour_24S = "24S";
            public const string Miinto = "Miinto";
        }

        public string Color { get; set; }

        /// <summary>
        /// This is the SKU-ID. With the size code.
        /// </summary>
        public string Id { get; set; }

        public string[] ColorFilter { get; set; }

        public string SizeCn { get; set; }

        public string SizeEu { get; set; }

        public string SizeUk { get; set; }

        public string SizeUsMen { get; set; }

        public string SizeUsWomen { get; set; }

        public string CountryOfManufacture { get; set; }

        public string[] CountryOfManufactureKey { get; set; }

        public string Description { get; set; }

        public string DetailsList { get; set; }

        public string Division { get; set; }

        public string ProductGender { get; set; }

        public string HsCode { get; set; }

        public string InternalName { get; set; }

        public string Category { get; set; }

        public string ItemCategoryKey { get; set; }

        public string Marketplace24Category { get; set; }

        public string Marketplace24Collection { get; set; }

        public string Marketplace24Family { get; set; }

        public string Marketplace24Fitting { get; set; }

        public string Marketplace24GenericColor { get; set; }

        public double? Marketplace24HeelSize { get; set; }

        public string Marketplace24Mid { get; set; }

        public string Marketplace24Composition { get; set; }

        public string Marketplace24Status { get; set; }

        public string Marketplace24Season { get; set; }

        public string Marketplace24SubCategory { get; set; }

        public int? Marketplace24Year { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// This is the product number, shared for all skus.
        /// </summary>
        public string Number { get; set; }

        public string Tags { get; set; }

        public string SizeGuide { get; set; }

        public double? SustPeopleScoreOutOf10 { get; set; }

        public double? SustPlanetScoreOutOf10 { get; set; }

        public string SustSavingsPercentKgCo2Eq { get; set; }

        public string SustSavingsPercentLh2O { get; set; }

        public string SustTraceabilityFactsNrPeople { get; set; }

        public string SustTraceabilityFactsNrProcess { get; set; }

        public string SustTransparencyMaterialCertificates { get; set; }

        public string SustTransparencyMaterialOrigin1 { get; set; }

        public string SustTransparencyMaterialOrigin2 { get; set; }

        public string SustTransparencyMaterialOrigin3 { get; set; }

        public string SustTransparencyPackagingOrigin { get; set; }

        public double? SustTransparencyScoreOutOf10 { get; set; }

        public double? SustUnitImpactKgCo2Eq { get; set; }

        public double? SustUnitImpactKgWaste { get; set; }

        public double? SustUnitImpactLh2O { get; set; }

        public double? Weight { get; set; }

        public string UrlKey { get; set; }

        public string Barcode { get; set; }

        public string Type { get; set; }

        public string TypeKey { get; set; }

        public bool? GiftWrap { get; set; }

        public string DateCreated { get; set; }

        public string LastModified { get; set; }

        public string CompleteUrl { get; set; }

        public string[] ShowOnTouchPoints { get; set; }

        public string[] ProductNotApprovedCountriesKey { get; set; }

        public string MadeInKey { get; set; }

        public string ProductComposition { get; set; }

        public Resource[] Media { get; set; }

        public string ProductGenderKey { get; set; }

        public string ProductSizeChartKey { get; set; }

        public string[] ProductNavigationCategories { get; set; }

        public string[] ProductNavigationCategoriesKey { get; set; }

        public string ProductFitPredictor { get; set; }

        public string ProductSwitchForMenWomen { get; set; }

        public string ProductDropDateTimeKey { get; set; }

        public class Resource
        {
            public string Name { get; set; }

            public string[] Type { get; set; }

            public string Url { get; set; }

            public string ResourceFileName { get; set; }

            public int? Position { get; set; }
        }
    }

    public class ProductVariantComparer : IEqualityComparer<OcctooProductVariant>
    {
        public bool Equals(OcctooProductVariant left, OcctooProductVariant right)
        {
            if ((object)left == null && (object)right == null)
            {
                return true;
            }
            if ((object)left == null || (object)right == null)
            {
                return false;
            }
            return left.Id == right.Id;
        }

        public int GetHashCode(OcctooProductVariant product)
        {
            return (product.Id).GetHashCode();
        }
    }
}
