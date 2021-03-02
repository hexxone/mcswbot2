using mcswbot2.Objects;
using Telegram.Bot.Types;

namespace mcswbot2.Commands
{
    internal abstract class ICommand
    {
        /// <summary>
        ///     Needs to be overwritten and return a low-cased string for representing when to call the command.
        /// </summary>
        /// <returns></returns>
        internal abstract string Command();

        /// <summary>
        ///     Needs to be overwritten by the command-sepcific logic
        /// </summary>
        /// <param name="m">Message which contained the calling command</param>
        /// <param name="g">Group where the message was sent in</param>
        /// <param name="u">Bot-user equivalent of the command sender</param>
        /// <param name="args">command arguments splitted by space</param>
        /// <param name="dev">message was sent by developer</param>
        internal abstract void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev);
        
    }
}