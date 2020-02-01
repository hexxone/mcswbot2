using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Commands
{
    class CmdSven : ICommand
    {
        public override string Command() => "sven";

        public override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            Respond(m.Chat.Id, "stinkt", ParseMode.Default, m.MessageId);
        }
    }
}
