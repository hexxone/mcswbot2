using mcswbot2.Objects;
using System;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Commands
{
    internal class CmdPlayers : ICommand
    {
        internal override string Command()
        {
            return "players";
        }

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            if (g.Servers.Count < 1)
            {
                g.SendMsg("Please /add a server first.");
                return;
            }

            var res = g.SendPlayersMessage();

            if (res != null) g.LivePlayerMsgId = res.MessageId;
        }
    }
}