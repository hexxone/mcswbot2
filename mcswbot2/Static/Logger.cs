using System;

namespace McswBot2.Static
{
    public static class Logger
    {
        public static Types.LogLevel LogLevel = Types.LogLevel.Info;

        /// <summary>
        ///     DateTime Wrapper for Console WriteLine
        /// </summary>
        /// <param name="line"></param>
        /// <param name="logLevel"></param>
        public static void WriteLine(string line, Types.LogLevel logLevel = Types.LogLevel.Info)
        {
            if (LogLevel >= logLevel)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-ss HH:mm:ss}] {line}");
            }
        }
    }
}