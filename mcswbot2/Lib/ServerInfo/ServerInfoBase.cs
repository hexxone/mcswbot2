﻿using System;
using System.Collections.Generic;
using mcswbot2.Lib.Event;

namespace mcswbot2.Lib.ServerInfo
{
    internal class ServerInfoBase
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ServerInfoBase" /> with specified values
        ///     => successful request
        /// </summary>
        /// <param name="dt">When did the request start?</param>
        /// <param name="sp">How long did the request take?</param>
        /// <param name="motd">Server's MOTD</param>
        /// <param name="maxPlayers">Server's max player count</param>
        /// <param name="playerCount">Server's current player count</param>
        /// <param name="version">Server's Minecraft version</param>
        /// <param name="players">Server's online players</param>
        internal ServerInfoBase(DateTime dt, long sp, string motd, int maxPlayers, int playerCount, string version,
            List<PlayerPayLoad> players)
        {
            HadSuccess = true;
            RequestDate = dt;
            RequestTime = sp;
            RawMotd = motd;
            MaxPlayerCount = maxPlayers;
            CurrentPlayerCount = playerCount;
            MinecraftVersion = version;
            OnlinePlayers = players;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ServerInfoBase" /> with specified values
        ///     => failed request
        /// </summary>
        /// <param name="ex">the Last occured Exception when determining Server status</param>
        public ServerInfoBase(DateTime dt, long sp, Exception ex)
        {
            HadSuccess = false;
            RequestDate = dt;
            RequestTime = sp;
            LastError = ex;
            MinecraftVersion = "0.0.0";
        }

        /// <summary>
        ///     TimeStamp when the request was done
        /// </summary>
        public DateTime RequestDate { get; }

        /// <summary>
        ///     How long did the request take to complete in MS?
        /// </summary>
        public long RequestTime { get; }

        /// <summary>
        ///     Determines if the request was successfull
        /// </summary>
        public bool HadSuccess { get; }

        /// <summary>
        ///     Returns the last occured runtime error
        /// </summary>
        public Exception LastError { get; }

        /// <summary>
        ///     Get the raw Message of the day including formatting's and color codes.
        /// </summary>
        public string RawMotd { get; private set; }

        /// <summary>
        ///     Gets the server's MOTD as Text
        /// </summary>
        public string ServerMotd => Types.FixMcChat(RawMotd);

        /// <summary>
        ///     Gets the server's max player count
        /// </summary>
        public int MaxPlayerCount { get; }

        /// <summary>
        ///     Gets the server's current player count
        /// </summary>
        public int CurrentPlayerCount { get; }

        /// <summary>
        ///     Gets the server's Minecraft version
        /// </summary>
        public string MinecraftVersion { get; }

        /// <summary>
        ///     Gets the server's Online Players as object List
        /// </summary>
        public List<PlayerPayLoad> OnlinePlayers { get; }

        /// <summary>
        ///     String override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                $"Success:{HadSuccess},LasError:{LastError},Motd:{ServerMotd},MaxPlayers:{MaxPlayerCount},CurrentPlayers:{CurrentPlayerCount},MCVersion:{MinecraftVersion};");
        }

    }
}