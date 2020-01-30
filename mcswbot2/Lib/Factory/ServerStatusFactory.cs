using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mcswbot2.Lib.Factory
{
    class ServerStatusFactory
    {

        private static ServerStatusFactory self;

        public static ServerStatusFactory Get()
        {
            if (self == null) self = new ServerStatusFactory();
            return self;
        }

        private ServerStatusFactory() { }

        private List<ServerStatusBase> serverStatusBases = new List<ServerStatusBase>();

        private List<ServerStatus> serverStates = new List<ServerStatus>();

        /// <summary>
        ///     Ping & update all the servers and groups
        /// </summary>
        public void PingAll()
        {
            Parallel.ForEach(serverStatusBases, srv => srv.Ping());
        }

        public ServerStatus Make(string label, string addr, int port = 25565)
        {
            // Get Serverstatusbase or make & add one
            var atl = addr.ToLower();
            var found = serverStatusBases.FirstOrDefault(s => s.Address.ToLower() == atl && s.Port == port);
            if (found == null) serverStatusBases.Add(found = new ServerStatusBase() { Address = addr, Port = port });
            // Make & add new status
            var state = new ServerStatus() { Label = label, Base = found };
            serverStates.Add(state);
            return state;
        }

        public bool Destroy(ServerStatus status)
        {
            if (!serverStates.Contains(status)) return false;
            serverStates.Remove(status);
            // check if the base is still in use and if not remove it
            var anyUsed = serverStates.FirstOrDefault(s => s.Base.Address == status.Base.Address && s.Base.Port == status.Base.Port);
            if (anyUsed == null) serverStatusBases.Remove(status.Base);
            // done
            return true;
        }
    }
}
