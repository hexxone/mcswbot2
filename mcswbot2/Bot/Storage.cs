using System;
using mcswbot2.Bot.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mcswbot2.Bot
{
    internal static class Storage
    {
        /// <summary>
        ///     Load & Json Decode the user & group settings
        /// </summary>
        internal static void Load()
        {
            try
            {
                // load config
                if (System.IO.File.Exists("config.json"))
                {
                    var json = System.IO.File.ReadAllText("config.json");
                    TgBot.Conf = JsonConvert.DeserializeObject<Config>(json);
                }
                else
                {
                    Save();
                    Program.WriteLine("\r\n\r\n\tWARNING: CONFIG JUST GOT CREATED. PLEASE MODIFY IT BEFORE STARTING AGAIN.\r\n");
                    Environment.Exit(0);
                }

                // load users objects if file exists
                if (System.IO.File.Exists("users.json"))
                {
                    var json = System.IO.File.ReadAllText("users.json");
                    TgBot.TgUsers.AddRange(JsonConvert.DeserializeObject<TgUser[]>(json));
                }

                // load group objects if file exists
                if (System.IO.File.Exists("groups.json"))
                {
                    var json = System.IO.File.ReadAllText("groups.json");
                    var des = JsonConvert.DeserializeObject<TgGroup[]>(json);
                    foreach (var g in des)
                    {
                        var asd = g.Servers.ToArray();
                        g.Servers.Clear();
                        foreach (var pair in asd)
                            g.AddServer(pair.Label, pair.Address, pair.Port);
                    }

                    TgBot.TgGroups.AddRange(des);
                }
                // done
                Program.WriteLine($"Loaded data. [{TgBot.TgUsers.Count} Users, {TgBot.TgGroups.Count} Groups]");
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
                // write config
                var str = JsonConvert.SerializeObject(TgBot.Conf);
                str = JToken.Parse(str).ToString(Formatting.Indented);
                System.IO.File.WriteAllText("config.json", str);
                // write user if any
                if (TgBot.TgUsers.Count > 0)
                {
                    str = JsonConvert.SerializeObject(TgBot.TgUsers);
                    System.IO.File.WriteAllText("users.json", str);
                }
                // write groups if any
                if (TgBot.TgGroups.Count > 0)
                {
                    str = JsonConvert.SerializeObject(TgBot.TgGroups);
                    System.IO.File.WriteAllText("groups.json", str);
                }
                // done
                Program.WriteLine($"Saved data. [{TgBot.TgUsers.Count} Users, {TgBot.TgGroups.Count} Groups]");
            }
            catch (Exception e)
            {
                Program.WriteLine("Error when saving data: " + e);
            }
        }
    }
}