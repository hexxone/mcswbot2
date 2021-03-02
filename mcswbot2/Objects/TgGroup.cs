using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using mcswbot2.Event;
using mcswbot2.ServerInfo;
using mcswbot2.ServerStatus;
using mcswbot2.Telegram;
using SkiaSharp;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace mcswbot2.Objects
{
    [Serializable]
    public class TgGroup
    {
        // Identity
        public List<ServerStatusWrapped> Servers = new();
        public Chat Base { get; set; }

        public bool Tahnos = false;

        public List<TahnosInfo> ImagingData = new();

        public int LivePlayerMsgId = 0;

        /// <summary>
        ///     Register & Start updating all the servers in this group after deserializing
        /// </summary>
        private void Update(ServerStatus.ServerStatus status, EventBase[] events)
        {
            foreach (var srv in Servers)
            {
                if (srv.Wrapped != status) continue;

                var updateMsg = $"[<code>{srv.Label}</code>]";
                var addMore = true;
                var added = false;
                foreach (var evt in events)
                {
                    var (txt, more) = ProcessEventMessage(srv, evt);
                    if(txt == null) continue;
                    // stop adding further text after "online-status" event
                    if (addMore) updateMsg += txt;
                    addMore = addMore && more;
                    added = true;
                }

                // no events processed => no message to send
                if(!added) continue;

                // get & scale server image or use empty
                var sent = false;
                if (srv.Sticker)
                {
                    TahnosInfo t = null;
                    if (srv.Wrapped.Last.FavIcon != null)
                    {
                        using (var txtBmp = Imaging.MakeSticker(srv.Wrapped.Last.FavIcon, updateMsg))
                            SendMsg(null, txtBmp, ParseMode.Default, 0, true);
                        sent = true;
                    }
                    else if (Tahnos && (t = TahnosInfo.Get()) != null)
                    {
                        using var txtBmp = Imaging.MakeSticker(t.Bmap, updateMsg);
                        var msg = SendMsg(null, txtBmp, ParseMode.Default, 0, true);
                        t.RelatedMsgID = msg.MessageId;
                        t.Bmap.Dispose();
                        t.Bmap = null;
                        ImagingData.Add(t);
                        sent = true;
                    }
                }
                // send message if sticker disabled or failed
                if (!sent) SendMsg(updateMsg, null, ParseMode.Html, 0, false);
            }
        }

        /// <summary>
        ///     Processes EventBase and returns adequate message
        /// </summary>
        /// <param name="srv"></param>
        /// <param name="evt"></param>
        /// <returns>   string (message), bool (continue processing?)</returns>
        private static Tuple<string, bool> ProcessEventMessage(ServerStatusWrapped srv, EventBase evt)
        {
            switch (evt)
            {
                case OnlineStatusEvent ose:
                    if (!srv.NotifyServer) return new Tuple<string, bool>(null, false);
                    return new Tuple<string, bool>((ose.ServerStatus ? EventMessages.ServerOnline : EventMessages.ServerOffline)
                        .Replace("<text>", ose.StatusText)
                        .Replace("<version>", ose.Version)
                        .Replace("<players>", $"{ose.CurrentPlayers} / {ose.MaxPlayers}"), false);

                case PlayerChangeEvent pce:
                    if (!srv.NotifyCount) return new Tuple<string, bool>(null, false);
                    var abs = Math.Abs(pce.PlayerDiff);
                    var msg2 = pce.PlayerDiff > 0 ? EventMessages.CountJoin : EventMessages.CountLeave;
                    msg2 = msg2.Replace("<count>", abs.ToString());
                    msg2 = msg2.Replace("<player>", "Player" + (abs > 1 ? "s" : ""));
                    return new Tuple<string, bool>(msg2, true);

                case PlayerStateEvent pse:
                    // update player history / time etc..
                    var now = DateTime.Now;

                    // add seen data
                    if (!srv.SeenTime.ContainsKey(pse.Player.Id))
                        srv.SeenTime.Add(pse.Player.Id, now);
                    else
                        srv.SeenTime[pse.Player.Id] = now;

                    // "Add" Play time
                    if (!srv.PlayTime.ContainsKey(pse.Player.Id))
                        srv.PlayTime.Add(pse.Player.Id, TimeSpan.Zero);

                    var playSpan = DateTime.Now - srv.SeenTime[pse.Player.Id];
                    if (pse.Online)
                    {
                        // Add current player name
                        if (!srv.NameHistory.ContainsKey(pse.Player.Id))
                            srv.NameHistory.Add(pse.Player.Id, pse.Player.Name);
                        else
                            srv.NameHistory[pse.Player.Id] = pse.Player.Name;
                    }
                    else srv.PlayTime[pse.Player.Id] += playSpan;

                    if (!srv.NotifyNames) return new Tuple<string, bool>(null, false);
                    // build & return message
                    var msg1 = (pse.Online ? EventMessages.NameJoin : EventMessages.NameLeave);
                    msg1 = msg1.Replace("<name>", pse.Player.Name);
                    msg1 = msg1.Replace("<time>", playSpan.TotalHours.ToString("0.00") + " h");
                    return new Tuple<string, bool>(msg1, true);
            }
            return new Tuple<string, bool>("", false);
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
            var news = MCSWBot.Factory.Make(adr, p, false, l);
            news.ChangedEvent += StatusChanged;
            var wrap = new ServerStatusWrapped(news);
            Servers.Add(wrap);
        }

        /// <summary>
        ///     RE-Adding a Server from loaded Json
        /// </summary>
        /// <param name="ssw"></param>
        internal void LoadedServer(ServerStatusWrapped loaded)
        {
            var news = MCSWBot.Factory.Make(loaded.Address, loaded.Port, false, loaded.Label);
            news.ChangedEvent += StatusChanged;
            loaded.Wrapped = news;
        }

        /// <summary>
        ///     Event on Server status change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatusChanged(object? sender, EventBase[] e)
        {
            // get the sending status
            var status = (ServerStatus.ServerStatus) sender;
            // get wrapped object
            var wrap = GetServer(status.Label);
            // add current status to history
            wrap.History.Add(new ServerInfoWrapped(status.Last, 0));

            // do the updating
            if(e.Length > 0) Update(status, e);
            UpdateLivePlayer();
            
            // free resources
            CleanData();
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
            return MCSWBot.Factory.Destroy(stat.Wrapped);
        }


        /// <summary>
        ///     Send a message to this group with a given bitmap
        /// </summary>
        /// <param name="text"></param>
        /// <param name="bitmap"></param>
        /// <param name="pm"></param>
        /// <param name="replyMsg"></param>
        /// <param name="sticker"></param>
        internal Message SendMsg(string text = null, SKImage bitmap = null, ParseMode pm = ParseMode.Default, int replyMsg = 0, bool sticker = false, int editMsg = 0)
        {
            if (bitmap == null) return SendMsgStream(text, null, pm, replyMsg, sticker);

            var ms = bitmap.Encode(sticker ? SKEncodedImageFormat.Webp : SKEncodedImageFormat.Png, 100).AsStream();
            ms.Position = 0;
            return SendMsgStream(text, ms, pm, replyMsg, sticker, editMsg);

        }

        /// <summary>
        ///     Send a message to this group with a given image stream?
        /// </summary>
        /// <param name="text"></param>
        /// <param name="imgStream"></param>
        /// <param name="pm"></param>
        /// <param name="replyMsg"></param>
        /// <param name="sticker"></param>
        private Message SendMsgStream(string text = null, Stream imgStream = null, ParseMode pm = ParseMode.Default, int replyMsg = 0, bool sticker = false, int editMsg = 0)
        {
            Message lMsg = null;
            try
            {
                if (imgStream != null)
                {

                    // send sticker (cant be updated!)
                    if (sticker)
                    {
                        var iof = new InputOnlineFile(imgStream);

                        if (editMsg != 0) Logger.WriteLine("Stickers cant be updated!");

                        lMsg = MCSWBot.Client.SendStickerAsync(Base.Id, iof, false, replyMsg).Result;
                        // send text in response immediately afterwards ?
                        if (text != null)
                        {
                            replyMsg = lMsg.MessageId;
                            lMsg = MCSWBot.Client.SendTextMessageAsync(Base.Id,
                                text,
                                pm,
                                true,
                                false,
                                replyMsg).Result;
                        }
                    }
                    // send normal image
                    else
                    {
                        // Update existing image
                        lMsg = editMsg != 0 ? MCSWBot.Client.EditMessageMediaAsync(new ChatId(Base.Id),
                                editMsg,
                                new InputMediaPhoto(new InputMedia(imgStream, "updated.png")) { Caption = text, ParseMode = pm }).Result :
                            // Send new Image
                            MCSWBot.Client.SendPhotoAsync(Base.Id, 
                                new InputOnlineFile(imgStream),
                                text,
                                pm, 
                                false,
                                replyMsg).Result;
                    }
                    
                    imgStream.Dispose();
                }
                // send normal text
                else if (text != null)
                {
                        // Update existing message
                    lMsg = editMsg != 0 ? MCSWBot.Client.EditMessageTextAsync(new ChatId(Base.Id),
                            editMsg,
                            text,
                            pm,
                            true).Result :
                        // Send New
                        MCSWBot.Client.SendTextMessageAsync(Base.Id,
                            text,
                            pm,
                            true,
                            false,
                            replyMsg).Result;
                }
                // uhoh?
                else throw new Exception("Nothing to send!");
            }
            catch (Exception ex)
            {
                if (ex.StackTrace.Contains("chat not found")) MCSWBot.DestroyGroup(this);
                else Program.WriteLine("Send Exception: " + ex + "\r\nGroup: " + Base.Id + "\r\nStack: " + ex.StackTrace);
            }
            return lMsg;
        }

        internal void Destroy()
        {
            foreach (var item in Servers)
                MCSWBot.Factory.Destroy(item.Wrapped);
        }

        /// <summary>
        ///     Automatically update live player message?
        /// </summary>
        private void UpdateLivePlayer()
        {
            if (LivePlayerMsgId == 0) return;
            SendPlayerMessage(LivePlayerMsgId);
        }

        /// <summary>
        ///     Send or Edit Online Player Message
        /// </summary>
        /// <param name="editMessage"></param>
        /// <returns></returns>
        internal Message SendPlayerMessage(int editMessage = 0)
        {
            var msg = "Player Online:";
            var plots = new List<SkiaPlotter.PlottableData>();
            foreach (var item in Servers)
            {
                var status = item.Wrapped.Last;

                msg += "\r\n[<code>" + item.Label + "</code>] ";
                if ((!status?.HadSuccess) ?? true) msg += " Offline";
                else msg += status.CurrentPlayerCount + " / " + status.MaxPlayerCount;

                if (MCSWBot.Conf.DrawPlots)
                {
                    var ud = SkiaPlotter.GetUserData(item);
                    if (ud.Length > 4) plots.Add(ud);
                }

                // add player names or continue
                if ((status?.OnlinePlayers?.Count ?? 0) <= 0) continue;
                var n = "";
                foreach (var plr in status.OnlinePlayers)
                {
                    var span = DateTime.Now - item.SeenTime[plr.Id];
                    n += $"\r\n  + {plr.Name} ({span.TotalHours:0.00} hrs)";
                }
                msg += "<code>" + n + "</code>";
            }

            // Send text only?
            if (!MCSWBot.Conf.DrawPlots || plots.Count <= 0)
                return SendMsg(msg, null, ParseMode.Html, 0, false, editMessage);

            // Send text on image
            using var bm = SkiaPlotter.PlotData(plots, "Days Ago", "Player Online");
            return SendMsg(msg, bm, ParseMode.Html, 0, false, editMessage);
        }


        /// <summary>
        ///     Clean-up Imaging & History Data
        /// </summary>
        private void CleanData()
        {
            // clear old elements > 3 days
            ImagingData.FindAll(id => id.Acquired < DateTime.Now - TimeSpan.FromHours(MCSWBot.Conf.HistoryHours)).ForEach(id =>
            {
                ImagingData.Remove(id);
            });
            
            // clear old elements if count > 30
            ImagingData.OrderBy(id => id.Acquired.Ticks).Where((a, i) => i > 30).ToList().ForEach(id =>
            {
                ImagingData.Remove(id);
            });
            
            Servers.ForEach(s => s.CleanData());

            GC.Collect();
        }
    }
}