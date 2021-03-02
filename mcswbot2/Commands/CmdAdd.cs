using System;
using mcswbot2.Objects;
using mcswbot2.Static;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Commands
{
    internal class CmdAdd : ICommand
    {
        internal override string Command() => "add";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            var use = "Usage: /add [label] [address] (port default 25565)";
            try
            {
                // verification
                if (g.Servers.Count > 2 && !dev) throw new Exception("Only up to 3 servers are allowed per group.");
                if (args.Length < 3 || args.Length > 4) throw new Exception("Invalid arguments.");
                var serverLabel = args[1];
                Utils.VerifyLabel(serverLabel);
                if (g.GetServer(serverLabel) != null) throw new Exception("Label name already in use.");

                // get target
                var addr = args[2];
                var port = 25565;
                
                // try to parse notation <address> <port>
                if (args.Length == 4 && !int.TryParse(args[3], out port))
                    throw new Exception("Port is not a number.");

                // try to parse notation <address:port>
                if (args.Length == 3 && args[2].Contains(":"))
                {
                    var splits = args[2].Split(":");
                    if (splits.Length == 2 && int.TryParse(splits[1], out port))
                        addr = splits[0];
                }

                try
                {
                    // more complicated verification
                    Utils.VerifyAddress(addr, port);
                }
                catch (Exception e)
                {
                    // bypass ip check if user is developer
                    if (dev) g.SendMsg("Verify Warning: " + e.Message);
                    else throw;
                }

                // add & respond
                g.AddServer(serverLabel, addr, port);
                g.SendMsg("Server added to watchlist: [<code>" + serverLabel + "</code>]", null, ParseMode.Html);
            }
            catch (Exception ex)
            {
                g.SendMsg("Error adding server: " + ex.Message + "\r\n" + use);
            }
        }
    }
}