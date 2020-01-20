using System.Collections.Generic;
using Newtonsoft.Json;

namespace mcswbot2.Lib.Payload
{
    internal class PlayerListPayLoad
    {
        [JsonProperty(PropertyName = "max")]
        public int Max { get; set; }

        [JsonProperty(PropertyName = "online")]
        public int Online { get; set; }

        [JsonProperty(PropertyName = "sample")]
        public List<PlayerPayLoad> Sample { get; set; }
    }
}