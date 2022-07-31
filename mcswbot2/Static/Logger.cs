using System;

namespace McswBot2.Static
{
    public static class Logger
    {
        public static Types.LogLevel LogLevel = Types.LogLevel.Normal;

        /// <summary>
        ///     DateTime Wrapper for Console WriteLine
        /// </summary>
        /// <param name="l"></param>
        public static void WriteLine(string l, Types.LogLevel lv = Types.LogLevel.Normal)
        {
            if (LogLevel >= lv)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-ss HH:mm:ss")}] {l}");
            }
        }
    }
}