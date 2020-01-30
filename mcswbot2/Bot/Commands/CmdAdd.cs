using mcswbot2.Lib.Event;
using mcswbot2.Lib;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System;

namespace mcswbot2.Bot.Commands
{
    internal class CmdAdd : ICommand
    {
        public override string Command() => "add";

        public override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            var use = "Usage: /add [label] [address] (port default 25565)";
            if (g.Servers.Count > 2 && m.From.Id != Config.DeveloperId)
            {
                Respond(m.Chat.Id, "Sorry - in order to avoid abuse, only up to 3 servers are allowed.");
                return;
            }
            if (args.Length < 3 || args.Length > 4)
            {
                Respond(m.Chat.Id, use);
                return;
            }
            if (g.GetServer(args[1]) != null)
            {
                Respond(m.Chat.Id, "Label name already taken.");
                return;
            }
            var addr = args[2];
            var port = 25565;
            if (args.Length == 4)
            {
                if (!int.TryParse(args[3], out port))
                {
                    Respond(m.Chat.Id, "Port is not a number.\r\n"+use);
                    return;
                }
            }

            // bypass ip check if user is developer
            if(true || m.From.Id != Config.DeveloperId)
            {
                try
                {
                    Utils.VerifyAddress(addr, port);
                }
                catch (Exception ex)
                {
                    Respond(m.Chat.Id, "Error adding server: " + ex.Message + "\r\n" + use);
                    return;
                }
            }

            g.AddServer(args[1], addr, port);
            Respond(m.Chat.Id, "Server added to watchlist: [" + EventBase.Wrap(Types.Formatting.Html, args[1]) + "]",
                ParseMode.Html);
        }
    }
}