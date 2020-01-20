﻿using System.Diagnostics;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Commands
{
    internal class CmdStats : ICommand
    {
        public override string Command()
        {
            return "stats";
        }

        public override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            var msg = "Bot stats:";
            msg += "\r\n  known users:<code> " + TgBot.TgUsers.Count;
            msg += "</code>\r\n  known groups:<code> " + TgBot.TgGroups.Count;
            var serverCount = 0;
            var userCount = 0;
            foreach (var gr in TgBot.TgGroups)
            {
                serverCount += gr.Servers.Count;
                foreach (var sr in gr.Servers)
                    userCount += sr.Bind_OnlinePlayers;
            }

            msg += "</code>\r\n  watched servers:<code> " + serverCount;
            msg += "</code>\r\n  online MC users:<code> " + userCount;

            double totalSize = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;
            msg += $"</code>\r\n  memory usage:<code> {totalSize:0.00} MB";

            Respond(m.Chat.Id, msg + "</code>", ParseMode.Html);
        }
    }
}