using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Commands
{
    internal abstract class ICommand
    {
        public abstract string Command();
        public abstract void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev);

        protected void Respond(ChatId cid, string txt, ParseMode pm = ParseMode.Default)
        {
            try
            {
                TgBot.Client.SendTextMessageAsync(cid, txt, pm).Wait();
            }
            catch (Exception ex)
            {
                TgBot.WriteLine("Response Error: " + ex + "\r\nStack: " + ex.StackTrace);
            }
        }

        public override string ToString()
        {
            return $"[ICMD: {Command()}]";
        }
    }
}