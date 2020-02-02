using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using mcswbot2.Lib.ServerInfo;
using Newtonsoft.Json;

namespace mcswbot2.Lib.Factory
{
    internal class ServerStatusBase
    {
        // the time over which server infos are held in memory...
        private static readonly TimeSpan ClearSpan = new TimeSpan(0, 10, 0);

        // contains the received Server-Infos.
        [JsonIgnore]
        public readonly List<ServerInfoBase> History = new List<ServerInfoBase>();

        public string Address { get; set; }
        public int Port { get; set; }


        /// <summary>
        ///     This method will repeatedly ping the server to request infos.
        ///     It will then trigger given events.
        /// </summary>
        public void Ping(CancellationToken ct)
        {
            var srv = "[" + Address + ":" + Port + "]";
            Program.WriteLine("Pinging server " + srv);
            // safety-wrapper
            try
            {
                // current server-info object
                ServerInfoBase current = null;
                for (var i = 0; i < 2; i++)
                    if ((current = GetMethod(i, ct)).HadSuccess || ct.IsCancellationRequested)
                        break;

                // if the result is null, nothing to do here
                if (current != null)
                {
                    Program.WriteLine("Ping result " + srv + " is " + current.HadSuccess);
                    History.Add(current);
                }
                else Program.WriteLine("Ping result null " + srv);
            }
            catch (Exception ex)
            {
                Program.WriteLine("Fatal Error when Pinging... " + ex.ToString());
            }
            // cleanup, done
            ClearMem();
        }

        /// <summary>
        ///     Manually register a Timeout from out of scope...
        /// </summary>
        public void RegisterTimeout()
        {
            Program.WriteLine("Ping Timeout? [" + Address + ":" + Port + "]");
            History.Add(new ServerInfoBase(DateTime.Now.Subtract(TimeSpan.FromSeconds(30)), 30000, new TimeoutException()));
        }

        /// <summary>
        ///     Returns the latest (successfull) ServerInfo
        /// </summary>
        /// <param name="successful">filter for the last successfull</param>
        /// <returns></returns>
        public ServerInfoBase GetLatestServerInfo(bool successful = false)
        {
            var tmpList = new List<ServerInfoBase>();
            tmpList.AddRange(successful ? History.FindAll(o => o.HadSuccess) : History);
            return tmpList.Count > 0
                ? tmpList.OrderByDescending(ob => ob.RequestDate.AddMilliseconds(ob.RequestTime)).First()
                : null;
        }

        /// <summary>
        ///     This method will request the server infos for the given version/method.
        ///     it is run as task to make it cancelable
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private ServerInfoBase GetMethod(int method, CancellationToken ct)
        {
            switch (method)
            {
                case 1:
                    return new GetServerInfoOld(Address, Port).DoAsync(ct).Result;
                default:
                    return new GetServerInfoNew(Address, Port).DoAsync(ct).Result;
            }
        }

        /// <summary>
        ///     Remove all objects of which the Timestamp exceeds the Clearspan and run GC.
        /// </summary>
        private void ClearMem()
        {
            _ = History.RemoveAll(o => o.RequestDate < DateTime.Now.Subtract(ClearSpan));
            GC.Collect();
        }
    }
}