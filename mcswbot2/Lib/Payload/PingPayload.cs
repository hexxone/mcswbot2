using Newtonsoft.Json;

namespace mcswbot2.Lib.Payload
{
    /// <summary>
    ///     C# represenation of the ping result packet
    /// </summary>
    internal class PingPayLoad
    {
        /// <summary>
        ///     Protocol that the server is using and the given name
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public VersionPayLoad Version { get; set; }

        [JsonProperty(PropertyName = "players")]
        public PlayerListPayLoad Players { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Motd { get; set; }

        /// <summary>
        ///     Server icon, important to note that it's encoded in base 64
        /// </summary>
        [JsonProperty(PropertyName = "favicon")]
        public string Icon { get; set; }
    }
}