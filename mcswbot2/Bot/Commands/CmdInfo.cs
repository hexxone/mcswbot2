using System;
using System.Linq;
using mcswbot2.Bot.Objects;
using Telegram.Bot.Types;

namespace mcswbot2.Bot.Commands
{
    class CmdInfo : ICommand
    {
        internal override string Command() => "info";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            if (args.Length > 1)
            {
                var name = args[1];
                for (var i = 2; i < args.Length - 1; i++)
                    name += " " + args[i];

                name = name.Trim();
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
                    var (id, fullname) = s.NameHistory.FirstOrDefault(x => x.Value.ToLower().Contains(name) || x.Key.ToLower().Contains(name));
                    if (id == null || fullname == null) continue;
                    found = true;
                    // common info
                    msg += "\r\n--------------------";
                    msg += $"\r\n[<code>{s.Label}</code>";
                    msg += $"\r\n  Player: <code>{fullname}</code>";
                    // status & seen time
                    var online = s.Wrapped.Last.OnlinePlayers.FindAll(s => s.Name == name || s.Id == name).Count > 0;
                    var seenSpan = DateTime.Now - s.SeenTime[id];
                    msg += "\r\n  Status: <code>";
                    msg += online ? "Online" : "Offline";
                    msg += $" since ${seenSpan.TotalHours:0.00}h";
                    // playtime
                    msg += $"  On-Time: <code>{(seenSpan + s.PlayTime[id]).TotalDays:0.00} days</code>";

                }

                if (!found) msg = "Nothing found.";

                g.SendMsg(msg);
            }
            else
            {
                g.SendMsg("Use: /info <player-name-or-id>");
            }
        }
    }
}
