using System;
using Newtonsoft.Json;

namespace mcswbot2.Bot.Objects
{
    [Serializable]
    public class ServerInfoWrapped
    {
        public ServerInfoBasic Wrapped { get; set; }

        public int QLevel { get; set; }

        [JsonConstructor]
        public ServerInfoWrapped(ServerInfoBasic wrapped, int qLevel)
        {
            Wrapped = wrapped;
            QLevel = qLevel;
        }

        public ServerInfoWrapped(long date, double time, double count, int qLevel)
        {
            Wrapped = new ServerInfoBasic()
            {
                HadSuccess = false,
                RequestDate = new DateTime(date),
                RequestTime = time,
                CurrentPlayerCount = count,
            };
            QLevel = qLevel;
        }
    }
}
