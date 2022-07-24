using System;

namespace McswBot2;

internal class Program
{
    private static void Main(string[] args)
    {
        McswBot.Start();
    }

    /// <summary>
    ///     DateTime Wrapper for Console WriteLine
    /// </summary>
    /// <param name="l"></param>
    internal static void WriteLine(string l)
    {
        Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + l);
    }
}