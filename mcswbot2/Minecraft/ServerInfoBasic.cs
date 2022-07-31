using System;
using Newtonsoft.Json;

namespace McswBot2.Minecraft
{
    [Serializable]
    public class ServerInfoBasic
    {
        public ServerInfoBasic(bool hadSuccess, DateTime requestDate, double requestTime, int currentPlayerCount)
        {
            HadSuccess = hadSuccess;
            RequestDate = requestDate;
            RequestTime = requestTime;
            CurrentPlayerCount = currentPlayerCount;
        }

        /// <summary>
        ///     Determines if the request was successful
        /// </summary>
        public bool HadSuccess { get; set; }

        /// <summary>
        ///     TimeStamp when the request was done
        /// </summary>
        public DateTime RequestDate { get; set; }

        /// <summary>
        ///     How long did the request take to complete in MS?
        /// </summary>
        public double RequestTime { get; set; }

        /// <summary>
        ///     Gets the server's current player count
        /// </summary>
        public int CurrentPlayerCount { get; set; }
        
    }
}