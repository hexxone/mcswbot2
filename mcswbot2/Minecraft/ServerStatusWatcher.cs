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
        public async Task<ServerInfoExtended> Execute(CancellationToken token, int retries, int retryMs)
        {
            ServerInfoExtended sie;
            try
            {
                sie = await InternalExecute(token, retries, retryMs)
                      ?? throw new Exception("Connection Timeout.");
            }
            catch (Exception e)
            {
                Logger.WriteLine("Execute Error? [" + Address + ":" + Port + "]: " + e);
                sie = new ServerInfoExtended(DateTime.Now, e);
            }

            return sie;
        }


        private async Task<ServerInfoExtended?> InternalExecute(CancellationToken ct, int retries, int retryMs)
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
                    current = await si.GetAsync(ct, dt);
                    if (current.HadSuccess || ct.IsCancellationRequested)
                    {
                        break;
                    }

                    await Task.Delay(retryMs, ct).WaitAsync(ct);
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