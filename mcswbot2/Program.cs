using mcswbot2.Bot;
using System;

namespace mcswbot2
{
    class Program
    {
        static void Main(string[] args)
        {
            TgBot.Start();
        }

        /// <summary>
        ///     DateTime Wrapper for Console WriteLine
        /// </summary>
        /// <param name="l"></param>
        internal static void WriteLine(string l)
        {
            Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-ss HH:mm:ss") + "] " + l);
        }
    }
}
