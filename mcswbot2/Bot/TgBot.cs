using mcswbot2.Bot.Commands;
using mcswbot2.Lib.Factory;
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
        private static readonly List<TgUser> _tgUsers = new List<TgUser>();
        private static readonly List<TgGroup> _tgGroups = new List<TgGroup>();


        public static TgUser[] TgUsers => _tgUsers.ToArray();
        public static TgGroup[] TgGroups => _tgGroups.ToArray();


        public static Config Conf { get; private set; }
        public static TelegramBotClient Client { get; private set; }
        public static User TgBotUser { get; private set; }

        /// <summary>
        ///     Setup the bot command, load the settings and start
        /// </summary>
        public static void Start()
        {
            Program.WriteLine("MineCraftServerWatchBotV2 for Telegram made by @hexxon");
            Program.WriteLine("starting...");

            // default config
            Conf = new Config();

            // Add Bot commands
            Commands.Add(new CmdAdd());
            Commands.Add(new CmdList());
            Commands.Add(new CmdNotify());
            Commands.Add(new CmdPing());
            Commands.Add(new CmdPlayer());
            Commands.Add(new CmdRemove());
            Commands.Add(new CmdStart());
            Commands.Add(new CmdStats());
            Commands.Add(new CmdSven());

            // Start the bot async
            _ = RunBotAsync();

            // Load users, groups & settings
            Load();

            // main ping loop
            var sw = new Stopwatch();
            var avgWait = 60000;
            while (true)
            {
                sw.Reset();
                sw.Start();
                ServerStatusFactory.Get().PingAll();
                Parallel.ForEach(_tgGroups, tgg => tgg.UpdateAll());
                sw.Stop();

                // calculate average wait time
                var waitme = 60000 - Convert.ToInt32(sw.ElapsedMilliseconds);
                avgWait = (avgWait + waitme) / 2;
                PutTaskDelay(Math.Max(1000, avgWait)).Wait();

                // save data
                Save();
            }
        }

        /// <summary>
        ///     Non-blocking way of waiting
        /// </summary>
        /// <returns></returns>
        private static async Task PutTaskDelay(int ms)
        {
            await Task.Delay(ms);
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

            var usrCmd = args[0].Substring(1).ToLower();
            if (usrCmd.Contains("@"))
            {
                var spl = usrCmd.Split('@');
                // this command is malformatted or meant for another bot
                if (spl.Length != 2 || spl[1] != TgBotUser.Username.ToLower()) return;
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
        public static TgUser GetUser(User u)
        {
            foreach (var usr in _tgUsers)
                if (usr.Base.Id == u.Id)
                {
                    usr.Base = u;
                    return usr;
                }
            var newU = new TgUser(u);
            _tgUsers.Add(newU);
            return newU;
        }


        /// <summary>
        ///     Find or Add Group
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static TgGroup GetGroup(Chat c)
        {
            foreach (var cc in _tgGroups)
                if (cc.Base.Id == c.Id)
                    return cc;
            var newC = new TgGroup() { Base = c };
            _tgGroups.Add(newC);
            return newC;
        }

        #endregion


        #region Storage

        /// <summary>
        ///     Load & Json Decode the user & group settings
        /// </summary>
        private static void Load()
        {
            try
            {
                // load config
                if (System.IO.File.Exists("config.json"))
                {
                    var json = System.IO.File.ReadAllText("config.json");
                    Conf = JsonConvert.DeserializeObject<Config>(json);
                }
                else
                {
                    Save();
                    Console.WriteLine("\r\n\r\n\tWARNING: CONFIG JUST GOT CREATED. PLEASE MODIFY IT BEFORE STARTING AGAIN.\r\n");
                    Environment.Exit(0);
                }

                // load users objects if file exists
                if (System.IO.File.Exists("users.json"))
                {
                    var json = System.IO.File.ReadAllText("users.json");
                    _tgUsers.AddRange(JsonConvert.DeserializeObject<TgUser[]>(json));
                }

                // load group objects if file exists
                if (System.IO.File.Exists("groups.json"))
                {
                    var json = System.IO.File.ReadAllText("groups.json");
                    var des = JsonConvert.DeserializeObject<TgGroup[]>(json);
                    // post-processing
                    foreach (var grp in des)
                    {
                        // get deserialized servers & clear the originals
                        var arr = grp.Servers.ToArray();
                        grp.Servers.Clear();
                        // add all servers back using the factory
                        foreach (var srv in arr)
                            grp.AddServer(srv.Label, srv.Base.Address, srv.Base.Port);
                    }
                    // add objects after deserializing & initializing
                    _tgGroups.AddRange(des);
                }
                // done
                Program.WriteLine($"Loaded data. [{_tgUsers.Count} Users, {_tgGroups.Count} Groups]");
            }
            catch (Exception e)
            {
                Program.WriteLine("Error when loading data: " + e);
            }
        }

        /// <summary>
        ///     Json encode & save the user & group settings
        /// </summary>
        private static void Save()
        {
            try
            {
                // write config
                var str = JsonConvert.SerializeObject(Conf);
                str = JToken.Parse(str).ToString(Formatting.Indented);
                System.IO.File.WriteAllText("config.json", str);
                // write user if any
                if (_tgUsers.Count > 0)
                {
                    str = JsonConvert.SerializeObject(_tgUsers);
                    System.IO.File.WriteAllText("users.json", str);
                }
                // write groups if any
                if (_tgGroups.Count > 0)
                {
                    str = JsonConvert.SerializeObject(_tgGroups);
                    System.IO.File.WriteAllText("groups.json", str);
                }
                // done
                Program.WriteLine($"Saved data. [{_tgUsers.Count} Users, {_tgGroups.Count} Groups]");
            }
            catch (Exception e)
            {
                Program.WriteLine("Error when saving data: " + e);
            }
        }

        #endregion
    }
}
