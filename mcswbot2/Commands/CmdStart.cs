using McswBot2.Objects;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace McswBot2.Commands;

internal class CmdStart : ICommand
{
    internal override string Command()
    {
        return "start";
    }

    internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
    {
        var msg =
            "This bot keeps track of your MineCraft servers and the online users with the 'server list' information.";
        msg += " It should support any server version upward from 'beta' - including ModPacks!";
        msg += "\r\n<a href=\"https://github.com/hexxone/mcswbot2\">Bot Source (github)</a>";

        /*
        msg += "\r\nBot Commands:<code>";
        msg += "\r\n  /start - this";
        msg += "\r\n  /players - Server players (Live)";
        msg += "\r\n  /ping - Server response time (ms)";
        msg += "\r\n  /notify - Notification settings";
        msg += "\r\n  /add - Add a server to watch-list";
        msg += "\r\n  /servers - List of watched servers";
        msg += "\r\n  /remove - Stop watching a server";
        msg += "\r\n  /stats [user] - Bot / User statistics";
        msg += "</code>";
        */

        g.SendMsg(msg, pm: ParseMode.Html);
    }
}