using mcswbot2.Objects;
using mcswbot2.ServerStatus;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Commands
{
    internal class CmdNotify : ICommand
    {
        internal override string Command() => "notify";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            var usage = "Usage: /notify <label> (<option> [true|false])";
            usage += "\r\nOptions:";
            usage += "\r\n- state (server online status)";
            usage += "\r\n- count (server user count)";
            usage += "\r\n- name (player name samples)";
            usage += "\r\n- sticker (send sticker)";

            switch (args.Length)
            {
                default:
                    g.SendMsg(usage);
                    break;
                case 2:
                    var srv = g.GetServer(args[1]);
                    if (srv != null) g.SendMsg(GetSrvNotifications(srv), null, ParseMode.Html);
                    else g.SendMsg("Server label not found.\r\n\r\n" + usage);
                    break;
                case 4:
                    var srv2 = g.GetServer(args[1]);
                    if (srv2 != null)
                    {
                        var argl = args[3].ToLower() == "true";
                        switch (args[2].ToLower())
                        {
                            case "state":
                                srv2.NotifyServer = argl;
                                break;
                            case "count":
                                srv2.NotifyCount = argl;
                                break;
                            case "name":
                                srv2.NotifyNames = argl;
                                break;
                            case "sticker":
                                srv2.Sticker = argl;
                                break;
                            default:
                                g.SendMsg("Unknown setting.\r\n\r\n" + usage);
                                return;
                        }
                        g.SendMsg(GetSrvNotifications(srv2), null, ParseMode.Html);
                    }
                    else g.SendMsg("Server label not found.\r\n\r\n" + usage);
                    break;
            }
        }

        private static string GetSrvNotifications(ServerStatusWrapped wra)
        {
            var msg = "[<code>" + wra.Wrapped.Label + "</code>] Notifications:";
            msg += "\r\nState change:<code> " + wra.NotifyServer + "</code>";
            msg += "\r\nCount change:<code> " + wra.NotifyCount + "</code>";
            msg += "\r\nPlayer change:<code> " + wra.NotifyNames + "</code>";
            msg += "\r\nSend Sticker:<code> " + wra.Sticker + "</code>";
            return msg;
        }
    }
}