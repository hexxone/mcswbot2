using mcswbot2.Bot.Objects;
using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Commands
{
    internal class CmdRemove : ICommand
    {
        internal override string Command() => "remove";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            var usage = "Usage: /remove [label]";
            try
            {
                if (args.Length != 2) throw new Exception("Invalid arguments.");
                var lbl = args[1];
                Utils.VerifyLabel(lbl);

                if (!g.RemoveServer(lbl)) throw new Exception("Label not found.");

                g.SendMsg("Server removed from watchlist: [<code>" + lbl + "</code>]", null, ParseMode.Html);
            }
            catch (Exception ex)
            {
                g.SendMsg("Error removing server: " + ex.Message + "\r\n" + usage);
            }
        }
    }
}