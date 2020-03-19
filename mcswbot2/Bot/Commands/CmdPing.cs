using mcswbot2.Bot.Objects;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace mcswbot2.Bot.Commands
{
    class CmdPing : ICommand
    {
        internal override string Command() => "ping";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            var plots = new List<PlottableData>();
            foreach (var item in g.Servers)
            {
                var pd = Imaging.GetPingData(item.Wrapped);
                if(pd.DataX.Length > 0) plots.Add(pd);
            }
            if (plots.Count > 0)
                using (var bm = Imaging.PlotData(plots.ToArray(), "Minutes Ago", "Response time (ms)"))
                    g.SendMsg(null, bm);
            else
                g.SendMsg("Not enough data. Did you /add a server?");
        }
    }
}
