using Newtonsoft.Json;

namespace mcswbot2.Lib.Payload
{
    internal class ExtraPayLoad
    {
        [JsonProperty(PropertyName = "color")]
        public string TextColor { get; set; }

        [JsonProperty(PropertyName = "strikethrough")]
        public bool StrikeThrough { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}