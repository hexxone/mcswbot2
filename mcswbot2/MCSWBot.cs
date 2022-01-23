using mcswbot2.Commands;
using mcswbot2.Minecraft;
using mcswbot2.Objects;
using mcswbot2.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2
{
    internal class MCSWBot
    {
        internal const int TG_TRIES = 30;
        internal const int TG_SLEEP = 30000;

        private static readonly List<ICommand> Commands = new();
        internal static readonly List<TgUser> TgUsers = new();
        internal static readonly List<TgGroup> TgGroups = new();

        internal static CancellationTokenSource botCts;

        internal static Config Conf { get; set; }

        internal static TelegramBotClient Client { get; private set; }
        internal static User TgBotUser { get; private set; }

        /// <summary>
        ///     Setup the bot command, load the settings and start
        /// </summary>
        internal static void Start()
        {
            Program.WriteLine("MineCraftServerWatchBotV2 for Telegram");
            Program.WriteLine("starting...");

            // default config
            Conf = new Config();

            // Add Bot commands
            Commands.Add(new CmdAdd());
            Commands.Add(new CmdNotify());
            Commands.Add(new CmdPing());
            Commands.Add(new CmdPlayers());
            Commands.Add(new CmdRemove());
            Commands.Add(new CmdServers());
            Commands.Add(new CmdStart());
            Commands.Add(new CmdStats());

            // Load users, groups & settings
            Storage.Load();

            // Apply static Config vars

            ServerStatusWatcher.Retries = Conf.Retries;
            ServerStatusWatcher.RetryMs = Conf.RetryMs;


            var enumValues = Enum.GetValues(typeof(Types.LogLevel)).Cast<int>().ToList();
            if (enumValues.Contains(Conf.LogLevel)) Logger.LogLevel = (Types.LogLevel)Conf.LogLevel;
            else Logger.WriteLine("Invalid LogLevel: " + Conf.LogLevel, Types.LogLevel.Error);

            // Start the telegram bot
            StartTelegramBotClient();

            // Minecraft Update Lööp
            while (!botCts.IsCancellationRequested)
                try
                {
                    ServerStatus.UpdateAll();

                    Program.WriteLine("Sleeping...");
                    Task.Delay(Conf.SleepTime).Wait();

                    // save data
                    Storage.Save();
                }
                catch (Exception e)
                {
                    Logger.WriteLine("MAIN LOOP EXCEPTION: " + e, Types.LogLevel.Error);
                }

            Console.WriteLine("Reached end of pr0gram! Auto updating stopped for some reason ???");
            Console.ReadLine();
        }

        /// <summary>
        ///     Auto (re-)Connect the Telegram bot
        /// </summary>
        /// <param name="tries"></param>
        private static async void StartTelegramBotClient(int tries = 0)
        {
            Program.WriteLine("Telegram bot connecting...");
            Thread.Sleep(3000);

            try
            {
                Client = new TelegramBotClient(Conf.ApiKey);

                botCts = new CancellationTokenSource();

                // receive all update types
                var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

                // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
                // from here on, we are running async events.
                Client.StartReceiving(
                    Client_OnMessage,
                    Client_OnError,
                    receiverOptions,
                    cancellationToken: botCts.Token);

                // get bot 
                TgBotUser = await Client.GetMeAsync();

                Program.WriteLine("Watashi wa: " + Newtonsoft.Json.JsonConvert.SerializeObject(TgBotUser));
            }
            catch (Exception e)
            {
                Program.WriteLine("Was not able to connect.. (" + tries + ")\r\n" + e);
                if (tries++ < TG_TRIES)
                {
                    Program.WriteLine($"Retry in {TG_SLEEP / 1000:0.0} s");
                    Thread.Sleep(TG_SLEEP);
                    StartTelegramBotClient(tries);
                }
                else
                {
                    Program.WriteLine("Unable to receive Telegram messages. Shutting down...");
                    botCts.Cancel();
                    Environment.Exit(0);
                }
            }
        }

        private static Task Client_OnMessage(ITelegramBotClient arg1, Update arg2, CancellationToken arg3)
        {
            try
            {
                if(arg2 == null || arg2.Type != UpdateType.Message || 
                   arg2.Message == null || arg2.Message.Type != MessageType.Text || arg2.Message.From == null)
                {
                    Program.WriteLine("Update was skipped: " + Newtonsoft.Json.JsonConvert.SerializeObject(arg2));
                    return Task.CompletedTask;
                }

                return HandleMessage(arg2.Message);
            }
            catch (Exception ex)
            {
                Program.WriteLine(ex.ToString());
#if DEBUG
                throw;
#endif
                return Task.FromException(ex);
            }
        }

        private static Task Client_OnError(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            Program.WriteLine("Telegram General Error:\r\n" + arg2);
            StartTelegramBotClient();
            return Task.CompletedTask;
        }


        /// <summary>
        ///     Will handle all incoming bot messages
        /// </summary>
        /// <param name="msg"></param>
        private static Task HandleMessage(Message msg)
        {
            // get sender
            var user = GetUser(msg.From);
            // message sent in group?
            var isGroup = msg.Chat.Type != ChatType.Private;
            // message sent by Owner?
            var isDev = user.Base.Id == Conf.DeveloperId;

            if (!isGroup)
            {
                Client.SendTextMessageAsync(msg.Chat.Id,
                    "This bot is intended for group use only.\r\n<a href=\"https://t.me/" + TgBotUser.Username +
                    "?startgroup=add\">Add me</a>", ParseMode.Html).Wait();
                return Task.CompletedTask;
            }

            // get group context
            var group = GetGroup(msg.Chat);

            // shouldn't usually happen in privacy mode (only commands)
            if (msg.Text == null) return Task.CompletedTask;

            // build text/command arguments
            var text = msg.Text;
            var args = new[] { text };
            if (text.Contains(" "))
                args = text.Split(' ');

            // Process commands only
            if (!args[0].StartsWith('/')) return Task.CompletedTask;

            var usrCmd = args[0][1..].ToLower();
            if (usrCmd.Contains("@"))
            {
                var spl = usrCmd.Split('@');
                // this command is malformatted or meant for another bot
                if (spl[1] != TgBotUser.Username.ToLower()) return Task.CompletedTask;
                // dont include botname in command
                usrCmd = spl[0];
            }

            // check all registered command modules for a matching command
            foreach (var cmd in Commands)
                if (usrCmd == cmd.Command().ToLower())
                {
                    Program.WriteLine("Command: " + cmd.Command() + " by " + user.Base.Id + " in " + group.Base.Id);
                    cmd.Call(msg, group, user, args, isDev);
                }

            return Task.CompletedTask;
        }

        #region Helper

        /// <summary>
        ///     Find or Add user
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        private static TgUser GetUser(User u)
        {
            foreach (var usr in TgUsers)
                if (usr.Base.Id == u.Id)
                {
                    usr.Base = u;
                    return usr;
                }

            var newU = new TgUser(u);
            TgUsers.Add(newU);
            return newU;
        }


        /// <summary>
        ///     Find or Add Group
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static TgGroup GetGroup(Chat c)
        {
            foreach (var cc in TgGroups)
                if (cc.Base.Id == c.Id)
                    return cc;
            var newC = new TgGroup(c);
            TgGroups.Add(newC);
            return newC;
        }

        internal static void DestroyGroup(TgGroup tgg)
        {
            TgGroups.Remove(tgg);
        }

        #endregion
    }
}