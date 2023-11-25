using McswBot2.Static;
using System;
using System.Linq;
using Telegram.Bot.Types.Enums;

namespace McswBot2.Commands
{
    internal class CmdDetails : ICommand
    {
        internal override string Command()
        {
            return "details";
        }

        internal override void Call(ICommandArgs a)
        {
            var (bot, msg, group, user, args, isDev) = a;
            if (!bot.Conf.WatchedServers.Any())
            {
                group.SendMsg(bot.Client!, "No servers watched - Please adjust your config.");
                return;
            }

            var pingResults = bot.Conf.WatchedServers.PingAll(bot.Conf.TimeoutMs, bot.Conf.Retries, bot.Conf.RetryMs);

            var txt = "Servers:<code> " + bot.Conf.WatchedServers.Count + "</code>";
            foreach (var (watcher, status) in pingResults)
            {
                txt += "\r\n=== == = = = = = = == ===";
                txt += $"\r\n[<a href=\"{watcher.Address}:{watcher.Port}\">{watcher.Label}</a>] ";

                if (status is { HadSuccess: true })
                {
                    txt += $"🌐 " +
                           $"\r\n  Version:<code> {status.MinecraftVersion}</code>" +
                           $"\r\n  Ping:<code> {status.RequestTime:0} ms</code>" +
                           $"\r\n  MotD:<code> {status.FixedMotd?.Replace("\r\n", "\r\n  ")}</code>" +
                           $"\r\n  Player:<code> {status.CurrentPlayerCount} / {status.MaxPlayerCount}</code>";

                    txt = status.OnlinePlayers.Aggregate(txt,
                        (current, plr) => current + $"\r\n  # {plr.Name}");
                }
                else
                {
                    txt += $"❌" +
                           $"\r\n  Error: <code>{status?.LastError?.Message ?? "Unknown"}</code>";
                }
            }

            group.SendMsg(bot.Client!, txt, ParseMode.Html);
        }
    }
}