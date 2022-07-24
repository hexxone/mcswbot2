# [mcswbot2](https://github.com/Hexxonite/mcswbot2)

### Minecraft Server Watch Bot V2

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

### Deploy:

- Install docker && docker-compose
- Clone the repo
- Copy `example.config.json` to `config.json` and customize it
- `docker-compose up -d`

### Dependencies:
- [Docker &-compose](https://docker.com/) cross-platform running
- [.NET 6](https://dotnet.microsoft.com/) runtime
- [SkiaSharp](https://github.com/mono/SkiaSharp) status image & sticker processing
- [ScottPlot](https://github.com/swharden/ScottPlot) time-data plotting (See `/player` and `/ping` command).
- [Newtonsoft.JSON](https://github.com/JamesNK/Newtonsoft.Json) (de-)serializing server-info and settings
- [Telegram.Bot](https://github.com/TelegramBots/telegram.bot) telegram bot part


