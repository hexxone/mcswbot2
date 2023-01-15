using McswBot2.Static;
using System.Diagnostics;
using Telegram.Bot.Types.Enums;

namespace McswBot2.Commands
{
    internal class CmdStats : ICommand
    {
        private const string PlaceHolder = "--------------------";

        internal override string Command()
        {
            return "stats";
        }

        internal override void Call(ICommandArgs a)
        {
            var (bot, _, group, _, _, _) = a;

            var memUsage = Process.GetCurrentProcess().WorkingSet64 / 1024d / 1024d;
            var serverCount = bot.Conf.WatchedServers.Count;

            var txt = "Global Bot stats:";
            txt += $"\r\n  Servers:<code>   {serverCount:0}</code>";
            txt += $"\r\n  RAM use:<code>   {memUsage:0.00} mb</code>";
            group.SendMsg(bot.Client!, txt, ParseMode.Html);
        }
    }
}