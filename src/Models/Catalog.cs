using Newtonsoft.Json;
using System.Collections.Generic;

namespace Occtoo.Formatter.Newstore.Models
{
    public class Catalog
    {
        [JsonProperty("head")]
        public CatalogHead Head { get; set; }

        [JsonProperty("items")]
        public List<CatalogItem> Items { get; set; }
    }

    public class CatalogHead
    {
        [JsonProperty("shop")]
        public string Shop { get; set; }

        [JsonProperty("catalog")]
        public string Catalog { get; set; }

        [JsonProperty("shop_display_name")]
        public string ShopDisplayName { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("internal_disable_image_processing")]
        public bool InternalDisableImageProcessing { get; set; }

        [JsonProperty("is_master")]
        public bool IsMaster { get; set; }

        [JsonProperty("filterable_attributes")]
        public SrchFlterAbleAttributes[] FilterableAttributes { get; set; }

        [JsonProperty("searchable_attributes")]
        public SrchFlterAbleAttributes[] SearchableAttributes { get; set; }
    }

    public class SrchFlterAbleAttributes
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }

    public class CatalogItem
    {
        [JsonProperty("product_id")]
        public string ProductId { get; set; }

        [JsonProperty("variant_group_id")]
        public string VariantGroupId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("show_in_listing")]
        public bool ShowInListing { get; set; }

        [JsonProperty("images")]
        public List<Image> Images { get; set; }

        [JsonProperty("tax_class_id")]
        public string TaxClassId { get; set; }

        [JsonProperty("categories")]
        public List<Category> Categories { get; set; }

        [JsonProperty("shipping_weight_unit")]
        public string ShippingWeightUnit { get; set; }

        [JsonProperty("variation_color_value")]
        public string VariationColorValue { get; set; }

        [JsonProperty("variation_size_value")]
        public string VariationSizeValue { get; set; }

        [JsonProperty("variation_size_gender")]
        public string VariationSizeGender { get; set; }

        [JsonProperty("external_identifiers")]
        public List<ExternalIdentifiers> ExternalIdentifiers { get; set; }

        [JsonProperty("extended_attributes")]
        public List<ExtendedAttributes> ExtendedAttributes { get; set; }
    }

    public class Image
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("is_main")]
        public bool IsMain { get; set; }
    }

    public class Category
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("is_main")]
        public bool IsMain { get; set; }
    }

    public class ExternalIdentifiers
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class ExtendedAttributes
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
