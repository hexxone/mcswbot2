﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace mcswbot2.Minecraft
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
        public List<ServerInfoBasic> History { get; set; }


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
            History = new List<ServerInfoBasic>();
        }

        [JsonConstructor]
        public ServerStatusWrapped(string label, string address, int port, bool notifyServer, bool notifyCount, bool notifyNames, bool sticker,
            Dictionary<string, string> nameHistory, Dictionary<string, TimeSpan> playTime, Dictionary<string, DateTime> seenTime, List<ServerInfoBasic> history)
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
            foreach (var hk in History.Where(hk => hk.RequestDate < DateTime.Now - TimeSpan.FromHours(MCSWBot.Conf.HistoryHours)))
            {
                History.Remove(hk);
            }

            // Quantize, I don't even know...
            var qThreshold = MCSWBot.Conf.QThreshold;
            var qRatio = MCSWBot.Conf.QRatio;

            var quInd = 0;
            while (History.Count > qThreshold)
            {
                var search = History.Where(h => h.QLevel == quInd).OrderBy(h => h.RequestDate);
                if (search.Count() > qRatio * 2)
                {
                    var counter = 0;
                    double date = 0;
                    double time = 0;
                    double online = 0;
                    foreach (var sib in search)
                    {
                        if (counter++ >= qRatio) break;
                        date += (sib.RequestDate.Ticks / qRatio);
                        time += sib.RequestTime / qRatio;
                        online += sib.CurrentPlayerCount / qRatio;
                        History.Remove(sib);
                    }
                    History.Add(new ServerInfoBasic(true, new DateTime((long)date), time, online, quInd + 1));
                }
                else
                {
                    quInd++;
                }
            }
        }
    }
}