using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using McswBot2.Event;
using McswBot2.Minecraft;
using McswBot2.Static;
using Newtonsoft.Json;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace McswBot2.Objects;

[Serializable]
public class TgGroup
{
    public int LivePlayerMsgId;

    // Identity
    public List<ServerStatus> Servers = new();

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
    /// <param name="servers"></param>
    /// <param name="Base"></param>
    /// <param name="livePlayerMsgId"></param>
    [JsonConstructor]
    public TgGroup(List<ServerStatus> servers, Chat Base, int livePlayerMsgId)
    {
        Servers = servers;
        // register Events
        Servers.ForEach(srv => { srv.ChangedEvent += StatusChanged; });
        this.Base = Base;
        LivePlayerMsgId = livePlayerMsgId;
    }

    public Chat Base { get; set; }


    /// <summary>
    ///     Register & Start updating all the servers in this group after deserializing
    /// </summary>
    private void Update(ServerStatus status, EventBase[] events)
    {
        foreach (var srv in Servers)
        {
            if (srv != status) continue;

            var updateMsg = $"[<code>{srv.Label}</code>]";
            var addMore = true;
            var added = false;
            foreach (var evt in events)
            {
                var (txt, more) = ProcessEventMessage(srv, evt);
                if (txt == null) continue;
                // stop adding further text after "online-status" event
                if (addMore) updateMsg += txt;
                addMore = addMore && more;
                added = true;
            }

            // no events processed => no message to send
            if (!added) continue;

            // get & scale server image or use empty
            var sent = false;
            if (srv.Sticker && srv.Last?.FavIcon != null)
            {
                using (var txtBmp = Imaging.MakeSticker(srv.Last.FavIcon, updateMsg))
                {
                    SendMsg(null, txtBmp, ParseMode.Html, 0, true);
                }

                sent = true;
            }

            // send message if sticker disabled or failed
            if (!sent) SendMsg(updateMsg, null, ParseMode.Html);
        }
    }

    /// <summary>
    ///     Processes EventBase and returns adequate message
    /// </summary>
    /// <param name="srv"></param>
    /// <param name="evt"></param>
    /// <returns>   string (message), bool (continue processing?)</returns>
    private static Tuple<string, bool> ProcessEventMessage(ServerStatus srv, EventBase evt)
    {
        // build & return message
        switch (evt)
        {
            case OnlineStatusEvent ose:
                return new Tuple<string, bool>(
                    (ose.ServerStatus ? EventMessages.ServerOnline : EventMessages.ServerOffline)
                    .Replace("<text>", ose.StatusText)
                    .Replace("<version>", ose.Version)
                    .Replace("<players>", $"{ose.CurrentPlayers} / {ose.MaxPlayers}"), false);

            case PlayerChangeEvent pce:
                var abs = Math.Abs(pce.PlayerDiff);
                var msg2 = pce.PlayerDiff > 0 ? EventMessages.CountJoin : EventMessages.CountLeave;
                msg2 = msg2.Replace("<count>", abs.ToString());
                msg2 = msg2.Replace("<player>", "Player" + (abs > 1 ? "s" : ""));
                return new Tuple<string, bool>(msg2, true);

            case PlayerStateEvent pse:
                var msg1 = pse.Online ? EventMessages.NameJoin : EventMessages.NameLeave;
                msg1 = msg1.Replace("<name>", pse.Player.Name);
                msg1 = msg1.Replace("<time>", pse.Player.PlayTime.TotalDays.ToString("0.00") + " d");
                return new Tuple<string, bool>(msg1, true);
        }

        return new Tuple<string, bool>("", false);
    }

