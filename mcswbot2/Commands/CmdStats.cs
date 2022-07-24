using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using McswBot2.Event;
using McswBot2.Minecraft;
using McswBot2.Objects;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace McswBot2.Commands;

internal class CmdStats : ICommand
{
    private const string PlaceHolder = "--------------------";

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
            var msg = new StringBuilder($"Search results for: '{name}'");
            msg.AppendLine(PlaceHolder);
            foreach (var s in g.Servers)
            {
                // check known partial name
                var player = s.Watcher?
                    .AllPlayers
                    .FirstOrDefault(x =>
                        (x.Name != null && x.Name.ToLower().Contains(name)) ||
                        (x.Id != null && x.Id.ToLower().Contains(name)));

                if (player == null)
                    continue;

                found = true;
                msg = AppendStatMsg(msg, s, player);
            }

            if (!found)
                msg.AppendLine("Nothing found.");

            g.SendMsg(msg.ToString(), null, ParseMode.Html);
        }
        else
        {
            var totalSize = Process.GetCurrentProcess().WorkingSet64 / 1024d / 1024d;
            double serverCount = 0, userCount = 0;
            foreach (var gr in McswBot.TgGroups)
            {
                serverCount += gr.Servers.Count;
                userCount += gr.Servers.Sum(sr => sr.Last?.CurrentPlayerCount ?? 0);
            }

            var msg = "Global Bot stats:";
            msg += $"\r\n  known users:<code> {McswBot.TgUsers.Count}</code>";
            msg += $"\r\n  known groups:<code> {McswBot.TgGroups.Count}</code>";
            msg += $"\r\n  watched servers:<code> {serverCount:0}</code>";
            msg += $"\r\n  online MC users:<code> {userCount:0}</code>";
            msg += $"\r\n  live ram usage:<code> {totalSize:0.00} MB</code>";
            msg += "\r\n\r\nUse /stats [player] to search for Minecraft-Players.";
            g.SendMsg(msg, null, ParseMode.Html);
        }
    }

    private static StringBuilder AppendStatMsg(StringBuilder msg, ServerStatus s, PlayerPayLoad player)
    {
        // common info
        msg.AppendLine($"[<code>{s.Label}</code>]");
        msg.AppendLine($"  Player: <code>{player.Name}</code>");
        // status & seen time
        var seenSpan = DateTime.Now - player.LastSeen;
        msg.AppendLine("  Status: <code>");
        msg.AppendLine((player.Online ? "Online" : "Offline") + $" since {seenSpan.TotalHours:0.00}h</code>");
        // playtime
        msg.AppendLine($"  Playtime: <code>{(seenSpan + player.PlayTime).TotalDays:0.00} days</code>");
        msg.AppendLine(PlaceHolder);
        return msg;
    }
}