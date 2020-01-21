using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static mcswbot2.Lib.Types;

namespace mcswbot2.Bot.Commands
{
    class CmdPing : ICommand
    {
        public override string Command() => "ping";

        public override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            var plots = new List<PlottableData>();
            foreach (var item in g.Servers)
            {
                plots.Add(item.GetPingData());
            }

            if (plots.Count > 0)
            {
                using (var bm = Utils.PlotData(plots.ToArray(), "Minutes Ago", "Reponse time (ms)"))
                {
                    using (var ms = new MemoryStream())
                    {
                        bm.Save(ms, ImageFormat.Png);
                        bm.Dispose();
                        ms.Position = 0;
                        var iof = new Telegram.Bot.Types.InputFiles.InputOnlineFile(ms);
                        TgBot.Client.SendPhotoAsync(m.Chat.Id, iof).Wait();
                        ms.Close();
                    }
                }
            }
        }
    }
}
