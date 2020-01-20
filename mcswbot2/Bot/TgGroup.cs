using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mcswbot2.Lib;
using mcswbot2.Lib.Event;
using mcswbot2.Lib.ServerInfo;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot
{
    public class TgGroup
    {
        public List<ServerStatus> Servers = new List<ServerStatus>();

        /// <summary>
        ///     An object representing a telegram group
        /// </summary>
        /// <param name="basis"></param>
        public TgGroup(Chat basis)
        {
            Base = basis;
        }

        public Chat Base { get; set; }

        /// <summary>
        ///     Register & Start updating all the servers in this group after deserializing
        /// </summary>
        public void RegisterOnce()
        {
            foreach (var srv in Servers)
            {
                srv.ServerChangeEvent += OnServerChangeEvent;
                srv.Start();
            }
        }

        private void OnServerChangeEvent(ServerStatus self, ServerInfoBase newInfo, EventBase[] events)
        {
            var updateMsg = $"[<code>{self.Bind_Label}</code>]";

            foreach (var @event in events)
                updateMsg += "\r\n" + @event.GetEventString(Types.Formatting.Html);

            _ = SendMsg(updateMsg);
        }


        /// <summary>
        ///     Tries to find & return the server with defined label, if not found returns null
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public ServerStatus GetServer(string l)
        {
            foreach (var item in Servers)
                if (item.Bind_Label == l)
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
            var news = new ServerStatus {Bind_Label = l, Bind_Host = adr, Bind_Port = p};
            news.ServerChangeEvent += OnServerChangeEvent;
            news.Bind_ServerNotify = true;
            news.Bind_CountNotify = true;
            news.Bind_PlayerNotify = true;
            news.Start();
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
            stat.ServerChangeEvent -= OnServerChangeEvent;
            stat.Stop();
            Servers.Remove(stat);
            return true;
        }

        /// <summary>
        ///     Send a message to this group with Parse Mode HTML
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private async Task<Message> SendMsg(string m)
        {
            try
            {
                return await TgBot.Client.SendTextMessageAsync(Base.Id, m, ParseMode.Html);
            }
            catch (Exception ex)
            {
                TgBot.WriteLine("Send Exception: " + ex + "\r\nGroup: " + Base.Id + "\r\nMsg: " + m + "\r\nStack: " +
                                  ex.StackTrace);
                return null;
            }
        }
    }
}