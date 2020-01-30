using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Commands
{
    internal abstract class ICommand
    {
        /// <summary>
        ///     Needs to be overwritten and return a low-cased string for representing when to call the command.
        /// </summary>
        /// <returns></returns>
        public abstract string Command();

        /// <summary>
        ///     Needs to be overwritten by the command-sepcific logic
        /// </summary>
        /// <param name="m">Message which contained the calling command</param>
        /// <param name="g">Group where the message was sent in</param>
        /// <param name="u">Bot-user equivalent of the command sender</param>
        /// <param name="args">command arguments splitted by space</param>
        /// <param name="dev">message was sent by developer</param>
        public abstract void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev);

        /// <summary>
        ///     Short-wrapper for responding in current context
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="txt"></param>
        /// <param name="pm"></param>
        /// <param name="replyMsg"></param>
        protected void Respond(ChatId cid, string txt, ParseMode pm = ParseMode.Default, int replyMsg = 0)
        {
            try
            {
                TgBot.Client.SendTextMessageAsync(cid, txt, pm, false, false, replyMsg).Wait();
            }
            catch (Exception ex)
            {
                Program.WriteLine("Response Error: " + ex + "\r\nStack: " + ex.StackTrace);
            }
        }

        /// <summary>
        ///     toString override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[ICommand: {Command()}]";
        }
    }
}