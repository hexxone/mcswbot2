using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Commands
{
    internal class CmdList : ICommand
    {
        public override string Command() => "list";

        public override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            var msg = "Watchlist:<code> " + g.Servers.Count + " / 3</code>";
            foreach (var s in g.Servers)
            {
                msg += "\r\n=== == = = = = = = == ===";
                msg += "\r\n[<code>" + s.Label + "</code>] <b>" + s.Base.Address + ":" + s.Base.Port +
                       "</b>\r\n  Status: ";
                if (s.IsOnline)
                {
                    msg += "Online 🌐";
                    msg += "\r\n  Version:<code> " + s.Version;
                    msg += "</code>\r\n  MOTD:<code> " + s.MOTD;
                    msg += "</code>\r\n  Player:<code> " + s.PlayerCount + " / " + s.MaxPlayerCount + "</code>";
                }
                else
                {
                    msg += "Offline ❌";
                    msg += "\r\n  Error:<code> " + s.LastError + "</code>";
                }
            }

            Respond(m.Chat.Id, msg, ParseMode.Html);
        }
    }
}