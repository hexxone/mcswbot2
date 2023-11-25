using McswBot2.Static;
using System;
using System.Linq;
using System.Text;
using Telegram.Bot.Types.Enums;

namespace McswBot2.Commands
{
    internal class CmdPlayer : ICommand
    {
        internal override string Command()
        {
            return "player";
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

            var sb = new StringBuilder();
            foreach (var (watcher, status) in pingResults)
            {
                var serverPrefix = $"[<a href=\"{watcher.Address}:{watcher.Port}\">{watcher.Label}</a>] ";
                sb.Append(serverPrefix);

                if (status is { HadSuccess: true })
                {
                    var successLine = $"🌐 <code>{status.CurrentPlayerCount}/{status.MaxPlayerCount}</code> ({status.RequestTime:##.##} ms)";
                    sb.Append(successLine);

                    foreach (var player in status.OnlinePlayers)
                    {
                        var playerLine = $"\r\n  # {player.Name}";
                        sb.Append(playerLine);
                    }

                }
                else
                {
                    var errorLine = $"❌ {status?.LastError?.Message ?? "Unknown"}";
                    sb.Append(errorLine);
                }

                sb.Append("\r\n"); // next server
            }

            group.SendMsg(bot.Client!, sb.ToString(), ParseMode.Html);
        }
    }
}