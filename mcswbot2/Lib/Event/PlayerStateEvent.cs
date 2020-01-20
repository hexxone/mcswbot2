using mcswbot2.Lib.Payload;
using mcswbot2.Lib;

namespace mcswbot2.Lib.Event
{
    internal class PlayerStateEvent : EventBase
    {
        public PlayerStateEvent(PlayerPayLoad ppl, bool on)
        {
            Player = ppl;
            Online = on;
        }

        public PlayerPayLoad Player { get; }
        public bool Online { get; }

        public override string GetEventString(Types.Formatting format)
        {
            return (Online ? "✅ " : "🚫 ") + Wrap(format, Player.Name);
        }
    }
}