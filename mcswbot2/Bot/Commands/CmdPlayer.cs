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
                g.SendMsg("No servers watched. Use: /add", null);
                return;
            }

            var msg = "Player Online:";
            var plots = new List<PlottableData>();
            foreach (var item in g.Servers)
            {
                var status = item.Wrapped.Last;

                msg += "\r\n[<code>" + item.Label + "</code>] ";
                if (!status.HadSuccess) msg += "Offline";
                else msg += status.CurrentPlayerCount + " / " + status.MaxPlayerCount;

                if (TgBot.Conf.DrawPlots)
                {
                    var ud = GetUserData(item.Wrapped);
                    if(ud.Length > 1) plots.Add(ud);
                }

                // add player names or continue
                if (status.OnlinePlayers.Count <= 0) continue;
                var n = "";
                foreach (var plr in status.OnlinePlayers)
                {
                    if (!string.IsNullOrEmpty(n)) n += ", ";
                    var span = DateTime.Now - item.SeenTime[plr.Id];
                    n += plr.Name + " (" + span.ToString("hh:MM") + ")";
                }
                msg += "\r\nNames: <code>" + n + "</code>";
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