using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using mcswbot2.Lib.Event;
using mcswbot2.Lib.Payload;
using mcswbot2.Lib.ServerInfo;
using static mcswbot2.Lib.Types;

namespace mcswbot2.Lib
{
    public class ServerStatus
    {
        /// <summary>
        ///     Change Event Handler
        /// </summary>
        /// <param name="self"></param>
        /// <param name="newInfo"></param>
        /// <param name="events"></param>
        public delegate void ServerChange(ServerStatus self, ServerInfoBase newInfo, EventBase[] events);

        private static readonly TimeSpan ClearSpan = new TimeSpan(0, 10, 0);

        // TODO visualize
        // will contain the received Server-Infos.
        private readonly List<ServerInfoBase> _infoList = new List<ServerInfoBase>();

        /// <summary>
        ///     Include a list of Names or UID's of Minecraft-Users.
        ///     When they join or leave, the PlayerStateChangedEvent will be triggerd.
        ///     NOTE; Only new Servers(1.11+) support this feature! Also, large Servers
        ///     don't usually return the actual/complete player list. Hence, this may
        ///     not work for some cases.
        /// </summary>
        private readonly Dictionary<string, string> userNames = new Dictionary<string, string>();

        private readonly Dictionary<string, bool> userStates = new Dictionary<string, bool>();

        /// <summary>
        ///     Will frequently update the Server status and notify changes.
        /// </summary>
        public ServerStatus()
        {
            Bind_PlayerList = new List<PlayerPayLoad>();
            ApplyServerInfo(null);
        }

        /// <summary>
        ///     Public online player representation with name
        /// </summary>
        public List<PlayerPayLoad> Bind_PlayerList { get; }

        // Basics
        public string Bind_Label { get; set; }
        public string Bind_Host { get; set; }
        public int Bind_Port { get; set; }

        // Settings
        public bool Bind_ServerNotify { get; set; }
        public bool Bind_CountNotify { get; set; }
        public bool Bind_PlayerNotify { get; set; }

        // Actual Status Vars
        // TODO JSON IGNORE
        public string Bind_LastStatus { get; private set; }
        public bool Bind_ServerOnline { get; private set; }
        public int Bind_OnlinePlayers { get; private set; }
        public int Bind_MaxPlayers { get; private set; }
        public string Bind_Version { get; private set; }
        public string Bind_MOTD { get; private set; }
        public string Bind_Error { get; private set; }


        /// <summary>
        ///     returns all the time-plottable online player count data
        /// </summary>
        /// <returns></returns>
        public PlottableData GetUserData()
        {
            var nauw = DateTime.Now;
            var lx = new List<double>();
            var ly = new List<double>();
            foreach (var infoBase in _infoList)
            {
                var diff = infoBase.RequestDate.AddMilliseconds(infoBase.RequestTime).Subtract(nauw).TotalMinutes;
                lx.Add(diff);
                ly.Add(infoBase.CurrentPlayerCount);
            }
            return new PlottableData(Bind_Label, lx.ToArray(), ly.ToArray());
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
            foreach (var infoBase in _infoList)
            {
                var diff = infoBase.RequestDate.AddMilliseconds(infoBase.RequestTime).Subtract(nauw).TotalMinutes;
                lx.Add(diff);
                ly.Add(infoBase.RequestTime);
            }
            return new PlottableData(Bind_Label, lx.ToArray(), ly.ToArray());
        }

