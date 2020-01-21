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
                msg += "\r\n[<code>" + s.Bind_Label + "</code>] <b>" + s.Bind_Host + ":" + s.Bind_Port +
                       "</b>\r\n  Status: ";
                if (s.Bind_ServerOnline)
                {
                    msg += "Online 🌐";
                    msg += "\r\n  Version:<code> " + s.Bind_Version;
                    msg += "</code>\r\n  MOTD:<code> " + Utils.FixMcChat(s.Bind_MOTD);
                    msg += "</code>\r\n  Player:<code> " + s.Bind_OnlinePlayers + " / " + s.Bind_MaxPlayers + "</code>";
                }
                else
                {
                    msg += "Offline ❌";
                    msg += "  Error:<code> " + s.Bind_Error + "</code>";
                }
            }

            Respond(m.Chat.Id, msg, ParseMode.Html);
        }
    }
}