using System;
using mcswbot2.Objects;
using mcswbot2.Telegram;
using SkiaSharp;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZufallSatz;

namespace mcswbot2.Commands
{
    class CmdTahnos : ICommand
    {
        internal override string Command() => "tahnos";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            if (m.Text.ToLower() == "/tahnos did nothing wrong")
            {
                g.Tahnos = !g.Tahnos;
                g.SendMsg(g.Tahnos.ToString(), replyMsg: m.MessageId);
                return;
            }

            // check all saved infos for reply "NoTalkInfo" message id
            if (m.ReplyToMessage != null && m.ReplyToMessage.From.Id == MCSWBot.TgBotUser.Id)
            {
                foreach (var nti in g.ImagingData)
                {
                    if (nti.RelatedMsgID != m.ReplyToMessage.MessageId) continue;

                    var replMsg = "⚠️ <b>NSFW</b> ⚠️\r\n";
                    if (!string.IsNullOrWhiteSpace(nti.SResult.Source))
                        replMsg += Utils.WrapLink(nti.SResult.Source, "Source") + "\r\n";
                    if (nti.SResult.FileUrl != null)
                        replMsg += Utils.WrapLink(nti.SResult.FileUrl.ToString(), "File");
                    g.SendMsg(replMsg, null, ParseMode.Html, m.ReplyToMessage.MessageId);
                    return;
                }
                return;
            }

            var name = "Tahnos";
            if (args.Length > 1 && args[1].Length > 1) name = args[1].Substring(0, Math.Min(20, args[1].Length));
            var t = Generator.RandomSatz(name);
            var bb = TahnosInfo.Get();
            using var b = bb != null ? bb.Bmap : SKImage.Create(new SKImageInfo(512, 512));
            using var c = Imaging.MakeSticker(b, t);
            g.SendMsg(null, c, ParseMode.Default, 0, true);
        }
    }
}
