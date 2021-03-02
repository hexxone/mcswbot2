using System;
using static mcswbot2.Objects.Types;

namespace mcswbot2.Objects
{
    public static class Logger
    {
        public static LogLevel LogLevel = LogLevel.Normal;

        /// <summary>
        ///     DateTime Wrapper for Console WriteLine
        /// </summary>
        /// <param name="l"></param>
        public static void WriteLine(string l, LogLevel lv = LogLevel.Normal)
        {
            if(LogLevel >= lv)  Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-ss HH:mm:ss")}] {l}");
        }
    }
}
