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
            if(m.Text.ToLower() == "/sven did nothing wrong")
            {
                g.Thanos = !g.Thanos;
                g.SendMsg(g.Thanos.ToString(), replyMsg:m.MessageId);
                return;
            }
            var name = "Sven";
            if (args.Length > 1 && args[1].Length > 1) name = args[1].Substring(0, Math.Min(20, args[1].Length));
            var t = Generator.RandomSatz(name);
            using (var b = new Bitmap(512, 256))
            using (var c = Imaging.MakeSticker(b, t))
                g.SendMsg(null, c, ParseMode.Default, 0, true);
        }
    }
}
