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
            try
            {
                if (g.Servers.Count > 2 && !dev) throw new Exception("Only up to 3 servers are allowed per group.");
                if (args.Length < 3 || args.Length > 4) throw new Exception("Invalid arguments.");
                Utils.VerifyLabel(args[1]);
                if (g.GetServer(args[1]) != null) throw new Exception("Label name already in use.");

                var addr = args[2];
                var port = 25565;
                // try to parse port if given
                if (args.Length == 4 && !int.TryParse(args[3], out port))
                    throw new Exception("Port is not a number.");

                try
                {
                    Utils.VerifyAddress(addr, port);
                }
                catch(Exception e)
                {
                    // bypass ip check if user is developer
                    if (dev) Respond(m.Chat.Id, "Verify Warning: " + e.Message);
                    else throw e;
                }

                // add & respond
                g.AddServer(args[1], addr, port);
                Respond(m.Chat.Id, "Server added to watchlist: [" + EventBase.Wrap(Types.Formatting.Html, args[1]) + "]",
                    ParseMode.Html);
            }
            catch (Exception ex)
            {
                Respond(m.Chat.Id, "Error adding server: " + ex.Message + "\r\n" + use);
            }
        }
    }
}