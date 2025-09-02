namespace McswBot2
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Program
    {
        private static void Main()
        {
            new McswBot().StartAsync().GetAwaiter().GetResult();
        }
    }
}