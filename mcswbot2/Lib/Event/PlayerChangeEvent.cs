using System;
using mcswbot2.Lib;

namespace mcswbot2.Lib.Event
{
    internal class PlayerChangeEvent : EventBase
    {
        public PlayerChangeEvent(int diff)
        {
            PlayerDiff = diff;
        }

        public int PlayerDiff { get; }

        public override string GetEventString(Types.Formatting format)
        {
            var abs = Math.Abs(PlayerDiff);
            return Wrap(format, abs.ToString()) + " Player" + (abs > 1 ? "s" : "") +
                   (PlayerDiff > 0 ? " joined" : " left") + ".";
        }
    }
}