    /// <summary>
    ///     Tries to find & return the server with defined label, if not found returns null
    /// </summary>
    /// <param name="l"></param>
    /// <returns></returns>
    internal ServerStatus? GetServer(string l)
    {
        return Servers.FirstOrDefault(item =>
            string.Equals(item.Label, l, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    ///     Add a server with given label, address and port
    /// </summary>
    /// <param name="l"></param>
    /// <param name="adr"></param>
    /// <param name="p"></param>
    /// <param name="reuse"></param>
    internal void AddServer(string l, string adr, int p, bool reuse = true)
    {
        var news = new ServerStatus(l, adr, p, reuse);
        news.ChangedEvent += StatusChanged;
        Servers.Add(news);
    }


    /// <summary>
    ///     Event on Server status change
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StatusChanged(object? sender, EventBase[] e)
    {
        // get the sending status && do the updating
        if (sender is ServerStatus status && e.Length > 0)
            Update(status, e);

        UpdateLivePlayer();

        // free resources
        GC.Collect();
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
        return true;
    }


    /// <summary>
    ///     Send a message to this group with a given bitmap
    /// </summary>
    /// <param name="text"></param>
    /// <param name="bitmap"></param>
    /// <param name="pm"></param>
    /// <param name="replyMsg"></param>
    /// <param name="sticker"></param>
    /// <param name="editMsg"></param>
    internal Message? SendMsg(string? text = null, SKImage? bitmap = null, ParseMode pm = ParseMode.Markdown,
        int replyMsg = 0, bool sticker = false, int editMsg = 0)
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
    /// <param name="editMsg"></param>
    private Message? SendMsgStream(string? text = null, Stream? imgStream = null, ParseMode pm = ParseMode.Markdown,
        int replyMsg = 0, bool sticker = false, int editMsg = 0)
    {
        Message? lMsg;
        if (imgStream != null)
        {
            // send sticker (cant be updated!)
            if (sticker)
            {
                var iof = new InputOnlineFile(imgStream);

                if (editMsg != 0) Logger.WriteLine("Stickers cant be updated!");

                lMsg = McswBot.Client?.SendStickerAsync(Base.Id, iof, false, replyMsg).Result;
                // send text in response immediately afterwards ?
                if (text != null && lMsg != null)
                {
                    replyMsg = lMsg.MessageId;
                    lMsg = McswBot.Client?.SendTextMessageAsync(Base.Id,
                        text,
                        pm,
                        disableWebPagePreview: true,
                        disableNotification: false,
                        replyToMessageId: replyMsg).Result;
                }
            }
            // send normal image
            else
            {
                // Update existing image
                lMsg = editMsg != 0
                    ? McswBot.Client?.EditMessageMediaAsync(new ChatId(Base.Id),
                        editMsg,
                        new InputMediaPhoto(new InputMedia(imgStream, "updated.png"))
                            { Caption = text, ParseMode = pm }).Result
                    :
                    // Send new Image
                    McswBot.Client?.SendPhotoAsync(Base.Id,
                        new InputOnlineFile(imgStream),
                        text,
                        pm,
                        disableNotification: false,
                        replyToMessageId: replyMsg).Result;
            }

            imgStream.Dispose();
        }
        // send normal text
        else if (text != null)
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

    /// <summary>
    ///     Automatically update live player message?
    /// </summary>
    private void UpdateLivePlayer()
    {
        if (LivePlayerMsgId == 0) return;
        SendPlayersMessage(LivePlayerMsgId);
    }

    /// <summary>
    ///     Send or Edit Online Player Message
    /// </summary>
    /// <param name="editMessage"></param>
    /// <returns></returns>
    internal Message? SendPlayersMessage(int editMessage = 0)
    {
        var msg = "";
        var plots = new List<SkiaPlotter.PlottableData>();
        var dn = DateTime.Now;
        var scaleTxt = SkiaPlotter.GetTimeScale(Servers, out var minuteRange);

        foreach (var item in Servers)
        {
            var status = item.Last;
            if (status == null)
                continue;

            msg += "\r\n[<code>" + item.Label + "</code>] ";
            if (!status.HadSuccess) msg += " Offline";
            else msg += status.CurrentPlayerCount + " / " + status.MaxPlayerCount;

            // Draw Plots
            var ud = SkiaPlotter.GetUserData(item, minuteRange);
            if (ud.Length > 0) plots.Add(ud);

            // add player names or continue
            if (status.OnlinePlayers.Count <= 0) continue;
            var n = "";
            foreach (var plr in status.OnlinePlayers)
            {
                var span = dn - plr.LastSeen;
                n += $"\r\n  + {plr.Name} ({span.TotalHours:0.00} hrs)";
            }

            msg += "<code>" + n + "</code>";
        }

        Program.WriteLine("Updating live Players msg in group: " + Base.Id);

        // Send text only?
        if (plots.Count <= 0)
            return SendMsg(msg, null, ParseMode.Html, 0, false, editMessage);

        // Send text on image
        using var bm = SkiaPlotter.PlotData(plots, scaleTxt, "Players");
        return SendMsg(msg, bm, ParseMode.Html, 0, false, editMessage);
    }
}