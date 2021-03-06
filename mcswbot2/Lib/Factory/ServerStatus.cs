﻿using mcswbot2.Lib.Event;
using mcswbot2.Lib.ServerInfo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static mcswbot2.Lib.Types;

namespace mcswbot2.Lib.Factory
{
    internal class ServerStatus
    {
        public ServerStatus()
        {
            NotifyServer = true;
            NotifyCount = true;
            NotifyNames = true;
            PlayerList = new List<PlayerPayLoad>();
            ApplyServerInfo(null);
        }

        // Identity

        public string Label { get; set; }
        public ServerStatusBase Base { get; set; }

        // Settings

        public bool NotifyServer { get; set; }
        public bool NotifyCount { get; set; }
        public bool NotifyNames { get; set; }

        // Current / Last Status

        private ServerInfoBase last = null;

        /// <summary>
        ///     Include a list of Names or UID's of Minecraft-Users.
        ///     When they join or leave, the PlayerStateChangedEvent will be triggerd.
        ///     NOTE; Only new Servers(1.11+) support this feature! Also, large Servers
        ///     don't usually return the actual/complete player list. Hence, this may
        ///     not work for some cases.
        /// </summary>
        private readonly Dictionary<string, string> userNames = new Dictionary<string, string>();
        private readonly Dictionary<string, bool> userStates = new Dictionary<string, bool>();

        [JsonIgnore]
        public List<PlayerPayLoad> PlayerList { get; private set; }
        [JsonIgnore]
        public string LastStatusDate { get; private set; }
        [JsonIgnore]
        public bool IsOnline { get; private set; }
        [JsonIgnore]
        public int PlayerCount { get; private set; }
        [JsonIgnore]
        public int MaxPlayerCount { get; private set; }
        [JsonIgnore]
        public string Version { get; private set; }
        [JsonIgnore]
        public string MOTD { get; private set; }
        [JsonIgnore]
        public string LastError { get; private set; }


        /// <summary>
        ///     returns all the time-plottable online player count data
        /// </summary>
        /// <returns></returns>
        public PlottableData GetUserData()
        {
            var nauw = DateTime.Now;
            var lx = new List<double>();
            var ly = new List<double>();
            foreach (var infoBase in Base.History)
            {
                var diff = nauw.Subtract(infoBase.RequestDate.AddMilliseconds(infoBase.RequestTime)).TotalMinutes;
                lx.Add(diff);
                ly.Add(infoBase.CurrentPlayerCount);
            }
            return new PlottableData(Label, lx.ToArray(), ly.ToArray());
        }

        /// <summary>
        ///     returns all the time-plottable ping data
        /// </summary>
        /// <returns></returns>
        public PlottableData GetPingData()
        {
            var nauw = DateTime.Now;
            var lx = new List<double>();
            var ly = new List<double>();
            foreach (var infoBase in Base.History)
            {
                var diff = nauw.Subtract(infoBase.RequestDate.AddMilliseconds(infoBase.RequestTime)).TotalMinutes;
                lx.Add(diff);
                ly.Add(infoBase.RequestTime);
            }
            return new PlottableData(Label, lx.ToArray(), ly.ToArray());
        }

        /// <summary>
        ///     Will compare the last status with the current one and return event updates.
        /// </summary>
        /// <returns></returns>
        public EventBase[] Update()
        {
            // event-queue
            var events = new List<EventBase>();
            var isFirst = last == null;
            var current = Base.GetLatestServerInfo();
            if (current != null)
            {
                // if first info, or last success was different from this (either went online or went offline) => invoke
                if (NotifyServer && (isFirst || last.HadSuccess != current.HadSuccess))
                {
                    Debug.WriteLine("Server '" + Base.Address + ":" + Base.Port + "' status change: " + current.HadSuccess);
                    var errMsg = current.LastError != null ? "Connection Failed: " + current.LastError.GetType().Name : "";
                    events.Add(new OnlineStatusEvent(current.HadSuccess, current.HadSuccess ? current.ServerMotd : errMsg));
                }

                // if first info, or last player count was different (player went online or offline) => invoke
                if (NotifyCount)
                {
                    var diff = isFirst
                        ? current.CurrentPlayerCount
                        : current.CurrentPlayerCount - last.CurrentPlayerCount;
                    if (diff != 0)
                    {
                        Debug.WriteLine("Server '" + Base.Address + ":" + Base.Port + "' count change: " + diff);
                        events.Add(new PlayerChangeEvent(diff));
                    }
                }

                // check current list for new players 
                var onlineIds = new List<string>();
                if (current.OnlinePlayers != null)
                    foreach (var p in current.OnlinePlayers)
                    {
                        // save online user id temporarily
                        if (!onlineIds.Contains(p.Id))
                            onlineIds.Add(p.Id);
                        // register name
                        userNames[p.Id] = p.Name;
                        // if notify and user has state and last state was offline and user is watched, notify change
                        if (NotifyNames && (!userStates.ContainsKey(p.Id) || !userStates[p.Id]))
                            events.Add(new PlayerStateEvent(p, true));
                        // register state or set to true
                        userStates[p.Id] = true;
                    }

                // this needs to be done to avoid ElementChangedException
                var keys = userStates.Keys.ToArray();
                // check all states for players who went offline
                foreach (var k in keys)
                {
                    if (!userStates[k] || onlineIds.Contains(k)) continue;
                    // if user state still true, but he is not in online list => went offline
                    userStates[k] = false;
                    // create payload
                    var p = new PlayerPayLoad { Id = k, RawName = userNames[k] };
                    // notify => invoke
                    if (NotifyNames)
                        events.Add(new PlayerStateEvent(p, false));

                }
            }
            // set new last
            last = current;
            ApplyServerInfo(current);
            return events.ToArray();
        }


        /// <summary>
        ///     Will apply the current server-info to the public vars
        /// </summary>
        /// <param name="si"></param>
        private void ApplyServerInfo(ServerInfoBase si)
        {
            var nu = si == null;

            LastStatusDate = nu ? "-" : si.RequestDate.AddMilliseconds(si.RequestTime).ToString("HH:mm:ss");
            IsOnline = !nu && si.HadSuccess;
            PlayerCount = nu ? 0 : si.CurrentPlayerCount;
            MaxPlayerCount = nu ? 0 : si.MaxPlayerCount;
            Version = nu ? "0.0.0" : si.MinecraftVersion;
            MOTD = nu || !si.HadSuccess ? "-" : si.ServerMotd;
            LastError = !nu && si.LastError != null ? si.LastError.GetType().Name : "-";

            PlayerList.Clear();
            if (!nu && si.OnlinePlayers != null) PlayerList.AddRange(si.OnlinePlayers);
        }
    }
}
