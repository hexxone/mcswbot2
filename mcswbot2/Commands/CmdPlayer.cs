using System;
using System.Linq;
using mcswbot2.Objects;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Commands
{
    internal class CmdPlayer : ICommand
    {
        internal override string Command() => "player";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            if(g.Servers.Count < 1)
            {
                g.SendMsg("Please /add a server first.");
                return;
            }


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
                    var (id, fullname) = s.NameHistory.FirstOrDefault(x => x.Value.ToLower().Contains(name) || x.Key.ToLower().Contains(name));
                    if (id == null || fullname == null) continue;
                    found = true;
                    // common info
                    msg += "\r\n--------------------";
                    msg += $"\r\n[<code>{s.Label}</code>]";
                    msg += $"\r\n  Player: <code>{fullname}</code>";
                    // status & seen time
                    var online = s.Wrapped.Last != null && (s.Wrapped.Last.OnlinePlayers.FindAll(s => s.Id == id).Count > 0);
                    var seenSpan = DateTime.Now - s.SeenTime[id];
                    msg += "\r\n  Status: <code>";
                    msg += online ? "Online" : "Offline";
                    msg += $" since {seenSpan.TotalHours:0.00}h</code>";
                    // playtime
                    msg += $"\r\n  Playtime: <code>{(seenSpan + s.PlayTime[id]).TotalDays:0.00} days</code>";

                }

                if (!found) msg = "Nothing found.";

                g.SendMsg(msg, null, ParseMode.Html);
            }
            else
            {
                var res = g.SendPlayerMessage();
                if (res != null) g.LivePlayerMsgId = res.MessageId;
            }
        }
    }
}