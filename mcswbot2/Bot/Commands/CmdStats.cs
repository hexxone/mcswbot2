using mcswbot2.Bot.Objects;
using System.Diagnostics;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Commands
{
    internal class CmdStats : ICommand
    {
        internal override string Command() => "stats";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            double totalSize = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;
            int serverCount = 0, userCount = 0;
            foreach (var gr in TgBot.TgGroups)
            {
                serverCount += gr.Servers.Count;
                userCount += gr.Servers.Sum(sr => sr.Wrapped.Last.CurrentPlayerCount);
            }

            var msg = "Global Bot stats:";
            msg += $"\r\n  known users:<code> {TgBot.TgUsers.Length}</code>";
            msg += $"\r\n  known groups:<code> {TgBot.TgGroups.Length}</code>";
            msg += $"\r\n  watched servers:<code> {serverCount}</code>";
            msg += $"\r\n  online MC users:<code> {userCount}</code>";
            msg += $"\r\n  live ram usage:<code> {totalSize:0.00} MB</code>";
            g.SendMsg(msg, null, ParseMode.Html);
        }
    }
}