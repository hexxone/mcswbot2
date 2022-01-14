using mcswbot2.Objects;
using mcswbot2.Static;
using System.Linq;
using Telegram.Bot.Types;
using static mcswbot2.Static.SkiaPlotter;

namespace mcswbot2.Commands
{
    internal class CmdPing : ICommand
    {
        internal override string Command()
        {
            return "ping";
        }

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            // collect Plot Data
            if (g.Servers.Count > 0)
            {
                var scaleTxt = SkiaPlotter.GetTimeScale(g.Servers, out double minuteRange);

                var plots = g.Servers.Select(srv => GetPingData(srv, minuteRange)) /*.Where(pd => pd.Length > 0)*/.ToList();

                using var bm = PlotData(plots, scaleTxt, "Response time (ms)");
                g.SendMsg(null, bm);
            }
            else
            {
                g.SendMsg("Please /add a server  first.");
            }
        }
    }
}