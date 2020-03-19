using mcswbot2.Bot.Objects;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
                msg += "\r\n[<code>" + item.Label + "</code>] ";
                if (!item.Wrapped.IsOnline) msg += "Offline";
                else msg += item.Wrapped.PlayerCount + " / " + item.Wrapped.MaxPlayerCount;

                if (TgBot.Conf.DrawPlots)
                {
                    var ud = Imaging.GetUserData(item.Wrapped);
                    if(ud.DataX.Length > 0) plots.Add(ud);
                }

                // add player names or continue
                if (item.Wrapped.PlayerList.Count <= 0) continue;
                var n = "";
                foreach (var plr in item.Wrapped.PlayerList)
                {
                    if (!string.IsNullOrEmpty(n)) n += ", ";
                    n += plr.Name;
                }
                msg += "\r\nNames: <code>" + n + "</code>";
            }

            if (TgBot.Conf.DrawPlots && plots.Count > 0)
                using (var bm = Imaging.PlotData(plots.ToArray(), "Minutes Ago", "Player Online"))
                    g.SendMsg(msg, bm, ParseMode.Html);
            else
                g.SendMsg(msg, null, ParseMode.Html);
        }
    }
}