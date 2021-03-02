using System.Diagnostics;
using System.Linq;
using mcswbot2.Objects;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Commands
{
    internal class CmdStats : ICommand
    {
        internal override string Command() => "stats";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            double totalSize = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;
            double serverCount = 0, userCount = 0;
            foreach (var gr in MCSWBot.TgGroups)
            {
                serverCount += gr.Servers.Count;
                userCount += gr.Servers.Sum(sr => sr.Wrapped.Last?.CurrentPlayerCount ?? 0);
            }

            var msg = "Global Bot stats:";
            msg += $"\r\n  known users:<code> {MCSWBot.TgUsers.Count}</code>";
            msg += $"\r\n  known groups:<code> {MCSWBot.TgGroups.Count}</code>";
            msg += $"\r\n  watched servers:<code> {serverCount:0}</code>";
            msg += $"\r\n  online MC users:<code> {userCount:0}</code>";
            msg += $"\r\n  live ram usage:<code> {totalSize:0.00} MB</code>";
            g.SendMsg(msg, null, ParseMode.Html);
        }
    }
}