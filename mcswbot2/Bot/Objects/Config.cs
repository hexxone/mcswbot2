﻿using System;
using mcswlib;

namespace mcswbot2.Bot.Objects
{
    public class Config
    {
        /// <summary>
        ///     Time for one Execute
        /// </summary>
        public int SleepTime = 60000;

        /// <summary>
        ///     How often will we retry if a status had no success?
        /// </summary>
        public int Retries = 3;

        /// <summary>
        ///     How long will we wait between retries?
        /// </summary>
        public int RetryMs = 3000;

        /// <summary>
        ///     Telegram BOT Api Key
        /// </summary>
        public string ApiKey = "123456:XXXXXX";

        /// <summary>
        ///     Bot Admin Telegram ID
        /// </summary>
        public int DeveloperId = 87654321;

        /// <summary>
        ///     Draw & send visual time graphs?
        ///     (For Execute & Player Command)
        /// </summary>
        public bool DrawPlots = true;
        
        /// <summary>
        ///     Use debug Logging?
        /// </summary>
        public int LogLevel = Convert.ToInt32(Types.LogLevel.Normal);

        /// <summary>
        ///     Amount of hours to keep historic data
        /// </summary>
        public int HistoryHours = 24 * 7;

        /// <summary>
        ///     Quantize Threshold (Max amount of Data points)
        /// </summary>
        public int QThreshold = 36;

        /// <summary>
        ///     Amount of Data points to compress into one, if the total number exceeds the Threshold
        /// </summary>
        public int QRatio = 6;
    }
}