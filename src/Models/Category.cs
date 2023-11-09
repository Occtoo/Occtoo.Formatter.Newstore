using Newtonsoft.Json;
using System.Collections.Generic;

namespace Occtoo.Formatter.Newstore.Models
{
    public class Head
    {
        [JsonProperty("shop")]
        public string Shop;

        [JsonProperty("locale")]
        public string Locale;

        [JsonProperty("is_master")]
        public bool IsMaster;
    }

    public class ItemCategory
    {
        [JsonProperty("path")]
        public string Path;

        [JsonProperty("description")]
        public string Description;
    }

    public class Categories
    {
        [JsonProperty("head")]
        public Head Head;

        [JsonProperty("items")]
        public List<ItemCategory> Items;
    }
}
