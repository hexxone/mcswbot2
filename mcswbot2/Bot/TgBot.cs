using mcswbot2.Bot.Commands;
using mcswbot2.Bot.Objects;
using mcswlib.ServerStatus;
using mcswlib.ServerStatus.Event;
using ZufallSatz;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot
{
    class TgBot
    {
        private static readonly List<ICommand> Commands = new List<ICommand>();
        internal static readonly List<TgUser> TgUsers = new List<TgUser>();
        internal static readonly List<TgGroup> TgGroups = new List<TgGroup>();

        internal static Config Conf { get; set; }

        internal static ServerStatusFactory Factory { get; private set; }
        internal static TelegramBotClient Client { get; private set; }
        internal static User TgBotUser { get; private set; }

        /// <summary>
        ///     Setup the bot command, load the settings and start
        /// </summary>
        internal static void Start()
        {
            Program.WriteLine("MineCraftServerWatchBotV2 for Telegram");
            Program.WriteLine("starting...");

            Generator.PreInit();

            // default config
            Conf = new Config();
            // server status & updater factory
            Factory = new ServerStatusFactory();

            // Add Bot commands
            Commands.Add(new CmdAdd());
            Commands.Add(new CmdInfo());
            Commands.Add(new CmdList());
            Commands.Add(new CmdNotify());
            Commands.Add(new CmdPing());
            Commands.Add(new CmdPlayer());
            Commands.Add(new CmdRemove());
            Commands.Add(new CmdStart());
            Commands.Add(new CmdStats());
            Commands.Add(new CmdTahnos());

            // Load users, groups & settings
            Storage.Load();

            // Start the bot async
            _ = RunBotAsync();

            // main ping loop
            var sw = new Stopwatch();
            var avgWait = Conf.SleepTime;
            while (true)
            {
                sw.Reset();
                sw.Start();
                Factory.PingAll();
                Parallel.ForEach(TgGroups, tgg => tgg.UpdateAll());
                sw.Stop();

                // calculate average wait time
                var waitme = Conf.SleepTime - Convert.ToInt32(sw.ElapsedMilliseconds);
                avgWait = Math.Max(10000, (avgWait + waitme) / 2);
                Program.WriteLine($"Sleeping {avgWait} MS...");
                Task.Delay(avgWait).Wait();

                // save data
                Storage.Save();
            }
        }

        /// <summary>
        ///     Start receiving message updates on the Telegram Bot
        /// </summary>
        /// <returns></returns>
        private static async Task RunBotAsync()
        {
            Client = new TelegramBotClient(Conf.ApiKey);
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
#if DEBUG
                throw;
#endif
            }
        }

        /// <summary>
        ///     Will handle all incoming bot messages
        /// </summary>
        /// <param name="msg"></param>
        private static void HandleMessage(Message msg)
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
                return;
            }

            // get group context
            var group = GetGroup(msg.Chat);

            // shouldn't usually happen in privacy mode (only commands)
            if (msg.Text == null) return;

            // build text/command arguments
            var text = msg.Text;
            var args = new[] { text };
            if (text.Contains(" "))
                args = text.Split(' ');

            // Process commands only
            if (!args[0].StartsWith('/')) return;

            var usrCmd = args[0][1..].ToLower();
            if (usrCmd.Contains("@"))
            {
                var spl = usrCmd.Split('@');
                // this command is malformatted or meant for another bot
                if (spl[1] != TgBotUser.Username.ToLower()) return;
                // dont include botname in command
                usrCmd = spl[0];
            }

            // check all registered command modules for a matching command
            foreach (var cmd in Commands)
                if (usrCmd == cmd.Command().ToLower())
                {
                    Program.WriteLine("Command: " + cmd + " by " + user);
                    cmd.Call(msg, group, user, args, isDev);
                }
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
            var newC = new TgGroup() { Base = c };
            TgGroups.Add(newC);
            return newC;
        }

        internal static void DestroyGroup(TgGroup tgg)
        {
            tgg.Destroy();
            TgGroups.Remove(tgg);
        }

        #endregion

    }
}
