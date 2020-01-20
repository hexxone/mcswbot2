using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace mcswbot2.Lib.Payload
{
    internal class ModItemPayLoad
    {
        [JsonProperty(PropertyName = "modid")]
        public string ModId { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }
    }
}
