using mcswbot2.Lib;

namespace mcswbot2.Lib.Event
{
    public abstract class EventBase
    {
        public abstract string GetEventString(Types.Formatting format = Types.Formatting.None);

        public static string Wrap(Types.Formatting format, string text)
        {
            switch (format)
            {
                case Types.Formatting.Html: return $"<code>{text}</code>";
                case Types.Formatting.Markup: return $"```{text}```";
                default: return text;
            }
        }
    }
}