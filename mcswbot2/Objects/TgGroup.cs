using System;
using System.Collections.Generic;
using System.Linq;
using McswBot2.Minecraft;
using McswBot2.Static;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace McswBot2.Objects
{
    [Serializable]
    public class TgGroup
    {
        /// <summary>
        ///     Normal contructing
        /// </summary>
        /// <param name="basis"></param>
        public TgGroup(Chat basis)
        {
            Base = basis;
        }

        /// <summary>
        ///     Re-Constructing from Json
        /// </summary>
        /// <param name="Base"></param>
        [JsonConstructor]
        public TgGroup(List<ServerStatusWatcher> watchedServers, Chat Base)
        {
            WatchedServers = watchedServers;
            // register Events
            this.Base = Base;
        }

        // Identity
        public List<ServerStatusWatcher> WatchedServers { get; set; } = new();

        public Chat Base { get; set; }

        /// <summary>
        ///     Tries to find & return the server with defined label, if not found returns null
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        internal ServerStatusWatcher? GetServer(string l)
        {
            return WatchedServers.FirstOrDefault(item =>
                string.Equals(item.Label, l, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        ///     Add a server with given label, address and port
        /// </summary>
        /// <param name="l"></param>
        /// <param name="adr"></param>
        /// <param name="p"></param>
        /// <param name="reuse"></param>
        internal void AddServer(string l, string adr, int p)
        {
            var news = new ServerStatusWatcher(l, adr, p);
            WatchedServers.Add(news);
        }


        /// <summary>
        ///     Remove a server with given label
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        internal bool RemoveServer(string l)
        {
            var stat = GetServer(l);
            if (stat == null)
            {
                return false;
            }

            WatchedServers.Remove(stat);
            return true;
        }


        /// <summary>
        ///     Send a message to this group with a given bitmap
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pm"></param>
        /// <param name="replyMsg"></param>
        /// <param name="editMsg"></param>
        internal Message? SendMsg(string? text = null, ParseMode pm = ParseMode.Markdown,
            int replyMsg = 0, int editMsg = 0)
        {
            return SendMsgStream(text, pm, replyMsg, editMsg);
        }

        /// <summary>
        ///     Send a message to this group with a given image stream?
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pm"></param>
        /// <param name="replyMsg"></param>
        /// <param name="editMsg"></param>
        private Message? SendMsgStream(string? text = null, ParseMode pm = ParseMode.Markdown,
            int replyMsg = 0, int editMsg = 0)
        {
            Message? lMsg;
            // send normal text
            if (text != null)
            {
                // Update existing message
                lMsg = editMsg != 0
                    ? McswBot.Client?.EditMessageTextAsync(new ChatId(Base.Id),
                        editMsg,
                        text,
                        pm,
                        disableWebPagePreview: true).Result
                    :
                    // Send New
                    McswBot.Client?.SendTextMessageAsync(Base.Id,
                        text,
                        pm,
                        disableWebPagePreview: true,
                        disableNotification: false,
                        replyToMessageId: replyMsg).Result;
            }
            // uhoh?
            else
            {
                throw new Exception("Nothing to send!");
            }

            return lMsg;
        }
    }
}