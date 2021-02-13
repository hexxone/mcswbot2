using mcswlib.ServerStatus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace mcswbot2.Bot.Objects
{
    public class ServerStatusWrapped
    {
        [JsonIgnore]
        public ServerStatus Wrapped { get; }

        public string Label { get; }

        public string Address { get; }

        public int Port { get; }

        public bool Sticker { get; set;  }

        // <UID>:<NAME>
        // SAVE PLAYER NAMES -> NOTICE / ANNOUNCE RENAMES ?
        public Dictionary<string,string> NameHistory { get; }

        // <UID>:<MINUTES_ONLINE>
        // OVERALL ON-TIME, GETS ADDED ON LEAVE
        public Dictionary<string,TimeSpan> PlayTime { get; }

        // <UID>:<TIMESTAMP>
        // if user currently online, STAMP = JOIN TIME -> PLAY TIME
        // if user offline, STAMP = LEAVE TIME -> OFFLINE TIME
        public Dictionary<string,DateTime> SeenTime { get; }

        internal ServerStatusWrapped(ServerStatus wrap)
        {
            Wrapped = wrap;
            Label = wrap.Label;
            Address = wrap.Updater.Address;
            Port = wrap.Updater.Port;
            Sticker = true;
            NameHistory = new Dictionary<string, string>();
            PlayTime = new Dictionary<string, TimeSpan>();
            SeenTime = new Dictionary<string, DateTime>();
        }

        [JsonConstructor]
        public ServerStatusWrapped(string label, string address, int port, Dictionary<string, string> nameHistory, Dictionary<string, TimeSpan> playTime, Dictionary<string, DateTime> seenTime)
        {
            Label = label;
            Address = address;
            Port = port;
            NameHistory = nameHistory;
            PlayTime = playTime;
            SeenTime = seenTime;
        }
    }
}
