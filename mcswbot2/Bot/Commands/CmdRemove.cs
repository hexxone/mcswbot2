using mcswbot2.Lib.Event;
using mcswbot2.Lib;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Commands
{
    internal class CmdRemove : ICommand
    {
        public override string Command() => "remove";

        public override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            if (args.Length != 2)
            {
                Respond(m.Chat.Id, "Usage: /remove [label]");
                return;
            }

            var res = g.RemoveServer(args[1]);
            var msg = res
                ? "Server removed from watchlist: [" + EventBase.Wrap(Types.Formatting.Html, args[1]) + "]"
                : "Label not found.";
            Respond(m.Chat.Id, msg, ParseMode.Html);
        }
    }
}