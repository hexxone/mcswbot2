using Newtonsoft.Json;

namespace mcswbot2.Lib.Payload
{
    internal class VersionPayLoad
    {
        [JsonProperty(PropertyName = "protocol")]
        public int Protocol { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}