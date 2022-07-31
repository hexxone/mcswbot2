using System.Diagnostics;
using System.Linq;
using McswBot2.Objects;
using Telegram.Bot.Types;
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

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            var memUsage = Process.GetCurrentProcess().WorkingSet64 / 1024d / 1024d;
            var serverCount = McswBot.TgGroups.Aggregate(0,
                (current, group) => current + group.WatchedServers.Count);

            var msg = "Global Bot stats:";
            msg += $"\r\n  known users:<code>     {McswBot.TgUsers.Count}</code>";
            msg += $"\r\n  known groups:<code>    {McswBot.TgGroups.Count}</code>";
            msg += $"\r\n  watched servers:<code> {serverCount:0}</code>";
            msg += $"\r\n  Bot RAM usage:<code>   {memUsage:0.00} mb</code>";
            msg += "\r\n\r\nUse /stats [player] to search for Minecraft-Players.";
            g.SendMsg(msg, ParseMode.Html);
        }
    }
}