namespace mcswbot2.Lib.Event
{
    internal class PlayerPayLoad
    {
        public string Name => Types.FixMcChat(Name);
        public string RawName { get; set; }
        public string Id { get; set; }
    }
}