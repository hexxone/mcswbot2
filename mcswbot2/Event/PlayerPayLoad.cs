using mcswbot2.Objects;
using mcswbot2.Static;

namespace mcswbot2.Event
{
    public class PlayerPayLoad
    {
        internal PlayerPayLoad() { }

        public string Name => Types.FixMcChat(RawName);
        public string RawName { get; set; }
        public string Id { get; set; }
    }
}