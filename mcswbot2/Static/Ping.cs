using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using McswBot2.Minecraft;
using McswBot2.Objects;

namespace McswBot2.Static
{
    internal static class Ping
    {
        /// <summary>
        ///     TODO test
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        internal static ConcurrentBag<Tuple<ServerStatusWatcher, ServerInfoExtended?>> PingAllServers(this TgGroup g)
        {
            var cts = new CancellationTokenSource();
            var pOptions = new ParallelOptions
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = 5
            };

            var pingResults = new ConcurrentBag<Tuple<ServerStatusWatcher, ServerInfoExtended?>>();
            var timeOut = McswBot.Conf.TimeoutMs;

            try
            {
                // creating second thread to cancel Parallel.For loop
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(timeOut);
                    cts.Cancel();
                });
                // request info parallel
                var res = Parallel.ForEach(g.WatchedServers, pOptions,
                    (watcher, state) =>
                    {
                        pingResults.Add(
                            new Tuple<ServerStatusWatcher, ServerInfoExtended?>(watcher, watcher.Execute(cts.Token)));
                    });
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e);
                throw;
            }

            return pingResults;
        }
    }
}