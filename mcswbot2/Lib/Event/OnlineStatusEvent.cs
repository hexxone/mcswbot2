namespace mcswbot2.Lib.Event
{
    internal class OnlineStatusEvent : EventBase
    {
        /// <summary>
        ///     Online Status of a  Server, given parameters are online bool & statusText msg if offline
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="statusText"></param>
        public OnlineStatusEvent(bool stat, string statusText = "")
        {
            ServerStatus = stat;
            StatusText = statusText;
        }

        public bool ServerStatus { get; }

        public string StatusText { get; }

        public override string GetEventString(Types.Formatting format)
        {
            return "Server status: "
                   + (ServerStatus ? "online 🌐" : "offline ❌")
                   + (ServerStatus ? "\r\nMOTD:\r\n" : "\r\nReason:\r\n")
                   + Wrap(format, StatusText);
        }
    }
}