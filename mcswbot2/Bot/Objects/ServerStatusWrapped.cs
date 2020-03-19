using mcswlib.ServerStatus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace mcswbot2.Bot.Objects
{
    public class ServerStatusWrapped
    {
        [JsonIgnore]
        public ServerStatus Wrapped { get; private set; }

        public string Label { get; private set; }

        public string Address { get; private set; }

        public int Port { get; private set; }

        public bool Sticker = true;

        internal ServerStatusWrapped(ServerStatus wrap)
        {
            Wrapped = wrap;
            Label = wrap.Label;
            Address = wrap.Updater.Address;
            Port = wrap.Updater.Port;
        }

        [JsonConstructor]
        public ServerStatusWrapped(string label, string address, int port)
        {
            Label = label;
            Address = address;
            Port = port;
        }
    }
}
