using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using mcswlib.ServerStatus.Event;
using SkiaSharp;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Objects
{
    public class TgGroup
    {
        // the time over which server infos are held in memory...
        [JsonIgnore]
        public static TimeSpan ClearSpan = new TimeSpan(3, 0, 0, 0);

        // Identity
        public List<ServerStatusWrapped> Servers = new List<ServerStatusWrapped>();
        public Chat Base { get; set; }

        public bool Tahnos = false;

        public List<TahnosInfo> ImagingData = new List<TahnosInfo>();

        /// <summary>
        ///     Register & Start updating all the servers in this group after deserializing
        /// </summary>
        internal void UpdateAll()
        {
            Parallel.ForEach(Servers, srv =>
            {
                var res = srv.Wrapped.Update();
                if (res.Length <= 0) return;

                var updateMsg = $"[<code>{srv.Label}</code>]";
                foreach (var evt in res)
                {
                    updateMsg += "\r\n" + evt;
                    if (!(evt is PlayerStateEvent)) continue;
                    var pse = evt as PlayerStateEvent;
                    var now = DateTime.Now;
                    if (pse.Online)
                    {
                        srv.NameHistory[pse.Player.Id] = pse.Player.Name;
                        srv.SeenTime[pse.Player.Id] = now;
                    }
                    else
                    {
                        var span = now - srv.SeenTime[pse.Player.Id];
                        srv.SeenTime[pse.Player.Id] = now;

                        if (!srv.PlayTime.ContainsKey(pse.Player.Id))
                            srv.PlayTime[pse.Player.Name] = TimeSpan.Zero;

                        srv.PlayTime[pse.Player.Id] += span;
                    }
                }

                // get & scale server image or use empty
                var sent = false;
                if (srv.Sticker)
                {
                    TahnosInfo t = null;
                    if (Tahnos && (t = TahnosInfo.Get()) != null)
                    {
                        using var txtBmp = Imaging.MakeSticker(t.Bmap, updateMsg);
                        var msg = SendMsg(null, txtBmp, ParseMode.Default, 0, true);
                        t.RelatedMsgID = msg.MessageId;
                        t.Bmap.Dispose();
                        t.Bmap = null;
                        ImagingData.Add(t);
                        sent = true;
                    }
                    else if (srv.Wrapped.FavIcon != null)
                    {
                        using (var txtBmp = Imaging.MakeSticker(srv.Wrapped.FavIcon, updateMsg))
                            SendMsg(null, txtBmp, ParseMode.Default, 0, true);
                        sent = true;
                    }
                }
                // send message if sticker disabled or failed
                if(!sent) SendMsg(updateMsg, null, ParseMode.Html, 0, false);
            });
            // free resources
            CleanData();
        }

        /// <summary>
        ///     Clean-up ImagingData
        /// </summary>
        private void CleanData()
        {
            // clear old elements > 3 days
            ImagingData.FindAll(id => id.Acquired < DateTime.Now.Subtract(ClearSpan)).ForEach(id =>
            {
                ImagingData.Remove(id);
            });
            // TODO TEST
            // clear old elements if count > 30
            ImagingData.OrderBy(id => id.Acquired.Ticks).Where((a,i) => i > 30).ToList().ForEach(id =>
            {
                ImagingData.Remove(id);
            });
            GC.Collect();
        }

        /// <summary>
        ///     Tries to find & return the server with defined label, if not found returns null
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        internal ServerStatusWrapped GetServer(string l)
        {
            return Servers.FirstOrDefault(item => string.Equals(item.Label, l, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        ///     Add a server with given label, address and port
        /// </summary>
        /// <param name="l"></param>
        /// <param name="adr"></param>
        /// <param name="p"></param>
        internal void AddServer(string l, string adr, int p)
        {
            var news = TgBot.Factory.Make(adr, p, false, l);
            Servers.Add(new ServerStatusWrapped(news));
        }

        /// <summary>
        ///     Remove a server with given label
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        internal bool RemoveServer(string l)
        {
            var stat = GetServer(l);
            if (stat == null) return false;
            Servers.Remove(stat);
            return TgBot.Factory.Destroy(stat.Wrapped);
        }


        /// <summary>
        ///     Send a message to this group with a given bitmap
        /// </summary>
        /// <param name="text"></param>
        /// <param name="bitmap"></param>
        /// <param name="pm"></param>
        /// <param name="replyMsg"></param>
        /// <param name="sticker"></param>
        internal Message SendMsg(string text = null, SKImage bitmap = null, ParseMode pm = ParseMode.Default, int replyMsg = 0, bool sticker = false)
        {
            if (bitmap != null)
            {
                using var ms = bitmap.Encode(sticker ? SKEncodedImageFormat.Webp : SKEncodedImageFormat.Png, 100).AsStream();
                ms.Position = 0;
                return SendMsgStream(text, ms, pm, replyMsg, sticker);
            }
            else
            {
                return SendMsgStream(text, null, pm, replyMsg, sticker);
            }
        }

        /// <summary>
        ///     Send a message to this group with a given image stream?
        /// </summary>
        /// <param name="text"></param>
        /// <param name="imgStream"></param>
        /// <param name="pm"></param>
        /// <param name="replyMsg"></param>
        /// <param name="sticker"></param>
        private Message SendMsgStream(string text = null, Stream imgStream = null, ParseMode pm = ParseMode.Default, int replyMsg = 0, bool sticker = false)
        {
            Message lMsg = null;
            try
            {
                var sendText = (text != null);
                if (imgStream != null)
                {
                    var iof = new Telegram.Bot.Types.InputFiles.InputOnlineFile(imgStream);
                    if (sticker)
                    {
                        lMsg = TgBot.Client.SendStickerAsync(Base.Id, iof, false, replyMsg).Result;
                        replyMsg = lMsg.MessageId;
                    }
                    else
                    {
                        lMsg = TgBot.Client.SendPhotoAsync(Base.Id, iof, text, pm, false, replyMsg).Result;
                        sendText = false;
                    }
                }

                if (sendText) lMsg = TgBot.Client.SendTextMessageAsync(Base.Id, text, pm, true, false, replyMsg).Result;
            }
            catch (Exception ex)
            {
                if (ex.StackTrace.Contains("chat not found")) TgBot.DestroyGroup(this);
                else Program.WriteLine("Send Exception: " + ex + "\r\nGroup: " + Base.Id + "\r\nStack: " + ex.StackTrace);
            }
            return lMsg;
        }

        internal void Destroy()
        {
            foreach (var item in Servers)
                TgBot.Factory.Destroy(item.Wrapped);
        }
    }
}