        /// <summary>
        ///     This method will repeatedly ping the server to request infos.
        ///     It will then trigger given events.
        /// </summary>
        public EventBase[] PingUpdate()
        {
            Program.WriteLine("Pinging server '" + Bind_Host + ":" + Bind_Port + "'...");
            // event-queue
            var events = new List<EventBase>();
            // current server-info object
            ServerInfoBase current = null;
            for (var i = 0; i < 3; i++)
                if ((current = MultipleTryRequestMethod(i, 1)).HadSuccess)
                    break;
            // get last result
            var last = GetLatestServerInfo();
            // first Info we got?
            var isFirst = last == null;
            // update public vars
            ApplyServerInfo(current);
            // if the result is null, nothing to do here
            if (current == null) return new EventBase[0];
            // add current result to list
            _infoList.Add(current);
            // if first info, or last success was different from this (either went online or went offline) => invoke
            if (Bind_ServerNotify && (isFirst || last.HadSuccess != current.HadSuccess))
            {
                Program.WriteLine("Server '" + Bind_Host + ":" + Bind_Port + "' status change: " + current.HadSuccess);
                var errMsg = current.LastError != null ? current.LastError.ToString() : "";
                if (errMsg.Contains(" at "))
                    errMsg = errMsg.Split(new[] { " at " }, StringSplitOptions.None)[0];
                events.Add(new OnlineStatusEvent(current.HadSuccess, current.HadSuccess ? current.ServerMotd : errMsg));
            }

            // if first info, or last player count was different (player went online or offline) => invoke
            if (Bind_CountNotify)
            {
                var diff = isFirst
                    ? current.CurrentPlayerCount
                    : current.CurrentPlayerCount - last.CurrentPlayerCount;
                if (diff != 0)
                {
                    Program.WriteLine("Server '" + Bind_Host + ":" + Bind_Port + "' count change: " + diff);
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
                    if (Bind_PlayerNotify && (!userStates.ContainsKey(p.Id) || !userStates[p.Id]))
                        events.Add(new PlayerStateEvent(p, true));
                    // register state or set to true
                    userStates[p.Id] = true;
                }

            // this needs to be done to avoid ElementChangedException
            var keys = userStates.Keys.ToArray();
            // check all states for players who went offline
            foreach (var k in keys)
                // if user state still true, but he is not in online list => went offline
                if (userStates[k] && !onlineIds.Contains(k))
                {
                    userStates[k] = false;
                    // create payload
                    var p = new PlayerPayLoad { Id = k, Name = userNames[k] };
                    // notify => invoke
                    if (Bind_PlayerNotify)
                        events.Add(new PlayerStateEvent(p, false));
                }

            // cleanup, sleep, repeat
            ClearMem();
            return events.ToArray();
        }

        /// <summary>
        /// </summary>
        /// <param name="successful"></param>
        /// <returns></returns>
        public ServerInfoBase GetLatestServerInfo(bool successful = false)
        {
            var tmpList = new List<ServerInfoBase>();
            tmpList.AddRange(successful ? _infoList.FindAll(o => o.HadSuccess) : _infoList);
            return tmpList.Count > 0
                ? tmpList.OrderByDescending(ob => ob.RequestDate.AddMilliseconds(ob.RequestTime)).First()
                : null;
        }

        #region Internal

        /// <summary>
        ///     Try a request method multiple times
        /// </summary>
        /// <param name="m"></param>
        /// <param name="tries"></param>
        /// <returns></returns>
        private ServerInfoBase MultipleTryRequestMethod(int m, int tries)
        {
            ServerInfoBase res = null;
            for (var i = 0; i < tries; i++)
            {
                res = GetMethod(m);
                if (res != null && res.HadSuccess)
                {
                    Program.WriteLine("Server '" + Bind_Host + ":" + Bind_Port + "' ping Success! Method: " + m + " Try: " + i);
                    break;
                }
                else if (res != null)
                {
                    Console.WriteLine("Error: " + res.LastError);
                }
            }
            return res;
        }

        /// <summary>
        ///     This method will request the server infos for the given version/method.
        ///     it is run as task to make it cancelable
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private ServerInfoBase GetMethod(int method)
        {
            switch (method)
            {
                case 1:
                    return Get14ServerInfo.Get(Bind_Host, Bind_Port);
                case 2:
                    return GetBetaServerInfo.Get(Bind_Host, Bind_Port);
                default:
                    return GetNewServerInfo.Get(Bind_Host, Bind_Port);
            }
        }


        /// <summary>
        ///     Will apply the current server-info to the public vars
        /// </summary>
        /// <param name="si"></param>
        private void ApplyServerInfo(ServerInfoBase si)
        {
            var nu = si == null;

            Bind_LastStatus = nu ? "-" : si.RequestDate.AddMilliseconds(si.RequestTime).ToString("HH:mm:ss");
            Bind_ServerOnline = !nu && si.HadSuccess;
            Bind_OnlinePlayers = nu ? 0 : si.CurrentPlayerCount;
            Bind_MaxPlayers = nu ? 0 : si.MaxPlayerCount;
            Bind_Version = nu ? "0.0.0" : si.MinecraftVersion;
            Bind_MOTD = nu || !si.HadSuccess ? "-" : si.ServerMotd;
            Bind_Error = !nu && si.LastError != null ? si.LastError.ToString() : "-";

            Bind_PlayerList.Clear();
            if (!nu && si.OnlinePlayers != null) Bind_PlayerList.AddRange(si.OnlinePlayers);
        }

        // RUN GC
        private void ClearMem()
        {
            _ = _infoList.RemoveAll(o => o.RequestDate < DateTime.Now.Subtract(ClearSpan));
            GC.Collect();
        }

        #endregion
    }
}