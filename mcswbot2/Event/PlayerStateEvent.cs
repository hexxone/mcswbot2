namespace mcswbot2.Event
{
    public class PlayerStateEvent : EventBase
    {
        internal PlayerStateEvent(PlayerPayLoad ppl, bool on)
        {
            Player = ppl;
            Online = on;
        }

        public PlayerPayLoad Player { get; }
        public bool Online { get; }
    }
}