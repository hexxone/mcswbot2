using Imazen.WebP;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Objects
{
    public class TgGroup
    {
        // Identity
        public List<ServerStatusWrapped> Servers = new List<ServerStatusWrapped>();
        public Chat Base { get; set; }

        public bool Thanos = false;

        /// <summary>
        ///     Register & Start updating all the servers in this group after deserializing
        /// </summary>
        internal void UpdateAll()
        {
            Parallel.ForEach(Servers, srv =>
            {
                var res = srv.Wrapped.Update();
                if (res.Length > 0)
                {
                    var updateMsg = $"[<code>{srv.Label}</code>]";
                    foreach (var evt in res)
                        updateMsg += "\r\n" + evt;

                    // get any successfull result for server image
                    var lastSuccess = srv.Wrapped.Updater.GetLatestServerInfo(true);
                    // get & scale server image or use empty
                    if (srv.Sticker)
                    {
                        // todo soome kind of color formatting maybe?
                        Bitmap t = null;
                        if(Thanos && (t = Imaging.TheThingWeDontTalkAbout()) != null)
                        {
                            using (var txtBmp = Imaging.MakeSticker(t, updateMsg))
                                SendMsg(null, txtBmp, ParseMode.Default, 0, true);
                        }
                        else if(lastSuccess != null && lastSuccess.FavIcon != null)
                        {
                            using (var txtBmp = Imaging.MakeSticker(lastSuccess.FavIcon, updateMsg))
                                SendMsg(null, txtBmp, ParseMode.Default, 0, true);
                        }
                    }
                    else
                    {
                        SendMsg(updateMsg, null, ParseMode.Html, 0, true);
                    }
                }
            });
        }

        /// <summary>
        ///     Tries to find & return the server with defined label, if not found returns null
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        internal ServerStatusWrapped GetServer(string l)
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
        internal void SendMsg(string text = null, Bitmap bitmap = null, ParseMode pm = ParseMode.Default, int replyMsg = 0, bool sticker = false)
        {
            if (bitmap != null)
            {
                using (var ms = new MemoryStream())
                {
                    if (sticker)
                    {
                        var encoder = new SimpleEncoder();
                        encoder.Encode(bitmap, ms, -1);
                    }
                    else
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                    }
                    ms.Position = 0;
                    SendMsgStream(text, ms, pm, replyMsg, sticker);
                }
            }
            else SendMsgStream(text, null, pm, replyMsg, sticker);
        }

        /// <summary>
        ///     Send a message to this group with a given image stream?
        /// </summary>
        /// <param name="text"></param>
        /// <param name="imgStream"></param>
        /// <param name="pm"></param>
        /// <param name="replyMsg"></param>
        /// <param name="sticker"></param>
        internal void SendMsgStream(string text = null, Stream imgStream = null, ParseMode pm = ParseMode.Default, int replyMsg = 0, bool sticker = false)
        {
            try
            {
                var sendText = (text != null);
                if (imgStream != null)
                {
                    var iof = new Telegram.Bot.Types.InputFiles.InputOnlineFile(imgStream);
                    if (sticker)
                    {
                        var msg = TgBot.Client.SendStickerAsync(Base.Id, iof, false, replyMsg).Result;
                        replyMsg = msg.MessageId;
                    }
                    else
                    {
                        TgBot.Client.SendPhotoAsync(Base.Id, iof, text, pm, false, replyMsg).Wait();
                        // sending image with text did succeed, so we dont need to send text-only message.
                        sendText = false;
                    }
                }
                else if (!sendText)
                    throw new Exception("Nothing to send!");

                if (sendText)
                    TgBot.Client.SendTextMessageAsync(Base.Id, text, pm, true, false, replyMsg).Wait();
            }
            catch (Exception ex)
            {
                Program.WriteLine("Send Exception: " + ex + "\r\nGroup: " + Base.Id + "\r\nStack: " + ex.StackTrace);
            }
        }
    }
}