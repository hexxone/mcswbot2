using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;

namespace mcswbot2.Bot.Commands
{
    class CmdSven : ICommand
    {
        public override string Command() => "sven";

        public override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            Respond(m.Chat.Id, "stinkt", Telegram.Bot.Types.Enums.ParseMode.Default, m.MessageId);
        }
    }
}
