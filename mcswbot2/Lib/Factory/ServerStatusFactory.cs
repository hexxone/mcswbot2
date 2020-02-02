using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace mcswbot2.Lib.Factory
{
    class ServerStatusFactory
    {
        private static ServerStatusFactory self;

        /// <summary>
        ///     Return Singleton instance of Factory
        /// </summary>
        /// <returns></returns>
        public static ServerStatusFactory Get()
        {
            if (self == null) self = new ServerStatusFactory();
            return self;
        }

        private ServerStatusFactory() { }

        private readonly List<ServerStatusBase> serverStatusBases = new List<ServerStatusBase>();

        private readonly List<ServerStatus> serverStates = new List<ServerStatus>();

        /// <summary>
        ///     Ping & update all the servers and groups
        /// </summary>
        public void PingAll()
        {
            Parallel.ForEach(serverStatusBases, new ParallelOptions { MaxDegreeOfParallelism = 10 }, srv =>
            {
                try
                {
                    var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    var token = tokenSource.Token;

                    var task = Task.Run(() => srv.Ping(token), token);
                    task.Wait(token);
                }
                catch(Exception e)
                {
                    srv.RegisterTimeout();
                }
            });
        }

        /// <summary>
        ///     Will either reuse a given ServerStatusBase with same address or create one
        /// </summary>
        /// <param name="label"></param>
        /// <param name="addr"></param>
        /// <param name="port"></param>
        /// <returns>a new ServerStatus object with given params</returns>
        public ServerStatus Make(string label, string addr, int port = 25565)
        {
            // Get Serverstatusbase or make & add one
            var found = GetByAddr(addr, port);
            if (found == null) serverStatusBases.Add(found = new ServerStatusBase() { Address = addr, Port = port });
            // Make & add new status
            var state = new ServerStatus() { Label = label, Base = found };
            serverStates.Add(state);
            return state;
        }

        /// <summary>
        ///     Destroy a created ServerStatus object.
        ///     Will only destroy the underlying ServerStatusBase if its not used anymore.
        /// </summary>
        /// <param name="status"></param>
        /// <returns>Success indicator</returns>
        public bool Destroy(ServerStatus status)
        {
            if (!serverStates.Contains(status)) return false;
            serverStates.Remove(status);
            // check if the base is still in use and if not remove it
            var anyUsed = GetByAddr(status.Base.Address, status.Base.Port);
            if (anyUsed == null) serverStatusBases.Remove(status.Base);
            // done
            return true;
        }

        /// <summary>
        ///     Will try to find a ServerStatusBase with given constraints
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private ServerStatusBase GetByAddr(string addr, int port)
        {
            return serverStatusBases.FirstOrDefault(s => s.Address.ToLower() == addr.ToLower() && s.Port == port);
        }
    }
}
