using System;
using McswBot2.Static;

namespace McswBot2.Objects
{
    public class Config
    {
        /// <summary>
        ///     Telegram BOT Api Key
        /// </summary>
        public string ApiKey = "123456:XXXXXX";

        /// <summary>
        ///     Time between data saved
        /// </summary>
        public int DataSaveInterval = 60000;

        /// <summary>
        ///     Bot Admin Telegram ID
        /// </summary>
        public int DeveloperId = 87654321;

        /// <summary>
        ///     Use debug Logging?
        /// </summary>
        public int LogLevel = Convert.ToInt32(Types.LogLevel.Normal);

        /// <summary>
        ///     How often will we retry if a status had no success?
        /// </summary>
        public int Retries = 3;

        /// <summary>
        ///     How long will we wait between retries?
        /// </summary>
        public int RetryMs = 3000;

        /// <summary>
        ///     How long will we wait between retries?
        /// </summary>
        public int TimeoutMs = 10000;
    }
}