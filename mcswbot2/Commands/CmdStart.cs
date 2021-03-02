using mcswbot2.Objects;
using Telegram.Bot.Types;

namespace mcswbot2.Commands
{
    internal class CmdStart : ICommand
    {
        internal override string Command() => "start";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            g.SendMsg("This bot keeps track of your MineCraft servers and the online users."
                + " It should support any server version upward from beta 1.3.1 including ModPacks since it only uses the 'server list' information.");
        }
    }
}