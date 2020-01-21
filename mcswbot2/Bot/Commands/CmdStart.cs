using Telegram.Bot.Types;

namespace mcswbot2.Bot.Commands
{
    internal class CmdStart : ICommand
    {
        public override string Command() => "start";

        public override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            Respond(m.Chat.Id,
                "This bot keeps track of your Minecraft servers and the online users. It should support any server version upward from beta 1.3.1 including Modpacks since it only uses the 'server list' information.");
        }
    }
}