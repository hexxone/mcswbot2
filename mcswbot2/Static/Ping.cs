using McswBot2.Minecraft;
using McswBot2.Objects;
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
            var queueWorker = MultiThreadedQueueWorker.Instance;

            try
            {
                cts.CancelAfter(timeOutMs);

                // request info in parallel
                var allTasks = new List<Task>();
                foreach (var serverStatusWatcher in servers)
                {
                    var pingTask = queueWorker.Execute(async () =>
                    {
                        var executeResult = await serverStatusWatcher.Execute(cts.Token, retries, retryMs);
                        var newItem = new Tuple<ServerStatusWatcher, ServerInfoExtended?>(serverStatusWatcher, executeResult);
                        pingResults.Add(newItem);
                        return Task.CompletedTask;
                    });
                    allTasks.Add(pingTask);
                }

                Task.WaitAll(allTasks.ToArray());
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