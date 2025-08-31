using McswBot2.Static;

namespace McswBot2.Objects
{
    public class PlayerPayload
    {
        public string? Name => Types.FixMcChat(RawName);
        public string? RawName { get; set; }
        public string? Id { get; set; }
    }
}