using Telegram.Bot.Types;

namespace mcswbot2.Bot
{
    public class TgUser
    {
        /// <summary>
        ///     An object representing a Telegram user
        /// </summary>
        /// <param name="basis"></param>
        public TgUser(User basis)
        {
            Base = basis;
        }

        public User Base { get; set; }

        public override string ToString()
        {
            return $"[TGUser: {Base.Id}, name: {Base.FirstName} {Base.LastName}, username: {Base.Username}]";
        }
    }
}