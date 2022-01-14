using mcswbot2.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace mcswbot2.Static
{
    internal static class Storage
    {
        private const string CON = "config.json";
        private const string USR = "data/users.json";
        private const string GRP = "data/groups.json";

        /// <summary>
        ///     Load & Json Decode the user & group settings
        /// </summary>
        internal static void Load()
        {
            try
            {
                var set = new JsonSerializerSettings();

                // load config
                if (File.Exists(CON))
                {
                    var json = File.ReadAllText(CON);
                    MCSWBot.Conf = JsonConvert.DeserializeObject<Config>(json, set);
                }
                else
                {
                    Save();
                    Program.WriteLine(
                        "\r\n\r\n\tWARNING: CONFIG JUST GOT CREATED. PLEASE MODIFY IT BEFORE STARTING AGAIN.\r\n");
                    Environment.Exit(0);
                }

                // make data dir
                if(!Directory.Exists("data"))
                    Directory.CreateDirectory("data");

                // load users objects if file exists
                if (File.Exists(USR))
                {
                    var json = File.ReadAllText(USR);
                    MCSWBot.TgUsers.AddRange(
                        JsonConvert.DeserializeObject<TgUser[]>(json, set) ?? Array.Empty<TgUser>());
                }

                // load group objects if file exists
                if (File.Exists(GRP))
                {
                    var json = File.ReadAllText(GRP);
                    MCSWBot.TgGroups.AddRange(JsonConvert.DeserializeObject<TgGroup[]>(json, set) ??
                                              Array.Empty<TgGroup>());
                }

                // done
                Program.WriteLine($"Loaded data. [{MCSWBot.TgUsers.Count} Users, {MCSWBot.TgGroups.Count} Groups]");
            }
            catch (Exception e)
            {
                Program.WriteLine("Error when loading data: " + e);
            }
        }

        /// <summary>
        ///     Json encode & save the user & group settings
        /// </summary>
        internal static void Save()
        {
            try
            {
                var set = new JsonSerializerSettings();

                // write config
                var str = JsonConvert.SerializeObject(MCSWBot.Conf, Formatting.None, set);
                str = JToken.Parse(str).ToString(Formatting.Indented);
                File.WriteAllText(CON, str);
                // write user if any
                if (MCSWBot.TgUsers.Count > 0)
                {
                    str = JsonConvert.SerializeObject(MCSWBot.TgUsers, Formatting.None, set);
                    File.WriteAllText(USR, str);
                }

                // write groups if any
                if (MCSWBot.TgGroups.Count > 0)
                {
                    str = JsonConvert.SerializeObject(MCSWBot.TgGroups, Formatting.None, set);
                    File.WriteAllText(GRP, str);
                }

                // done
                Program.WriteLine($"Saved data. [{MCSWBot.TgUsers.Count} Users, {MCSWBot.TgGroups.Count} Groups]");
            }
            catch (Exception e)
            {
                Program.WriteLine("Error when saving data: " + e);
            }
        }
    }
}