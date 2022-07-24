using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using McswBot2.Event;
using Newtonsoft.Json;

namespace McswBot2.Minecraft;

public class ServerStatus
{
    private static readonly List<ServerStatusWatcher> Watchers = new();


    /// <summary>
    ///     Include a list of Names or UIDs of MineCraft-Users.
    ///     When they join or leave, the PlayerStateChangedEvent will be triggered.
    ///     NOTE; Only new Servers(1.11+) support this feature! Also, large Servers
    ///     don't usually return the actual/complete player list. Hence, this may
    ///     not work for some cases.
    /// </summary>
    private readonly Dictionary<string, bool> _userStates = new();


    [JsonIgnore] public EventHandler<EventBase[]>? ChangedEvent;


    /// <summary>
    ///     Normal constructor
    /// </summary>
    /// <param name="label"></param>
    /// <param name="address"></param>
    /// <param name="port"></param>
    /// <param name="reuse"></param>
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
    internal ServerStatus(string label, string address, int port, bool notifyServer, bool notifyCount,
        bool notifyNames, bool sticker)
    {
        Label = label;
        RegisterWatcher(address, port);

        NotifyServer = notifyServer;
        NotifyCount = notifyCount;
        NotifyNames = notifyNames;
        Sticker = sticker;
    }

    // Runtime


    [JsonIgnore] public ServerStatusWatcher? Watcher { get; set; }
    [JsonIgnore] public ServerInfoExtended? Last { get; set; }


    // Identity

    public string Label { get; set; }
    public string Address => Watcher?.Address ?? "undefined";
    public int Port => Watcher?.Port ?? 25565;

    // Settings

    public bool NotifyServer { get; set; }
    public bool NotifyCount { get; set; }
    public bool NotifyNames { get; set; }
    public bool Sticker { get; set; }


    public static void UpdateAll(int timeOutMs = 30000)
    {
        Parallel.ForEach(Watchers, w => { w.Execute(timeOutMs); });
    }

    /// <summary>
    ///     TODO this is where the magic happens
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    /// <param name="reuse"></param>
    private void RegisterWatcher(string address, int port, bool reuse = true)
    {
        // search for existing watchers
        var res = Watchers.FindAll(watcher =>
            string.Equals(watcher.Address, address, StringComparison.CurrentCultureIgnoreCase) && watcher.Port == port);
        // reuse existing watcher?
        if (reuse && res.Count > 0)
            Watcher = res.First();
        else
        {
            Watcher = new ServerStatusWatcher(address, port);
            if (reuse) Watchers.Add(Watcher);
        }

        // Finally register event...
        Watcher.UpdatedEvent += UpdatedEvent;
    }


    /// <summary>
    ///     Event Handler for changed Minecraft Info
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UpdatedEvent(object? sender, ServerInfoExtended? e)
    {
        var events = Update(e);
        ChangedEvent?.Invoke(this, events);
    }

    /// <summary>
    ///     Will compare the Last status with the current one and return event updates.
    /// </summary>
    /// <returns></returns>
    private EventBase[] Update(ServerInfoExtended? current)
    {
        // event-queue
        var events = new List<EventBase>();
        var queue = new List<EventBase>();

        if (current == null || current == Last) return events.ToArray();

        // if first info, or Last success was different from this (either went online or went offline) => invoke
        var isFirst = Last == null;
        if (isFirst || Last?.HadSuccess != current.HadSuccess)
        {
            Debug.WriteLine($"Server '{Watcher?.Address}:{Watcher?.Port}' status change: {current.HadSuccess}");

            var errMsg = $"Connection Failed: '{current.LastError?.GetType().Name}'";
            if (NotifyServer)
                events.Add(new OnlineStatusEvent(
                    current.HadSuccess, current.HadSuccess ? current.FixedMotd : errMsg,
                    current.HadSuccess ? current.MinecraftVersion : "0.0.0",
                    current.CurrentPlayerCount, current.MaxPlayerCount));
        }


        // JOIN PLAYER QUEUE

        var joins = 0;
        foreach (var p in current.OnlinePlayers)
        {
            // if user has state and Last state was online => no change
            if (_userStates.ContainsKey(p.Id))
            {
                if (_userStates[p.Id]) continue;
                _userStates[p.Id] = true;
            }
            else
            {
                _userStates.Add(p.Id, true);
            }

            // notify
            p.LastSeen = DateTime.Now;
            joins++;
            if (NotifyNames)
                queue.Add(new PlayerStateEvent(p, true));
        }

        if (joins > 0)
        {
            Debug.WriteLine($"Server '{Watcher?.Address}:{Watcher?.Port}' player join: [{joins}]");
            if (NotifyCount)
                events.Add(new PlayerChangeEvent(joins));
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
            if (!_userStates[k] || current.OnlinePlayers.Any(op => op.Id == k)) continue;
            // if user state still true, but he is not in online list => went offline
            _userStates[k] = false;

            // try to get && update player
            var p = Watcher?.AllPlayers.FirstOrDefault(p => p.Id == k);
            if (p != null)
            {
                var now = DateTime.Now;
                p.PlayTime += now - p.LastSeen;
                p.LastSeen = now;

                if (NotifyNames)
                    queue.Add(new PlayerStateEvent(p, false));
            }

            // notify
            leaves--;
        }

        if (leaves < 0)
        {
            Debug.WriteLine($"Server '{Watcher?.Address}:{Watcher?.Port}' player leave: [{joins}]");
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
                : current.CurrentPlayerCount - Last?.CurrentPlayerCount ?? 0;

            if (diff != 0)
            {
                Debug.WriteLine($"Server '{Watcher?.Address}:{Watcher?.Port}' player diff: [{diff}]");
                if (NotifyCount)
                    events.Add(new PlayerChangeEvent(diff));
            }
        }

        Last = current;
        return events.ToArray();
    }
}