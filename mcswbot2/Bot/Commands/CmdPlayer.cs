using System;
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
        public override string Command()
        {
            return "player";
        }

        public override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            var plotFile = true;

            var msg = "Online Player:";
            var plots = new List<PlottableData>();
            foreach (var item in g.Servers)
            {
                msg += "\r\n[<code>" + item.Bind_Label + "</code>] ";
                if (!item.Bind_ServerOnline) msg += "Offline";
                else msg += item.Bind_OnlinePlayers + " / " + item.Bind_MaxPlayers;

                if(plotFile) plots.Add(item.GetPlottableData());

                // add player names if any
                if (item.Bind_PlayerList.Count <= 0) continue;

                var n = "";
                foreach (var plr in item.Bind_PlayerList)
                {
                    if (!string.IsNullOrEmpty(n)) n += ", ";
                    n += Utils.FixMcChat(plr.Name);
                }

                msg += "\r\nNames: <code>" + n + "</code>";
            }

            if(plotFile && plots.Count > 0)
            {
                var bm = Utils.PlotData(plots.ToArray());
                using (MemoryStream ms = new MemoryStream())
                {
                    bm.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    var iof = new Telegram.Bot.Types.InputFiles.InputOnlineFile(ms);
                    TgBot.Client.SendPhotoAsync(m.Chat.Id, iof, msg, ParseMode.Html);
                }

            }
            else
            {
                Respond(m.Chat.Id, msg, ParseMode.Html);
            }
        }
    }
}