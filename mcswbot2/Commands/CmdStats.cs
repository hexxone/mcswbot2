using mcswbot2.Objects;
using System;
using System.Diagnostics;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Commands
{
    internal class CmdStats : ICommand
    {
        internal override string Command()
        {
            return "stats";
        }

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {

            if (args.Length > 1)
            {
                var name = args[1];
                for (var i = 2; i < args.Length - 1; i++)
                    name += " " + args[i];

                name = name.Trim().ToLower();
                if (name.Length < 3)
                {
                    g.SendMsg("Search for at least 3 characters!");
                    return;
                }

                var found = false;
                var msg = "Search results for: '" + name + "'";
                foreach (var s in g.Servers)
                {
                    // check known partial name
                    var player = s.Watcher.AllPlayers.FirstOrDefault(x =>
                        x.Name.ToLower().Contains(name) || x.Id.ToLower().Contains(name));
                    if (player == null) continue;
                    found = true;
                    // common info
                    msg += "\r\n--------------------";
                    msg += $"\r\n[<code>{s.Label}</code>]";
                    msg += $"\r\n  Player: <code>{player.Name}</code>";
                    // status & seen time
                    var seenSpan = DateTime.Now - player.LastSeen;
                    msg += "\r\n  Status: <code>";
                    msg += player.Online ? "Online" : "Offline";
                    msg += $" since {seenSpan.TotalHours:0.00}h</code>";
                    // playtime
                    msg += $"\r\n  Playtime: <code>{(seenSpan + player.PlayTime).TotalDays:0.00} days</code>";
                }

                if (!found) msg = "Nothing found.";

                g.SendMsg(msg, null, ParseMode.Html);
            }
            else
            {

                double totalSize = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;
                double serverCount = 0, userCount = 0;
                foreach (var gr in MCSWBot.TgGroups)
                {
                    serverCount += gr.Servers.Count;
                    userCount += gr.Servers.Sum(sr => sr.Last?.CurrentPlayerCount ?? 0);
                }

                var msg = "Global Bot stats:";
                msg += $"\r\n  known users:<code> {MCSWBot.TgUsers.Count}</code>";
                msg += $"\r\n  known groups:<code> {MCSWBot.TgGroups.Count}</code>";
                msg += $"\r\n  watched servers:<code> {serverCount:0}</code>";
                msg += $"\r\n  online MC users:<code> {userCount:0}</code>";
                msg += $"\r\n  live ram usage:<code> {totalSize:0.00} MB</code>";
                msg += "\r\n\r\nUse /stats [player] to search for Minecraft-Players.";
                g.SendMsg(msg, null, ParseMode.Html);
            }


        }
    }
}