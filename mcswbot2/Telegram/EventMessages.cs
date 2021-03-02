
namespace mcswbot2.Telegram
{
    /// <summary>
    ///     Public class representing Event messages.
    /// </summary>
    public static class EventMessages
    {
        internal static string ServerOnline = "\r\nServer status: <code>online</code> ++\r\nVersion: <code><version></code>\r\nPlayers: <code><players></code>\r\nMOTD:\r\n<code><text></code>";
        internal static string ServerOffline = "\r\nServer status: <code>offline</code> --";

        internal static string CountJoin = "\r\n<code><count></code> <player> joined.";
        internal static string CountLeave = "\r\n<code><count></code> <player> left.";

        internal static string NameJoin = "\r\n+ <code><name></code>";
        internal static string NameLeave = "\r\n- <code><name></code> (<time>)";
    }
}

