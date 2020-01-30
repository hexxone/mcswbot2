using mcswbot2.Bot;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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

            var newC = new TgGroup() { Base = c };
            TgBot.TgGroups.Add(newC);
            return newC;
        }

        /// <summary>
        ///     Load & Json Decode the user & group settings
        /// </summary>
        public static void Load()
        {
            // load users simply
            if (File.Exists("users.json"))
            {
                var json = File.ReadAllText("users.json");
                TgBot.TgUsers.AddRange(JsonConvert.DeserializeObject<TgUser[]>(json));
            }

            // load group objects
            if (File.Exists("groups.json"))
            {
                var json = File.ReadAllText("groups.json");
                var des = JsonConvert.DeserializeObject<TgGroup[]>(json);
                // post-processing
                foreach (var grp in des)
                {
                    // get deserialized servers & clear the originals
                    var arr = grp.Servers.ToArray();
                    grp.Servers.Clear();
                    // add all servers back using the factory
                    foreach (var srv in arr)
                        grp.AddServer(srv.Label, srv.Base.Address, srv.Base.Port);
                }
                // add objects after deserializing & initializing
                TgBot.TgGroups.AddRange(des);
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

        /// <summary>
        ///     Will verify a given server label string
        /// </summary>
        /// <param name="txt"></param>
        public static void VerifyLabel(string txt)
        {
            if (txt.Contains('.')) throw new Exception("Label should not contain Dots! ('.')");
            if (txt.Length > 12) throw new Exception("Label should be 12 characters at max!");
        }

        /// <summary>
        ///     Will verify a given server address and port
        ///     by basic port checking, Uri-checking,
        ///     name resolving and regex-checking for private ip ranges.
        /// </summary>
        /// <param name="addr">server address ip or domain</param>
        /// <param name="port">mc server port</param>
        /// <returns></returns>
        public static void VerifyAddress(string addr, int port)
        {
            // dont 
            if (addr.Length > 256) throw new Exception("The address length should not exceed 256 characters!");
            // port check
            if (port < 80 || port > 65534) throw new Exception("Invalid Port! Choose one above 79 and below 65535.");
            // check if ip address was entered
            var ipRegex = @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}";
            var resolved = "";
            if (Regex.IsMatch(addr, ipRegex)) resolved = addr;
            else
            {
                // some hostname checks
                if (Dns.GetHostName().ToLower() == addr.ToLower() ||
                    Uri.CheckHostName(addr) == UriHostNameType.Unknown ||
                    !addr.Contains('.'))
                        throw new Exception("Invalid hostname!");
                // resolve
                var host = Dns.GetHostEntry(addr);
                if (host == null || host.AddressList == null || host.AddressList.Length == 0) throw new Exception("No hostname address entries!");
                // try toget ipv4 entry
                try
                {
                    resolved = host.AddressList.First(h => h.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
                }
                catch {
                    try
                    {
                        resolved = host.AddressList.First(h => h.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6).ToString();
                    }
                    catch { }
                }
                if (string.IsNullOrEmpty(resolved)) throw new Exception("No valid hostname resolved.");
            }

            /* Block following ip-ranges
                127. 0.0.0 – 127.255.255.255     127.0.0.0 /8
                10.  0.0.0 –  10.255.255.255      10.0.0.0 /8
                172. 16.0.0 – 172. 31.255.255    172.16.0.0 /12
                192.168.0.0 – 192.168.255.255   192.168.0.0 /16
            */
            // assumes that ipv4 format sanity checking has already been done 
            var blockStr = @"(192\.168(\.[0-9]{1,3}){2})|(172\.(1[6-9]|2[0-9]|3[0-1])(\.[0-9]{1,3}){2})|([10|27]+(\.[0-9]{1,3}){3})";
            // private check
            if (Regex.IsMatch(resolved, blockStr)) throw new Exception("Invalid IP-Address Range!");
            // all ok
        }

    }
}