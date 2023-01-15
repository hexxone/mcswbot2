using Telegram.Bot.Types;

namespace McswBot2.Commands
{
    internal class ICommandArgs
    {
        internal McswBot Bot { get; set; } = default!;

        internal Message Msg { get; set; } = default!;

        internal Chat Group { get; set; } = default!;

        internal User User { get; set; } = default!;

        internal string[] Args { get; set; } = default!;

        internal bool IsDev { get; set; }


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
