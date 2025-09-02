using McswBot2.Commands;
using McswBot2.Objects;
using McswBot2.Static;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
// ReSharper disable EmptyGeneralCatchClause

namespace McswBot2
{
    internal class McswBot
    {
        private readonly List<Command> _commands = [];
        private readonly SemaphoreSlim _reconnectGate = new(1, 1);

        private CancellationTokenSource _appCts = null!;
        private CancellationTokenSource? _receiverCts;

        internal Config Conf { get; set; } = new();
        internal TelegramBotClient? Client { get; private set; }
        internal User? TgBotUser { get; private set; }

        internal async Task StartAsync()
        {
            Logger.WriteLine("MineCraftServerWatchBotV2 for Telegram by @hexxone");
            Logger.WriteLine("starting...");

            var assemblyCmds = typeof(Command).FindDerivedAssemblyTypes();
            foreach (var cmd in assemblyCmds)
            {
                if (Activator.CreateInstance(cmd) is not Command cmdObj)
                {
                    Logger.WriteLine($"Failed to Instantiate: {cmd.Name}", Types.LogLevel.Error);
                    continue;
                }

                _commands.Add(cmdObj);
                Logger.WriteLine($"Registered Command: '{cmdObj.GetCommand()}'");
            }

            Storage.Load(this);

            var enumValues = Enum.GetValues(typeof(Types.LogLevel)).Cast<int>().ToList();
            if (enumValues.Contains(Conf.LogLevel))
                Logger.LogLevel = (Types.LogLevel)Conf.LogLevel;
            else
                Logger.WriteLine($"Invalid LogLevel: {Conf.LogLevel}", Types.LogLevel.Error);

            _appCts = new CancellationTokenSource();

            var botLoop = RunTelegramLoopAsync(_appCts.Token);
            var maintenanceLoop = RunMaintenanceLoopAsync(_appCts.Token);

            await Task.WhenAll(botLoop, maintenanceLoop);

            Logger.WriteLine("Reached end of pr0gram! Auto updating stopped for some reason ?");
        }

        private async Task RunMaintenanceLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Logger.WriteLine("Sleeping...");
                    await Task.Delay(Conf.DataSaveInterval, ct);

                    GC.Collect();
                }
                catch (OperationCanceledException)
                {
                    /* normal on Stop */
                }
                catch (Exception e)
                {
                    Logger.WriteLine($"MAIN LOOP EXCEPTION: {e}", Types.LogLevel.Error);
                }
            }
        }

        private async Task RunTelegramLoopAsync(CancellationToken appCt)
        {
            int backoffMs = 2000;

            while (!appCt.IsCancellationRequested)
            {
                try
                {
                    await StartReceiverOnceAsync(appCt);

                    await Task.Delay(Timeout.Infinite, _receiverCts!.Token);

                    backoffMs = 2000;
                }
                catch (OperationCanceledException)
                {
                    if (appCt.IsCancellationRequested) break;
                }
                catch (Exception ex)
                {
                    Logger.WriteLine($"Unable to connect..\r\n{ex}", Types.LogLevel.Error);
                }

                Logger.WriteLine($"Retry in {backoffMs / 1000.0:0.0} s");
                try
                {
                    await Task.Delay(backoffMs, appCt);
                }
                catch
                {
                }

                backoffMs = Math.Min(backoffMs * 2, 60000);
            }
        }

        private async Task StartReceiverOnceAsync(CancellationToken appCt)
        {
            await _reconnectGate.WaitAsync(appCt);
            try
            {
                Logger.WriteLine("Telegram bot connecting...");

                try
                {
                    if (_receiverCts != null)
                        await _receiverCts.CancelAsync();
                }
                catch
                {
                }

                // try
                // {
                //     // TODO Client?.CloseAsync();
                // }
                // catch
                // {
                // }

                _receiverCts = CancellationTokenSource.CreateLinkedTokenSource(appCt);

                var socketsHandler = new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(30),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(15),
                    AutomaticDecompression = DecompressionMethods.All
                };
                var httpClient = new HttpClient(socketsHandler);

                Client = new TelegramBotClient(Conf.ApiKey, httpClient);

                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = [],
                    ThrowPendingUpdates = true
                };

                Client.StartReceiving(
                    Client_OnMessage,
                    Client_OnError,
                    receiverOptions,
                    _receiverCts.Token);

                TgBotUser = await Client.GetMeAsync(_receiverCts.Token);
                Logger.WriteLine($"Watashi wa: {TgBotUser.Username}");
            }
            finally
            {
                _reconnectGate.Release();
            }
        }

        private Task Client_OnMessage(ITelegramBotClient bot, Update update, CancellationToken ct)
        {
            try
            {
                if (update is { Type: UpdateType.Message, Message: { Type: MessageType.Text, From: not null } })
                    return HandleMessage(update.Message);

                Logger.WriteLine($"Update was skipped: {JsonConvert.SerializeObject(update)}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex.ToString());
#if DEBUG
                throw;
#else
                return Task.FromException(ex);
#endif
            }
        }

        private Task Client_OnError(ITelegramBotClient bot, Exception ex, CancellationToken ct)
        {
            Logger.WriteLine($"Telegram General Error:\r\n{ex}", Types.LogLevel.Error);
            try
            {
                _receiverCts?.Cancel();
            }
            catch
            {
            }

            return Task.CompletedTask;
        }

        private Task HandleMessage(Message msg)
        {
            try
            {
                if (msg.From != null)
                    _ = HandleUserMessage(msg, msg.From);
            }
            catch (Exception ex)
            {
                var name = $"Id=[{msg.From?.Id}] @{msg.From?.Username} ({msg.From?.FirstName} {msg.From?.LastName})";
                Logger.WriteLine($"User '{name}' fucked up: {ex.Message}", Types.LogLevel.Error);
            }

            return Task.CompletedTask;
        }

        private bool HandleUserMessage(Message msg, User user)
        {
            var isGroup = msg.Chat.Type != ChatType.Private;
            var isDev = user.Id == Conf.DeveloperId;

            if (!isGroup)
            {
                _ = Client!.SendTextMessageAsync(
                    msg.Chat.Id,
                    $"This bot is intended for group use only.\r\n<a href=\"https://t.me/{TgBotUser!.Username}?startgroup=add\">Add me</a>",
                    parseMode: ParseMode.Html);
                return false;
            }

            if (msg.Text == null) return false;

            var args = msg.Text.Contains(' ') ? msg.Text.Split(' ') : [msg.Text];
            if (!args[0].StartsWith('/')) return false;

            var usrCmd = args[0][1..].ToLower();
            if (usrCmd.Contains('@'))
            {
                var spl = usrCmd.Split('@');
                if (spl[1] != TgBotUser?.Username?.ToLower()) return false;
                usrCmd = spl[0];
            }

            foreach (var cmd in _commands.Where(c => usrCmd == c.GetCommand().ToLower()))
            {
                Logger.WriteLine($"Command: {cmd.GetCommand()} by {user.Id} in {msg.Chat.Id}");
                cmd.Call(new CommandArgs
                {
                    Bot = this,
                    Group = msg.Chat,
                    User = user,
                    Msg = msg,
                    Args = args,
                    IsDev = isDev
                });
            }

            return true;
        }
    }
}