namespace mcswbot2.Event
{
    /// <summary>
    ///     Public class representing Event messages.
    /// </summary>
    public static class EventMessages
    {
        internal static string ServerOnline =
            "\r\nStatus: <code>  online </code> 🌐\r\nVersion: <code> <version></code>\r\nPlayers:  <code> <players></code>\r\nText:\r\n<code><text></code>";

        internal static string ServerOffline = "\r\nStatus:  <code>  offline </code> ❓";

        // may be used to display extra info on the response (mods?)
        internal static string ExtraInfo = "\r\nInfo:\r\n<code><info></code>";

        internal static string CountJoin = "\r\n<code><count></code> <player> joined.";
        internal static string CountLeave = "\r\n<code><count></code> <player> left.";

        internal static string NameJoin = "\r\n+ <code> <name> </code>";
        internal static string NameLeave = "\r\n- <code> <name> </code> (<time>)";
    }
}