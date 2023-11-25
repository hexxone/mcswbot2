using McswBot2.Minecraft;
using McswBot2.Static;
using System;
using System.Collections.Generic;

namespace McswBot2.Objects
{
    public class Config
    {
        /// <summary>
        ///     Telegram BOT Api Key
        /// </summary>
        public string ApiKey { get; set; } = "123456:XXXXXX";

        /// <summary>
        ///     Time between data saved
        /// </summary>
        public int DataSaveInterval { get; set; } = 60000;

        /// <summary>
        ///     Bot Admin Telegram ID
        /// </summary>
        public int DeveloperId { get; set; } = 87654321;

        /// <summary>
        ///     Use debug Logging?
        /// </summary>
        public int LogLevel { get; set; } = Convert.ToInt32(Types.LogLevel.Normal);

        /// <summary>
        ///     How often will we retry if a status had no success?
        /// </summary>
        public int Retries { get; set; } = 3;

        /// <summary>
        ///     How long will we wait between retries?
        /// </summary>
        public int RetryMs { get; set; } = 3000;

        /// <summary>
        ///     How long will we wait between retries?
        /// </summary>
        public int TimeoutMs { get; set; } = 3000;


        // Identity
        public List<ServerStatusWatcher> WatchedServers { get; set; } = new();
    }
}