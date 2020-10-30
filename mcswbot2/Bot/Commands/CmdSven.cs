using mcswbot2.Bot.Objects;
using ZufallSatz;
using System;
using System.Drawing;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace mcswbot2.Bot.Commands
{
    class CmdSven : ICommand
    {
        internal override string Command() => "sven";

        internal override void Call(Message m, TgGroup g, TgUser u, string[] args, bool dev)
        {
            if (m.Text.ToLower() == "/sven did nothing wrong")
            {
                g.Tahnos = !g.Tahnos;
                g.SendMsg(g.Tahnos.ToString(), replyMsg: m.MessageId);
                return;
            }

            // check all saved infos for reply "NoTalkInfo" message id
            if (m.ReplyToMessage != null && m.ReplyToMessage.From.Id == TgBot.TgBotUser.Id)
            {
                foreach (var nti in g.ImagingData)
                {
                    if (nti.RelatedMsgID == m.ReplyToMessage.MessageId)
                    {
                        var replMsg = "⚠️ <b>NSFW</b> ⚠️\r\n";
                        if (!string.IsNullOrWhiteSpace(nti.SResult.source))
                            replMsg += Utils.WrapLink(nti.SResult.source, "Source") + "\r\n";
                        if (nti.SResult.fileUrl != null)
                            replMsg += Utils.WrapLink(nti.SResult.fileUrl.ToString(), "File");
                        g.SendMsg(replMsg, null, ParseMode.Html, m.ReplyToMessage.MessageId);
                        return;
                    }
                }
                return;
            }

            var name = "Sven";
            if (args.Length > 1 && args[1].Length > 1) name = args[1].Substring(0, Math.Min(20, args[1].Length));
            var t = Generator.RandomSatz(name);
            var bb = TahnosInfo.Get();
            using (var b = bb != null ? bb.Bmap : new Bitmap(512, 512))
            using (var c = Imaging.MakeSticker(b, t))
                g.SendMsg(null, c, ParseMode.Default, 0, true);
        }
    }
}
