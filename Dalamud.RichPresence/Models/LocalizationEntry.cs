using Newtonsoft.Json;

namespace Dalamud.RichPresence.Models
{
    internal class LocalizationEntry
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
