# [mcswbot2](https://github.com/Hexxonite/mcswbot2)

### MinecraftServerWatchBotV2 (.NET Core 2.1)

Use this bot to frequently request the server list information from minecraft servers and detect changes (server offline, player change).

Due to the architecture, the bot should use very low bandwidth over time and Modded Server should be supported aswell.

For newer versions, the Server List info may also contain a list of online player names, which will trigger an event when joining/leaving aswell.

### Usage:

- Add the Bot to your favourite Telegram Minecraft Group
- `/add` up to 3 server
- set `/notify` settings to your preference
- Play with friends

### Credits:

This Bot is using a modified version of [swharden/ScottPlot](https://github.com/swharden/ScottPlot) for plotting User Data over time.

See `/player` command.

### (Probably) Supported Minecraft Protocol Versions:

(1.0)

- 1.3.1  - 1.3.2  == 39
- 1.4.2           == 47
- 1.4.4  - 1.4.5  == 49
- 1.4.6  - 1.4.7  == 51
- 1.5    - 1.5.1  == 60
- 1.5.2           == 61
- 1.6.1           == 73
- 1.6.2           == 74
- 1.6.4           == 78

(2.0)

- 1.7    - 1.7.1  == 3
- 1.7.2  - 1.7.5  == 4 
- 1.7.6  - 1.7.10 == 5
- 1.8    - 1.8.9  == 47
- 1.9    - 1.9.1  == 107 & 108
- 1.9.2  - 1.9.4  == 109
- 1.9.3  - 1.9.4  == 110
- 1.10   - 1.10.2 == 210
- 1.11            == 315
- 1.11.1 - 1.11.2 == 316
- 1.12            == 335
- 1.12.1          == 338
- 1.12.2          == 340
- 1.13            == 393
- 1.13.1          == 401
- 1.14.3          == 409

