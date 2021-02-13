using System;
using mcswbot2.Bot.Objects;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static mcswbot2.Bot.SkiaPlotter;

namespace mcswbot2.Bot.Commands
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

            var msg = "Player Online:";
            var plots = new List<PlottableData>();
            foreach (var item in g.Servers)
            {
                var status = item.Wrapped.Last;

                msg += "\r\n[<code>" + item.Label + "</code>] ";
                if ((!status?.HadSuccess) ?? true) msg += " Offline";
                else msg += status.CurrentPlayerCount + " / " + status.MaxPlayerCount;

                if (TgBot.Conf.DrawPlots)
                {
                    var ud = GetUserData(item.Wrapped);
                    if(ud.Length > 4) plots.Add(ud);
                }

                // add player names or continue
                if ((status?.OnlinePlayers.Count ?? 0) <= 0) continue;
                var n = "";
                foreach (var plr in status.OnlinePlayers)
                {
                    var span = DateTime.Now - item.SeenTime[plr.Id];
                    n += $"\r\n  + {plr.Name} ({span.TotalHours:0.00} hrs)";
                }
                msg += "<code>" + n + "</code>";
            }

            if (TgBot.Conf.DrawPlots && plots.Count > 0)
            {
                using var bm = PlotData(plots, "Minutes Ago", "Player Online");
                g.SendMsg(msg, bm, ParseMode.Html);
            }
            else g.SendMsg(msg, null, ParseMode.Html);
        }
    }
}