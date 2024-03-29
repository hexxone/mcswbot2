﻿using McswBot2.Objects;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace McswBot2.Commands;

internal class CmdServers : ICommand
{
    internal override string Command()
    {
        return "servers";
    }

    internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
    {
        if (g.Servers.Count < 1)
        {
            g.SendMsg("No servers watched. Use: /add");
            return;
        }

        var msg = "Watchlist:<code> " + g.Servers.Count + " / 3</code>";
        foreach (var s in g.Servers)
        {
            msg += "\r\n=== == = = = = = = == ===";
            msg += "\r\n[<code>" + s.Label + "</code>] <b>" + s.Address + ":" + s.Port + "</b>";

            var status = s.Last;
            if (status != null && status.HadSuccess)
            {
                msg += "\r\n  Status:<code> Online</code> 🌐";
                msg += "\r\n  Version:<code> " + status.MinecraftVersion + "</code>";
                msg += "\r\n  MOTD:<code> " + status.FixedMotd + "</code>";
                msg += "\r\n  Player:<code> " + status.CurrentPlayerCount + " / " + status.MaxPlayerCount +
                       "</code>";
            }
            else
            {
                msg += "\r\n  Status:<code> Offline</code> ❌";
                if (status?.LastError != null)
                    msg += "\r\n  Error:<code> " + status.LastError + "</code>";
            }
        }

        g.SendMsg(msg, null, ParseMode.Html);
    }
}