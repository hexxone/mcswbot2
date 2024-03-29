﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McswBot2.Event;
using McswBot2.Static;
using Newtonsoft.Json;
using SkiaSharp;

namespace McswBot2.Minecraft;

public class ServerStatusWatcher
{
    // tries before a server is determined offline
    internal static int Retries = 3;
    internal static int RetryMs = 3000;


    public EventHandler<ServerInfoExtended>? UpdatedEvent;


    /// <summary>
    ///     Normal constructor
    /// </summary>
    internal ServerStatusWatcher(string address, int port)
    {
        Address = address;
        Port = port;

        InfoHistory = new List<ServerInfoBasic>();
        AllPlayers = new List<PlayerPayLoad>();
    }

    /// <summary>
    ///     Re-Constructing
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    /// <param name="infoHistory"></param>
    /// <param name="allPlayers"></param>
    [JsonConstructor]
    internal ServerStatusWatcher(string address, int port, List<ServerInfoBasic> infoHistory,
        List<PlayerPayLoad> allPlayers)
    {
        Address = address;
        Port = port;
        InfoHistory = infoHistory;
        AllPlayers = allPlayers;
    }

    public string Address { get; set; }
    public int Port { get; set; }


    /// <summary>
    ///     The Icon for the Server
    /// </summary>
    [JsonIgnore]
    public SKImage? FavIcon { get; private set; }


    // List of past received Server Infos
    public List<ServerInfoBasic> InfoHistory { get; set; }


    // List of all known Players
    public List<PlayerPayLoad> AllPlayers { get; set; }


    // List of currently online players
    [JsonIgnore] public List<PlayerPayLoad> OnlinePlayers => AllPlayers.Where(ap => ap.Online).ToList();


    /// <summary>
    ///     This method will ping the server to request infos.
    ///     This is done in context of a task and 10 second timeout
    /// </summary>
    public void Execute(int timeOutMs = 30000)
    {
        var tSpan = TimeSpan.FromMilliseconds(timeOutMs);
        ServerInfoExtended sie;
        try
        {
            using var tokenSource = new CancellationTokenSource(tSpan);
            var token = tokenSource.Token;
            var task = Task.Run(() => Execute(token), token);
            task.Wait(token);
            sie = task.Result ?? throw new Exception("null response");

            // list of all "last-online" player ids
            var oIds = from op in sie.OnlinePlayers select op.Id;
            // Set all players leaving to offline
            if (oIds.Any() && OnlinePlayers.Count > 0)
                OnlinePlayers.FindAll(player => !oIds.Contains(player.Id))
                    .ForEach(player => player.Online = false);
        }
        catch (Exception e)
        {
            Logger.WriteLine("Execute Error? [" + Address + ":" + Port + "]: " + e);
            sie = new ServerInfoExtended(DateTime.Now, e);
        }

        // update server icon
        if (sie.FavIcon != null)
        {
            // always dispose previous one -> avoid mem leak
            FavIcon?.Dispose();
            FavIcon = sie.FavIcon;
        }

        InfoHistory.Add(sie);
        UpdatedEvent?.Invoke(this, sie);
        CleanData();
    }


    private ServerInfoExtended? Execute(CancellationToken ct)
    {
        var srv = "[" + Address + ":" + Port + "]";
        Logger.WriteLine("Pinging server " + srv);
        // safety-wrapper
        try
        {
            // current server-info object
            var dt = DateTime.Now;
            ServerInfoExtended? current = null;
            var si = new ServerInfo(Address, Port);
            for (var r = 0; r < Retries; r++)
            {
                current = si.GetAsync(ct, dt, AllPlayers).Result;
                if (current.HadSuccess || ct.IsCancellationRequested) break;
                Task.Delay(RetryMs, ct).Wait(ct);
            }

            // if the result is null, nothing to do here
            if (current != null)
            {
                Logger.WriteLine(
                    "Execute result: " + srv + " is: " + current.HadSuccess + " Err: " + current.LastError,
                    Types.LogLevel.Debug);
                return current;
            }

            Logger.WriteLine("Execute result null: " + srv, Types.LogLevel.Debug);
        }
        catch (Exception ex)
        {
            Logger.WriteLine("Fatal Error when Pinging... " + ex, Types.LogLevel.Error);
        }

        return null;
    }


    // Quantize, I don't even know...
    public void CleanData()
    {
        // Remove very old data
        foreach (var hk in InfoHistory.Where(hk =>
                     hk.RequestDate < DateTime.Now - TimeSpan.FromHours(McswBot.Conf.HistoryHours)))
            InfoHistory.Remove(hk);

        var qThreshold = McswBot.Conf.QThreshold;
        var qRatio = McswBot.Conf.QRatio;

        var quInd = 0;
        while (InfoHistory.Count > qThreshold)
        {
            var search = InfoHistory
                .Where(h => h.QLevel == quInd)
                .OrderBy(h => h.RequestDate).ToArray();

            if (search.Count() > qRatio * 1.5)
            {
                var counter = 0;
                double date = 0;
                double time = 0;
                double online = 0;
                foreach (var sib in search)
                {
                    if (counter++ >= qRatio) break;
                    date += (double)sib.RequestDate.Ticks / qRatio;
                    time += sib.RequestTime / qRatio;
                    online = Math.Max(sib.CurrentPlayerCount, online);
                    InfoHistory.Remove(sib);
                }

                InfoHistory.Add(new ServerInfoBasic(true, new DateTime((long)date), time, online, quInd + 1));
            }
            else if (quInd > InfoHistory.Max(ih => ih.QLevel))
            {
                Logger.WriteLine("Cant compress further!");
                break;
            }
            else
            {
                quInd++;
            }
        }
    }
}