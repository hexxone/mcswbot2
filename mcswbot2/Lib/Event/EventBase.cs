namespace mcswbot2.Lib.Event
{
    internal abstract class EventBase
    {
        /// <summary>
        ///     This function needs to be overwritten to return the event-specific message
        ///     with given formatting.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public abstract string GetEventString(Types.Formatting format = Types.Formatting.None);

        /// <summary>
        ///     Helper function for wrapping stuff in code-tags
        /// </summary>
        /// <param name="format"></param>
        /// <param name="text"></param>
        /// <returns></returns>
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