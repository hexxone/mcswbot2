using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System;
using mcswbot2.Bot.Objects;

namespace mcswbot2.Bot.Commands
{
    internal class CmdAdd : ICommand
    {
        internal override string Command() => "add";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            var use = "Usage: /add [label] [address] (port default 25565)";
            try
            {
                if (g.Servers.Count > 2 && !dev) throw new Exception("Only up to 3 servers are allowed per group.");
                if (args.Length < 3 || args.Length > 4) throw new Exception("Invalid arguments.");
                var lbl = args[1];
                Utils.VerifyLabel(lbl);
                if (g.GetServer(lbl) != null) throw new Exception("Label name already in use.");

                var addr = args[2];
                var port = 25565;
                // try to parse port if given
                if (args.Length == 4 && !int.TryParse(args[3], out port))
                    throw new Exception("Port is not a number.");

                try
                {
                    Utils.VerifyAddress(addr, port);
                }
                catch (Exception e)
                {
                    // bypass ip check if user is developer
                    if (dev) g.SendMsg("Verify Warning: " + e.Message);
                    else throw e;
                }

                // add & respond
                g.AddServer(lbl, addr, port);
                g.SendMsg("Server added to watchlist: [<code>" + lbl + "</code>]", null, ParseMode.Html);
            }
            catch (Exception ex)
            {
                g.SendMsg("Error adding server: " + ex.Message + "\r\n" + use);
            }
        }
    }
}