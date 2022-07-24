using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McswBot2.Commands;
using McswBot2.Minecraft;
using McswBot2.Objects;
using McswBot2.Static;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace McswBot2;

internal class McswBot
{
    internal const int TgTries = 30;
    internal const int TgSleep = 30000;

    private static readonly List<ICommand> Commands = new();
    internal static readonly List<TgUser> TgUsers = new();
    internal static readonly List<TgGroup> TgGroups = new();

    internal static CancellationTokenSource BotCts = default!;

    // default config
    internal static Config Conf { get; set; } = new();

    internal static TelegramBotClient? Client { get; private set; }
    internal static User? TgBotUser { get; private set; }

    /// <summary>
    ///     Setup the bot command, load the settings and start
    /// </summary>
    internal static void Start()
    {
        Program.WriteLine("MineCraftServerWatchBotV2 for Telegram by @hexxone");
        Program.WriteLine("starting...");

        // Add Bot commands
        var assemblyCmds = typeof(ICommand).FindDerivedAssemblyTypes();
        foreach (var cmd in assemblyCmds)
        {
            if (Activator.CreateInstance(cmd) is not ICommand cmdObj)
                continue;

            Commands.Add(cmdObj);
            Program.WriteLine($"Registered Command: '{cmdObj.Command()}'");
        }

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

        // Memecraft Update Lööp
        while (!BotCts.IsCancellationRequested)
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
            Program.WriteLine("Was not able to connect.. (" + tries + ")\r\n" + e);
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

    private static Task Client_OnMessage(ITelegramBotClient arg1, Update arg2, CancellationToken arg3)
    {
        try
        {
            if (arg2 is { Type: UpdateType.Message, Message: { Type: MessageType.Text, From: { } } })
                return HandleMessage(arg2.Message);

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
        if (user != null)
            _ = HandleUserMessage(msg, user);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Will handle Messages in Bot-User context
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private static bool HandleUserMessage(Message msg, TgUser user)
    {
        // message sent in group?
        var isGroup = msg.Chat.Type != ChatType.Private;
        // message sent by Owner?
        var isDev = user.Base.Id == Conf.DeveloperId;

        if (!isGroup)
        {
            Client!.SendTextMessageAsync(msg.Chat.Id,
                "This bot is intended for group use only.\r\n<a href=\"https://t.me/" + TgBotUser!.Username +
                "?startgroup=add\">Add me</a>", ParseMode.Html).Wait();
            {
                return false;
            }
        }

        // get group context
        var group = GetGroup(msg.Chat);

        // shouldn't usually happen in privacy mode (only commands)
        if (msg.Text == null)
            return false;


        // build text/command arguments
        var text = msg.Text;
        var args = new[] { text };
        if (text.Contains(' ' ))
            args = text.Split(' ');

        // Process commands only
        if (!args[0].StartsWith('/'))
            return false;


        var usrCmd = args[0][1..].ToLower();
        if (usrCmd.Contains('@'))
        {
            var spl = usrCmd.Split('@');
            // this command is malformed or meant for another bot
            if (spl[1] != TgBotUser?.Username?.ToLower())
                return false;

            // don't include bot name in command
            usrCmd = spl[0];
        }

        // check all registered command modules for a matching command
        foreach (var cmd in Commands.Where(cmd => usrCmd == cmd.Command().ToLower()))
        {
            Program.WriteLine("Command: " + cmd.Command() + " by " + user.Base.Id + " in " + group.Base.Id);
            cmd.Call(msg, group, user, args, isDev);
        }

        return true;
    }

    #region Helper

    /// <summary>
    ///     Find or Add user
    /// </summary>
    /// <param name="u"></param>
    /// <returns></returns>
    private static TgUser? GetUser(User? u)
    {
        if (u == null)
            return null;

        foreach (var usr in TgUsers.Where(usr => usr.Base.Id == u.Id))
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
        TgGroup? group;
        if ((group = TgGroups.FirstOrDefault(cc => cc.Base.Id == c.Id)) is not null)
            return group;

        group = new TgGroup(c);
        TgGroups.Add(group);
        return group;
    }

    internal static void DestroyGroup(TgGroup tgg)
    {
        TgGroups.Remove(tgg);
    }

    #endregion
}