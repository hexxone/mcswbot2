using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using mcswbot2.Event;
using Newtonsoft.Json;

namespace mcswbot2.Minecraft
{
    public class ServerStatus
    {
        private static List<ServerStatusWatcher> _watchers = new();

        public static void UpdateAll(int timeOutMs = 30000)
        {
            Parallel.ForEach(_watchers, w =>
            {
                w.Execute(timeOutMs);
            });
        }

        // Identity
        public string Label { get; set; }

        [JsonIgnore]
        public ServerStatusWatcher Watcher { get; set; }

        public string Address => Watcher.Address;
        public int Port => Watcher.Port;

        // Settings
        
        public bool NotifyServer { get; set; }
        public bool NotifyCount { get; set; }
        public bool NotifyNames { get; set; }
        public bool Sticker { get; set; }


        [JsonIgnore]
        public EventHandler<EventBase[]> ChangedEvent;
        
        [JsonIgnore]
        public ServerInfoExtended Last { get; set; }



        /// <summary>
        ///     Include a list of Names or UIDs of MineCraft-Users.
        ///     When they join or leave, the PlayerStateChangedEvent will be triggered.
        ///     NOTE; Only new Servers(1.11+) support this feature! Also, large Servers
        ///     don't usually return the actual/complete player list. Hence, this may
        ///     not work for some cases.
        /// </summary>
        private readonly Dictionary<string, bool> _userStates = new();


        /// <summary>
        ///     Normal constructor
        /// </summary>
        /// <param name="label"></param>
        /// <param name="watcher"></param>
        internal ServerStatus(string label, string address, int port, bool reuse = true)
        {
            Label = label;
            RegisterWatcher(address, port, reuse);

            NotifyServer = true;
            NotifyCount = false;
            NotifyNames = false;
            Sticker = false;
        }

        /// <summary>
        ///     Constructor for deserializing json
        /// </summary>
        [JsonConstructor]
        internal ServerStatus(string label, string address, int port, bool notifyServer, bool notifyCount, bool notifyNames, bool sticker)
        {
            Label = label;
            RegisterWatcher(address, port);

            NotifyServer = notifyServer;
            NotifyCount = notifyCount;
            NotifyNames = notifyNames;
            Sticker = sticker;
        }
        
        /// <summary>
        ///     TODO this is where the magic happens
        /// </summary>
        /// <param name="watcher"></param>
        private void RegisterWatcher(string address, int port, bool reuse = true)
        {
            // search for existing watchers
            var res = _watchers.FindAll(wtch => string.Equals(wtch.Address, address, StringComparison.CurrentCultureIgnoreCase) && wtch.Port == port);
            // reuse existing watcher?
            if (reuse && res.Count > 0) Watcher = res.First();
            else
            {
                Watcher = new ServerStatusWatcher(address, port);
                if(reuse) _watchers.Add(Watcher);
            }
            // Finally register event...
            Watcher.UpdatedEvent += UpdatedEvent;
        }


        /// <summary>
        ///     Event Handler for changed Minecraft Info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdatedEvent(object? sender, ServerInfoExtended e)
        {
            var events = Update(e);
            ChangedEvent.Invoke(this, events);
        }

        /// <summary>
        ///     Will compare the Last status with the current one and return event updates.
        /// </summary>
        /// <returns></returns>
        private EventBase[] Update(ServerInfoExtended current)
        {
            // event-queue
            var events = new List<EventBase>();
            var queue = new List<EventBase>();

            var isFirst = Last == null;
            if (current == null || current == Last) return events.ToArray();


            // if first info, or Last success was different from this (either went online or went offline) => invoke
            if (isFirst || Last.HadSuccess != current.HadSuccess)
            {
                Debug.WriteLine("Server '" + Watcher.Address + ":" + Watcher.Port + "' status change: " + current.HadSuccess);
                var errMsg = current.LastError != null ? "Connection Failed: " + current.LastError.GetType().Name : "";
                if(NotifyServer) events.Add(new OnlineStatusEvent(
                    current.HadSuccess, current.HadSuccess ? current.ServerMotd : errMsg,
                    current.HadSuccess ? current.MinecraftVersion : "0.0.0",
                    current.CurrentPlayerCount, current.MaxPlayerCount));
            }


            // JOIN PLAYER QUEUE

            var joins = 0;
                
            if (current.OnlinePlayers != null)
            {
                foreach (var p in current.OnlinePlayers)
                {
                    // if user has state and Last state was online => no change
                    if (_userStates.ContainsKey(p.Id))
                    {
                        if (_userStates[p.Id]) continue;
                        _userStates[p.Id] = true;
                    }
                    else _userStates.Add(p.Id, true);

                    // notify
                    p.LastSeen = DateTime.Now;
                    joins++;
                    if (NotifyNames) queue.Add(new PlayerStateEvent(p, true));
                }
            }

            if (joins > 0)
            {
                Debug.WriteLine("Server '" + Watcher.Address + ":" + Watcher.Port + "' join change: " + joins);
                if (NotifyCount) events.Add(new PlayerChangeEvent(joins));
            }
            events.AddRange(queue);
            queue.Clear();

            // LEAVE PLAYER QUEUE

            var leaves = 0;

            // this needs to be done to avoid ElementChangedException
            var keys = _userStates.Keys.ToArray();
            // check all states for players who went offline
            foreach (var k in keys)
            {
                if (!_userStates[k]) continue;
                // if user state still true, but he is not in online list => went offline
                _userStates[k] = false;

                // notify
                var p = Watcher.AllPlayers.Find(p => p.Id == k);
                p.LastSeen = DateTime.Now;
                leaves--;
                if(NotifyNames) queue.Add(new PlayerStateEvent(p, false));
            }

            if (leaves < 0)
            {
                Debug.WriteLine("Server '" + Watcher.Address + ":" + Watcher.Port + "' leave change: " + leaves);
                if (NotifyCount) events.Add(new PlayerChangeEvent(leaves));
            }
            events.AddRange(queue);
            queue.Clear();


            // Fallback, when no PlayerList changes where detected from sample...
            // if first info, or Last player count was different => Notify
            if (joins == 0 && leaves == 0)
            {
                var diff = isFirst
                    ? current.CurrentPlayerCount
                    : current.CurrentPlayerCount - Last.CurrentPlayerCount;
                if (diff != 0)
                {
                    Debug.WriteLine("Server '" + Watcher.Address + ":" + Watcher.Port + "' count change: " + diff);
                    if (NotifyCount) events.Add(new PlayerChangeEvent(diff));
                }
            }

            Last = current;
            return events.ToArray();
        }

    }
}
