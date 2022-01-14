using System;

namespace mcswbot2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MCSWBot.Start();
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