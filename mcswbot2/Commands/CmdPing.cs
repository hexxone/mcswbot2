using System.Linq;
using McswBot2.Objects;
using Telegram.Bot.Types;
using static McswBot2.Static.SkiaPlotter;

namespace McswBot2.Commands;

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
            var scaleTxt = GetTimeScale(g.Servers, out var minuteRange);

            var plots = g.Servers.Select(srv => GetPingData(srv, minuteRange)) /*.Where(pd => pd.Length > 0)*/
                .ToList();

            if (plots.Count > 0)
            {
                using var bm = PlotData(plots, scaleTxt, "Response time (ms)");
                g.SendMsg(null, bm);
            }
            else
            {
                g.SendMsg("Not enough data. Please try again later.");
            }
        }
        else
        {
            g.SendMsg("Please /add a server  first.");
        }
    }
}