using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using mcswbot2.Lib.Payload;
using mcswbot2.Lib;

namespace mcswbot2.Lib.ServerInfo
{
    // Old protocol version codes
    // 1.3.1  - 1.3.2  == 39
    // 1.4.2           == 47
    // 1.4.4  - 1.4.5  == 49
    // 1.4.6  - 1.4.7  == 51
    // 1.5    - 1.5.1  == 60
    // 1.5.2           == 61
    // 1.6.1           == 73
    // 1.6.2           == 74
    // 1.6.4           == 78

    // New protocol version codes
    // 1.7    - 1.7.1  == 3
    // 1.7.2  - 1.7.5  == 4 
    // 1.7.6  - 1.7.10 == 5
    // 1.8    - 1.8.9  == 47
    // 1.9    - 1.9.1  == 107 & 108
    // 1.9.2  - 1.9.4  == 109
    // 1.9.3  - 1.9.4  == 110
    // 1.10   - 1.10.2 == 210
    // 1.11            == 315
    // 1.11.1 - 1.11.2 == 316
    // 1.12            == 335
    // 1.12.1          == 338
    // 1.12.2          == 340
    // 1.13            == 393
    // 1.13.1          == 401
    // 1.14.3          == 409
    public class ServerInfoBase
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
        public ServerInfoBase(DateTime dt, TimeSpan sp, string motd, int maxPlayers, int playerCount, string version,
            List<PlayerPayLoad> players)
        {
            RequestDate = dt;
            RequestTime = sp;
            HadSuccess = true;
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
        public ServerInfoBase(Exception ex)
        {
            RequestDate = DateTime.Now;
            RequestTime = TimeSpan.FromMilliseconds(500);
            HadSuccess = false;
            LastError = ex;
            MinecraftVersion = "0.0.0";
        }

        /// <summary>
        ///     TimeStamp when the request was done
        /// </summary>
        public DateTime RequestDate { get; }

        /// <summary>
        ///     How long did the request take to complete?
        /// </summary>
        public TimeSpan RequestTime { get; }

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
        public string ServerMotd => Motd();

        /// <summary>
        ///     Gets the server's MOTD converted into HTML style code
        /// </summary>
        public string ServerMotdHtml => Motd(true);

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

        /// <summary>
        ///     Gets the server's MOTD formatted as HTML
        /// </summary>
        /// <returns>HTML-formatted MOTD</returns>
        private string Motd(bool useHtml = false)
        {
            var regex = new Regex("§([k-oK-O])(.*?)(§[0-9a-fA-Fk-oK-OrR]|$)");
            var s = RawMotd;
            while (regex.IsMatch(s))
                s = regex.Replace(s, m =>
                {
                    if (!useHtml) return m.Groups[2].Value + m.Groups[3].Value;
                    var ast = "text-decoration:" + Types.MinecraftStyles[m.Groups[1].Value[0]];
                    var html = "<span style=\"" + ast + "\">" + m.Groups[2].Value + "</span>" + m.Groups[3].Value;
                    return html;
                });
            regex = new Regex("§([0-9a-fA-F])(.*?)(§[0-9a-fA-FrR]|$)");
            while (regex.IsMatch(s))
                s = regex.Replace(s, m =>
                {
                    if (!useHtml) return m.Groups[2].Value + m.Groups[3].Value;
                    var ast = "color:" + Types.MinecraftColors[m.Groups[1].Value[0]];
                    var html = "<span style=\"" + ast + "\">" + m.Groups[2].Value + "</span>" + m.Groups[3].Value;
                    return html;
                });
            return s;
        }
    }
}