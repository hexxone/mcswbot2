using Newtonsoft.Json;

namespace mcswbot2.Lib.Payload
{
    public sealed class PlayerPayLoad
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}