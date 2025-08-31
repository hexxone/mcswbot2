using Telegram.Bot.Types;

namespace McswBot2.Commands
{
    internal class CommandArgs
    {
        internal required McswBot Bot { get; init; }

        internal required Message Msg { get; init; } 

        internal required Chat Group { get; init; }

        internal required User User { get; init; }

        internal required string[] Args { get; init; }

        internal bool IsDev { get; init; }


        // Return the first and last name.
        internal void Deconstruct(out McswBot bot, out Message msg, out Chat group, out User user, out string[] args, out bool isDev)
        {
            bot = Bot;
            msg = Msg;
            group = Group;
            user = User;
            args = Args;
            isDev = IsDev;
        }
    }
}
