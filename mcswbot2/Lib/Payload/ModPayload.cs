using System.Collections.Generic;
using Newtonsoft.Json;

namespace mcswbot2.Lib.Payload
{
    internal class ModPayLoad
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "modList")]
        public List<ModItemPayLoad> ModList { get; set; }
    }
}