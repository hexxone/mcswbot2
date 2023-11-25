using McswBot2.Commands;
using McswBot2.Objects;
using McswBot2.Static;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace McswBot2
{
    internal class McswBot
    {
        internal const int TgTries = 30;
        internal const int TgSleep = 30000;

        private readonly List<ICommand> Commands = new();


        internal CancellationTokenSource BotCts = default!;

        // default config
        internal Config Conf { get; set; } = new();

        internal TelegramBotClient? Client { get; private set; }
        internal User? TgBotUser { get; private set; }

        /// <summary>
        ///     Setup the bot command, load the settings and start
        /// </summary>
        internal void Start()
        {
            Program.WriteLine("MineCraftServerWatchBotV2 for Telegram by @hexxone");
            Program.WriteLine("starting...");

            // Add Bot commands
            var assemblyCmds = typeof(ICommand).FindDerivedAssemblyTypes();
            foreach (var cmd in assemblyCmds)
            {
                if (Activator.CreateInstance(cmd) is not ICommand cmdObj)
                {
                    continue;
                }

                Commands.Add(cmdObj);
                Program.WriteLine($"Registered Command: '{cmdObj.Command()}'");
            }

            // Load users, groups & settings
            Storage.Load(this);


            var enumValues = Enum.GetValues(typeof(Types.LogLevel)).Cast<int>().ToList();
            if (enumValues.Contains(Conf.LogLevel))
            {
                Logger.LogLevel = (Types.LogLevel)Conf.LogLevel;
            }
            else
            {
                Logger.WriteLine("Invalid LogLevel: " + Conf.LogLevel, Types.LogLevel.Error);
            }

            // Start the telegram bot
            StartTelegramBotClient();

            // Memecraft Update Lööp
            while (!BotCts.IsCancellationRequested)
            {
                try
                {
                    Program.WriteLine("Sleeping...");
                    Task.Delay(Conf.DataSaveInterval).Wait();

                    // save data
                    GC.Collect();
                }
                catch (Exception e)
                {
                    Logger.WriteLine("MAIN LOOP EXCEPTION: " + e, Types.LogLevel.Error);
                }
            }

            Console.WriteLine("Reached end of pr0gram! Auto updating stopped for some reason ???");
            Console.ReadLine();
        }

        /// <summary>
        ///     Auto (re-)Connect the Telegram bot
        /// </summary>
        /// <param name="tries"></param>
        private async void StartTelegramBotClient(int tries = 0)
        {
            Program.WriteLine("Telegram bot connecting...");
            Thread.Sleep(3000);

            try
            {
                Client = new TelegramBotClient(Conf.ApiKey);

                BotCts = new CancellationTokenSource();

                // receive all update types
                var receiverOptions = new ReceiverOptions();

                // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
                // from here on, we are running async events.
                Client.StartReceiving(
                    Client_OnMessage,
                    Client_OnError,
                    receiverOptions,
                    BotCts.Token);

                // get bot 
                TgBotUser = await Client.GetMeAsync();

                Program.WriteLine("Watashi wa: " + TgBotUser.Username);
            }
            catch (Exception e)
            {
                Program.WriteLine("Unable to connect.. (" + tries + ")\r\n" + e);
                if (tries++ < TgTries)
                {
                    Program.WriteLine($"Retry in {TgSleep / 1000:0.0} s");
                    Thread.Sleep(TgSleep);
                    StartTelegramBotClient(tries);
                }
                else
                {
                    Program.WriteLine("Unable to receive Telegram messages. Shutting down...");
                    BotCts.Cancel();
                    Environment.Exit(0);
                }
            }
        }

        private Task Client_OnMessage(ITelegramBotClient arg1, Update arg2, CancellationToken arg3)
        {
            try
            {
                if (arg2 is { Type: UpdateType.Message, Message: { Type: MessageType.Text, From: { } } })
                {
                    return HandleMessage(arg2.Message);
                }

                Program.WriteLine("Update was skipped: " + JsonConvert.SerializeObject(arg2));
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Program.WriteLine(ex.ToString());
#if DEBUG
                throw;
#else
                return Task.FromException(ex);
#endif
            }
        }

        private Task Client_OnError(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            Program.WriteLine("Telegram General Error:\r\n" + arg2);
            StartTelegramBotClient();
            return Task.CompletedTask;
        }


        /// <summary>
        ///     Will handle all incoming bot messages
        /// </summary>
        /// <param name="msg"></param>
        private Task HandleMessage(Message msg)
        {
            // get sender
            if (msg.From != null)
            {
                _ = HandleUserMessage(msg, msg.From);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Will handle Messages in Bot-User context
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool HandleUserMessage(Message msg, User user)
        {
            // message sent in group?
            var isGroup = msg.Chat.Type != ChatType.Private;
            // message sent by Owner?
            var isDev = user.Id == Conf.DeveloperId;

            if (!isGroup)
            {
                Client!.SendTextMessageAsync(msg.Chat.Id,
                    "This bot is intended for group use only.\r\n<a href=\"https://t.me/" + TgBotUser!.Username +
                    "?startgroup=add\">Add me</a>", parseMode: ParseMode.Html).Wait();
                {
                    return false;
                }
            }

            // get group context
            var group = msg.Chat;

            // shouldn't usually happen in privacy mode (only commands)
            if (msg.Text == null)
            {
                return false;
            }


            // build text/command arguments
            var text = msg.Text;
            var args = new[] { text };
            if (text.Contains(' '))
            {
                args = text.Split(' ');
            }

            // Process commands only
            if (!args[0].StartsWith('/'))
            {
                return false;
            }

            var usrCmd = args[0][1..].ToLower();
            if (usrCmd.Contains('@'))
            {
                var spl = usrCmd.Split('@');
                // this command is malformed or meant for another bot
                if (spl[1] != TgBotUser?.Username?.ToLower())
                {
                    return false;
                }

                // don't include bot name in command
                usrCmd = spl[0];
            }

            // check all registered command modules for a matching command
            foreach (var cmd in Commands.Where(cmd => usrCmd == cmd.Command().ToLower()))
            {
                Program.WriteLine("Command: " + cmd.Command() + " by " + user.Id + " in " + group.Id);
                cmd.Call(new ICommandArgs()
                {
                    Bot = this,
                    Group = group,
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