namespace mcswbot2.Bot.Objects
{
    public class Config
    {
        /// <summary>
        ///     Time for one Ping
        /// </summary>
        public int SleepTime = 60000;

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
        ///     (For Ping & Player Command)
        /// </summary>
        public bool DrawPlots = true;
    }
}