using System;
using System.Linq;
using McswBot2.Objects;
using McswBot2.Static;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace McswBot2.Commands
{
    internal class CmdPlayer : ICommand
    {
        internal override string Command()
        {
            return "player";
        }

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            if (g.WatchedServers.Count < 1)
            {
                g.SendMsg("No servers watched. Use: /add");
                return;
            }

            var pingResults = g.PingAllServers();

            var msg = "Player:";
            foreach (var (watcher, status) in pingResults)
            {
                msg += $"\r\n[<a href=\"{watcher.Address}:{watcher.Port}\">{watcher.Label}</a>] ";

                if (status is { HadSuccess: true })
                {
                    msg += $"🌐 " +
                           $"\r\n  Player:<code> {status.CurrentPlayerCount} / {status.MaxPlayerCount}</code>";

                    msg = status.OnlinePlayers.Aggregate(msg,
                        (current, plr) => current + $"\r\n  # {plr.Name}");
                }
                else
                {
                    msg += $"❌" +
                           $"\r\n  Player: ? / ?" +
                           $"\r\n  Error:<code> {status?.LastError?.ToString() ?? "Unknown"}</code>";
                }
            }

            g.SendMsg(msg, ParseMode.Html);
        }
    }
}