using Newtonsoft.Json;

namespace Occtoo.Formatter.Newstore.Models
{
    public class JobImport
    {
        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("source_uri")]
        public string SourceUri { get; set; }

        [JsonProperty("entities")]
        public string[] Entities { get; set; }

        [JsonProperty("full")]
        public bool Full { get; set; }

        [JsonProperty("shop")]
        public string Shop { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }
    }
}
