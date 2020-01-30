# [mcswbot2](https://github.com/Hexxonite/mcswbot2)

### MinecraftServerWatchBotV2 (.NET Core 2.1)

[Use this bot](https://t.me/mcsw_bot) to frequently request the server list information from minecraft servers and detect changes (server offline, player change).

Due to the architecture, the bot should use very low bandwidth over time and Modded Server should be supported aswell.

For newer versions, the Server List info may also contain a sample list of online player names, which can be set to trigger an event when changing aswell. If the server is however modded or has plugins to display custom player-list-info, this can cause issues.

### Usage:

- [Add the Bot](https://t.me/mcsw_bot?startgroup=add) to your favourite Telegram Minecraft Group(s)
- `/add` up to 3 servers
- set the `/notify` settings to your preference
- Play with friends

### Credits:

I have done a lot of research on the minecraft-server protocol so most of the code is actually self-written. I have however taken some inspiration from [this gist](https://gist.github.com/csh/2480d14fbbb33b4bbae3) for example.

For detailed info on minecraft protocol versions go here: https://wiki.vg/Protocol_version_numbers

### Libraries:
- [.NET Core 2.1](https://dotnet.microsoft.com/)
- [ScottPlot](https://github.com/swharden/ScottPlot) for plotting Data over time (See `/player` and `/ping` command).
- [Newtonsoft.JSON](https://github.com/JamesNK/Newtonsoft.Json) for (de-)serializing the server info and settings
- [Telegram.Bot](https://github.com/TelegramBots/telegram.bot) for the actual Bot part


