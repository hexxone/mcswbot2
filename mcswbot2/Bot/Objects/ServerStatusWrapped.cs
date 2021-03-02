using mcswlib.ServerStatus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mcswbot2.Bot.Objects
{
    [Serializable]
    public class ServerStatusWrapped
    {
        [JsonIgnore]
        public ServerStatus Wrapped { get; set; }

        // Settings

        public string Label { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

        public bool NotifyServer { get; set; }
        public bool NotifyCount { get; set; }
        public bool NotifyNames { get; set; }
        public bool Sticker { get; set; }

        // Life-Time

        // <UID>:<NAME>
        // SAVE PLAYER NAMES -> NOTICE / ANNOUNCE RENAMES ?
        public Dictionary<string, string> NameHistory { get; set; }

        // <UID>:<MINUTES_ONLINE>
        // OVERALL ON-TIME, GETS ADDED ON LEAVE
        public Dictionary<string, TimeSpan> PlayTime { get; set; }

        // <UID>:<TIMESTAMP>
        // if user currently online, STAMP = JOIN TIME -> PLAY TIME
        // if user offline, STAMP = LEAVE TIME -> OFFLINE TIME
        public Dictionary<string, DateTime> SeenTime { get; set; }

        // List of past received Server Infos
        public List<ServerInfoWrapped> History { get; set; }


        internal ServerStatusWrapped(ServerStatus wrap)
        {
            Wrapped = wrap;
            Label = wrap.Label;
            Address = wrap.Updater.Address;
            Port = wrap.Updater.Port;
            // default notify settings
            NotifyServer = true;
            NotifyCount = true;
            NotifyNames = true;
            Sticker = true;
            // default objects
            NameHistory = new Dictionary<string, string>();
            PlayTime = new Dictionary<string, TimeSpan>();
            SeenTime = new Dictionary<string, DateTime>();
            History = new List<ServerInfoWrapped>();
        }

        [JsonConstructor]
        public ServerStatusWrapped(string label, string address, int port, bool notifyServer, bool notifyCount, bool notifyNames, bool sticker,
            Dictionary<string, string> nameHistory, Dictionary<string, TimeSpan> playTime, Dictionary<string, DateTime> seenTime, List<ServerInfoWrapped> history)
        {
            Label = label;
            Address = address;
            Port = port;

            NotifyServer = notifyServer;
            NotifyCount = notifyCount;
            NotifyNames = notifyNames;
            Sticker = sticker;

            NameHistory = nameHistory;
            PlayTime = playTime;
            SeenTime = seenTime;
            History = history;
        }

        /// <summary>
        ///     Remove all objects of which the Timestamp exceeds the ClearSpan and run GC.
        /// </summary>
        ///

        public void CleanData()
        {
            // Remove very old data
            foreach (var hk in History.Where(hk => hk.Wrapped.RequestDate < DateTime.Now - TimeSpan.FromHours(TgBot.Conf.HistoryHours)))
            {
                History.Remove(hk);
            }

            // Quantize, I don't even know...
            var qThreshold = TgBot.Conf.QThreshold;
            var qRatio = TgBot.Conf.QRatio;

            var quInd = 0;
            while (History.Count > qThreshold)
            {
                var search = History.Where(h => h.QLevel == quInd).OrderBy(h => h.Wrapped.RequestDate);
                if (search.Count() > qRatio * 2)
                {
                    var counter = 0;
                    double date = 0;
                    double time = 0;
                    double online = 0;
                    foreach (var siw in search)
                    {
                        var sib = siw.Wrapped;
                        if (counter++ >= qRatio) break;
                        date += (sib.RequestDate.Ticks / qRatio);
                        time += sib.RequestTime / qRatio;
                        online += sib.CurrentPlayerCount / qRatio;
                        History.Remove(siw);
                    }
                    History.Add(new ServerInfoWrapped((long)date, time, online, quInd + 1));
                }
                else
                {
                    quInd++;
                }
            }
        }
    }
}
