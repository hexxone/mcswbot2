using mcswbot2.Lib;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Commands
{
    internal class CmdNotify : ICommand
    {
        public override string Command()
        {
            return "notify";
        }

        public override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            var usage = "Usage: /notify <label> (<option> [true|false])";
            usage += "\r\nOptions:";
            usage += "\r\n- state (server online status)";
            usage += "\r\n- count (server user count)";
            usage += "\r\n- name (player name samples)";

            switch (args.Length)
            {
                default:
                    Respond(m.Chat.Id, usage);
                    break;
                case 2:
                    var srv = g.GetServer(args[1]);
                    if (srv != null) Respond(m.Chat.Id, GetSrvNotifications(srv), ParseMode.Html);
                    else Respond(m.Chat.Id, "Server label not found.\r\n\r\n" + usage);
                    break;
                case 4:
                    var srv2 = g.GetServer(args[1]);
                    if (srv2 != null)
                    {
                        var argl = args[3].ToLower() == "true";
                        switch (args[2].ToLower())
                        {
                            case "state":
                                srv2.Bind_ServerNotify = argl;
                                break;
                            case "count":
                                srv2.Bind_CountNotify = argl;
                                break;
                            case "name":
                                srv2.Bind_PlayerNotify = argl;
                                break;
                            default:
                                Respond(m.Chat.Id, "Unknown setting.\r\n\r\n" + usage);
                                return;
                        }
                        Respond(m.Chat.Id, GetSrvNotifications(srv2), ParseMode.Html);
                    }
                    else Respond(m.Chat.Id, "Server label not found.\r\n\r\n" + usage);
                    break;
            }
        }

        private static string GetSrvNotifications(ServerStatus srv)
        {
            var msg = "[" + srv.Bind_Label + "] Notifications:";
            msg += "\r\nState change:<code> " + srv.Bind_ServerNotify;
            msg += "</code>\r\nCount change:<code> " + srv.Bind_CountNotify;
            msg += "</code>\r\nPlayer change:<code> " + srv.Bind_PlayerNotify;
            return msg + "</code>";
        }
    }
}