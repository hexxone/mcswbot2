using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static mcswbot2.Lib.Types;

namespace mcswbot2.Bot.Commands
{
    internal class CmdPlayer : ICommand
    {
        public override string Command() => "player";

        public override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            var plotFile = true;

            var msg = "Online Player:";
            var plots = new List<PlottableData>();
            foreach (var item in g.Servers)
            {
                msg += "\r\n[<code>" + item.Label + "</code>] ";
                if (!item.IsOnline) msg += "Offline";
                else msg += item.PlayerCount + " / " + item.MaxPlayerCount;

                if (plotFile) plots.Add(item.GetUserData());

                // add player names if any
                if (item.PlayerList.Count <= 0) continue;

                var n = "";
                foreach (var plr in item.PlayerList)
                {
                    if (!string.IsNullOrEmpty(n)) n += ", ";
                    n += Utils.FixMcChat(plr.Name);
                }

                msg += "\r\nNames: <code>" + n + "</code>";
            }

            if (plotFile && plots.Count > 0)
            {
                using (var bm = Utils.PlotData(plots.ToArray(), "Minutes Ago", "Player Online"))
                {
                    using (var ms = new MemoryStream())
                    {
                        bm.Save(ms, ImageFormat.Png);
                        bm.Dispose();
                        ms.Position = 0;
                        var iof = new Telegram.Bot.Types.InputFiles.InputOnlineFile(ms);
                        TgBot.Client.SendPhotoAsync(m.Chat.Id, iof, msg, ParseMode.Html).Wait();
                        ms.Close();
                    }
                }
            }
            else
            {
                Respond(m.Chat.Id, msg, ParseMode.Html);
            }
        }
    }
}