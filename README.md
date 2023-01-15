# [mcswbot2](https://github.com/hexxone/mcswbot2)

### Minecraft Server Watch Bot V2 (Minimal edition)

Use this bot to request the server list information from minecraft servers.

Due to the architecture, the bot should use very low bandwidth over time and Modded Server should be supported aswell.

For newer versions, the Server List info may also contain a sample list of online player names.

If the server is however modded or has plugins to display custom player-list-info, this can cause issues.

### Usage:

- Customize the config
- Deploy the docker container
- See your online friends

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
- [Newtonsoft.JSON](https://github.com/JamesNK/Newtonsoft.Json) (de-)serializing server-info and settings
- [Telegram.Bot](https://github.com/TelegramBots/telegram.bot) telegram bot part

