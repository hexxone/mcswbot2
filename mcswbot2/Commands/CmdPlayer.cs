﻿using McswBot2.Static;
using System;
using System.Linq;
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

            var txt = "Player:";
            foreach (var (watcher, status) in pingResults)
            {
                txt += $"\r\n[<a href=\"{watcher.Address}:{watcher.Port}\">{watcher.Label}</a>] ";

                if (status is { HadSuccess: true })
                {
                    txt += $" <code>{status.CurrentPlayerCount}/{status.MaxPlayerCount}</code> ({status.RequestTime:##.##} ms)";

                    txt = status.OnlinePlayers.Aggregate(txt,
                        (current, plr) => current + $"\r\n  # {plr.Name}");
                }
                else
                {
                    txt += $"❌" +
                           $"\r\n  Player: ? / ?" +
                           $"\r\n  Error:<code> {status?.LastError?.ToString() ?? "Unknown"}</code>";
                }
            }

            group.SendMsg(bot.Client!, txt, ParseMode.Html);
        }
    }
}