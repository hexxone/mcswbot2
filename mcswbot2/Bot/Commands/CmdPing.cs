using mcswbot2.Bot.Objects;
using System.Linq;
using Telegram.Bot.Types;
using static mcswbot2.Bot.SkiaPlotter;

namespace mcswbot2.Bot.Commands
{
    class CmdPing : ICommand
    {
        internal override string Command() => "ping";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            // collect Plot Data
            var plots = g.Servers.Select(item => GetPingData(item.Wrapped)).Where(pd => pd.Length > 4).ToList();
            if (plots.Count > 0)
            {
                using var bm = PlotData(plots, "Minutes Ago", "Response time (ms)");
                g.SendMsg(null, bm);
            }
            else if (g.Servers.Count < 1) g.SendMsg("Please /add a server  first.");
            else g.SendMsg("Not enough data. Please wait for up to 10 minutes...");
        }
    }
}
