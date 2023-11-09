using Newtonsoft.Json;

namespace Occtoo.Formatter.Newstore.Models
{
    public interface IOcctooBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
