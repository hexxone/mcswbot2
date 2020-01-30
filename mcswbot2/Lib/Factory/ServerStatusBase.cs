using System;
using System.Collections.Generic;
using System.Linq;
using mcswbot2.Lib.ServerInfo;
using Newtonsoft.Json;

namespace mcswbot2.Lib.Factory
{
    public class ServerStatusBase
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
        public void Ping()
        {
            Program.WriteLine("Pinging server '" + Address + ":" + Port + "'...");
            // safety-wrapper
            try
            {
                // current server-info object
                ServerInfoBase current = null;
                for (var i = 0; i < 3; i++)
                    if ((current = GetMethod(i)).HadSuccess)
                        break;

                // get last result
                var last = GetLatestServerInfo();
                // first Info we got?
                var isFirst = last == null;
                // if the result is null, nothing to do here
                if (current != null)
                    History.Add(current);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal Error when Pinging... " + ex.ToString());
            }
            // cleanup, done
            ClearMem();
        }

        /// <summary>
        /// </summary>
        /// <param name="successful"></param>
        /// <returns></returns>
        public ServerInfoBase GetLatestServerInfo(bool successful = false)
        {
            var tmpList = new List<ServerInfoBase>();
            tmpList.AddRange(successful ? History.FindAll(o => o.HadSuccess) : History);
            return tmpList.Count > 0
                ? tmpList.OrderByDescending(ob => ob.RequestDate.AddMilliseconds(ob.RequestTime)).First()
                : null;
        }

        #region Internal

        /// <summary>
        ///     This method will request the server infos for the given version/method.
        ///     it is run as task to make it cancelable
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private ServerInfoBase GetMethod(int method)
        {
            switch (method)
            {
                case 1:
                    return Get14ServerInfo.Get(Address, Port);
                case 2:
                    return GetBetaServerInfo.Get(Address, Port);
                default:
                    return GetNewServerInfo.Get(Address, Port);
            }
        }


        // RUN GC
        private void ClearMem()
        {
            _ = History.RemoveAll(o => o.RequestDate < DateTime.Now.Subtract(ClearSpan));
            GC.Collect();
        }

        #endregion
    }
}