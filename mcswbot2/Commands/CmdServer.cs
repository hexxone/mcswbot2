using System;
using System.Linq;
using McswBot2.Objects;
using McswBot2.Static;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace McswBot2.Commands
{
    internal class CmdServer : ICommand
    {
        internal override string Command()
        {
            return "server";
        }

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            if (g.WatchedServers.Count < 1)
            {
                g.SendMsg("No servers watched. Use: /add");
                return;
            }

            var pingResults = g.PingAllServers();

            var msg = "Servers:<code> " + g.WatchedServers.Count + " / 3</code>";
            foreach (var (watcher, status) in pingResults)
            {
                msg += "\r\n=== == = = = = = = == ===";
                msg += $"\r\n[<a href=\"{watcher.Address}:{watcher.Port}\">{watcher.Label}</a>] ";

                if (status is { HadSuccess: true })
                {
                    msg += $"🌐 " +
                           $"\r\n  Version:<code> {status.MinecraftVersion}</code>" +
                           $"\r\n  Ping:<code> {status.RequestTime:0} ms</code>" +
                           $"\r\n  MotD:<code> {status.FixedMotd?.Replace("\r\n", "\r\n  ")}</code>"+
                           $"\r\n  Player:<code> {status.CurrentPlayerCount} / {status.MaxPlayerCount}</code>";

                    msg = status.OnlinePlayers.Aggregate(msg,
                        (current, plr) => current + $"\r\n  # {plr.Name}");
                }
                else
                {
                    msg += $"❌" +
                           $"\r\n  Offline: <code> Error {status?.LastError?.ToString() ?? "Unknown"}</code>";
                }
            }

            g.SendMsg(msg, ParseMode.Html);
        }
    }
}