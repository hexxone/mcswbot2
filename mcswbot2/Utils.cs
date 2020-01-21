using mcswbot2.Bot;
using Newtonsoft.Json;
using System.Drawing;
using Telegram.Bot.Types;
using static mcswbot2.Lib.Types;
using File = System.IO.File;

namespace mcswbot2
{
    public class Utils
    {
        /// <summary>
        ///     removes Minecraft Chat Syle informations
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string FixMcChat(string s)
        {
            var l = new[]
            {
                "§4", "§c", "§6", "§e",
                "§2", "§a", "§b", "§3",
                "§1", "§9", "§d", "§5",
                "§f", "§7", "§8", "§0",
                "§l", "§m", "§n", "§o", "§r"
            };
            foreach (var t in l) s = s.Replace(t, "");
            return s;
        }

        /// <summary>
        ///     Find or Add user
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static TgUser GetUser(User u)
        {
            foreach (var usr in TgBot.TgUsers)
                if (usr.Base.Id == u.Id)
                {
                    usr.Base = u;
                    return usr;
                }

            var newU = new TgUser(u);
            TgBot.TgUsers.Add(newU);
            return newU;
        }

        /// <summary>
        ///     Find or Add Group
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static TgGroup GetGroup(Chat c)
        {
            foreach (var cc in TgBot.TgGroups)
                if (cc.Base.Id == c.Id)
                    return cc;

            var newC = new TgGroup(c);
            TgBot.TgGroups.Add(newC);
            return newC;
        }

        /// <summary>
        ///     Load & Json Decode the user & group settings
        /// </summary>
        public static void Load()
        {
            if (File.Exists("users.json"))
            {
                var json = File.ReadAllText("users.json");
                TgBot.TgUsers.AddRange(JsonConvert.DeserializeObject<TgUser[]>(json));
            }

            if (File.Exists("groups.json"))
            {
                var json = File.ReadAllText("groups.json");
                TgBot.TgGroups.AddRange(JsonConvert.DeserializeObject<TgGroup[]>(json));
            }

            Program.WriteLine($"Loaded data. [{TgBot.TgUsers.Count} Users, {TgBot.TgGroups.Count} Groups]");
        }

        /// <summary>
        ///     Json encode & save the user & group settings
        /// </summary>
        public static void Save()
        {
            var str1 = JsonConvert.SerializeObject(TgBot.TgUsers);
            File.WriteAllText("users.json", str1);

            var str2 = JsonConvert.SerializeObject(TgBot.TgGroups);
            File.WriteAllText("groups.json", str2);

            Program.WriteLine("Saved data.");
        }

        /// <summary>
        ///     Will Plot and save Data to a file
        /// </summary>
        /// <param name="dat"></param>
        public static Bitmap PlotData(PlottableData[] dat, string xLab, string yLab)
        {
            var plt = new ScottPlot.Plot(345, 210);
            plt.XLabel(xLab);
            plt.YLabel(yLab);
            plt.Legend(true);
            foreach (var da in dat)
                plt.PlotScatter(da.dataX, da.dataY, null, 1D, 5D, da.Label);
            return plt.GetBitmap();
        }
    }
}