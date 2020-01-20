using System.Collections.Generic;
using Newtonsoft.Json;

namespace mcswbot2.Lib.Payload
{
    internal class DescriptionPayLoad
    {
        [JsonProperty(PropertyName = "extra")]
        public List<ExtraPayLoad> Extras { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        public string ToSimpleString()
        {
            var str = Text ?? "";
            if (Extras == null) return str;
            foreach (var pl in Extras)
                if (pl != null && !string.IsNullOrEmpty(pl.Text))
                    str += pl.Text;
            return str;
        }
    }
}