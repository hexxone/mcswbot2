using mcswbot2.Lib.Event;
using mcswbot2.Lib;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Commands
{
    internal class CmdAdd : ICommand
    {
        public override string Command() => "add";

        public override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            if (g.Servers.Count > 2 && u.Base.Id != Config.DeveloperId)
            {
                Respond(m.Chat.Id, "Sorry - in order to avoid abuse, only up to 3 servers are allowed.");
                return;
            }
            if (args.Length != 3)
            {
                Respond(m.Chat.Id, "Usage: /add [label] [address:port]");
                return;
            }
            if (g.GetServer(args[1]) != null)
            {
                Respond(m.Chat.Id, "Label name already taken.");
                return;
            }
            var addr = args[2];
            var port = 25565;
            if (addr.Contains(":"))
            {
                var spl = addr.Split(':');
                addr = spl[0];
                port = int.Parse(spl[1]);
            }

            g.AddServer(args[1], addr, port);
            Respond(m.Chat.Id, "Server added to watchlist: [" + EventBase.Wrap(Types.Formatting.Html, args[1]) + "]",
                ParseMode.Html);
        }
    }
}