using McswBot2.Static;
using Telegram.Bot.Types.Enums;

namespace McswBot2.Commands
{
    internal class CmdStart : ICommand
    {
        internal override string Command()
        {
            return "start";
        }

        internal override void Call(ICommandArgs a)
        {
            var (b, m, g, u, args, isDev) = a;

            var txt =
                "This bot keeps track of your MineCraft servers and the online users with the 'server list' information.";
            txt += " It should support any server version upward from 'beta' - including ModPacks!";
            txt += "\r\n<a href=\"https://github.com/hexxone/mcswbot2\">Bot Source (github)</a>";

            g.SendMsg(b.Client!, txt, ParseMode.Html);
        }
    }
}