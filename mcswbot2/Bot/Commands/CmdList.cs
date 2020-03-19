using mcswbot2.Bot.Objects;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Commands
{
    internal class CmdList : ICommand
    {
        internal override string Command() => "list";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            if (g.Servers.Count < 1)
            {
                g.SendMsg("No servers watched. Use: /add", null);
                return;
            }
            var msg = "Watchlist:<code> " + g.Servers.Count + " / 3</code>";
            foreach (var s in g.Servers)
            {
                msg += "\r\n=== == = = = = = = == ===";
                msg += "\r\n[<code>" + s.Label + "</code>] <b>" + s.Address + ":" + s.Port + "</b>\r\n  Status: ";
                if (s.Wrapped.IsOnline)
                {
                    msg += "Online 🌐";
                    msg += "\r\n  Version:<code> " + s.Wrapped.Version;
                    msg += "</code>\r\n  MOTD:<code> " + s.Wrapped.MOTD;
                    msg += "</code>\r\n  Player:<code> " + s.Wrapped.PlayerCount + " / " + s.Wrapped.MaxPlayerCount + "</code>";
                }
                else
                {
                    msg += "Offline ❌";
                    msg += "\r\n  Error:<code> " + s.Wrapped.LastError + "</code>";
                }
            }

            g.SendMsg(msg, null, ParseMode.Html);
        }
    }
}