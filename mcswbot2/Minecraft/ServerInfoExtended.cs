using System;
using System.Collections.Generic;
using McswBot2.Event;
using McswBot2.Static;
using Newtonsoft.Json;

namespace McswBot2.Minecraft
{
    public class ServerInfoExtended : ServerInfoBasic
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ServerInfoExtended" /> with specified values
        ///     => successful request
        /// </summary>
        /// <param name="dt">When did the request start?</param>
        /// <param name="sp">How long did the request take in ms?</param>
        /// <param name="motd">Server's Motd</param>
        /// <param name="maxPlayers">Server's max player count</param>
        /// <param name="playerCount">Server's current player count</param>
        /// <param name="version">Server's Minecraft version</param>
        /// <param name="players">Server's online players</param>
        internal ServerInfoExtended(DateTime dt, double sp, string motd, int maxPlayers, int playerCount,
            string version, List<PlayerPayload> players)
         : base(true, dt, sp, playerCount)
        {
            RawMotd = motd;
            MaxPlayerCount = maxPlayers;
            MinecraftVersion = version;
            OnlinePlayers = players;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ServerInfoExtended" /> with specified values
        ///     => failed request
        /// </summary>
        /// <param name="ex">the Last occurred Exception when determining Server status</param>
        internal ServerInfoExtended(DateTime dt, Exception ex)
            : base(false, dt, 1, 0)
        {
            LastError = ex;
            RawMotd = "";
            CurrentPlayerCount = 0;
            MinecraftVersion = "0.0.0";
            OnlinePlayers = new List<PlayerPayload>();
        }

        /// <summary>
        ///     Returns the Last occurred runtime error
        /// </summary>
        public Exception? LastError { get; }

        /// <summary>
        ///     Get the server's raw Message of the day including formatting and color codes.
        /// </summary>
        public string RawMotd { get; }

        /// <summary>
        ///     Gets the server's Message of the day as human readable Text
        /// </summary>
        [JsonIgnore]
        public string? FixedMotd => Types.FixMcChat(RawMotd);

        /// <summary>
        ///     Gets the server's max player count
        /// </summary>
        public double MaxPlayerCount { get; }

        /// <summary>
        ///     Gets the server's Minecraft version
        /// </summary>
        public string MinecraftVersion { get; }

        /// <summary>
        ///     Gets the server's Online Players as object List
        /// </summary>
        public List<PlayerPayload> OnlinePlayers { get; }
    }
}