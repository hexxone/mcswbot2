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

### Develop:

- Clone the repo
- Restore Nuget Packages
- Add missing references from `./Include/`

### Deploy:

- For windows first build, then copy all binaries from `./Include/` to your publish folder that are missing

- For Linux, do the same excpet for `libwebp.dll` 
- Install the package system-wide instead by using: `apt-get install libwebp-dev -y`

### Dependencies:
- [.NET Core 2.1](https://dotnet.microsoft.com/)
- [mcswlib](https://github.com/Hexxonite/mcswlib) my own library for minecraft-server pinging
- [ScottPlot](https://github.com/swharden/ScottPlot) for plotting Data over time (See `/player` and `/ping` command).
- [Newtonsoft.JSON](https://github.com/JamesNK/Newtonsoft.Json) for (de-)serializing the server info and settings
- [Telegram.Bot](https://github.com/TelegramBots/telegram.bot) for the actual Bot part


