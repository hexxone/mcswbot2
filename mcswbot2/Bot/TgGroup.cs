using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mcswbot2.Lib;
using mcswbot2.Lib.Factory;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot
{
    internal class TgGroup
    {
        // Identity

        public List<ServerStatus> Servers = new List<ServerStatus>();
        public Chat Base { get; set; }

        /// <summary>
        ///     Register & Start updating all the servers in this group after deserializing
        /// </summary>
        public void UpdateAll()
        {
            Parallel.ForEach(Servers, srv =>
            {
                var res = srv.Update();
                if (res.Length > 0)
                {
                    var updateMsg = $"[<code>{srv.Label}</code>]";

                    foreach (var @event in res)
                        updateMsg += "\r\n" + @event.GetEventString(Types.Formatting.Html);

                    SendMsg(updateMsg);
                }
            });
        }

        /// <summary>
        ///     Tries to find & return the server with defined label, if not found returns null
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public ServerStatus GetServer(string l)
        {
            foreach (var item in Servers)
                if (item.Label == l)
                    return item;
            return null;
        }

        /// <summary>
        ///     Add a server with given label, address and port
        /// </summary>
        /// <param name="l"></param>
        /// <param name="adr"></param>
        /// <param name="p"></param>
        public void AddServer(string l, string adr, int p)
        {
            var news = ServerStatusFactory.Get().Make(l, adr, p);
            Servers.Add(news);
        }

        /// <summary>
        ///     Remove a server with given label
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public bool RemoveServer(string l)
        {
            var stat = GetServer(l);
            if (stat == null) return false;
            Servers.Remove(stat);
            return ServerStatusFactory.Get().Destroy(stat);
        }

        /// <summary>
        ///     Send a message to this group with Parse Mode HTML
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private void SendMsg(string m)
        {
            try
            {
                TgBot.Client.SendTextMessageAsync(Base.Id, m, ParseMode.Html).Wait();
            }
            catch (Exception ex)
            {
                Program.WriteLine("Send Exception: " + ex + "\r\nGroup: " + Base.Id + "\r\nMsg: " + m + "\r\nStack: " + ex.StackTrace);
            }
        }
    }
}