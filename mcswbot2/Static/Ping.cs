using McswBot2.Minecraft;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace McswBot2.Static
{
    internal static class Ping
    {
        /// <summary>
        ///     TODO test
        /// </summary>
        /// <param name="servers"></param>
        /// <returns></returns>
        internal static ConcurrentBag<Tuple<ServerStatusWatcher, ServerInfoExtended?>> PingAll(this IEnumerable<ServerStatusWatcher> servers, int timeOutMs, int retries, int retryMs)
        {
            var cts = new CancellationTokenSource();
            var pOptions = new ParallelOptions
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = 5
            };

            var pingResults = new ConcurrentBag<Tuple<ServerStatusWatcher, ServerInfoExtended?>>();

            try
            {
                // creating second thread to cancel Parallel.For loop
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(timeOutMs);
                    cts.Cancel();
                });
                // request info parallel
                var res = Parallel.ForEach(servers, pOptions,
                    (watcher, state) =>
                    {
                        pingResults.Add(
                            new Tuple<ServerStatusWatcher, ServerInfoExtended?>(watcher, watcher.Execute(cts.Token, retries, retryMs)));
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