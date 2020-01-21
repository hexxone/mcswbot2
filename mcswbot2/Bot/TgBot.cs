using mcswbot2.Bot.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot
{
    class TgBot
    {
        private static readonly List<ICommand> Commands = new List<ICommand>();

        public static List<TgUser> TgUsers = new List<TgUser>();

        public static List<TgGroup> TgGroups = new List<TgGroup>();

        public static TelegramBotClient Client { get; private set; }

        public static User TgBotUser { get; private set; }

        public static void Start()
        {
            Program.WriteLine("MineCraftServerWatchBotV2 for Telegram made by @hexxon");
            Program.WriteLine("starting...");

            // Add Bot commands
            Commands.Add(new CmdAdd());
            Commands.Add(new CmdList());
            Commands.Add(new CmdNotify());
            Commands.Add(new CmdPing());
            Commands.Add(new CmdPlayer());
            Commands.Add(new CmdRemove());
            Commands.Add(new CmdStart());
            Commands.Add(new CmdStats());

            // Start the bot async
            _ = RunBotAsync();

            // Load users, groups & settings
            Utils.Load();

            // main ping loop
            while (true)
            {
                Parallel.ForEach(TgGroups, tgg => tgg.PingAll());

                PutTaskDelay().Wait();

                Utils.Save();
            }
        }

        /// <summary>
        ///     Non-blocking way of waiting
        /// </summary>
        /// <returns></returns>
        private static async Task PutTaskDelay()
        {
            await Task.Delay(30000);
        }

        /// <summary>
        ///     Start receiving message updates on the Telegram Bot
        /// </summary>
        /// <returns></returns>
        private static async Task RunBotAsync()
        {
            Client = new TelegramBotClient(Config.ApiKey);
            TgBotUser = await Client.GetMeAsync();
            Program.WriteLine("I am Bot: " + new TgUser(TgBotUser));
            Client.OnMessage += Client_OnMessage;

            // start taking requests
            Client.StartReceiving();
        }

        /// <summary>
        ///     Event Callback for Telegram Bot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Client_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            try
            {
                HandleMessage(e.Message);
                // manually cleanup unreferenced objects
                GC.Collect();
            }
            catch (Exception ex)
            {
                Program.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        ///     Will handle all incoming bot messages
        /// </summary>
        /// <param name="msg"></param>
        private static void HandleMessage(Message msg)
        {
            // get sender
            var user = Utils.GetUser(msg.From);
            // message sent in group?
            var isGroup = msg.Chat.Type != ChatType.Private;
            // message sent by Owner?
            var isDev = user.Base.Id == Config.DeveloperId;

            if (!isGroup)
            {
                Client.SendTextMessageAsync(msg.Chat.Id,
                    "This bot is intended for group use only.\r\n<a href=\"https://t.me/" + TgBotUser.Username +
                    "\">Add me</a>").Wait();
                return;
            }

            // get group context
            var group = Utils.GetGroup(msg.Chat);

            // shouldn't usually happen in privacy mode (only commands)
            if (msg.Text == null) return;

            // build text/command arguments
            var text = msg.Text;
            var args = new[] { text };
            if (text.Contains(" "))
                args = text.Split(' ');

            // Process command
            if (!args[0].StartsWith('/')) return;

            var ct = args[0].Substring(1).ToLower();
            if (ct.Contains("@")) ct = ct.Split('@')[0];
            // check all registered command modules for a matching name
            foreach (var cmd in Commands)
                if (cmd.Command() == ct)
                {
                    Program.WriteLine("Command: " + cmd + " by " + user);
                    cmd.Call(msg, group, user, args, isDev);
                }
        }
    }
}
