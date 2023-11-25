using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace McswBot2.Objects
{
    public class MultiThreadedQueueWorker : IDisposable
    {
        private static MultiThreadedQueueWorker? _instance;

        public static MultiThreadedQueueWorker Instance
        {
            get
            {
                return _instance ??= new MultiThreadedQueueWorker(5);
            }
        }


        private bool _runFlag = true;
        private readonly List<Thread> _threads = new();

        private readonly ConcurrentQueue<Func<Task>> _taskQueue = new();
        private readonly ManualResetEvent _newTaskEvent = new(false);
        private readonly int _numberOfThreads;

        private MultiThreadedQueueWorker(int numberOfThreads)
        {
            _numberOfThreads = numberOfThreads;
            InitializeWorkerThreads();
        }

        private void InitializeWorkerThreads()
        {
            for (var i = 0; i < _numberOfThreads; i++)
            {
                var thread = new Thread(async () =>
                {
                    while (_runFlag)
                    {
                        _newTaskEvent.WaitOne(10);
                        if (_taskQueue.TryDequeue(out var task))
                        {
                            await task();
                        }
                        if (_taskQueue.IsEmpty)
                        {
                            _newTaskEvent.Reset();
                        }
                    }
                })
                {
                    IsBackground = true
                };
                thread.Start();
                _threads.Add(thread);
            }
        }

        public Task<T> Execute<T>(Func<Task<T>> task)
        {
            var tcs = new TaskCompletionSource<T>();
            _taskQueue.Enqueue(async () =>
            {
                try
                {
                    var result = await task();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            _newTaskEvent.Set();
            return tcs.Task;
        }

        public void Dispose()
        {
            _runFlag = false;
            foreach (var thread in _threads)
            {
                thread.Join();
            }
            _newTaskEvent.Dispose();
        }
    }
}
