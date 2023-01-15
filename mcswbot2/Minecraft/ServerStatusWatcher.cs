using McswBot2.Static;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace McswBot2.Minecraft
{
    public class ServerStatusWatcher
    {


        /// <summary>
        ///     Normal constructor
        /// </summary>
        public ServerStatusWatcher(string label, string address, int port)
        {
            Label = label;
            Address = address;
            Port = port;
        }

        public string Label { get; set; }

        public string Address { get; set; }

        public int Port { get; set; }

        /// <summary>
        ///     This method will ping the server to request infos.
        ///     This is done in context of a task and 10 second timeout
        /// </summary>
        public ServerInfoExtended? Execute(CancellationToken token, int retries, int retryMs)
        {
            ServerInfoExtended sie;
            try
            {
                var task = Task.Run(() => InternalExecute(token, retries, retryMs), token);
                task.Wait(token);
                sie = task.Result ?? throw new Exception("null response");
            }
            catch (Exception e)
            {
                Logger.WriteLine("Execute Error? [" + Address + ":" + Port + "]: " + e);
                sie = new ServerInfoExtended(DateTime.Now, e);
            }

            return sie;
        }


        private ServerInfoExtended? InternalExecute(CancellationToken ct, int retries, int retryMs)
        {
            var srv = "[" + Address + ":" + Port + "]";
            Logger.WriteLine("Pinging server " + srv);
            // safety-wrapper
            try
            {
                // current server-info object
                var dt = DateTime.Now;
                ServerInfoExtended? current = null;
                var si = new ServerInfo(Address, Port);
                for (var r = 0; r < retries; r++)
                {
                    current = si.GetAsync(ct, dt).Result;
                    if (current.HadSuccess || ct.IsCancellationRequested)
                    {
                        break;
                    }

                    Task.Delay(retryMs, ct).Wait(ct);
                }

                // if the result is null, nothing to do here
                if (current != null)
                {
                    Logger.WriteLine(
                        "Execute result: " + srv + " is: " + current.HadSuccess + " Err: " + current.LastError,
                        Types.LogLevel.Debug);
                    return current;
                }

                Logger.WriteLine("Execute result null: " + srv, Types.LogLevel.Debug);
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Fatal Error when Pinging... " + ex, Types.LogLevel.Error);
            }

            return null;
        }
    }
}